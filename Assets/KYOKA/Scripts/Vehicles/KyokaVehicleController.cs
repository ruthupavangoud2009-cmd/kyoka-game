using UnityEngine;
using System;
using System.Collections.Generic;
using Kyoka.Core;
using Kyoka.Player;

namespace Kyoka.Vehicles
{
    public enum KyokaVehicleType { Car, Motorcycle, Boat, Aircraft }

    public class KyokaVehicleController : MonoBehaviour
    {
        [Header("Vehicle Classification")]
        public KyokaVehicleType vehicleType = KyokaVehicleType.Car;
        public string vehicleName = "Kyoka Cruiser";

        [Header("Movement & Handling")]
        public float acceleration = 25f;
        public float maxSpeed = 35f;
        public float turnSpeed = 70f;
        public float brakeForce = 35f;
        public float reverseSpeed = 12f;

        [Header("Aircraft Specifics")]
        public float flightLift = 20f;
        public float rollSpeed = 60f;
        public float pitchSpeed = 50f;

        [Header("Boat Specifics")]
        public float waterDrag = 1.5f;
        public float buoyancy = 12f;

        [Header("Health & Durability")]
        public int maxHealth = 300;
        public int currentHealth = 300;
        public float fuel = 100f;
        public float maxFuel = 100f;
        public float fuelConsumptionPerSec = 0.5f;

        [Header("Passenger Seating")]
        public int passengerCapacity = 4;
        private readonly List<Transform> currentPassengers = new List<Transform>();
        public Transform DriverSeat { get; private set; }
        public bool IsOccupied => DriverSeat != null;

        private Rigidbody body;
        private Renderer vehicleRenderer;

        public event Action<int, int> OnVehicleHealthChanged;
        public event Action<float, float> OnFuelChanged;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (!body) body = gameObject.AddComponent<Rigidbody>();

            vehicleRenderer = GetComponent<Renderer>();

            ConfigurePhysicsForType();
            currentHealth = maxHealth;
        }

