using UnityEngine;
using Kyoka.Vehicles;
using Kyoka.Core;

namespace Kyoka.Player
{
    /// <summary>
    /// Player interaction controller for boarding and exiting nearby vehicles.
    /// Operates via desktop [E] key or the mobile Touchscreen Interact button.
    /// </summary>
    public class KyokaVehicleInteractor : MonoBehaviour
    {
        public float interactionRange = 5.0f;
        private KyokaVehicleController activeVehicle;

        public bool IsInVehicle => activeVehicle != null;
        public KyokaVehicleController ActiveVehicle => activeVehicle;

        private void Update()
        {
            var input = KyokaInputBridge.Instance;
            if (input != null && input.InteractPressed)
            {
                Interact();
            }
        }

        public bool Interact()
        {
            if (activeVehicle != null)
            {
                activeVehicle.Exit();
                activeVehicle = null;
                return true;
            }

            KyokaVehicleController nearest = null;
            float nearestDistance = interactionRange;

            KyokaVehicleController[] vehicles = FindObjectsOfType<KyokaVehicleController>();
            foreach (var vehicle in vehicles)
            {
                if (!vehicle) continue;
                float distance = Vector3.Distance(transform.position, vehicle.transform.position);
                if (distance <= nearestDistance)
                {
                    nearest = vehicle;
                    nearestDistance = distance;
                }
            }

            if (nearest == null) return false;

            if (nearest.Enter(transform))
            {
                activeVehicle = nearest;
                return true;
            }

            return false;
        }
    }
}
