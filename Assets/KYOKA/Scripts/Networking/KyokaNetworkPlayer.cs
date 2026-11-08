using UnityEngine;
using Kyoka.Player;

namespace Kyoka.Networking
{
    /// <summary>
    /// Represents a remote player in the network session.
    /// Interpolates transform and reflects remote health, combat state, and animations.
    /// </summary>
    public class KyokaNetworkPlayer : MonoBehaviour
    {
        public string networkPlayerId = "";
        public string playerDisplayName = "Remote Pilot";
        public bool isLocalPlayer = false;

        [Header("Interpolation Settings")]
        public float positionLerpSpeed = 12f;
        public float rotationLerpSpeed = 12f;

        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private KyokaHealth health;

        private void Awake()
        {
            health = GetComponent<KyokaHealth>();
            targetPosition = transform.position;
            targetRotation = transform.rotation;
        }

        private void Update()
        {
            if (isLocalPlayer) return;

            // Interpolate position and rotation smoothly
            transform.position = Vector3.Lerp(transform.position, targetPosition, positionLerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
        }

        public void UpdateNetworkTransform(Vector3 pos, Quaternion rot)
        {
            targetPosition = pos;
            targetRotation = rot;
        }

        public void UpdateHealth(int currentHp)
        {
            if (health != null && health.CurrentHealth != currentHp)
            {
                int diff = health.CurrentHealth - currentHp;
                if (diff > 0) health.ApplyGameDamage(diff);
                else if (diff < 0) health.Restore(-diff);
            }
        }
    }
}

