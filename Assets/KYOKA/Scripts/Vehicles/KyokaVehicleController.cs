using UnityEngine;

namespace Kyoka.Vehicles
{
    public enum KyokaVehicleType { Car, Motorcycle, Boat, Aircraft }

    public class KyokaVehicleController : MonoBehaviour
    {
        public KyokaVehicleType vehicleType;
        public float moveSpeed = 12f;
        public float turnSpeed = 65f;
        public bool IsOccupied { get; private set; }
        private Transform occupant;
        private Rigidbody body;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (!body) body = gameObject.AddComponent<Rigidbody>();
            body.mass = vehicleType == KyokaVehicleType.Aircraft ? 800f : 500f;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        private void FixedUpdate()
        {
            if (!IsOccupied) return;
            float throttle = Input.GetAxis("Vertical");
            float steering = Input.GetAxis("Horizontal");
            body.AddForce(transform.forward * throttle * moveSpeed, ForceMode.Acceleration);
            transform.Rotate(Vector3.up, steering * turnSpeed * Time.fixedDeltaTime);
            if (vehicleType == KyokaVehicleType.Aircraft)
                body.AddForce(Vector3.up * Input.GetAxis("Jump") * moveSpeed * 0.4f, ForceMode.Acceleration);
        }

        public bool Enter(Transform player)
        {
            if (IsOccupied || !player) return false;
            IsOccupied = true;
            occupant = player;
            var controller = player.GetComponent<CharacterController>();
            if (controller) controller.enabled = false;
            player.SetParent(transform);
            player.localPosition = new Vector3(0, 1.1f, 0);
            return true;
        }

        public void Exit()
        {
            if (!IsOccupied) return;

            if (occupant)
            {
                occupant.SetParent(null);
                occupant.position = transform.position + transform.right * 2f + Vector3.up;
                var controller = occupant.GetComponent<CharacterController>();
                if (controller) controller.enabled = true;
            }

            occupant = null;
            IsOccupied = false;
        }
    }
}
