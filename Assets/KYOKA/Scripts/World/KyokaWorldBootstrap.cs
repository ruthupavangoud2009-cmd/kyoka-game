using UnityEngine;
using Kyoka.Survival;
using Kyoka.BattleRoyale;
using Kyoka.Character;
using Kyoka.Art;

namespace Kyoka.World
{
    /// <summary>
    /// Master Open World generator for KYOKA.
    /// Builds the Six Major Regions with high-fidelity architecture, stylized nature,
    /// interactive resource nodes, 3D humanoid NPCs, and fast-travel teleport arches.
    /// </summary>
    public class KyokaWorldBootstrap : MonoBehaviour
    {
        private void Start()
        {
            RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.65f);

            var sun = FindObjectOfType<Light>();
            if (sun == null)
            {
                var lightObject = new GameObject("KYOKA_Sun");
                sun = lightObject.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.intensity = 1.25f;
                sun.color = new Color(1.0f, 0.97f, 0.92f);
                sun.shadows = LightShadows.Soft;
                sun.transform.rotation = Quaternion.Euler(50, -30, 0);
            }

            // Ensure KyokaWorldManager exists
            var worldMgr = KyokaWorldManager.Instance;
            if (worldMgr == null)
            {
                var wmGO = new GameObject("KYOKA_WorldManager");
                worldMgr = wmGO.AddComponent<KyokaWorldManager>();
            }
            worldMgr.directionalSun = sun;

            // Generate all 6 distinct regions
            GenerateKyokaCity(worldMgr);
            GenerateWildlands(worldMgr);
            GenerateOceanRealm(worldMgr);
            GenerateAnimeRealm(worldMgr);
            GenerateBattleIslands(worldMgr);
            GenerateCreativeWorld(worldMgr);

            // Default active region: KYOKA City
            worldMgr.LoadRegion(KyokaRegionId.KYOKACity);
        }

        #region Region 1: KYOKA City

