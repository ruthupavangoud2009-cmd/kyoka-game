using UnityEngine;

namespace Kyoka.Vehicles
{
    public class KyokaVehicleSpawner : MonoBehaviour
    {
        private void Start()
        {
            Spawn(KyokaVehicleType.Car, new Vector3(5, 0.6f, 4), new Color(0.15f, 0.3f, 0.9f));
            Spawn(KyokaVehicleType.Motorcycle, new Vector3(9, 0.5f, 4), Color.red);
            Spawn(KyokaVehicleType.Boat, new Vector3(13, 0.5f, 4), Color.cyan);
            Spawn(KyokaVehicleType.Aircraft, new Vector3(18, 1f, 4), Color.yellow);
        }

        private void Spawn(KyokaVehicleType type, Vector3 position, Color color)
        {
            var go = GameObject.CreatePrimitive(type == KyokaVehicleType.Motorcycle
                ? PrimitiveType.Capsule : PrimitiveType.Cube);
            go.name = "Prototype_" + type;
            go.transform.position = position;
            go.transform.localScale = type == KyokaVehicleType.Aircraft
                ? new Vector3(2.5f, 0.7f, 4f)
                : new Vector3(2f, 0.8f, 3f);
            go.GetComponent<Renderer>().material.color = color;
            go.AddComponent<KyokaVehicleController>().vehicleType = type;
        }
    }
}
