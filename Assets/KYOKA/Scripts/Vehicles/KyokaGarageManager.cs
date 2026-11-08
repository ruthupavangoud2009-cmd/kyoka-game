using UnityEngine;
using System.Collections.Generic;
using Kyoka.Economy;

namespace Kyoka.Vehicles
{
    public class KyokaGarageManager : MonoBehaviour
    {
        public static KyokaGarageManager Instance { get; private set; }

        [Header("Garage Facilities")]
        public Transform vehicleSpawnPoint;
        public int repairCost = 50;
        public int refuelCost = 25;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool RepairActiveVehicle(KyokaVehicleController vehicle)
        {
            if (vehicle == null) return false;
            var eco = KyokaEconomyManager.Instance;
            if (eco != null && !eco.SpendCredits(repairCost)) return false;

            vehicle.Repair(vehicle.maxHealth);
            Debug.Log("[KYOKA Garage] Vehicle repaired to full health.");
            return true;
        }

        public bool RefuelActiveVehicle(KyokaVehicleController vehicle)
        {
            if (vehicle == null) return false;
            var eco = KyokaEconomyManager.Instance;
            if (eco != null && !eco.SpendCredits(refuelCost)) return false;

            vehicle.Refuel(vehicle.maxFuel);
            Debug.Log("[KYOKA Garage] Vehicle refueled.");
            return true;
        }

        public void RepaintVehicle(KyokaVehicleController vehicle, Color newColor)
        {
            if (vehicle != null)
            {
                vehicle.SetCustomColor(newColor);
            }
        }
    }
}

