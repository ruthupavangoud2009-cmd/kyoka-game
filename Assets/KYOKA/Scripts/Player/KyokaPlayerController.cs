using UnityEngine;
using Kyoka.Core;

namespace Kyoka.Player
{
    public enum PlayerMovementState
    {
        Walking,
        Running,
        Sprinting,
        Crouching,
        Jumping,
        Falling,
        Swimming,
        Climbing
    }

    [RequireComponent(typeof(CharacterController))]
    public class KyokaPlayerController : MonoBehaviour
    {
        [Header("Movement Speeds")]
        public float walkSpeed = 4.5f;
        public float runSpeed = 7.0f;
        public float sprintSpeed = 10.5f;
        public float crouchSpeed = 2.5f;
        public float swimSpeed = 4.0f;
        public float climbSpeed = 3.5f;

        [Header("Jump & Physics")]
        public float jumpHeight = 1.4f;
        public float gravity = -22.0f;
        public float waterGravity = -4.0f;

        [Header("State")]
        [SerializeField] private PlayerMovementState currentState = PlayerMovementState.Walking;
        public PlayerMovementState CurrentState => currentState;
        public bool IsGrounded => controller != null && controller.isGrounded;
        public bool IsSwimming { get; set; }
        public bool IsClimbing { get; set; }
        public bool IsCrouching { get; private set; }

        [Header("Tactical Slide")]
        public float slideSpeed = 13.5f;
        public float slideDuration = 0.65f;
        private float slideTimer = 0f;
        public bool IsSliding => slideTimer > 0f;

        private CharacterController controller;
        private Vector3 verticalVelocity;
        private float yaw;
        private bool inputEnabled = true;

        private Kyoka.Character.KyokaProceduralAnimator anim;
        private Kyoka.Character.KyokaCharacterModel characterModel;
        private Kyoka.Combat.KyokaCombatSystem combatSystem;

        public bool InputEnabled { get => inputEnabled; set => inputEnabled = value; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            yaw = transform.eulerAngles.y;
            anim = GetComponent<Kyoka.Character.KyokaProceduralAnimator>();
            characterModel = GetComponent<Kyoka.Character.KyokaCharacterModel>();
            combatSystem = GetComponent<Kyoka.Combat.KyokaCombatSystem>();
        }

        private void Update()
        {
            if (!inputEnabled) return;

            var input = KyokaInputBridge.Instance;
            Vector2 moveVec = input != null ? input.MoveVector : Vector2.zero;
            Vector2 lookDelta = input != null ? input.LookDelta : Vector2.zero;
            bool sprint = input != null && input.SprintHeld;
            bool jump = input != null && input.JumpPressed;
            bool crouch = input != null && input.CrouchPressed;

            // Weapon switching hotkeys
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon("AssaultRifle");
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon("Shotgun");
            if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon("Sniper");
            if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchWeapon("Katana");

            // Tactical Slide (Sprint + Crouch)
            if (sprint && crouch && controller.isGrounded && slideTimer <= 0f)
            {
                slideTimer = slideDuration;
                controller.height = 0.9f;
                controller.center = new Vector3(0, 0.45f, 0);
            }

            if (slideTimer > 0f)
            {
                slideTimer -= Time.deltaTime;
                if (slideTimer <= 0f && !IsCrouching)
                {
                    controller.height = 1.8f;
                    controller.center = new Vector3(0, 0.9f, 0);
                }
            }

            if (crouch && slideTimer <= 0f)
            {
                IsCrouching = !IsCrouching;
                controller.height = IsCrouching ? 1.2f : 1.8f;
                controller.center = new Vector3(0, IsCrouching ? 0.6f : 0.9f, 0);
            }

            // Look yaw rotation
            yaw += lookDelta.x;
            transform.rotation = Quaternion.Euler(0, yaw, 0);

            if (IsSwimming)
            {
                HandleSwimming(moveVec, jump, sprint);
            }
            else if (IsClimbing)
            {
                HandleClimbing(moveVec);
            }
            else
            {
                HandleStandardMovement(moveVec, jump, sprint);
            }

            // Synchronize with Procedural Joint Animator
            if (anim != null)
            {
                anim.moveSpeed = controller.velocity.magnitude;
                anim.isGrounded = controller.isGrounded;
                anim.isAiming = (Input.GetMouseButton(1) || (input != null && input.DefendHeld));
            }
        }

