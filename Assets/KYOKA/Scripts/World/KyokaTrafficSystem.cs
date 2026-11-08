using UnityEngine;
using System.Collections.Generic;
using Kyoka.Vehicles;

namespace Kyoka.World
{
    /// <summary>
    /// Autonomous traffic simulation for KYOKA City. Spawns and manages civilian
    /// vehicular traffic navigating road network waypoints with obstacle collision avoidance.
    /// </summary>
    public class KyokaTrafficSystem : MonoBehaviour
    {
        public static KyokaTrafficSystem Instance { get; private set; }

        [Header("Traffic Population")]
        public int maxCivilianVehicles = 8;
        public float vehicleSpeed = 12f;

        [Header("Road Waypoints")]
        public List<Vector3> roadWaypoints = new List<Vector3>();

        private readonly List<GameObject> activeVehicles = new List<GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            GenerateCityRoadWaypoints();
            SpawnInitialTraffic();
        }

        private void GenerateCityRoadWaypoints()
        {
            // Circuit loop around central city blocks
            roadWaypoints.Clear();
            roadWaypoints.Add(new Vector3(-25, 0.4f, 10));
            roadWaypoints.Add(new Vector3(-25, 0.4f, 65));
            roadWaypoints.Add(new Vector3(25, 0.4f, 65));
            roadWaypoints.Add(new Vector3(25, 0.4f, 10));
        }

        private void SpawnInitialTraffic()
        {
            for (int i = 0; i < maxCivilianVehicles && i < roadWaypoints.Count; i++)
            {
                SpawnCivilianCar(roadWaypoints[i], (i + 1) % roadWaypoints.Count);
            }
        }

        private void SpawnCivilianCar(Vector3 position, int targetWaypointIdx)
        {
            var car = GameObject.CreatePrimitive(PrimitiveType.Cube);
            car.name = $"Civilian_TrafficCar_{activeVehicles.Count}";
            car.transform.position = position;
            car.transform.localScale = new Vector3(2f, 0.8f, 3.8f);

            Color[] colors = new Color[] { Color.gray, Color.white, new Color(0.2f, 0.4f, 0.6f), new Color(0.6f, 0.2f, 0.2f) };
            car.GetComponent<Renderer>().material.color = colors[Random.Range(0, colors.Length)];

            var agent = car.AddComponent<KyokaTrafficAgent>();
            agent.speed = vehicleSpeed;
            agent.targetWaypointIndex = targetWaypointIdx;
            agent.waypoints = roadWaypoints;

            activeVehicles.Add(car);
        }
    }

    public class KyokaTrafficAgent : MonoBehaviour
    {
        public float speed = 12f;
        public int targetWaypointIndex = 0;
        public List<Vector3> waypoints;
        public float stoppingDistance = 6f;

        private void Update()
        {
            if (waypoints == null || waypoints.Count == 0) return;

            Vector3 target = waypoints[targetWaypointIndex];
            Vector3 direction = (target - transform.position);
            direction.y = 0;

            // Check for obstacle in front via Raycast
            bool obstacleInFront = Physics.Raycast(transform.position + Vector3.up * 0.4f, transform.forward, stoppingDistance);

            if (!obstacleInFront)
            {
                if (direction.magnitude < 2.0f)
                {
                    targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Count;
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction.normalized), 5f * Time.deltaTime);
                    transform.position += transform.forward * speed * Time.deltaTime;
                }
            }
        }
    }
}

