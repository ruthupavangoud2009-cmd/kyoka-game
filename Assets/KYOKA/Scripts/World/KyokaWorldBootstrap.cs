using UnityEngine;
using Kyoka.Survival;
using Kyoka.BattleRoyale;

namespace Kyoka.World
{
    public class KyokaWorldBootstrap : MonoBehaviour
    {
        private void Start()
        {
            RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.62f);

            var sun = FindObjectOfType<Light>();
            if (sun == null)
            {
                var lightObject = new GameObject("KYOKA_Sun");
                sun = lightObject.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.intensity = 1.2f;
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

            // Generate all 6 regions
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

            // City ground / asphalt base
            var ground = CreateGround(root.transform, new Vector3(0, 0, 0), new Vector3(15, 1, 15), new Color(0.2f, 0.22f, 0.25f));
            ground.name = "City_AsphaltPavement";

            // Central Plaza & Fountain
            var plaza = CreateLandmark(root.transform, new Vector3(0, 0.2f, 15), new Vector3(8, 0.2f, 8), new Color(0.7f, 0.72f, 0.75f), "Central_Plaza");
            var fountain = CreateLandmark(root.transform, new Vector3(0, 1.2f, 15), new Vector3(2.5f, 2f, 2.5f), new Color(0.2f, 0.6f, 0.9f), "Aether_Fountain");

            // Downtown Skyscrapers
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    float x = (c - 1.5f) * 16f;
                    float z = 35f + r * 18f;
                    float height = 12f + ((r + c) % 3) * 6f;
                    Color bldgColor = ((r + c) % 2 == 0) ? new Color(0.25f, 0.35f, 0.45f) : new Color(0.18f, 0.22f, 0.32f);

                    var bldg = CreateLandmark(root.transform, new Vector3(x, height * 0.5f, z), new Vector3(9f, height, 9f), bldgColor, $"Skyscraper_{r}_{c}");

                    // Neon rooftop sign
                    var neon = CreateLandmark(bldg.transform, new Vector3(0, height * 0.55f, 0), new Vector3(6f, 1.2f, 0.5f), new Color(0.1f, 0.85f, 0.95f), "Neon_Billboard");
                }
            }

            // Commercial district & Shops
            var shop = CreateLandmark(root.transform, new Vector3(-14, 2f, 12), new Vector3(6, 4, 6), new Color(0.85f, 0.55f, 0.2f), "Kyoka_General_Store");
            var garage = CreateLandmark(root.transform, new Vector3(14, 2f, 12), new Vector3(8, 4, 10), new Color(0.35f, 0.35f, 0.4f), "Kyoka_Custom_Garage");