        private void GenerateKyokaCity(KyokaWorldManager wm)
        {
            var root = new GameObject("Region_KYOKACity");
            wm.regionRoots[KyokaRegionId.KYOKACity] = root;

            // City ground / dark asphalt base
            var ground = CreateGround(root.transform, Vector3.zero, new Vector3(20, 1, 20), new Color(0.18f, 0.20f, 0.23f));
            ground.name = "City_AsphaltPavement";

            // Central Plaza & Glowing Aether Fountain
            CreateLandmark(root.transform, new Vector3(0, 0.15f, 15), new Vector3(12, 0.3f, 12), new Color(0.65f, 0.68f, 0.72f), "Central_Plaza");
            var fountain = CreateLandmark(root.transform, new Vector3(0, 1.2f, 15), new Vector3(3f, 2.2f, 3f), new Color(0.1f, 0.85f, 0.95f), "Aether_Fountain");
            fountain.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateEmissionMaterial(new Color(0.1f, 0.85f, 0.95f), 1.5f);

            // Downtown Skyscrapers with Neon Signage
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    float x = (c - 1.5f) * 22f;
                    float z = 38f + r * 22f;
                    float height = 18f + ((r + c) % 3) * 8f;
                    Color bldgColor = ((r + c) % 2 == 0) ? new Color(0.20f, 0.26f, 0.34f) : new Color(0.16f, 0.20f, 0.28f);

                    CreateSkyscraper(root.transform, new Vector3(x, 0, z), 12f, height, 12f, bldgColor, $"Skyscraper_{r}_{c}");
                }
            }

            // Commercial Garage & General Store
            CreateBuildingWithAwning(root.transform, new Vector3(-18, 0, 14), new Vector3(8, 5, 8), new Color(0.85f, 0.55f, 0.2f), "Kyoka_General_Store");
            CreateBuildingWithAwning(root.transform, new Vector3(18, 0, 14), new Vector3(10, 5, 12), new Color(0.32f, 0.34f, 0.38f), "Kyoka_Custom_Garage");

            // 3D Humanoid Citizen NPC
            CreateHumanoidNPC(root.transform, new Vector3(-3, 0, 13), "Kaito (Citizen)",
                "Welcome to KYOKA City! Explore the neon downtown or head east into the Wildlands for crafting timber and stone.",
                HairStyleType.SpikyAnime, OutfitPreset.UrbanStalker);

            // Fast-Travel Teleport Portal to Wildlands
            CreatePortalArch(root.transform, new Vector3(0, 0, -20), KyokaRegionId.Wildlands, "Portal to Wildlands");
        }

        #endregion

        #region Region 2: Wildlands

        private void GenerateWildlands(KyokaWorldManager wm)
        {
            var root = new GameObject("Region_Wildlands");
            wm.regionRoots[KyokaRegionId.Wildlands] = root;
            root.transform.position = new Vector3(250, 0, 0);

            // Lush Meadow Base
            CreateGround(root.transform, Vector3.zero, new Vector3(25, 1, 25), new Color(0.22f, 0.44f, 0.18f));

            // Distant Mountain Ridges
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 90f, 18f, Mathf.Sin(angle) * 90f);
                CreateLandmark(root.transform, pos, new Vector3(30f, 36f, 30f), new Color(0.32f, 0.34f, 0.32f), $"Mountain_{i}");
            }

            // Stylized Branching Trees (Harvestable Wood Nodes)
            for (int i = 0; i < 20; i++)
            {
                float x = (i % 5 - 2f) * 18f + Random.Range(-4f, 4f);
                float z = (i / 5 - 2f) * 18f + Random.Range(-4f, 4f);
                CreateStylizedTree(root.transform, new Vector3(x, 0, z));
            }

            // Faceted Stone & Ore Quarries (Harvestable Stone & Iron Nodes)
            for (int i = 0; i < 8; i++)
            {
                float x = Random.Range(-45f, 45f);
                float z = Random.Range(-45f, 45f);
                var resType = (i % 3 == 0) ? ResourceType.IronOre : (i % 3 == 1) ? ResourceType.Stone : ResourceType.AetherCrystal;
                CreateStylizedRock(root.transform, new Vector3(x, 0, z), resType);
            }

            // 3D Ranger NPC
            CreateHumanoidNPC(root.transform, new Vector3(4, 0, 0), "Ranger Ren",
                "The Wildlands are rich with timber and iron. Press 'B' to enter Build Mode and construct your personal outpost!",
                HairStyleType.TacticalBuzzcut, OutfitPreset.DesertCommando);

            // Portal back to City
            CreatePortalArch(root.transform, new Vector3(-20, 0, 0), KyokaRegionId.KYOKACity, "Portal to KYOKA City");
        }

        #endregion

        #region Region 3: Ocean Realm

        private void GenerateOceanRealm(KyokaWorldManager wm)
        {
            var root = new GameObject("Region_OceanRealm");
            wm.regionRoots[KyokaRegionId.OceanRealm] = root;
            root.transform.position = new Vector3(0, 0, 250);

            // Animated Ocean Water Surface
            var oceanWater = CreateGround(root.transform, new Vector3(0, -0.2f, 0), new Vector3(30, 1, 30), new Color(0.06f, 0.32f, 0.58f));
            oceanWater.name = "Ocean_WaterSurface";

            // Sandy Shore Island
            CreateGround(root.transform, new Vector3(0, 0.1f, 0), new Vector3(8, 1, 8), new Color(0.86f, 0.82f, 0.65f));

            // Lighthouse with Rotating Beacon
            var tower = CreateLandmark(root.transform, new Vector3(18, 12f, 18), new Vector3(4f, 24f, 4f), new Color(0.92f, 0.22f, 0.22f), "Lighthouse_Tower");
            var lantern = CreateLandmark(tower.transform, Vector3.up * 12.5f, new Vector3(3f, 3f, 3f), Color.yellow, "Lantern_Light");
            lantern.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateEmissionMaterial(new Color(1f, 0.95f, 0.4f), 2.0f);

            // Wooden Pier
            CreateLandmark(root.transform, new Vector3(0, 0.4f, -25), new Vector3(5f, 0.4f, 24f), new Color(0.45f, 0.30f, 0.16f), "Ocean_Pier");

            // Portal back to City
            CreatePortalArch(root.transform, new Vector3(0, 0, -5), KyokaRegionId.KYOKACity, "Portal to KYOKA City");
        }

        #endregion

        #region Region 4: Anime Realm

        private void GenerateAnimeRealm(KyokaWorldManager wm)
        {
            var root = new GameObject("Region_AnimeRealm");
            wm.regionRoots[KyokaRegionId.AnimeRealm] = root;
            root.transform.position = new Vector3(-250, 0, 0);

            // Floating Celestial Sky Island
            var island = CreateGround(root.transform, new Vector3(0, 15f, 0), new Vector3(15, 1, 15), new Color(0.38f, 0.18f, 0.58f));
            island.name = "Aether_CelestialIsland";

            // 4 Elemental Shrines
            CreateLandmark(root.transform, new Vector3(25, 18f, 0), new Vector3(4, 9, 4), new Color(1f, 0.35f, 0.1f), "Shrine_Fire");
            CreateLandmark(root.transform, new Vector3(-25, 18f, 0), new Vector3(4, 9, 4), new Color(0.1f, 0.65f, 1f), "Shrine_Water");
            CreateLandmark(root.transform, new Vector3(0, 18f, 25), new Vector3(4, 9, 4), new Color(0.2f, 0.95f, 0.85f), "Shrine_Wind");
            CreateLandmark(root.transform, new Vector3(0, 18f, -25), new Vector3(4, 9, 4), new Color(0.98f, 0.92f, 0.2f), "Shrine_Lightning");

            // Central Boss Arena
            CreateLandmark(root.transform, new Vector3(0, 16f, 0), new Vector3(22f, 0.5f, 22f), new Color(0.24f, 0.14f, 0.42f), "Celestial_BossArena");

            // Portal back to City
            CreatePortalArch(root.transform, new Vector3(0, 16f, -32), KyokaRegionId.KYOKACity, "Portal to KYOKA City");
        }

        #endregion

        #region Region 5: Battle Islands

        private void GenerateBattleIslands(KyokaWorldManager wm)
        {
            var root = new GameObject("Region_BattleIslands");
            wm.regionRoots[KyokaRegionId.BattleIslands] = root;
            root.transform.position = new Vector3(0, 0, -250);

            // Island terrain
            CreateGround(root.transform, Vector3.zero, new Vector3(25, 1, 25), new Color(0.28f, 0.38f, 0.22f));

            // Military supply outpost
            CreateLandmark(root.transform, new Vector3(0, 3.5f, 0), new Vector3(16, 7, 12), new Color(0.32f, 0.36f, 0.30f), "Military_Bunker");
            CreateLandmark(root.transform, new Vector3(-35, 9f, -35), new Vector3(4, 18, 4), new Color(0.24f, 0.24f, 0.26f), "Watchtower_Alpha");
            CreateLandmark(root.transform, new Vector3(35, 9f, 35), new Vector3(4, 18, 4), new Color(0.24f, 0.24f, 0.26f), "Watchtower_Bravo");

            // Attach loot spawner
            root.AddComponent<KyokaLootSpawner>();

            // Portal back to City
            CreatePortalArch(root.transform, new Vector3(0, 0, -40), KyokaRegionId.KYOKACity, "Portal to KYOKA City");
        }

        #endregion

        #region Region 6: Creative World

        private void GenerateCreativeWorld(KyokaWorldManager wm)
        {
            var root = new GameObject("Region_CreativeWorld");
            wm.regionRoots[KyokaRegionId.CreativeWorld] = root;
            root.transform.position = new Vector3(0, 500, 0);

            // Clean white/gray construction grid
            var grid = CreateGround(root.transform, Vector3.zero, new Vector3(35, 1, 35), new Color(0.88f, 0.90f, 0.92f));
            grid.name = "Creative_SandboxGrid";

            // Border pillars
            CreateLandmark(root.transform, new Vector3(-160, 12, -160), new Vector3(5, 24, 5), Color.cyan, "Corner_Pillar_SW");
            CreateLandmark(root.transform, new Vector3(160, 12, -160), new Vector3(5, 24, 5), Color.cyan, "Corner_Pillar_SE");
            CreateLandmark(root.transform, new Vector3(-160, 12, 160), new Vector3(5, 24, 5), Color.cyan, "Corner_Pillar_NW");
            CreateLandmark(root.transform, new Vector3(160, 12, 160), new Vector3(5, 24, 5), Color.cyan, "Corner_Pillar_NE");

            // Portal back to City
            CreatePortalArch(root.transform, new Vector3(0, 0, -30), KyokaRegionId.KYOKACity, "Portal to KYOKA City");
        }

        #endregion

        #region Stylized Procedural Asset Helpers

        private void CreateSkyscraper(Transform parent, Vector3 pos, float width, float height, float depth, Color color, string name)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;

            // Main tower body
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Tower_Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0, height * 0.5f, 0);
            body.transform.localScale = new Vector3(width, height, depth);
            body.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLitColorMaterial(color);

            // Glowing neon rooftop cornice strip
            var neon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            neon.name = "Neon_RoofStrip";
            neon.transform.SetParent(root.transform, false);
            neon.transform.localPosition = new Vector3(0, height + 0.4f, 0);
            neon.transform.localScale = new Vector3(width + 0.4f, 0.8f, depth + 0.4f);
            neon.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateEmissionMaterial(new Color(0.15f, 0.85f, 0.95f), 1.8f);
            Destroy(neon.GetComponent<Collider>());
        }

        private void CreateBuildingWithAwning(Transform parent, Vector3 pos, Vector3 size, Color color, string name)
        {
            var bldg = CreateLandmark(parent, pos + new Vector3(0, size.y * 0.5f, 0), size, color, name);
            var awning = GameObject.CreatePrimitive(PrimitiveType.Cube);
            awning.name = "Awning";
            awning.transform.SetParent(bldg.transform, false);
            awning.transform.localPosition = new Vector3(0, 0, 0.6f);
            awning.transform.localScale = new Vector3(0.9f, 0.08f, 0.35f);
            awning.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLitColorMaterial(new Color(0.9f, 0.2f, 0.2f));
            Destroy(awning.GetComponent<Collider>());
        }

        private void CreateStylizedTree(Transform parent, Vector3 pos)
        {
            var tree = new GameObject("Stylized_Tree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = pos;

            // Trunk (Cylinder)
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0, 2.0f, 0);
            trunk.transform.localScale = new Vector3(0.7f, 2.0f, 0.7f);
            trunk.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLitColorMaterial(new Color(0.38f, 0.24f, 0.12f));

            // Canopy Cluster 1
            var canopy1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy1.transform.SetParent(tree.transform, false);
            canopy1.transform.localPosition = new Vector3(0, 4.5f, 0);
            canopy1.transform.localScale = new Vector3(3.6f, 3.2f, 3.6f);
            canopy1.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLitColorMaterial(new Color(0.14f, 0.48f, 0.18f));

            // Canopy Cluster 2 (Upper)
            var canopy2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy2.transform.SetParent(tree.transform, false);
            canopy2.transform.localPosition = new Vector3(0.3f, 6.2f, -0.2f);
            canopy2.transform.localScale = new Vector3(2.6f, 2.4f, 2.6f);
            canopy2.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLitColorMaterial(new Color(0.18f, 0.58f, 0.22f));

            // Attach interactive wood resource node
            var node = tree.AddComponent<KyokaResourceNode>();
            node.resourceType = ResourceType.Tree;
        }

        private void CreateStylizedRock(Transform parent, Vector3 pos, ResourceType type)
        {
            var rock = new GameObject($"Rock_{type}");
            rock.transform.SetParent(parent, false);
            rock.transform.localPosition = pos;

            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesh.transform.SetParent(rock.transform, false);
            mesh.transform.localPosition = new Vector3(0, 1.0f, 0);
            mesh.transform.localRotation = Quaternion.Euler(15, 35, 20);
            mesh.transform.localScale = new Vector3(2.2f, 1.8f, 2.0f);
            mesh.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLitColorMaterial(new Color(0.45f, 0.47f, 0.50f));

            // Embedded Glowing Crystal Vein
            var crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crystal.transform.SetParent(rock.transform, false);
            crystal.transform.localPosition = new Vector3(0.5f, 1.2f, 0.6f);
            crystal.transform.localRotation = Quaternion.Euler(45, 15, 30);
            crystal.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            Color crystalCol = (type == ResourceType.AetherCrystal) ? new Color(0.1f, 0.85f, 0.95f) :
                               (type == ResourceType.IronOre) ? new Color(0.95f, 0.65f, 0.25f) : new Color(0.7f, 0.72f, 0.75f);
            crystal.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateEmissionMaterial(crystalCol, 1.5f);
            Destroy(crystal.GetComponent<Collider>());

            var node = rock.AddComponent<KyokaResourceNode>();
            node.resourceType = type;
        }

        private void CreateHumanoidNPC(Transform parent, Vector3 pos, string npcName, string dialogue, HairStyleType hair, OutfitPreset outfit)
        {
            var npcGO = new GameObject($"NPC_{npcName}");
            npcGO.transform.SetParent(parent, false);
            npcGO.transform.localPosition = pos;

            var model = npcGO.AddComponent<KyokaCharacterModel>();
            model.currentHairStyle = hair;
            model.currentOutfit = outfit;
            model.BuildHumanoidMesh();

            npcGO.AddComponent<KyokaProceduralAnimator>();

            var npc = npcGO.AddComponent<KyokaNPC>();
            npc.npcName = npcName;
            npc.greetingDialogue = dialogue;
        }

        private void CreatePortalArch(Transform parent, Vector3 pos, KyokaRegionId targetRegion, string portalLabel)
        {
            var portalRoot = new GameObject($"Portal_{targetRegion}");
            portalRoot.transform.SetParent(parent, false);
            portalRoot.transform.localPosition = pos;

            // Left Post
            var p1 = CreateLandmark(portalRoot.transform, new Vector3(-2f, 2.5f, 0), new Vector3(0.8f, 5f, 0.8f), new Color(0.2f, 0.25f, 0.35f), "Pillar_Left");
            // Right Post
            var p2 = CreateLandmark(portalRoot.transform, new Vector3(2f, 2.5f, 0), new Vector3(0.8f, 5f, 0.8f), new Color(0.2f, 0.25f, 0.35f), "Pillar_Right");
            // Header
            var header = CreateLandmark(portalRoot.transform, new Vector3(0, 5.2f, 0), new Vector3(5f, 0.8f, 0.8f), new Color(0.2f, 0.25f, 0.35f), "Pillar_Header");

            // Glowing Portal Gateway Energy Field
            var field = GameObject.CreatePrimitive(PrimitiveType.Cube);
            field.name = "Portal_Field";
            field.transform.SetParent(portalRoot.transform, false);
            field.transform.localPosition = new Vector3(0, 2.5f, 0);
            field.transform.localScale = new Vector3(3.2f, 4.6f, 0.2f);
            field.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateEmissionMaterial(new Color(0.1f, 0.85f, 0.95f), 1.8f);
            Destroy(field.GetComponent<Collider>());
        }

        private GameObject CreateGround(Transform parent, Vector3 localPos, Vector3 scale, Color color)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.SetParent(parent, false);
            ground.transform.localPosition = localPos;
            ground.transform.localScale = scale;
            ground.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLitColorMaterial(color);
            return ground;
        }

        private GameObject CreateLandmark(Transform parent, Vector3 localPos, Vector3 scale, Color color, string objectName)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = objectName;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLitColorMaterial(color);
            return go;
        }

        #endregion
    }
}
