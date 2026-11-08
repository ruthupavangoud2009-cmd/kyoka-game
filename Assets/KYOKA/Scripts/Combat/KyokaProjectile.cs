using UnityEngine;
using Kyoka.Player;

namespace Kyoka.Combat
{
    public class KyokaProjectile : MonoBehaviour
    {
        public int damage = 20;
        public float speed = 40f;
        public float lifetime = 5f;
        public float explosionRadius = 0f; // 0 for single target
        public Transform sourceOwner;
        public Color trailColor = Color.cyan;

        private Vector3 velocity;

        public void Initialize(Transform owner, Vector3 direction, int dmg, float spd, float radius = 0f)
        {
            sourceOwner = owner;
            damage = dmg;
            speed = spd;
            explosionRadius = radius;
            velocity = direction.normalized * speed;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            float stepDistance = speed * Time.deltaTime;
            Vector3 nextPos = transform.position + velocity * Time.deltaTime;

            if (Physics.Raycast(transform.position, velocity.normalized, out RaycastHit hit, stepDistance))
            {
                if (sourceOwner != null && hit.transform == sourceOwner)
                {
                    transform.position = nextPos;
                    return;
                }
                OnHit(hit);
                return;
            }

            transform.position = nextPos;
        }

        private void OnHit(RaycastHit hit)
        {
            if (explosionRadius > 0.1f)
            {
                // Splash damage
                Collider[] hits = Physics.OverlapSphere(hit.point, explosionRadius);
                foreach (var c in hits)
                {
                    if (sourceOwner != null && c.transform == sourceOwner) continue;
                    var targetHealth = c.GetComponent<KyokaHealth>();
                    if (targetHealth) targetHealth.ApplyGameDamage(damage);
                }
            }
            else
            {
                var targetHealth = hit.collider.GetComponent<KyokaHealth>();
                if (targetHealth) targetHealth.ApplyGameDamage(damage);
            }

            // Impact visual effect
            SpawnImpactVFX(hit.point, hit.normal);
            Destroy(gameObject);
        }

        private void SpawnImpactVFX(Vector3 point, Vector3 normal)
        {
            var vfx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            vfx.name = "Projectile_HitVFX";
            vfx.transform.position = point;
            vfx.transform.localScale = Vector3.one * 0.4f;
            var r = vfx.GetComponent<Renderer>();
            if (r) r.material.color = trailColor;
            var c = vfx.GetComponent<Collider>();
            if (c) Destroy(c);
            Destroy(vfx, 0.25f);
        }
    }
}