            // Ambient NPC citizen
            var npcGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npcGo.name = "NPC_Kaito_Citizen";
            npcGo.transform.SetParent(root.transform);
            npcGo.transform.position = new Vector3(-2, 1, 14);
            npcGo.GetComponent<Renderer>().material.color = new Color(0.9f, 0.7f, 0.4f);
            var npc = npcGo.AddComponent<KyokaNPC>();
            npc.npcName = "Kaito";
            npc.greetingDialogue = "Hey traveler! The city is bustling today. Check out the custom garage to tune your ride.";
        }

        #endregion

        #region Region 2: Wildlands

        private void GenerateWildlands(KyokaWorldManager wm)
        {
            var root = new GameObject("Region_Wildlands");
            wm.regionRoots[KyokaRegionId.Wildlands] = root;
            root.transform.position = new Vector3(250, 0, 0);

            // Terrain base
            CreateGround(root.transform, Vector3.zero, new Vector3(20, 1, 20), new Color(0.18f, 0.38f, 0.16f));

            // Mountains and hills
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 75f, 15f, Mathf.Sin(angle) * 75f);
                CreateLandmark(root.transform, pos, new Vector3(25f, 30f, 25f), new Color(0.35f, 0.35f, 0.32f), $"Mountain_{i}");
            }

            // Pine forest & Resource Nodes
            for (int i = 0; i < 15; i++)
            {
                Vector3 treePos = new Vector3(Random.Range(-50f, 50f), 0, Random.Range(-50f, 50f));
                var trunk = CreateLandmark(root.transform, treePos + Vector3.up * 2f, new Vector3(0.6f, 4f, 0.6f), new Color(0.4f, 0.25f, 0.1f), $"TreeTrunk_{i}");
                var foliage = CreateLandmark(trunk.transform, Vector3.up * 2.5f, new Vector3(3f, 5f, 3f), new Color(0.12f, 0.45f, 0.15f), "Foliage");

                // Attach wood resource node
                var node = trunk.AddComponent<KyokaResourceNode>();
                node.resourceItemId = "wood";
                node.yieldAmount = 3;
            }

            // Rock quarries
            for (int i = 0; i < 6; i++)
            {
                Vector3 rockPos = new Vector3(Random.Range(-40f, 40f), 1f, Random.Range(-40f, 40f));
                var rock = CreateLandmark(root.transform, rockPos, new Vector3(2.5f, 2f, 2.5f), new Color(0.5f, 0.52f, 0.55f), $"RockNode_{i}");
                var node = rock.AddComponent<KyokaResourceNode>();
                node.resourceItemId = "stone";
                node.yieldAmount = 2;
            }
        }

        #endregion

        #region Region 3: Ocean Realm

        private void GenerateOceanRealm(KyokaWorldManager wm)
        {
            var root = new GameObject("Region_OceanRealm");
            wm.regionRoots[KyokaRegionId.OceanRealm] = root;
            root.transform.position = new Vector3(0, 0, 250);

            // Vast Ocean Water Plane
            var oceanWater = CreateGround(root.transform, new Vector3(0, -0.2f, 0), new Vector3(25, 1, 25), new Color(0.08f, 0.38f, 0.65f, 0.85f));
            oceanWater.name = "Ocean_WaterPlane";

            // Central Coral Island
            CreateGround(root.transform, new Vector3(0, 0.1f, 0), new Vector3(6, 1, 6), new Color(0.85f, 0.82f, 0.65f));

            // Lighthouse
            var tower = CreateLandmark(root.transform, new Vector3(15, 10f, 15), new Vector3(3.5f, 20f, 3.5f), new Color(0.9f, 0.2f, 0.2f), "Lighthouse_Tower");
            var lantern = CreateLandmark(tower.transform, Vector3.up * 10.5f, new Vector3(2.5f, 2.5f, 2.5f), Color.yellow, "Lantern_Light");

            // Harbor and boat moorings
            var pier = CreateLandmark(root.transform, new Vector3(0, 0.4f, -25), new Vector3(4f, 0.4f, 20f), new Color(0.45f, 0.3f, 0.15f), "Ocean_Pier");
        }

        #endregion

        #region Region 4: Anime Realm

        private void GenerateAnimeRealm(KyokaWorldManager wm)
        {
            var root = new GameObject("Region_AnimeRealm");
            wm.regionRoots[KyokaRegionId.AnimeRealm] = root;
            root.transform.position = new Vector3(-250, 0, 0);

            // Floating crystalline ground
            var island = CreateGround(root.transform, new Vector3(0, 15f, 0), new Vector3(12, 1, 12), new Color(0.4f, 0.2f, 0.6f));
            island.name = "Aether_SkyIsland";

            // Elemental Shrines (Fire, Water, Wind, Lightning)
            CreateLandmark(root.transform, new Vector3(20, 18f, 0), new Vector3(4, 8, 4), new Color(1f, 0.3f, 0.1f), "Shrine_Fire");
            CreateLandmark(root.transform, new Vector3(-20, 18f, 0), new Vector3(4, 8, 4), new Color(0.1f, 0.6f, 1f), "Shrine_Water");
            CreateLandmark(root.transform, new Vector3(0, 18f, 20), new Vector3(4, 8, 4), new Color(0.2f, 0.9f, 0.8f), "Shrine_Wind");
            CreateLandmark(root.transform, new Vector3(0, 18f, -20), new Vector3(4, 8, 4), new Color(0.95f, 0.9f, 0.2f), "Shrine_Lightning");

            // Celestial Boss Arena in Center
            var bossArena = CreateLandmark(root.transform, new Vector3(0, 16f, 0), new Vector3(18f, 0.4f, 18f), new Color(0.25f, 0.15f, 0.45f), "Celestial_BossArena");
        }

        #endregion

        #region Region 5: Battle Islands

        private void GenerateBattleIslands(KyokaWorldManager wm)
        {
            var root = new GameObject("Region_BattleIslands");
            wm.regionRoots[KyokaRegionId.BattleIslands] = root;
            root.transform.position = new Vector3(0, 0, -250);

            // Island terrain
            CreateGround(root.transform, Vector3.zero, new Vector3(22, 1, 22), new Color(0.3f, 0.4f, 0.25f));

            // Military supply outpost
            CreateLandmark(root.transform, new Vector3(0, 3f, 0), new Vector3(14, 6, 10), new Color(0.35f, 0.38f, 0.33f), "Military_Bunker");
            CreateLandmark(root.transform, new Vector3(-30, 8f, -30), new Vector3(4, 16, 4), new Color(0.25f, 0.25f, 0.28f), "Watchtower_Alpha");
            CreateLandmark(root.transform, new Vector3(30, 8f, 30), new Vector3(4, 16, 4), new Color(0.25f, 0.25f, 0.28f), "Watchtower_Bravo");

            // Attach loot spawner
            root.AddComponent<KyokaLootSpawner>();
        }

        #endregion

        #region Region 6: Creative World

        private void GenerateCreativeWorld(KyokaWorldManager wm)
        {
            var root = new GameObject("Region_CreativeWorld");
            wm.regionRoots[KyokaRegionId.CreativeWorld] = root;
            root.transform.position = new Vector3(0, 500, 0);

            // Clean white/gray construction grid
            var grid = CreateGround(root.transform, Vector3.zero, new Vector3(30, 1, 30), new Color(0.85f, 0.88f, 0.9f));
            grid.name = "Creative_SandboxGrid";

            // Grid border pillars
            CreateLandmark(root.transform, new Vector3(-140, 10, -140), new Vector3(4, 20, 4), Color.cyan, "Corner_Pillar_SW");
            CreateLandmark(root.transform, new Vector3(140, 10, -140), new Vector3(4, 20, 4), Color.cyan, "Corner_Pillar_SE");
            CreateLandmark(root.transform, new Vector3(-140, 10, 140), new Vector3(4, 20, 4), Color.cyan, "Corner_Pillar_NW");
            CreateLandmark(root.transform, new Vector3(140, 10, 140), new Vector3(4, 20, 4), Color.cyan, "Corner_Pillar_NE");
        }

        #endregion

        private GameObject CreateGround(Transform parent, Vector3 localPos, Vector3 scale, Color color)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.SetParent(parent);
            ground.transform.localPosition = localPos;
            ground.transform.localScale = scale;
            ground.GetComponent<Renderer>().material.color = color;
            return ground;
        }

        private GameObject CreateLandmark(Transform parent, Vector3 localPos, Vector3 scale, Color color, string objectName)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = objectName;
            go.transform.SetParent(parent);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material.color = color;
            return go;
        }
    }
}
