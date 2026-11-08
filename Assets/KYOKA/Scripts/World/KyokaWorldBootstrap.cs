using UnityEngine;
using Kyoka.Survival;
using Kyoka.BattleRoyale;
using Kyoka.Character;
using Kyoka.Art;

namespace Kyoka.World
{
    /// <summary>
    /// Master Open World generator for KYOKA AAA Visual Overhaul.
    /// Synthesizes high-fidelity anime-realistic environments across all 6 regions:
    /// High-rise City with textured skyscrapers & neon signs, organic Wildlands with Sakura & Oak canopies,
    /// dynamic vertex-animated Ocean with lighthouse & pier, and military compounds.
    /// </summary>
    public class KyokaWorldBootstrap : MonoBehaviour
    {
        private void Start()
        {
            // Apply anime panoramic skybox & trilight environmental lighting
            RenderSettings.skybox = KyokaMaterialFactory.CreateSkyboxMaterial();
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.68f, 0.74f, 0.88f);
            RenderSettings.ambientEquatorColor = new Color(0.48f, 0.52f, 0.62f);
            RenderSettings.ambientGroundColor = new Color(0.24f, 0.26f, 0.28f);

            var sun = FindObjectOfType<Light>();
            if (sun == null)
            {
                var lightObject = new GameObject("KYOKA_Sun");
                sun = lightObject.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.intensity = 1.35f;
                sun.color = new Color(1.0f, 0.97f, 0.92f);
                sun.shadows = LightShadows.Soft;
                sun.transform.rotation = Quaternion.Euler(50, -35, 0);
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

            // Textured Asphalt Pavement
            var ground = CreateGround(root.transform, Vector3.zero, new Vector3(25, 1, 25), new Color(0.16f, 0.17f, 0.20f));
            ground.name = "City_AsphaltPavement";
            var asphaltMat = KyokaMaterialFactory.CreateLit(new Color(0.18f, 0.20f, 0.24f), KyokaTextureGenerator.GetCyberGridTexture());
            asphaltMat.mainTextureScale = new Vector2(30, 30);
            ground.GetComponent<Renderer>().material = asphaltMat;

            // Central Plaza with Tile Pattern
            var plaza = CreateLandmark(root.transform, new Vector3(0, 0.12f, 15), new Vector3(16, 0.24f, 16), new Color(0.70f, 0.72f, 0.76f), "Central_Plaza");
            var plazaMat = KyokaMaterialFactory.CreateLit(new Color(0.50f, 0.54f, 0.60f), KyokaTextureGenerator.GetHangarFloorTexture());
            plazaMat.mainTextureScale = new Vector2(6, 6);
            plaza.GetComponent<Renderer>().material = plazaMat;

            // Tiered Aether Fountain with glowing pool
            CreateTieredFountain(root.transform, new Vector3(0, 0.24f, 15));

            // Downtown Skyscrapers with Facade Textures and Glowing Neon Signs
            string[] neonBrands = new string[] { "KYOKA", "NEXUS", "AETHER", "CYBER", "HYPER", "VOLT" };
            Color[] neonColors = new Color[]
            {
                new Color(0.12f, 0.85f, 0.95f), // Cyan
                new Color(1.0f, 0.22f, 0.65f),  // Hot Pink
                new Color(0.98f, 0.85f, 0.15f), // Gold
                new Color(0.25f, 0.95f, 0.45f)  // Emerald
            };

            int brandIdx = 0;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    float x = (c - 1.5f) * 26f;
                    float z = 38f + r * 26f;
                    float height = 22f + ((r + c) % 3) * 10f;
                    Color wallCol = ((r + c) % 2 == 0) ? new Color(0.18f, 0.22f, 0.28f) : new Color(0.14f, 0.18f, 0.24f);

                    string brand = neonBrands[brandIdx % neonBrands.Length];
                    Color nColor = neonColors[brandIdx % neonColors.Length];
                    brandIdx++;

                    CreateSkyscraperWithFacade(root.transform, new Vector3(x, 0, z), 14f, height, 14f, wallCol, brand, nColor, $"Skyscraper_{r}_{c}");
                }
            }

