using UnityEngine;
using System;

namespace Kyoka.Core
{
    /// <summary>
    /// Unified input bridge for KYOKA. Seamlessly fuses Desktop (Keyboard/Mouse),
    /// Gamepad, and Mobile Touchscreen controls so gameplay systems work uniformly on PC and Android.
    /// </summary>
    public class KyokaInputBridge : MonoBehaviour
    {
        public static KyokaInputBridge Instance { get; private set; }

        [Header("Movement & Camera")]
        public Vector2 MoveVector { get; private set; }
        public Vector2 LookDelta { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool CrouchPressed { get; private set; }

        [Header("Combat & Actions")]
        public bool AttackPressed { get; private set; }
        public bool DefendHeld { get; private set; }
        public bool Ability1Pressed { get; private set; }
        public bool Ability2Pressed { get; private set; }
        public bool Ability3Pressed { get; private set; }
        public bool TransformPressed { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool CameraTogglePressed { get; private set; }
        public bool BuildPressed { get; private set; }
        public bool InventoryTogglePressed { get; private set; }
        public bool MapTogglePressed { get; private set; }
        public bool PauseTogglePressed { get; private set; }

        [Header("Hotbar Slots")]
        public int SelectedSlotIndex { get; private set; } = 0;

        // Mobile touch injections
        private Vector2 touchMoveInput;
        private Vector2 touchLookInput;
        private bool touchSprintToggle;
        private bool touchJumpTrigger;
        private bool touchAttackTrigger;
        private bool touchInteractTrigger;
        private bool touchAbility1Trigger;
        private bool touchAbility2Trigger;
        private bool touchCameraToggleTrigger;

        [Header("Sensitivity Settings")]
        public float mouseSensitivity = 2.0f;
        public float touchLookSensitivity = 1.5f;
        public bool invertY = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            ReadDesktopAndGamepadInput();
            PollHotbarInputs();
        }

        private void LateUpdate()
        {
            // Clear one-shot triggers at the end of the frame
            touchJumpTrigger = false;
            touchAttackTrigger = false;
            touchInteractTrigger = false;
            touchAbility1Trigger = false;
            touchAbility2Trigger = false;
            touchCameraToggleTrigger = false;
        }

        private void ReadDesktopAndGamepadInput()
        {
            // Desktop horizontal/vertical
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector2 rawDesktopMove = new Vector2(h, v);

            // Combine desktop move with mobile joystick
            Vector2 combinedMove = rawDesktopMove.sqrMagnitude > 0.01f ? rawDesktopMove : touchMoveInput;
            MoveVector = Vector2.ClampMagnitude(combinedMove, 1.0f);

            // Look delta (Mouse + Mobile Touch Swipe)
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? 1f : -1f);
            Vector2 rawMouseLook = new Vector2(mouseX, mouseY);

            if (touchLookInput.sqrMagnitude > 0.001f)
            {
                LookDelta = touchLookInput * touchLookSensitivity;
                touchLookInput = Vector2.zero; // Consumed
            }
            else
            {
                LookDelta = rawMouseLook;
            }

            // Sprint
            SprintHeld = Input.GetKey(KeyCode.LeftShift) || touchSprintToggle;

            // Jump
            JumpPressed = Input.GetKeyDown(KeyCode.Space) || touchJumpTrigger;

            // Crouch
            CrouchPressed = Input.GetKeyDown(KeyCode.C);

            // Attack (Left Click or Mobile Button)
            AttackPressed = Input.GetMouseButtonDown(0) || touchAttackTrigger;

            // Defend (Right Click)
            DefendHeld = Input.GetMouseButton(1);

            // Abilities
            Ability1Pressed = Input.GetKeyDown(KeyCode.Q) || touchAbility1Trigger;
            Ability2Pressed = Input.GetKeyDown(KeyCode.F) || touchAbility2Trigger;
            Ability3Pressed = Input.GetKeyDown(KeyCode.R);
            TransformPressed = Input.GetKeyDown(KeyCode.T);

            // Interact (E or Mobile Button)
            InteractPressed = Input.GetKeyDown(KeyCode.E) || touchInteractTrigger;

            // Camera toggle (V or Mobile Button)
            CameraTogglePressed = Input.GetKeyDown(KeyCode.V) || touchCameraToggleTrigger;

            // Creative building
            BuildPressed = Input.GetKeyDown(KeyCode.B);

            // UI Toggles
            InventoryTogglePressed = Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I);
            MapTogglePressed = Input.GetKeyDown(KeyCode.M);
            PauseTogglePressed = Input.GetKeyDown(KeyCode.Escape);
        }

        private void PollHotbarInputs()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectedSlotIndex = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectedSlotIndex = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectedSlotIndex = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectedSlotIndex = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha5)) SelectedSlotIndex = 4;
        }

        #region Mobile Input API (Called by KyokaTouchControls UI)

        public void SetTouchMoveInput(Vector2 direction) => touchMoveInput = direction;
        public void AddTouchLookDelta(Vector2 delta) => touchLookInput += delta;
        public void SetTouchSprint(bool active) => touchSprintToggle = active;
        public void TriggerTouchJump() => touchJumpTrigger = true;
        public void TriggerTouchAttack() => touchAttackTrigger = true;
        public void TriggerTouchInteract() => touchInteractTrigger = true;
        public void TriggerTouchAbility1() => touchAbility1Trigger = true;
        public void TriggerTouchAbility2() => touchAbility2Trigger = true;
        public void TriggerTouchCameraToggle() => touchCameraToggleTrigger = true;
        public void SetSelectedSlot(int index) => SelectedSlotIndex = Mathf.Clamp(index, 0, 9);

        #endregion
    }
}
