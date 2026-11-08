using UnityEngine;

namespace Kyoka.Vehicles
{
    public class KyokaVehicleSpawner : MonoBehaviour
    {
        private void Start()
        {
            SpawnVehicle(KyokaVehicleType.Car, new Vector3(8, 0.6f, 6), new Color(0.15f, 0.35f, 0.85f), "City Sports Car");
            SpawnVehicle(KyokaVehicleType.Motorcycle, new Vector3(14, 0.5f, 6), new Color(0.85f, 0.15f, 0.15f), "Apex Bike");
            SpawnVehicle(KyokaVehicleType.Boat, new Vector3(20, 0.5f, 6), new Color(0.1f, 0.75f, 0.8f), "Tide Runner");
            SpawnVehicle(KyokaVehicleType.Aircraft, new Vector3(28, 1.2f, 6), new Color(0.9f, 0.7f, 0.1f), "Aether Glider");
        }

        public GameObject SpawnVehicle(KyokaVehicleType type, Vector3 position, Color color, string vehicleName)
        {
            var go = GameObject.CreatePrimitive(type == KyokaVehicleType.Motorcycle ? PrimitiveType.Capsule : PrimitiveType.Cube);
            go.name = "Vehicle_" + type;
            go.tag = "Vehicle";
            go.transform.position = position;

            switch (type)
            {
                case KyokaVehicleType.Car:
                    go.transform.localScale = new Vector3(2.2f, 0.9f, 4.4f);
                    break;
                case KyokaVehicleType.Motorcycle:
                    go.transform.localScale = new Vector3(0.8f, 0.9f, 2.2f);
                    break;
                case KyokaVehicleType.Boat:
                    go.transform.localScale = new Vector3(2.5f, 1.0f, 5.0f);
                    break;
                case KyokaVehicleType.Aircraft:
                    go.transform.localScale = new Vector3(4.5f, 0.8f, 4.8f);
                    break;
            }

            var rend = go.GetComponent<Renderer>();
            if (rend) rend.material.color = color;

            var controller = go.AddComponent<KyokaVehicleController>();
            controller.vehicleType = type;
            controller.vehicleName = vehicleName;

            // Add simple visual headlights
            var lightObj = new GameObject("Headlights");
            lightObj.transform.SetParent(go.transform);
            lightObj.transform.localPosition = new Vector3(0, 0.2f, 0.55f);
            var spot = lightObj.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.range = 25f;
            spot.spotAngle = 60f;
            spot.intensity = 1.5f;
            spot.color = new Color(1f, 0.95f, 0.85f);

            return go;
        }
    }
}
