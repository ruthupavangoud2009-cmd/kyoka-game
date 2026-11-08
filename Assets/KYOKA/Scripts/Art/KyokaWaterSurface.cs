using UnityEngine;

namespace Kyoka.Art
{
    /// <summary>
    /// Dynamic vertex-animated water surface with wave displacement
    /// and scrolling specular ripple textures for KYOKA's Ocean Realm.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class KyokaWaterSurface : MonoBehaviour
    {
        [Header("Wave Parameters")]
        public float waveHeight = 0.45f;
        public float waveSpeed = 1.4f;
        public float waveFrequency = 0.08f;
        public Vector2 rippleScrollSpeed = new Vector2(0.04f, 0.03f);

        private Mesh mesh;
        private Vector3[] baseVertices;
        private Vector3[] displacedVertices;
        private Material waterMat;
        private Vector2 currentOffset = Vector2.zero;

        private void Start()
        {
            var mf = GetComponent<MeshFilter>();
            if (mf.sharedMesh == null)
            {
                mf.sharedMesh = KyokaMeshBuilder.CreateWaterWaveMesh(300f, 300f, 40, 40);
            }

            mesh = mf.mesh; // Unique instance
            baseVertices = mesh.vertices;
            displacedVertices = new Vector3[baseVertices.Length];

            var mr = GetComponent<MeshRenderer>();
            waterMat = mr.material;
        }

        private void Update()
        {
            if (mesh == null || baseVertices == null) return;

            float time = Time.time * waveSpeed;

            for (int i = 0; i < baseVertices.Length; i++)
            {
                Vector3 v = baseVertices[i];
                float wave1 = Mathf.Sin(time + v.x * waveFrequency) * waveHeight;
                float wave2 = Mathf.Cos(time * 0.85f + v.z * waveFrequency * 1.2f) * (waveHeight * 0.5f);
                float wave3 = Mathf.Sin(time * 1.3f + (v.x + v.z) * waveFrequency * 0.7f) * (waveHeight * 0.25f);

                v.y = wave1 + wave2 + wave3;
                displacedVertices[i] = v;
            }

            mesh.vertices = displacedVertices;
            mesh.RecalculateNormals();

            // Texture scroll
            if (waterMat != null)
            {
                currentOffset += rippleScrollSpeed * Time.deltaTime;
                waterMat.mainTextureOffset = currentOffset;
                if (waterMat.HasProperty("_BaseMap"))
                {
                    waterMat.SetTextureOffset("_BaseMap", currentOffset);
                }
            }
        }
    }
}
