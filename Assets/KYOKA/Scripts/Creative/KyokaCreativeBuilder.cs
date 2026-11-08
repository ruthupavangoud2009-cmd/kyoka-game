using UnityEngine;

namespace Kyoka.Creative
{
    public class KyokaCreativeBuilder : MonoBehaviour
    {
        public float placementDistance = 4f;
        public LayerMask placementMask = ~0;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B)) PlaceBlock();
        }

        public GameObject PlaceBlock()
        {
            Vector3 position = transform.position + transform.forward * placementDistance;
            position.y = Mathf.Round(position.y);
            position.x = Mathf.Round(position.x);
            position.z = Mathf.Round(position.z);

            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = "PlayerPlacedBlock";
            block.transform.position = position;
            block.GetComponent<Renderer>().material.color = new Color(0.7f, 0.65f, 0.48f);
            return block;
        }
    }
}
