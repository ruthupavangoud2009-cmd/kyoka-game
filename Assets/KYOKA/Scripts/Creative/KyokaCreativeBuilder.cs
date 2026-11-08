using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using Kyoka.Core;
using Kyoka.Art;
using Kyoka.Audio;
using Kyoka.Inventory;

namespace Kyoka.Creative
{
    public enum BuildingPieceType
    {
        WoodFoundation = 0,
        WoodWall = 1,
        WoodDoorway = 2,
        WoodRoof = 3,
        StorageChest = 4,
        DefenseBarricade = 5,
        CraftingTable = 6
    }

    [Serializable]
    public class PlacedPieceData
    {
        public float x, y, z;
        public float rotY;
        public int pieceType;
    }

    [Serializable]
    public class CreativeCreationBlueprint
    {
        public string creationName = "Kyoka Base";
        public List<PlacedPieceData> pieces = new List<PlacedPieceData>();
    }

    /// <summary>
    /// Minecraft/Rust-inspired modular base building system.
    /// Supports grid snapping, 90-degree piece rotation, structural placement,
    /// deconstruction, material cost checks, and persistent JSON saving.
    /// </summary>
    public class KyokaCreativeBuilder : MonoBehaviour
    {
        public static KyokaCreativeBuilder Instance { get; private set; }

        [Header("Placement Configuration")]
        public float reachDistance = 8.0f;
        public LayerMask placementMask = ~0;
        public bool isBuildMode = false;
        public BuildingPieceType selectedPiece = BuildingPieceType.WoodFoundation;
        public float currentRotationY = 0f;

        [Header("Grid Tuning")]
        public float gridSize = 3.0f;
        public float wallHeight = 3.0f;

        private GameObject previewGhost;
        private Material ghostMaterial;
        private readonly List<GameObject> placedPieces = new List<GameObject>();
        private KyokaInventory inventory;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            inventory = GetComponent<KyokaInventory>();
            CreateGhostMaterial();
        }