            // Central KYOKA Corporate Headquarters Monolith Tower
            CreateSkyscraperWithFacade(root.transform, new Vector3(0, 0, 78f), 22f, 62f, 22f, new Color(0.12f, 0.15f, 0.22f), "KYOKA HQ", new Color(0.12f, 0.88f, 0.98f), "Kyoka_Corporate_HQ");

            // Modern Street Lamps lining the plaza
            CreateStreetLamp(root.transform, new Vector3(-9, 0, 8));
            CreateStreetLamp(root.transform, new Vector3(9, 0, 8));
            CreateStreetLamp(root.transform, new Vector3(-9, 0, 22));
            CreateStreetLamp(root.transform, new Vector3(9, 0, 22));

            // Commercial Outposts
            CreateBuildingWithAwning(root.transform, new Vector3(-20, 0, 14), new Vector3(9, 6, 10), new Color(0.28f, 0.32f, 0.38f), "Kyoka_General_Store");
            CreateBuildingWithAwning(root.transform, new Vector3(20, 0, 14), new Vector3(11, 6, 12), new Color(0.24f, 0.27f, 0.32f), "Kyoka_Custom_Garage");

            // 3D Humanoid Citizen NPC
            CreateHumanoidNPC(root.transform, new Vector3(-3.5f, 0, 12), "Kaito (Citizen)",
                "Welcome to KYOKA City! Explore the neon downtown or head through the portal to the Wildlands.",
                HairStyleType.SpikyAnime, OutfitPreset.UrbanStalker);

