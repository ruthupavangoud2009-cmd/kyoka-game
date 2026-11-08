using UnityEngine;

namespace Kyoka.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class KyokaPlayerController : MonoBehaviour
    {
        public float walkSpeed = 5f;
        public float sprintSpeed = 8f;
        public float jumpHeight = 1.3f;
        public float gravity = -22f;
        public float lookSensitivity = 2f;

        private CharacterController controller;
        private Vector3 verticalVelocity;
        private float yaw;
        private bool inputEnabled = true;

        public bool InputEnabled { get => inputEnabled; set => inputEnabled = value; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            yaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            if (!inputEnabled) return;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 move = transform.right * h + transform.forward * v;
            move = Vector3.ClampMagnitude(move, 1f);

            float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
            controller.Move(move * speed * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity.y < 0) verticalVelocity.y = -2f;
            if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            verticalVelocity.y += gravity * Time.deltaTime;
            controller.Move(verticalVelocity * Time.deltaTime);

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                yaw += Input.GetAxis("Mouse X") * lookSensitivity;
                transform.rotation = Quaternion.Euler(0, yaw, 0);
            }
        }

        public void SetMoveInput(Vector2 input)
        {
            // Hook for mobile/controller adapters. The default desktop path uses Unity axes.
        }
    }
}