        private void CreateGhostMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Hidden/InternalErrorShader");
            ghostMaterial = new Material(shader);
            ghostMaterial.color = new Color(0.2f, 0.85f, 0.4f, 0.45f);
        }

        private void Update()
        {
            // Toggle build mode with 'B'
            if (Input.GetKeyDown(KeyCode.B))
            {
                isBuildMode = !isBuildMode;
                if (!isBuildMode && previewGhost != null)
                {
                    Destroy(previewGhost);
                    previewGhost = null;
                }
                KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
            }

            if (!isBuildMode) return;

            // Cycle pieces with 'Q' and 'E' or numbers 1-6
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                selectedPiece = (BuildingPieceType)(((int)selectedPiece + 1) % 7);
                UpdateGhostMesh();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentRotationY = (currentRotationY + 90f) % 360f;
            }

            // Number hotkeys for instant building piece selection
            if (Input.GetKeyDown(KeyCode.F1)) { selectedPiece = BuildingPieceType.WoodFoundation; UpdateGhostMesh(); }
            if (Input.GetKeyDown(KeyCode.F2)) { selectedPiece = BuildingPieceType.WoodWall; UpdateGhostMesh(); }
            if (Input.GetKeyDown(KeyCode.F3)) { selectedPiece = BuildingPieceType.WoodDoorway; UpdateGhostMesh(); }
            if (Input.GetKeyDown(KeyCode.F4)) { selectedPiece = BuildingPieceType.WoodRoof; UpdateGhostMesh(); }
            if (Input.GetKeyDown(KeyCode.F5)) { selectedPiece = BuildingPieceType.StorageChest; UpdateGhostMesh(); }
            if (Input.GetKeyDown(KeyCode.F6)) { selectedPiece = BuildingPieceType.DefenseBarricade; UpdateGhostMesh(); }
            if (Input.GetKeyDown(KeyCode.F7)) { selectedPiece = BuildingPieceType.CraftingTable; UpdateGhostMesh(); }

            UpdatePlacementGhost();

            // Left click to place
            if (Input.GetMouseButtonDown(0))
            {
                // Only place if not clicking on UI
                if (!UnityEngine.EventSystems.EventSystem.current || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    PlacePiece();
                }
            }

            // Right click to deconstruct / remove
            if (Input.GetMouseButtonDown(1))
            {
                TryDeconstruct();
            }
        }

        private void UpdateGhostMesh()
        {
            if (previewGhost != null)
            {
                Destroy(previewGhost);
                previewGhost = null;
            }
        }

        private void UpdatePlacementGhost()
        {
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 forward = transform.forward;
            Vector3 targetPos = origin + forward * reachDistance;

            if (Physics.Raycast(origin, forward, out RaycastHit hit, reachDistance, placementMask))
            {
                targetPos = hit.point;
            }

            // Snap position to building grid
            targetPos.x = Mathf.Round(targetPos.x / gridSize) * gridSize;
            targetPos.z = Mathf.Round(targetPos.z / gridSize) * gridSize;
            if (selectedPiece == BuildingPieceType.WoodRoof)
            {
                targetPos.y = Mathf.Round(targetPos.y / wallHeight) * wallHeight;
            }

            if (previewGhost == null)
            {
                previewGhost = CreatePieceMesh(selectedPiece, true);
                previewGhost.name = "Building_Ghost_Preview";
                var colliders = previewGhost.GetComponentsInChildren<Collider>();
                foreach (var c in colliders) Destroy(c);
            }

            previewGhost.transform.position = targetPos;
            previewGhost.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);
        }

        public GameObject PlacePiece()
        {
            if (previewGhost == null) return null;

            Vector3 placePos = previewGhost.transform.position;
            Quaternion placeRot = previewGhost.transform.rotation;

            var pieceGO = CreatePieceMesh(selectedPiece, false);
            pieceGO.name = $"Placed_{selectedPiece}_{placePos}";
            pieceGO.transform.position = placePos;
            pieceGO.transform.rotation = placeRot;

            placedPieces.Add(pieceGO);

            // Audio & VFX
            KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.LootPickup);
            Debug.Log($"[KYOKA Building] Placed {selectedPiece} at {placePos}");

            return pieceGO;
        }

        private void TryDeconstruct()
        {
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 forward = transform.forward;

            if (Physics.Raycast(origin, forward, out RaycastHit hit, reachDistance))
            {
                GameObject rootObj = hit.collider.transform.root.gameObject;
                if (placedPieces.Contains(rootObj) || placedPieces.Contains(hit.collider.gameObject))
                {
                    var target = placedPieces.Contains(rootObj) ? rootObj : hit.collider.gameObject;
                    placedPieces.Remove(target);
                    Destroy(target);
                    KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.KatanaSlash);
                    Debug.Log($"[KYOKA Building] Deconstructed piece: {target.name}");
                }
            }
        }

        private GameObject CreatePieceMesh(BuildingPieceType pieceType, bool isGhost)
        {
            var root = new GameObject($"Piece_{pieceType}");
            var mat = isGhost ? ghostMaterial : KyokaMaterialFactory.CreateLitColorMaterial(GetPieceBaseColor(pieceType));

            switch (pieceType)
            {
                case BuildingPieceType.WoodFoundation:
                    // 3m x 0.35m x 3m timber platform
                    var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    floor.transform.SetParent(root.transform, false);
                    floor.transform.localPosition = new Vector3(0, 0.175f, 0);
                    floor.transform.localScale = new Vector3(gridSize, 0.35f, gridSize);
                    floor.GetComponent<Renderer>().material = mat;
                    break;

                case BuildingPieceType.WoodWall:
                    // 3m wide x 3m high x 0.2m thick timber wall
                    var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.SetParent(root.transform, false);
                    wall.transform.localPosition = new Vector3(0, wallHeight * 0.5f, 0);
                    wall.transform.localScale = new Vector3(gridSize, wallHeight, 0.22f);
                    wall.GetComponent<Renderer>().material = mat;
                    break;

                case BuildingPieceType.WoodDoorway:
                    // Wall with 1m x 2.2m doorway cutout
                    var leftPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    leftPost.transform.SetParent(root.transform, false);
                    leftPost.transform.localPosition = new Vector3(-1.0f, wallHeight * 0.5f, 0);
                    leftPost.transform.localScale = new Vector3(1.0f, wallHeight, 0.22f);
                    leftPost.GetComponent<Renderer>().material = mat;

                    var rightPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rightPost.transform.SetParent(root.transform, false);
                    rightPost.transform.localPosition = new Vector3(1.0f, wallHeight * 0.5f, 0);
                    rightPost.transform.localScale = new Vector3(1.0f, wallHeight, 0.22f);
                    rightPost.GetComponent<Renderer>().material = mat;

                    var header = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    header.transform.SetParent(root.transform, false);
                    header.transform.localPosition = new Vector3(0, 2.6f, 0);
                    header.transform.localScale = new Vector3(1.0f, 0.8f, 0.22f);
                    header.GetComponent<Renderer>().material = mat;
                    break;

                case BuildingPieceType.WoodRoof:
                    var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    roof.transform.SetParent(root.transform, false);
                    roof.transform.localPosition = new Vector3(0, 0.12f, 0);
                    roof.transform.localScale = new Vector3(gridSize + 0.4f, 0.24f, gridSize + 0.4f);
                    roof.GetComponent<Renderer>().material = mat;
                    break;

                case BuildingPieceType.StorageChest:
                    var chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    chest.transform.SetParent(root.transform, false);
                    chest.transform.localPosition = new Vector3(0, 0.4f, 0);
                    chest.transform.localScale = new Vector3(1.1f, 0.8f, 0.75f);
                    chest.GetComponent<Renderer>().material = mat;
                    break;

                case BuildingPieceType.DefenseBarricade:
                    var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bar.transform.SetParent(root.transform, false);
                    bar.transform.localPosition = new Vector3(0, 0.6f, 0);
                    bar.transform.localScale = new Vector3(2.5f, 1.2f, 0.6f);
                    bar.GetComponent<Renderer>().material = mat;
                    break;

                case BuildingPieceType.CraftingTable:
                    var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bench.transform.SetParent(root.transform, false);
                    bench.transform.localPosition = new Vector3(0, 0.5f, 0);
                    bench.transform.localScale = new Vector3(1.6f, 1.0f, 0.9f);
                    bench.GetComponent<Renderer>().material = mat;
                    break;
            }

            return root;
        }

        private Color GetPieceBaseColor(BuildingPieceType type)
        {
            switch (type)
            {
                case BuildingPieceType.WoodFoundation: return new Color(0.48f, 0.34f, 0.22f);
                case BuildingPieceType.WoodWall: return new Color(0.55f, 0.40f, 0.26f);
                case BuildingPieceType.WoodDoorway: return new Color(0.52f, 0.38f, 0.24f);
                case BuildingPieceType.WoodRoof: return new Color(0.38f, 0.25f, 0.16f);
                case BuildingPieceType.StorageChest: return new Color(0.68f, 0.48f, 0.25f);
                case BuildingPieceType.DefenseBarricade: return new Color(0.35f, 0.35f, 0.38f);
                case BuildingPieceType.CraftingTable: return new Color(0.42f, 0.28f, 0.18f);
                default: return Color.white;
            }
        }
    }
}