            // Fast-Travel Teleport Portal to Wildlands
            CreatePortalArch(root.transform, new Vector3(0, 0, -18), KyokaRegionId.Wildlands, "Portal to Wildlands");
        }

        private void CreateTieredFountain(Transform parent, Vector3 pos)
        {
            var fRoot = new GameObject("Aether_Fountain");
            fRoot.transform.SetParent(parent, false);
            fRoot.transform.localPosition = pos;

            // Outer Basin (Octagonal ring)
            var basin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basin.name = "Fountain_Basin";
            basin.transform.SetParent(fRoot.transform, false);
            basin.transform.localPosition = new Vector3(0, 0.3f, 0);
            basin.transform.localScale = new Vector3(6f, 0.3f, 6f);
            basin.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLit(new Color(0.45f, 0.48f, 0.52f));

            // Glowing Water Surface
            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = "Fountain_Water";
            water.transform.SetParent(fRoot.transform, false);
            water.transform.localPosition = new Vector3(0, 0.45f, 0);
            water.transform.localScale = new Vector3(5.5f, 0.05f, 5.5f);
            water.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateWaterMaterial();
            Destroy(water.GetComponent<Collider>());

            // Center Pedestal & Spire
            var spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spire.name = "Center_Spire";
            spire.transform.SetParent(fRoot.transform, false);
            spire.transform.localPosition = new Vector3(0, 1.6f, 0);
            spire.transform.localScale = new Vector3(1.2f, 1.4f, 1.2f);
            spire.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLit(new Color(0.35f, 0.38f, 0.42f));

            // Floating Glowing Aether Core Crystal
            var crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crystal.name = "Aether_CrystalCore";
            crystal.transform.SetParent(fRoot.transform, false);
            crystal.transform.localPosition = new Vector3(0, 3.4f, 0);
            crystal.transform.localRotation = Quaternion.Euler(45, 45, 45);
            crystal.transform.localScale = new Vector3(0.9f, 1.4f, 0.9f);
            crystal.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateEmissionMaterial(new Color(0.12f, 0.88f, 0.98f), 2.4f);
            Destroy(crystal.GetComponent<Collider>());
        }

        private void CreateStreetLamp(Transform parent, Vector3 pos)
        {
            var lamp = new GameObject("StreetLamp");
            lamp.transform.SetParent(parent, false);
            lamp.transform.localPosition = pos;

            // Pole
            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.SetParent(lamp.transform, false);
            pole.transform.localPosition = new Vector3(0, 2.2f, 0);
            pole.transform.localScale = new Vector3(0.12f, 2.2f, 0.12f);
            pole.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLit(new Color(0.2f, 0.22f, 0.25f));
            Destroy(pole.GetComponent<Collider>());

            // Horizontal Arm
            var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "Arm";
            arm.transform.SetParent(lamp.transform, false);
            arm.transform.localPosition = new Vector3(0.4f, 4.3f, 0);
            arm.transform.localScale = new Vector3(0.9f, 0.08f, 0.08f);
            arm.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLit(new Color(0.2f, 0.22f, 0.25f));
            Destroy(arm.GetComponent<Collider>());

            // Emissive Lantern Head
            var lantern = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lantern.name = "Lantern";
            lantern.transform.SetParent(lamp.transform, false);
            lantern.transform.localPosition = new Vector3(0.8f, 4.15f, 0);
            lantern.transform.localScale = new Vector3(0.25f, 0.22f, 0.25f);
            lantern.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateEmissionMaterial(new Color(1.0f, 0.92f, 0.65f), 2.2f);
            Destroy(lantern.GetComponent<Collider>());
        }

        #endregion

        #region Region 2: Wildlands

        private void GenerateWildlands(KyokaWorldManager wm)
        {
            var root = new GameObject("Region_Wildlands");
            wm.regionRoots[KyokaRegionId.Wildlands] = root;
            root.transform.position = new Vector3(250, 0, 0);

            // Lush Meadow Base
            var ground = CreateGround(root.transform, Vector3.zero, new Vector3(28, 1, 28), new Color(0.22f, 0.42f, 0.18f));
            ground.name = "Wildlands_Meadow";
            var grassMat = KyokaMaterialFactory.CreateLit(new Color(0.24f, 0.44f, 0.20f), KyokaTextureGenerator.GetFoliageTexture(new Color(0.24f, 0.44f, 0.20f)));
            grassMat.mainTextureScale = new Vector2(35, 35);
            ground.GetComponent<Renderer>().material = grassMat;

            // Distant Mountain Ridges
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 95f, 22f, Mathf.Sin(angle) * 95f);
                var mtn = CreateLandmark(root.transform, pos, new Vector3(32f, 44f, 32f), new Color(0.28f, 0.32f, 0.30f), $"Mountain_{i}");
                mtn.transform.localRotation = Quaternion.Euler(0, i * 30f, 0);
            }

            // Stylized Branching Trees (Mix of Lush Oak and Sakura Cherry Blossoms)
            for (int i = 0; i < 24; i++)
            {
                float x = (i % 6 - 2.5f) * 16f + Random.Range(-3f, 3f);
                float z = (i / 6 - 2.0f) * 16f + Random.Range(-3f, 3f);
                bool isSakura = (i % 3 == 0); // 33% Sakura cherry blossoms
                CreateOrganicTree(root.transform, new Vector3(x, 0, z), isSakura);
            }

            // Faceted Stone & Ore Boulders with glowing veins
            for (int i = 0; i < 10; i++)
            {
                float x = Random.Range(-48f, 48f);
                float z = Random.Range(-48f, 48f);
                var resType = (i % 3 == 0) ? ResourceType.AetherCrystal : (i % 3 == 1) ? ResourceType.IronOre : ResourceType.Stone;
                CreateOrganicRock(root.transform, new Vector3(x, 0, z), resType, i);
            }

            // 3D Humanoid Ranger NPC
            CreateHumanoidNPC(root.transform, new Vector3(3.5f, 0, 0), "Ranger Ren",
                "The Wildlands are rich with timber and iron. Press 'B' to enter Build Mode and construct your outpost!",
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

            // Dynamic Vertex-Animated Water Surface
            var oceanGO = new GameObject("Ocean_DynamicSurface");
            oceanGO.transform.SetParent(root.transform, false);
            oceanGO.transform.localPosition = new Vector3(0, -0.1f, 0);

            var mf = oceanGO.AddComponent<MeshFilter>();
            mf.sharedMesh = KyokaMeshBuilder.CreateWaterWaveMesh(280f, 280f, 36, 36);

            var mr = oceanGO.AddComponent<MeshRenderer>();
            mr.material = KyokaMaterialFactory.CreateWaterMaterial();

            oceanGO.AddComponent<KyokaWaterSurface>();

            // Sandy Shore Island
            var island = CreateGround(root.transform, new Vector3(0, 0.15f, 0), new Vector3(9, 1, 9), new Color(0.86f, 0.82f, 0.65f));
            island.name = "Tropical_SandyIsland";
            var sandMat = KyokaMaterialFactory.CreateLit(new Color(0.86f, 0.82f, 0.65f), KyokaTextureGenerator.GetRockTexture(new Color(0.86f, 0.82f, 0.65f)));
            sandMat.mainTextureScale = new Vector2(12, 12);
            island.GetComponent<Renderer>().material = sandMat;

            // Coastal Palm Trees
            CreatePalmTree(root.transform, new Vector3(-12, 0, 8));
            CreatePalmTree(root.transform, new Vector3(-15, 0, -6));
            CreatePalmTree(root.transform, new Vector3(10, 0, -10));

            // Stone Lighthouse with Rotating Searchlight Beacon
            CreateLighthouse(root.transform, new Vector3(20, 0, 20));

            // Weathered Wooden Pier
            CreateWoodenPier(root.transform, new Vector3(0, 0.4f, -22));

            // Portal back to City (Positioned on the western shoreline away from main ocean vista)
            CreatePortalArch(root.transform, new Vector3(-16, 0, -10), KyokaRegionId.KYOKACity, "Portal to KYOKA City");
        }

        private void CreatePalmTree(Transform parent, Vector3 pos)
        {
            var palm = new GameObject("PalmTree");
            palm.transform.SetParent(parent, false);
            palm.transform.localPosition = pos;

            var barkMat = KyokaMaterialFactory.CreateLit(new Color(0.55f, 0.40f, 0.25f), KyokaTextureGenerator.GetBarkTexture(new Color(0.55f, 0.40f, 0.25f)));
            var leafMat = KyokaMaterialFactory.CreateFoliageMaterial(new Color(0.18f, 0.58f, 0.20f), false);

            // Curved segmented trunk
            for (int i = 0; i < 5; i++)
            {
                var seg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                seg.transform.SetParent(palm.transform, false);
                seg.transform.localPosition = new Vector3(i * 0.18f, i * 1.1f + 0.55f, 0);
                seg.transform.localRotation = Quaternion.Euler(0, 0, -8f * i);
                seg.transform.localScale = new Vector3(0.55f - i * 0.05f, 0.6f, 0.55f - i * 0.05f);
                seg.GetComponent<Renderer>().material = barkMat;
                Destroy(seg.GetComponent<Collider>());
            }

            // Palm frond fan
            for (int f = 0; f < 6; f++)
            {
                float angle = f * 60f;
                var frond = GameObject.CreatePrimitive(PrimitiveType.Cube);
                frond.transform.SetParent(palm.transform, false);
                frond.transform.localPosition = new Vector3(0.9f, 5.8f, 0);
                frond.transform.localRotation = Quaternion.Euler(22f, angle, 0);
                frond.transform.localScale = new Vector3(0.45f, 0.06f, 2.6f);
                frond.GetComponent<Renderer>().material = leafMat;
                Destroy(frond.GetComponent<Collider>());
            }
        }

        private void CreateLighthouse(Transform parent, Vector3 pos)
        {
            var lh = new GameObject("Lighthouse");
            lh.transform.SetParent(parent, false);
            lh.transform.localPosition = pos;

            var redMat = KyokaMaterialFactory.CreateLit(new Color(0.85f, 0.20f, 0.18f));
            var whiteMat = KyokaMaterialFactory.CreateLit(new Color(0.92f, 0.94f, 0.96f));
            var darkMetalMat = KyokaMaterialFactory.CreateLit(new Color(0.18f, 0.20f, 0.24f));
            var beaconGlowMat = KyokaMaterialFactory.CreateEmissionMaterial(new Color(1.0f, 0.96f, 0.65f), 3.0f);

            // Tier 1 Base (Red)
            var b1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            b1.name = "Tower_Tier1";
            b1.transform.SetParent(lh.transform, false);
            b1.transform.localPosition = new Vector3(0, 4.0f, 0);
            b1.transform.localScale = new Vector3(6.0f, 4.0f, 6.0f);
            b1.GetComponent<Renderer>().material = redMat;

            // Tier 2 Middle (White)
            var b2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            b2.name = "Tower_Tier2";
            b2.transform.SetParent(lh.transform, false);
            b2.transform.localPosition = new Vector3(0, 11.8f, 0);
            b2.transform.localScale = new Vector3(5.2f, 3.8f, 5.2f);
            b2.GetComponent<Renderer>().material = whiteMat;

            // Tier 3 Upper (Red)
            var b3 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            b3.name = "Tower_Tier3";
            b3.transform.SetParent(lh.transform, false);
            b3.transform.localPosition = new Vector3(0, 19.2f, 0);
            b3.transform.localScale = new Vector3(4.4f, 3.6f, 4.4f);
            b3.GetComponent<Renderer>().material = redMat;

            // Gallery Walkway Deck (Dark Metal)
            var deck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            deck.name = "Gallery_Deck";
            deck.transform.SetParent(lh.transform, false);
            deck.transform.localPosition = new Vector3(0, 23.0f, 0);
            deck.transform.localScale = new Vector3(5.4f, 0.25f, 5.4f);
            deck.GetComponent<Renderer>().material = darkMetalMat;
            Destroy(deck.GetComponent<Collider>());

            // Lantern Room & Searchlight Beacon (Glowing Amber/White)
            var lantern = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lantern.name = "Lantern_Beacon";
            lantern.transform.SetParent(lh.transform, false);
            lantern.transform.localPosition = new Vector3(0, 24.5f, 0);
            lantern.transform.localScale = new Vector3(3.2f, 1.3f, 3.2f);
            lantern.GetComponent<Renderer>().material = beaconGlowMat;
            Destroy(lantern.GetComponent<Collider>());

            // Domed Conical Roof (Dark Metal)
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            roof.name = "Lighthouse_Dome";
            roof.transform.SetParent(lh.transform, false);
            roof.transform.localPosition = new Vector3(0, 26.0f, 0);
            roof.transform.localScale = new Vector3(3.6f, 0.6f, 3.6f);
            roof.GetComponent<Renderer>().material = darkMetalMat;
            Destroy(roof.GetComponent<Collider>());
        }

        private void CreateWoodenPier(Transform parent, Vector3 pos)
        {
            var pier = new GameObject("WoodenPier");
            pier.transform.SetParent(parent, false);
            pier.transform.localPosition = pos;

            var woodMat = KyokaMaterialFactory.CreateLit(new Color(0.42f, 0.28f, 0.16f));

            // Planks deck
            var deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.transform.SetParent(pier.transform, false);
            deck.transform.localPosition = new Vector3(0, 0, 0);
            deck.transform.localScale = new Vector3(4.5f, 0.35f, 26f);
            deck.GetComponent<Renderer>().material = woodMat;

            // Support piles
            for (int z = -10; z <= 10; z += 5)
            {
                for (int x = -1; x <= 1; x += 2)
                {
                    var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    post.transform.SetParent(pier.transform, false);
                    post.transform.localPosition = new Vector3(x * 1.8f, -1.2f, z);
                    post.transform.localScale = new Vector3(0.24f, 1.4f, 0.24f);
                    post.GetComponent<Renderer>().material = woodMat;
                    Destroy(post.GetComponent<Collider>());
                }
            }
        }

        #endregion

        #region Region 4: Anime Realm

        private void GenerateAnimeRealm(KyokaWorldManager wm)
        {
            var root = new GameObject("Region_AnimeRealm");
            wm.regionRoots[KyokaRegionId.AnimeRealm] = root;
            root.transform.position = new Vector3(-250, 0, 0);

            // Floating Celestial Sky Island
            var island = CreateGround(root.transform, new Vector3(0, 15f, 0), new Vector3(16, 1, 16), new Color(0.35f, 0.18f, 0.52f));
            island.name = "Aether_CelestialIsland";

            // 4 Elemental Shrines
            CreateLandmark(root.transform, new Vector3(25, 18f, 0), new Vector3(4, 10, 4), new Color(1f, 0.35f, 0.1f), "Shrine_Fire");
            CreateLandmark(root.transform, new Vector3(-25, 18f, 0), new Vector3(4, 10, 4), new Color(0.1f, 0.65f, 1f), "Shrine_Water");
            CreateLandmark(root.transform, new Vector3(0, 18f, 25), new Vector3(4, 10, 4), new Color(0.2f, 0.95f, 0.85f), "Shrine_Wind");
            CreateLandmark(root.transform, new Vector3(0, 18f, -25), new Vector3(4, 10, 4), new Color(0.98f, 0.92f, 0.2f), "Shrine_Lightning");

            // Central Boss Arena
            CreateLandmark(root.transform, new Vector3(0, 16f, 0), new Vector3(24f, 0.6f, 24f), new Color(0.22f, 0.12f, 0.38f), "Celestial_BossArena");

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
            CreateGround(root.transform, Vector3.zero, new Vector3(26, 1, 26), new Color(0.26f, 0.34f, 0.22f));

            // Military supply outpost
            CreateLandmark(root.transform, new Vector3(0, 3.5f, 0), new Vector3(18, 7, 14), new Color(0.30f, 0.34f, 0.28f), "Military_Bunker");
            CreateLandmark(root.transform, new Vector3(-35, 9f, -35), new Vector3(4, 18, 4), new Color(0.22f, 0.24f, 0.26f), "Watchtower_Alpha");
            CreateLandmark(root.transform, new Vector3(35, 9f, 35), new Vector3(4, 18, 4), new Color(0.22f, 0.24f, 0.26f), "Watchtower_Bravo");

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

        #region Stylized High-Fidelity Asset Generators

        private void CreateSkyscraperWithFacade(Transform parent, Vector3 pos, float width, float height, float depth, Color wallColor, string brand, Color neonColor, string name)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;

            // Main tower body with window facade texture
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Tower_Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0, height * 0.5f, 0);
            body.transform.localScale = new Vector3(width, height, depth);

            var facadeTex = KyokaTextureGenerator.GetSkyscraperTexture(wallColor, new Color(1.0f, 0.92f, 0.65f));
            var facadeMat = KyokaMaterialFactory.CreateLit(wallColor, facadeTex);
            facadeMat.mainTextureScale = new Vector2(4, height * 0.4f);
            body.GetComponent<Renderer>().material = facadeMat;

            // Glowing neon rooftop cornice strip
            var neon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            neon.name = "Neon_RoofStrip";
            neon.transform.SetParent(root.transform, false);
            neon.transform.localPosition = new Vector3(0, height + 0.35f, 0);
            neon.transform.localScale = new Vector3(width + 0.5f, 0.7f, depth + 0.5f);
            neon.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateEmissionMaterial(neonColor, 2.0f);
            Destroy(neon.GetComponent<Collider>());

            // Rooftop Brand Neon Signboard
            var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "Rooftop_Signboard";
            sign.transform.SetParent(root.transform, false);
            sign.transform.localPosition = new Vector3(0, height + 1.8f, 0);
            sign.transform.localScale = new Vector3(width * 0.7f, 2.0f, 0.4f);

            var signTex = KyokaTextureGenerator.GetNeonSignTexture(brand, neonColor);
            var signMat = KyokaMaterialFactory.CreateLit(Color.white, signTex);
            sign.GetComponent<Renderer>().material = signMat;
            Destroy(sign.GetComponent<Collider>());
        }

        private void CreateBuildingWithAwning(Transform parent, Vector3 pos, Vector3 size, Color color, string name)
        {
            var bldg = CreateLandmark(parent, pos + new Vector3(0, size.y * 0.5f, 0), size, color, name);
            var awning = GameObject.CreatePrimitive(PrimitiveType.Cube);
            awning.name = "Awning";
            awning.transform.SetParent(bldg.transform, false);
            awning.transform.localPosition = new Vector3(0, 0, 0.55f);
            awning.transform.localScale = new Vector3(0.92f, 0.08f, 0.35f);
            awning.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLit(new Color(0.92f, 0.22f, 0.22f));
            Destroy(awning.GetComponent<Collider>());
        }

        private void CreateOrganicTree(Transform parent, Vector3 pos, bool isSakura)
        {
            var tree = new GameObject(isSakura ? "Sakura_Tree" : "Oak_Tree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = pos;

            Color barkCol = isSakura ? new Color(0.42f, 0.30f, 0.22f) : new Color(0.34f, 0.22f, 0.12f);
            var barkTex = KyokaTextureGenerator.GetBarkTexture(barkCol);
            var barkMat = KyokaMaterialFactory.CreateLit(barkCol, barkTex);

            Color leafCol = isSakura ? new Color(1.0f, 0.65f, 0.78f) : new Color(0.16f, 0.52f, 0.20f);
            var leafMat = KyokaMaterialFactory.CreateFoliageMaterial(leafCol, isSakura);

            // Tapered Organic Trunk
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0, 2.2f, 0);
            trunk.transform.localScale = new Vector3(0.75f, 2.2f, 0.75f);
            trunk.GetComponent<Renderer>().material = barkMat;

            // Branching limbs
            var branch1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            branch1.name = "Branch_Left";
            branch1.transform.SetParent(tree.transform, false);
            branch1.transform.localPosition = new Vector3(-0.8f, 3.8f, 0.2f);
            branch1.transform.localRotation = Quaternion.Euler(0, 0, 32f);
            branch1.transform.localScale = new Vector3(0.35f, 1.2f, 0.35f);
            branch1.GetComponent<Renderer>().material = barkMat;
            Destroy(branch1.GetComponent<Collider>());

            var branch2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            branch2.name = "Branch_Right";
            branch2.transform.SetParent(tree.transform, false);
            branch2.transform.localPosition = new Vector3(0.8f, 4.0f, -0.2f);
            branch2.transform.localRotation = Quaternion.Euler(0, 0, -32f);
            branch2.transform.localScale = new Vector3(0.35f, 1.2f, 0.35f);
            branch2.GetComponent<Renderer>().material = barkMat;
            Destroy(branch2.GetComponent<Collider>());

            // Layered Foliage Clouds
            var c1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            c1.transform.SetParent(tree.transform, false);
            c1.transform.localPosition = new Vector3(-0.9f, 4.8f, 0.2f);
            c1.transform.localScale = new Vector3(3.2f, 2.8f, 3.2f);
            c1.GetComponent<Renderer>().material = leafMat;

            var c2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            c2.transform.SetParent(tree.transform, false);
            c2.transform.localPosition = new Vector3(0.9f, 5.0f, -0.2f);
            c2.transform.localScale = new Vector3(3.4f, 2.9f, 3.4f);
            c2.GetComponent<Renderer>().material = leafMat;

            var c3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            c3.transform.SetParent(tree.transform, false);
            c3.transform.localPosition = new Vector3(0, 6.4f, 0);
            c3.transform.localScale = new Vector3(3.8f, 3.2f, 3.8f);
            c3.GetComponent<Renderer>().material = leafMat;

            // Harvestable Wood Resource Node
            var node = tree.AddComponent<KyokaResourceNode>();
            node.resourceType = ResourceType.Tree;
        }

        private void CreateOrganicRock(Transform parent, Vector3 pos, ResourceType type, int seed)
        {
            var rock = new GameObject($"Rock_{type}_{seed}");
            rock.transform.SetParent(parent, false);
            rock.transform.localPosition = pos;

            // Faceted procedural boulder
            var boulderMesh = KyokaMeshBuilder.CreateFacetedRockMesh(2.0f, seed * 17 + 5);
            var rockGO = new GameObject("Mesh_Boulder");
            rockGO.transform.SetParent(rock.transform, false);
            rockGO.transform.localPosition = new Vector3(0, 0.8f, 0);

            var mf = rockGO.AddComponent<MeshFilter>();
            mf.sharedMesh = boulderMesh;

            var mr = rockGO.AddComponent<MeshRenderer>();
            var rockMat = KyokaMaterialFactory.CreateLit(new Color(0.48f, 0.50f, 0.52f), KyokaTextureGenerator.GetRockTexture(new Color(0.48f, 0.50f, 0.52f)));
            mr.material = rockMat;

            var col = rockGO.AddComponent<MeshCollider>();
            col.sharedMesh = boulderMesh;

            // Embedded Glowing Crystal Cluster
            var crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crystal.name = "CrystalVein";
            crystal.transform.SetParent(rock.transform, false);
            crystal.transform.localPosition = new Vector3(0.4f, 1.2f, 0.5f);
            crystal.transform.localRotation = Quaternion.Euler(45, 25, 30);
            crystal.transform.localScale = new Vector3(0.65f, 0.9f, 0.65f);

            Color crystalCol = (type == ResourceType.AetherCrystal) ? new Color(0.12f, 0.88f, 0.98f) :
                               (type == ResourceType.IronOre) ? new Color(0.95f, 0.65f, 0.22f) : new Color(0.72f, 0.74f, 0.78f);
            crystal.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateEmissionMaterial(crystalCol, 2.0f);
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
            CreateLandmark(portalRoot.transform, new Vector3(-2.2f, 2.8f, 0), new Vector3(0.9f, 5.6f, 0.9f), new Color(0.20f, 0.24f, 0.32f), "Pillar_Left");
            // Right Post
            CreateLandmark(portalRoot.transform, new Vector3(2.2f, 2.8f, 0), new Vector3(0.9f, 5.6f, 0.9f), new Color(0.20f, 0.24f, 0.32f), "Pillar_Right");
            // Header
            CreateLandmark(portalRoot.transform, new Vector3(0, 5.6f, 0), new Vector3(5.4f, 0.9f, 0.9f), new Color(0.20f, 0.24f, 0.32f), "Pillar_Header");

            // Glowing Portal Gateway Energy Field
            var field = GameObject.CreatePrimitive(PrimitiveType.Cube);
            field.name = "Portal_Field";
            field.transform.SetParent(portalRoot.transform, false);
            field.transform.localPosition = new Vector3(0, 2.8f, 0);
            field.transform.localScale = new Vector3(3.6f, 5.0f, 0.25f);
            field.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateEmissionMaterial(new Color(0.12f, 0.88f, 0.98f), 2.4f);
            Destroy(field.GetComponent<Collider>());
        }

        private GameObject CreateGround(Transform parent, Vector3 localPos, Vector3 scale, Color color)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.SetParent(parent, false);
            ground.transform.localPosition = localPos;
            ground.transform.localScale = scale;
            ground.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLit(color);
            return ground;
        }

        private GameObject CreateLandmark(Transform parent, Vector3 localPos, Vector3 scale, Color color, string objectName)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = objectName;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLit(color);
            return go;
        }

        #endregion
    }
}
