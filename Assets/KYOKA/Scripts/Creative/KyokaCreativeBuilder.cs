using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Kyoka.Core;

namespace Kyoka.Creative
{
    [Serializable]
    public class PlacedBlockData
    {
        public float x, y, z;
        public int blockType;
        public float r, g, b;
    }

    [Serializable]
    public class CreativeCreationBlueprint
    {
        public string creationName = "Kyoka Blueprint";
        public List<PlacedBlockData> blocks = new List<PlacedBlockData>();
    }

    public class KyokaCreativeBuilder : MonoBehaviour
    {
        [Header("Placement Configuration")]
        public float reachDistance = 6.0f;
        public LayerMask placementMask = ~0;
        public bool canBuild = true;

        [Header("Selected Block Type")]
        public int currentBlockIndex = 0;
        public Color[] blockColors = new Color[]
        {
            new Color(0.75f, 0.70f, 0.55f), // Sandstone
            new Color(0.45f, 0.30f, 0.15f), // Hardwood
            new Color(0.20f, 0.75f, 0.90f), // Aether Crystal
            new Color(0.95f, 0.85f, 0.20f), // Neon Gold
            new Color(0.15f, 0.15f, 0.20f)  // Obsidian Void
        };

        private readonly List<GameObject> placedBlocks = new List<GameObject>();

        private void Update()
        {
            if (!canBuild) return;

            var input = KyokaInputBridge.Instance;
            if (input != null && input.BuildPressed)
            {
                PlaceBlockInFront();
            }

            // Number keys 1-5 change block color
            if (Input.GetKeyDown(KeyCode.Alpha1)) currentBlockIndex = 0;
            if (Input.GetKeyDown(KeyCode.Alpha2)) currentBlockIndex = 1;
            if (Input.GetKeyDown(KeyCode.Alpha3)) currentBlockIndex = 2;
            if (Input.GetKeyDown(KeyCode.Alpha4)) currentBlockIndex = 3;
            if (Input.GetKeyDown(KeyCode.Alpha5)) currentBlockIndex = 4;
        }

        public GameObject PlaceBlockInFront()
        {
            Vector3 origin = transform.position + Vector3.up * 1.2f;
            Vector3 forward = transform.forward;
            Vector3 targetPos = origin + forward * reachDistance;

            if (Physics.Raycast(origin, forward, out RaycastHit hit, reachDistance, placementMask))
            {
                targetPos = hit.point + hit.normal * 0.5f;
            }

            // Snap to grid
            targetPos.x = Mathf.Round(targetPos.x);
            targetPos.y = Mathf.Round(targetPos.y);
            targetPos.z = Mathf.Round(targetPos.z);

            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = $"PlacedBlock_{currentBlockIndex}_{targetPos}";
            block.transform.position = targetPos;
            block.transform.localScale = Vector3.one;

            var rend = block.GetComponent<Renderer>();
            Color col = blockColors[Mathf.Clamp(currentBlockIndex, 0, blockColors.Length - 1)];
            if (rend) rend.material.color = col;

            placedBlocks.Add(block);
            return block;
        }

        public void SaveCreation(string blueprintName)
        {
            var bp = new CreativeCreationBlueprint { creationName = blueprintName };
            foreach (var b in placedBlocks)
            {
                if (!b) continue;
                var col = b.GetComponent<Renderer>()?.material.color ?? Color.white;
                bp.blocks.Add(new PlacedBlockData
                {
                    x = b.transform.position.x,
                    y = b.transform.position.y,
                    z = b.transform.position.z,
                    blockType = currentBlockIndex,
                    r = col.r,
                    g = col.g,
                    b = col.b
                });
            }

            string dir = Path.Combine(Application.persistentDataPath, "Creations");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string json = JsonUtility.ToJson(bp, true);
            File.WriteAllText(Path.Combine(dir, $"{blueprintName}.json"), json);
            Debug.Log($"[KYOKA Creative] Saved blueprint '{blueprintName}' with {bp.blocks.Count} blocks.");
        }

        public void ClearAllPlacedBlocks()
        {
            foreach (var b in placedBlocks)
            {
                if (b) Destroy(b);
            }
            placedBlocks.Clear();
        }
    }
}