        public void SwitchWeapon(string weaponId)
        {
            if (characterModel != null) characterModel.EquipWeapon(weaponId);
            Kyoka.Audio.KyokaAudioManager.Instance?.PlaySoundEffect(Kyoka.Audio.SoundEffectType.Reload);
        }

        private void HandleStandardMovement(Vector2 moveVec, bool jump, bool sprint)
        {
            Vector3 moveDirection = transform.right * moveVec.x + transform.forward * moveVec.y;
            moveDirection = Vector3.ClampMagnitude(moveDirection, 1.0f);

            // Determine speed
            float targetSpeed = walkSpeed;
            if (IsSliding)
            {
                targetSpeed = slideSpeed;
                moveDirection = transform.forward; // Lock forward trajectory during slide
            }
            else if (IsCrouching)
            {
                targetSpeed = crouchSpeed;
                currentState = PlayerMovementState.Crouching;
            }
            else if (moveDirection.sqrMagnitude > 0.01f)
            {
                if (sprint)
                {
                    targetSpeed = sprintSpeed;
                    currentState = PlayerMovementState.Sprinting;
                }
                else
                {
                    targetSpeed = runSpeed;
                    currentState = PlayerMovementState.Running;
                }
            }
            else
            {
                currentState = PlayerMovementState.Walking;
            }

            controller.Move(moveDirection * targetSpeed * Time.deltaTime);

            // Vertical movement / Gravity
            if (controller.isGrounded)
            {
                if (verticalVelocity.y < 0) verticalVelocity.y = -2.0f;

                if (jump)
                {
                    verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
                    currentState = PlayerMovementState.Jumping;
                }
            }
            else
            {
                if (verticalVelocity.y < 0 && currentState != PlayerMovementState.Jumping)
                {
                    currentState = PlayerMovementState.Falling;
                }
            }

            verticalVelocity.y += gravity * Time.deltaTime;
            controller.Move(verticalVelocity * Time.deltaTime);
        }

        private void HandleSwimming(Vector2 moveVec, bool jump, bool sprint)
        {
            currentState = PlayerMovementState.Swimming;
            Vector3 move = transform.right * moveVec.x + transform.forward * moveVec.y;
            float speed = sprint ? swimSpeed * 1.4f : swimSpeed;

            // Vertical swim movement (Jump swims up, Crouch swims down)
            float swimVertical = 0f;
            if (jump) swimVertical = 2.5f;
            else if (IsCrouching) swimVertical = -2.5f;
            else swimVertical = -0.5f; // Slight sinking

            Vector3 swimVelocity = move * speed + Vector3.up * swimVertical;
            controller.Move(swimVelocity * Time.deltaTime);
        }

        private void HandleClimbing(Vector2 moveVec)
        {
            currentState = PlayerMovementState.Climbing;
            Vector3 climbVelocity = Vector3.up * moveVec.y * climbSpeed + transform.right * moveVec.x * (climbSpeed * 0.5f);
            controller.Move(climbVelocity * Time.deltaTime);
            verticalVelocity = Vector3.zero;
        }

        public void SetMoveInput(Vector2 input)
        {
            if (KyokaInputBridge.Instance != null)
                KyokaInputBridge.Instance.SetTouchMoveInput(input);
        }

        public void Teleport(Vector3 position)
        {
            if (controller) controller.enabled = false;
            transform.position = position;
            if (controller) controller.enabled = true;
        }
    }
}