        private void ConfigurePhysicsForType()
        {
            switch (vehicleType)
            {
                case KyokaVehicleType.Car:
                    body.mass = 800f;
                    body.drag = 0.2f;
                    body.angularDrag = 2.0f;
                    body.constraints = RigidbodyConstraints.FreezeRotationZ;
                    break;
                case KyokaVehicleType.Motorcycle:
                    body.mass = 250f;
                    body.drag = 0.15f;
                    body.angularDrag = 2.5f;
                    break;
                case KyokaVehicleType.Boat:
                    body.mass = 1200f;
                    body.drag = waterDrag;
                    body.angularDrag = 3.0f;
                    break;
                case KyokaVehicleType.Aircraft:
                    body.mass = 650f;
                    body.drag = 0.1f;
                    body.angularDrag = 1.5f;
                    body.useGravity = true;
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (!IsOccupied || fuel <= 0) return;

            var input = KyokaInputBridge.Instance;
            float throttle = input != null ? input.MoveVector.y : Input.GetAxis("Vertical");
            float steering = input != null ? input.MoveVector.x : Input.GetAxis("Horizontal");

            // Consume fuel
            if (Mathf.Abs(throttle) > 0.05f)
            {
                fuel = Mathf.Max(0, fuel - fuelConsumptionPerSec * Time.fixedDeltaTime);
                OnFuelChanged?.Invoke(fuel, maxFuel);
            }

            switch (vehicleType)
            {
                case KyokaVehicleType.Car:
                case KyokaVehicleType.Motorcycle:
                    HandleGroundVehicle(throttle, steering);
                    break;
                case KyokaVehicleType.Boat:
                    HandleWaterVehicle(throttle, steering);
                    break;
                case KyokaVehicleType.Aircraft:
                    HandleAirVehicle(throttle, steering);
                    break;
            }
        }

        private void HandleGroundVehicle(float throttle, float steering)
        {
            // Speed limiting
            float currentFwdSpeed = Vector3.Dot(body.velocity, transform.forward);

            if (throttle > 0 && currentFwdSpeed < maxSpeed)
            {
                body.AddForce(transform.forward * throttle * acceleration, ForceMode.Acceleration);
            }
            else if (throttle < 0 && currentFwdSpeed > -reverseSpeed)
            {
                body.AddForce(transform.forward * throttle * brakeForce, ForceMode.Acceleration);
            }

            // Steering depends on movement direction
            if (Mathf.Abs(currentFwdSpeed) > 0.5f)
            {
                float steerSign = Mathf.Sign(currentFwdSpeed);
                transform.Rotate(Vector3.up, steering * turnSpeed * steerSign * Time.fixedDeltaTime);
            }
        }

        private void HandleWaterVehicle(float throttle, float steering)
        {
            if (throttle > 0)
            {
                body.AddForce(transform.forward * throttle * acceleration * 1.2f, ForceMode.Acceleration);
            }
            transform.Rotate(Vector3.up, steering * turnSpeed * 0.8f * Time.fixedDeltaTime);

            // Water surface stabilizer
            if (transform.position.y < 0.2f)
            {
                body.AddForce(Vector3.up * buoyancy, ForceMode.Acceleration);
            }
        }

        private void HandleAirVehicle(float throttle, float steering)
        {
            // Forward thrust
            body.AddForce(transform.forward * throttle * acceleration * 1.5f, ForceMode.Acceleration);

            // Altitude control (Space on PC or Jump button)
            var input = KyokaInputBridge.Instance;
            bool ascend = input != null && input.JumpPressed;
            bool descend = input != null && input.CrouchPressed;

            if (ascend) body.AddForce(Vector3.up * flightLift, ForceMode.Acceleration);
            if (descend) body.AddForce(Vector3.down * flightLift * 0.7f, ForceMode.Acceleration);

            // Pitch & Yaw
            transform.Rotate(Vector3.up, steering * turnSpeed * Time.fixedDeltaTime, Space.Self);
            transform.Rotate(Vector3.right, -throttle * pitchSpeed * 0.3f * Time.fixedDeltaTime, Space.Self);
        }

        public bool Enter(Transform player)
        {
            if (!player) return false;

            if (DriverSeat == null)
            {
                DriverSeat = player;
                DisablePlayerController(player);
                player.SetParent(transform);
                player.localPosition = new Vector3(0, 0.9f, 0);
                return true;
            }

            if (currentPassengers.Count < passengerCapacity - 1)
            {
                currentPassengers.Add(player);
                DisablePlayerController(player);
                player.SetParent(transform);
                float xOffset = (currentPassengers.Count % 2 == 1) ? 0.6f : -0.6f;
                player.localPosition = new Vector3(xOffset, 0.8f, -0.6f);
                return true;
            }

            return false;
        }

        public void Exit()
        {
            if (DriverSeat != null)
            {
                Transform p = DriverSeat;
                DriverSeat = null;
                EjectPlayer(p, transform.position + transform.right * 2.2f + Vector3.up);
            }

            for (int i = currentPassengers.Count - 1; i >= 0; i--)
            {
                Transform pass = currentPassengers[i];
                currentPassengers.RemoveAt(i);
                EjectPlayer(pass, transform.position - transform.right * 2.2f + Vector3.up);
            }
        }

        private void EjectPlayer(Transform player, Vector3 targetPos)
        {
            if (!player) return;
            player.SetParent(null);
            player.position = targetPos;
            var cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = true;
            var controller = player.GetComponent<KyokaPlayerController>();
            if (controller) controller.InputEnabled = true;
        }

        private void DisablePlayerController(Transform player)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            var controller = player.GetComponent<KyokaPlayerController>();
            if (controller) controller.InputEnabled = false;
        }

        public void ApplyDamage(int dmg)
        {
            currentHealth = Mathf.Max(0, currentHealth - dmg);
            OnVehicleHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                ExplodeVehicle();
            }
        }

        public void Repair(int amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnVehicleHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void Refuel(float amount)
        {
            fuel = Mathf.Min(maxFuel, fuel + amount);
            OnFuelChanged?.Invoke(fuel, maxFuel);
        }

        public void SetCustomColor(Color color)
        {
            if (vehicleRenderer != null && vehicleRenderer.material != null)
            {
                vehicleRenderer.material.color = color;
            }
        }

        private void ExplodeVehicle()
        {
            Exit();
            var explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            explosion.name = "Vehicle_Explosion";
            explosion.transform.position = transform.position;
            explosion.transform.localScale = Vector3.one * 5f;
            var r = explosion.GetComponent<Renderer>();
            if (r) r.material.color = new Color(1f, 0.4f, 0.1f);
            var col = explosion.GetComponent<Collider>();
            if (col) Destroy(col);
            Destroy(explosion, 0.5f);

            Destroy(gameObject, 0.1f);
        }

        private void OnCollisionEnter(Collision collision)
        {
            float impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > 8f)
            {
                int dmg = Mathf.RoundToInt(impactForce * 4f);
                ApplyDamage(dmg);
            }
        }
    }
}
