using UnityEngine;
using Kyoka.Vehicles;

namespace Kyoka.Player
{
    /// <summary>Local prototype interaction for entering and exiting nearby vehicles.</summary>
    public class KyokaVehicleInteractor : MonoBehaviour
    {
        public float interactionRange = 4.5f;
        private KyokaVehicleController activeVehicle;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
                Interact();
        }

        public bool Interact()
        {
            if (activeVehicle != null && activeVehicle.IsOccupied)
            {
                activeVehicle.Exit();
                activeVehicle = null;
                return true;
            }

            KyokaVehicleController nearest = null;
            float nearestDistance = interactionRange;
            foreach (var vehicle in FindObjectsOfType<KyokaVehicleController>())
            {
                if (!vehicle || vehicle.IsOccupied) continue;
                float distance = Vector3.Distance(transform.position, vehicle.transform.position);
                if (distance <= nearestDistance)
                {
                    nearest = vehicle;
                    nearestDistance = distance;
                }
            }

            if (nearest == null || !nearest.Enter(transform)) return false;
            activeVehicle = nearest;
            return true;
        }
    }
}
