using UnityEngine;
using Kyoka.Player;

namespace Kyoka.World
{
    public class KyokaPoliceAI : MonoBehaviour
    {
        public float pursuitSpeed = 16f;
        public float arrestDistance = 3.5f;

        private Transform playerTarget;
        private Light sirenLight;
        private float sirenTimer = 0f;

        private void Start()
        {
            var lightObj = new GameObject("Police_SirenLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.up * 1.2f;
            sirenLight = lightObj.AddComponent<Light>();
            sirenLight.type = LightType.Point;
            sirenLight.range = 15f;
            sirenLight.intensity = 2.5f;
            sirenLight.color = Color.blue;
        }

        private void Update()
        {
            // Flash red / blue sirens
            sirenTimer += Time.deltaTime * 6f;
            if (sirenLight != null)
            {
                sirenLight.color = (Mathf.FloorToInt(sirenTimer) % 2 == 0) ? Color.red : Color.blue;
            }

            if (playerTarget == null)
            {
                var player = FindObjectOfType<KyokaPlayerController>();
                if (player != null) playerTarget = player.transform;
            }

            if (playerTarget != null)
            {
                Vector3 direction = (playerTarget.position - transform.position);
                direction.y = 0;

                if (direction.magnitude > arrestDistance)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction.normalized), 6f * Time.deltaTime);
                    transform.position += transform.forward * pursuitSpeed * Time.deltaTime;
                }
                else
                {
                    // In arrest range
                    Debug.Log("[KYOKA Police] Suspect in apprehension range! Stop your vehicle.");
                }
            }
        }
    }
}

