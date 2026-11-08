using UnityEngine;

namespace Kyoka.World
{
    public class KyokaWorldBootstrap : MonoBehaviour
    {
        private void Start()
        {
            RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.62f);
            if (!FindObjectOfType<Light>())
            {
                var lightObject = new GameObject("KYOKA_Sun");
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.transform.rotation = Quaternion.Euler(50, -30, 0);
            }

            CreateGround();
            CreateLandmark(new Vector3(0, 1.5f, 14), new Vector3(5, 3, 2), new Color(0.3f, 0.5f, 0.8f), "Training Tower");
            CreateLandmark(new Vector3(-12, 1, 8), new Vector3(3, 2, 3), new Color(0.25f, 0.65f, 0.4f), "Green District");
            CreateLandmark(new Vector3(12, 1, 12), new Vector3(4, 2, 4), new Color(0.7f, 0.55f, 0.25f), "Market Hub");
            for (int i = 0; i < 12; i++)
            {
                float x = (i % 4) * 8 - 12;
                float z = (i / 4) * 9 + 25;
                CreateLandmark(new Vector3(x, 2, z), new Vector3(4, 4, 4), new Color(0.35f, 0.4f, 0.48f), "CityBlock_" + i);
            }
        }

        private void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "KYOKA_TestIsland_Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(12, 1, 12);
            ground.GetComponent<Renderer>().material.color = new Color(0.25f, 0.42f, 0.28f);
        }

        private void CreateLandmark(Vector3 position, Vector3 scale, Color color, string objectName)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = objectName;
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material.color = color;
        }
    }
}
