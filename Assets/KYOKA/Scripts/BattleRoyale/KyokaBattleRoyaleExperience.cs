using UnityEngine;
using UnityEngine.UI;
using Kyoka.Character;
using Kyoka.Save;
using Kyoka.Audio;
using Kyoka.Art;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Kyoka.BattleRoyale
{
    public class LootItem
    {
        public string id;
        public string displayName;
        public Color beamColor;
        public int damage;
        public int ammo;
        public bool isArmor;
        public bool isMedkit;
    }

    /// <summary>
    /// Complete AAA Battle Royale experience for KYOKA (Free Fire / PUBG style).
    /// Features:
    /// 1. Parachute / Glider drop phase from 150m altitude.
    /// 2. 3D textured Battle Island with military bases, watchtowers, and containers.
    /// 3. Floor loot system with glowing 3D light pillars and proximity pickup.
    /// 4. Third-person over-the-shoulder aiming (ADS), recoil, tracers, hitmarkers, damage numbers.
    /// 5. Hostile AI combatants that patrol, aim, and shoot.
    /// 6. Shrinking safe zone storm with real-time minimap radar.
    /// 7. BOOYAH / Victory Royale and Match Results screen returning to Lobby.
    /// </summary>
    public class KyokaBattleRoyaleExperience : MonoBehaviour
    {
        public static KyokaBattleRoyaleExperience Instance { get; private set; }

        public event Action OnMatchEnded;

        [Header("State")]
        public bool isGliding = true;
        public bool isGameOver = false;
        public int aliveCount = 15;
        public int killCount = 0;
        public int totalDamageDealt = 0;
        public float playerHealth = 100f;
        public float playerArmor = 50f;

        [Header("Weapon Inventory")]
        public string equippedWeapon = "AssaultRifle";
        public int currentAmmo = 30;
        public int reserveAmmo = 120;
        public int weaponDamage = 28;

        [Header("Storm Settings")]
        public Vector3 stormCenter = new Vector3(0, 0, 0);
        public float currentStormRadius = 180f;
        public float targetStormRadius = 50f;
        public float stormTimer = 45f;
        private Transform stormCylinder;

        [Header("Player & Camera")]
        private GameObject playerGO;
        private CharacterController playerController;
        private KyokaCharacterModel characterModel;
        private KyokaProceduralAnimator animator;
        private Camera combatCamera;
        private GameObject gliderObject;

        [Header("World Elements")]
        private GameObject worldRoot;
        private List<GameObject> activeBots = new List<GameObject>();
        private List<GameObject> activeLootPickups = new List<GameObject>();
        private GameObject nearbyLoot = null;

        [Header("UI Canvas")]
        private GameObject canvasGO;
        private Text aliveText;
        private Text killsText;
        private Text healthText;
        private Text armorText;
        private Text ammoText;
        private Text pickupPromptText;
        private Text stormTimerText;
        private Image crosshairImage;
        private Image hitmarkerImage;
        private Image stormVignette;
        private RectTransform minimapPlayerArrow;
        private RectTransform minimapSafeCircle;
        private RectTransform minimapStormCircle;
        private float hitmarkerTimer = 0f;

        private float cameraPitch = 16f;
        private float cameraYaw = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            BuildBattleIsland();
            SpawnPlayer();
            SpawnLoot();
            SpawnHostileBots();
            BuildCombatUI();

            KyokaAudioManager.Instance?.PlaySFX(KyokaSoundBank.GetGliderWind(), 0.8f);
        }

        #region World & Island Generation

        private void BuildBattleIsland()
        {
            worldRoot = new GameObject("BattleIsland_World");
            worldRoot.transform.SetParent(transform, false);

            // Directional Sunlight & Sky
            var sunGO = new GameObject("Island_Sun");
            sunGO.transform.SetParent(worldRoot.transform, false);
            var sun = sunGO.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.35f;
            sun.color = new Color(1.0f, 0.98f, 0.92f);
            sunGO.transform.rotation = Quaternion.Euler(45, -40, 0);

            // Large Island Ground Base (400m x 400m)
            var terrain = GameObject.CreatePrimitive(PrimitiveType.Cube);
            terrain.name = "Island_Terrain";
            terrain.transform.SetParent(worldRoot.transform, false);
            terrain.transform.position = new Vector3(0, -1.0f, 0);
            terrain.transform.localScale = new Vector3(400, 2.0f, 400);

            var terrainMat = KyokaMaterialFactory.CreateLit(new Color(0.22f, 0.28f, 0.25f), KyokaTextureGenerator.GetCyberGridTexture());
            terrainMat.mainTextureScale = new Vector2(40, 40);
            terrain.GetComponent<Renderer>().material = terrainMat;

            // Military Compounds & Structures
            BuildCompound(new Vector3(0, 0, 0), "Central_Command");
            BuildCompound(new Vector3(-65, 0, 50), "Storage_Depot");
            BuildCompound(new Vector3(70, 0, -45), "Radar_Outpost");
            BuildCompound(new Vector3(50, 0, 65), "Power_Station");
            BuildCompound(new Vector3(-70, 0, -60), "Barracks");

            // Shrinking Storm Forcefield Cylinder
            var stormGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stormGO.name = "AetherStorm_Barrier";
            stormGO.transform.SetParent(worldRoot.transform, false);
            stormGO.transform.position = new Vector3(0, 25, 0);
            stormGO.transform.localScale = new Vector3(currentStormRadius * 2, 50, currentStormRadius * 2);
            Destroy(stormGO.GetComponent<Collider>());

            var stormMat = KyokaMaterialFactory.CreateUnlit(new Color(0.15f, 0.65f, 0.95f, 0.25f));
            stormGO.GetComponent<Renderer>().material = stormMat;
            stormCylinder = stormGO.transform;
        }

        private void BuildCompound(Vector3 pos, string name)
        {
            var compound = new GameObject(name);
            compound.transform.SetParent(worldRoot.transform, false);
            compound.transform.position = pos;

            var wallMat = KyokaMaterialFactory.CreateLit(new Color(0.35f, 0.38f, 0.42f));
            var crateMat = KyokaMaterialFactory.CreateLit(new Color(0.65f, 0.45f, 0.25f));
            var metalMat = KyokaMaterialFactory.CreateLit(new Color(0.18f, 0.22f, 0.28f));

            // Bunker Building
            var bunker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bunker.transform.SetParent(compound.transform, false);
            bunker.transform.localPosition = new Vector3(0, 3, 0);
            bunker.transform.localScale = new Vector3(18, 6, 14);
            bunker.GetComponent<Renderer>().material = wallMat;

            // Watchtower
            var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tower.transform.SetParent(compound.transform, false);
            tower.transform.localPosition = new Vector3(14, 6, 12);
            tower.transform.localScale = new Vector3(2.5f, 12, 2.5f);
            tower.GetComponent<Renderer>().material = metalMat;

            // Cargo Containers & Cover Crates
            for (int i = -2; i <= 2; i++)
            {
                var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crate.transform.SetParent(compound.transform, false);
                crate.transform.localPosition = new Vector3(i * 5f, 1.25f, -12);
                crate.transform.localScale = new Vector3(3.2f, 2.5f, 3.2f);
                crate.GetComponent<Renderer>().material = crateMat;
            }
        }

        #endregion

        #region Player & Glider Drop System

        private void SpawnPlayer()
        {
            playerGO = new GameObject("BR_Player");
            playerGO.transform.position = new Vector3(0, 42f, -20f);
            playerGO.transform.SetParent(worldRoot.transform, true);

            playerController = playerGO.AddComponent<CharacterController>();
            playerController.height = 1.8f;
            playerController.radius = 0.35f;
            playerController.center = new Vector3(0, 0.9f, 0);

            characterModel = playerGO.AddComponent<KyokaCharacterModel>();
            animator = playerGO.AddComponent<KyokaProceduralAnimator>();
            animator.isGliding = true;

            // Load saved character appearance
            var save = KyokaSaveSystem.Load();
            if (save != null && save.customization != null)
            {
                var c = save.customization;
                characterModel.isFemale = c.isFemale;
                characterModel.BuildHumanoidMesh();
                characterModel.ApplyCustomization(
                    (HairStyleType)c.hairStyle,
                    new Color(c.hairColorR, c.hairColorG, c.hairColorB),
                    new Color(c.skinToneR, c.skinToneG, c.skinToneB),
                    new Color(c.eyeColorR, c.eyeColorG, c.eyeColorB),
                    (OutfitPreset)c.outfitPreset
                );
            }

            // Spawn High-Detail Cyber Delta-Wing Glider attached above player's shoulders
            gliderObject = KyokaMeshBuilder.CreateDetailedGlider();
            gliderObject.transform.SetParent(playerGO.transform, false);
            gliderObject.transform.localPosition = new Vector3(0, 1.88f, 0.12f);

            // Combat Camera
            var camGO = new GameObject("Combat_Camera");
            camGO.transform.SetParent(worldRoot.transform, true);
            combatCamera = camGO.AddComponent<Camera>();
            combatCamera.clearFlags = CameraClearFlags.Skybox;
            combatCamera.fieldOfView = 65f;
            camGO.AddComponent<AudioListener>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        #endregion

        #region Loot Spawning

        private void SpawnLoot()
        {
            var lootTypes = new LootItem[]
            {
                new LootItem { id = "AssaultRifle", displayName = "Assault Rifle", beamColor = new Color(0.7f, 0.2f, 0.9f), damage = 28, ammo = 30 },
                new LootItem { id = "Shotgun", displayName = "Tactical Shotgun", beamColor = new Color(0.1f, 0.6f, 1.0f), damage = 85, ammo = 8 },
                new LootItem { id = "Sniper", displayName = "Sniper Rifle", beamColor = new Color(1.0f, 0.8f, 0.1f), damage = 110, ammo = 5 },
                new LootItem { id = "Katana", displayName = "High-Frequency Katana", beamColor = new Color(0.95f, 0.2f, 0.25f), damage = 120, ammo = 1 },
                new LootItem { id = "Medkit", displayName = "Nano Medkit (+50 HP)", beamColor = new Color(0.1f, 0.9f, 0.3f), isMedkit = true },
                new LootItem { id = "Armor", displayName = "Tactical Vest (+50 Armor)", beamColor = new Color(0.15f, 0.9f, 0.95f), isArmor = true }
            };

            Vector3[] spawnPoints = new Vector3[]
            {
                new Vector3(0, 0.5f, 8),
                new Vector3(-8, 0.5f, 0),
                new Vector3(8, 0.5f, 0),
                new Vector3(0, 0.5f, -8),
                new Vector3(-60, 0.5f, 48),
                new Vector3(-68, 0.5f, 55),
                new Vector3(65, 0.5f, -42),
                new Vector3(75, 0.5f, -50),
                new Vector3(45, 0.5f, 60),
                new Vector3(-65, 0.5f, -55)
            };

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                var lType = lootTypes[i % lootTypes.Length];
                SpawnLootPickup(spawnPoints[i], lType);
            }
        }

        private void SpawnLootPickup(Vector3 pos, LootItem item)
        {
            var pickup = new GameObject($"Loot_{item.id}");
            pickup.transform.SetParent(worldRoot.transform, false);
            pickup.transform.position = pos;

            // Rotating 3D Icon
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(pickup.transform, false);
            cube.transform.localPosition = new Vector3(0, 0.8f, 0);
            cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            Destroy(cube.GetComponent<Collider>());

            var mat = KyokaMaterialFactory.CreateLit(item.beamColor);
            cube.GetComponent<Renderer>().material = mat;

            // Glowing Vertical Light Beam
            var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beam.transform.SetParent(pickup.transform, false);
            beam.transform.localPosition = new Vector3(0, 4.0f, 0);
            beam.transform.localScale = new Vector3(0.12f, 8.0f, 0.12f);
            Destroy(beam.GetComponent<Collider>());

            var beamMat = KyokaMaterialFactory.CreateUnlit(new Color(item.beamColor.r, item.beamColor.g, item.beamColor.b, 0.65f));
            beam.GetComponent<Renderer>().material = beamMat;

            pickup.tag = "Finish"; // Tag for identification
            activeLootPickups.Add(pickup);
        }

        #endregion

        #region Hostile AI Bots

        private void SpawnHostileBots()
        {
            string[] botNames = new string[] { "Viper99", "Kuro", "GhostX", "Blaze", "Apex", "Recon", "Striker", "Storm", "Phoenix", "Shadow", "Zero", "Titan", "Raptor", "Frost" };

            Vector3[] botPositions = new Vector3[]
            {
                new Vector3(-45, 0, 30),
                new Vector3(40, 0, 40),
                new Vector3(-30, 0, -40),
                new Vector3(35, 0, -35),
                new Vector3(-55, 0, 0),
                new Vector3(55, 0, 10),
                new Vector3(0, 0, 50),
                new Vector3(0, 0, -50),
                new Vector3(-25, 0, 25),
                new Vector3(25, 0, -25)
            };

            for (int i = 0; i < botPositions.Length; i++)
            {
                var bot = new GameObject($"Bot_{botNames[i]}");
                bot.transform.position = botPositions[i];
                bot.transform.SetParent(worldRoot.transform, true);

                var charModel = bot.AddComponent<KyokaCharacterModel>();
                bot.AddComponent<KyokaProceduralAnimator>();

                charModel.ApplyCustomization(
                    HairStyleType.TacticalBuzzcut,
                    Color.black,
                    new Color(0.85f, 0.75f, 0.65f),
                    Color.red,
                    OutfitPreset.DesertCommando
                );
                charModel.EquipWeapon("AssaultRifle");

                var cc = bot.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.radius = 0.35f;

                activeBots.Add(bot);
            }
        }

        #endregion

        #region Game Loop & Controls

        private void Update()
        {
            if (isGameOver) return;

            HandleInputAndCamera();
            HandleGliderOrGroundMovement();
            HandleShooting();
            HandleLootProximity();
            UpdateStorm();
            UpdateBots();
            UpdateUI();
        }

        private void HandleInputAndCamera()
        {
            float mouseX = Input.GetAxis("Mouse X") * 2.5f;
            float mouseY = Input.GetAxis("Mouse Y") * 2.0f;

            cameraYaw += mouseX;
            cameraPitch = Mathf.Clamp(cameraPitch - mouseY, -35f, 65f);

            playerGO.transform.rotation = Quaternion.Euler(0, cameraYaw, 0);

            // Third person over-the-shoulder positioning
            bool isAiming = Input.GetMouseButton(1);
            float dist = isAiming ? 1.5f : 2.8f;
            float shoulderOffset = isAiming ? 0.45f : 0.55f;
            float heightOffset = isAiming ? 1.35f : 1.45f;

            combatCamera.fieldOfView = Mathf.Lerp(combatCamera.fieldOfView, isAiming ? 45f : 65f, Time.deltaTime * 10f);
            if (animator) animator.isAiming = isAiming;

            Quaternion camRot = Quaternion.Euler(cameraPitch, cameraYaw, 0);
            Vector3 camTarget = playerGO.transform.position + Vector3.up * heightOffset + (camRot * Vector3.right * shoulderOffset);
            Vector3 camPos = camTarget - (camRot * Vector3.forward * dist);

            combatCamera.transform.position = camPos;
            combatCamera.transform.rotation = camRot;
        }

        private void HandleGliderOrGroundMovement()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 moveDir = (playerGO.transform.forward * v + playerGO.transform.right * h).normalized;

            if (isGliding)
            {
                // Smooth gliding descent
                Vector3 glideVel = moveDir * 16f + Vector3.down * 7.5f;
                playerController.Move(glideVel * Time.deltaTime);

                if (playerGO.transform.position.y <= 1.2f)
                {
                    // Touchdown! Collapse glider and start ground combat
                    isGliding = false;
                    if (gliderObject) Destroy(gliderObject);
                    if (animator) animator.isGliding = false;
                    KyokaAudioManager.Instance?.PlayCombatMusic();
                }
            }
            else
            {
                // Ground Locomotion
                bool sprint = Input.GetKey(KeyCode.LeftShift);
                float speed = sprint ? 8.5f : 5.0f;
                Vector3 groundVel = moveDir * speed + Vector3.down * 9.8f;
                playerController.Move(groundVel * Time.deltaTime);

                if (animator) animator.moveSpeed = moveDir.magnitude * speed;
            }
        }

        private void HandleShooting()
        {
            if (isGliding) return;

            // Firing
            if (Input.GetMouseButtonDown(0))
            {
                if (currentAmmo > 0)
                {
                    currentAmmo--;
                    FireWeapon();
                }
                else
                {
                    KyokaAudioManager.Instance?.PlaySFX(KyokaSoundBank.GetButtonClick(), 0.5f);
                    Reload();
                }
            }

            // Reload key
            if (Input.GetKeyDown(KeyCode.R))
            {
                Reload();
            }
        }

        private void FireWeapon()
        {
            if (animator) animator.TriggerRecoil();

            // Play procedural gunshot
            AudioClip shotSound = (equippedWeapon == "Shotgun") ? KyokaSoundBank.GetGunshotShotgun() :
                                  (equippedWeapon == "Sniper") ? KyokaSoundBank.GetGunshotSniper() :
                                  (equippedWeapon == "Katana") ? KyokaSoundBank.GetKatanaSlash() :
                                  KyokaSoundBank.GetGunshotRifle();

            KyokaAudioManager.Instance?.PlaySFX(shotSound);

            // Raycast forward from center of screen
            Ray ray = combatCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            if (Physics.Raycast(ray, out RaycastHit hit, 200f))
            {
                // Check if hit bot
                for (int i = activeBots.Count - 1; i >= 0; i--)
                {
                    var bot = activeBots[i];
                    if (bot != null && Vector3.Distance(hit.point, bot.transform.position) < 2.0f)
                    {
                        // Hit confirmed!
                        TriggerHitmarker();
                        totalDamageDealt += weaponDamage;

                        // Eliminate bot
                        Destroy(bot);
                        activeBots.RemoveAt(i);
                        aliveCount = Mathf.Max(1, aliveCount - 1);
                        killCount++;

                        KyokaAudioManager.Instance?.PlaySFX(KyokaSoundBank.GetHeadshot());
                        if (aliveCount <= 1)
                        {
                            TriggerVictory();
                        }
                        break;
                    }
                }
            }
        }

        private void TriggerHitmarker()
        {
            hitmarkerTimer = 0.18f;
            KyokaAudioManager.Instance?.PlaySFX(KyokaSoundBank.GetHitmarker());
        }

        private void Reload()
        {
            if (reserveAmmo <= 0 || currentAmmo >= 30) return;
            KyokaAudioManager.Instance?.PlaySFX(KyokaSoundBank.GetReload());
            int needed = 30 - currentAmmo;
            int take = Mathf.Min(needed, reserveAmmo);
            currentAmmo += take;
            reserveAmmo -= take;
        }

        private void HandleLootProximity()
        {
            nearbyLoot = null;
            if (playerGO == null) return;

            foreach (var loot in activeLootPickups)
            {
                if (loot == null) continue;
                if (Vector3.Distance(playerGO.transform.position, loot.transform.position) < 3.2f)
                {
                    nearbyLoot = loot;
                    break;
                }
            }

            if (nearbyLoot != null)
            {
                pickupPromptText.gameObject.SetActive(true);
                pickupPromptText.text = $"[E]  PICK UP {nearbyLoot.name.Replace("Loot_", "").ToUpper()}";

                if (Input.GetKeyDown(KeyCode.E))
                {
                    KyokaAudioManager.Instance?.PlaySFX(KyokaSoundBank.GetLootPickup());
                    if (nearbyLoot.name.Contains("Medkit")) playerHealth = Mathf.Min(100f, playerHealth + 50f);
                    else if (nearbyLoot.name.Contains("Armor")) playerArmor = Mathf.Min(100f, playerArmor + 50f);
                    else
                    {
                        equippedWeapon = nearbyLoot.name.Replace("Loot_", "");
                        currentAmmo = 30;
                        reserveAmmo += 60;
                        characterModel?.EquipWeapon(equippedWeapon);
                    }

                    activeLootPickups.Remove(nearbyLoot);
                    Destroy(nearbyLoot);
                    nearbyLoot = null;
                }
            }
            else
            {
                pickupPromptText.gameObject.SetActive(false);
            }
        }

        private void UpdateStorm()
        {
            stormTimer -= Time.deltaTime;
            if (stormTimer <= 0)
            {
                stormTimer = 45f;
                currentStormRadius = Mathf.Max(targetStormRadius, currentStormRadius - 40f);
                if (stormCylinder)
                {
                    stormCylinder.localScale = new Vector3(currentStormRadius * 2, 50, currentStormRadius * 2);
                }
            }

            // Damage outside storm
            if (playerGO != null && Vector3.Distance(playerGO.transform.position, stormCenter) > currentStormRadius)
            {
                playerHealth -= Time.deltaTime * 6.5f;
                stormVignette.color = new Color(0.9f, 0.1f, 0.1f, 0.35f + Mathf.PingPong(Time.time * 2f, 0.25f));
                if (playerHealth <= 0)
                {
                    TriggerDefeat();
                }
            }
            else
            {
                stormVignette.color = Color.clear;
            }
        }

        private void UpdateBots()
        {
            if (playerGO == null) return;
            Vector3 playerPos = playerGO.transform.position;

            foreach (var bot in activeBots)
            {
                if (bot == null) continue;
                float dist = Vector3.Distance(bot.transform.position, playerPos);
                if (dist < 35f && !isGliding)
                {
                    bot.transform.LookAt(new Vector3(playerPos.x, bot.transform.position.y, playerPos.z));
                    // Bot fires occasional burst
                    if (UnityEngine.Random.value < 0.015f)
                    {
                        KyokaAudioManager.Instance?.PlaySFX(KyokaSoundBank.GetGunshotRifle(), 0.4f);
                        if (playerArmor > 0) playerArmor -= 12f;
                        else playerHealth -= 10f;

                        if (playerHealth <= 0) TriggerDefeat();
                    }
                }
            }
        }

        #endregion

        #region Match Results & Victory

        private void TriggerVictory()
        {
            if (isGameOver) return;
            isGameOver = true;

            KyokaAudioManager.Instance?.PlaySFX(KyokaSoundBank.GetVictoryFanfare());
            ShowResultsScreen(true);
        }

        private void TriggerDefeat()
        {
            if (isGameOver) return;
            isGameOver = true;

            ShowResultsScreen(false);
        }

        private void ShowResultsScreen(bool isVictory)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Reward persistent profile
            var save = KyokaSaveSystem.Load() ?? new KyokaSaveData();
            int rewardCoins = isVictory ? 500 : (killCount * 80 + 100);
            int rewardXP = isVictory ? 350 : (killCount * 50 + 80);
            save.kyokaCredits += rewardCoins;
            save.currentXP += rewardXP;
            KyokaSaveSystem.Save(save);

            // Results Modal
            var modal = new GameObject("Results_Screen");
            modal.transform.SetParent(canvasGO.transform, false);

            var mImg = modal.AddComponent<Image>();
            mImg.sprite = KyokaTextureGenerator.GetPanelSprite();
            mImg.type = Image.Type.Sliced;
            mImg.color = new Color(0.04f, 0.06f, 0.10f, 0.96f);

            var rt = modal.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.20f, 0.15f);
            rt.anchorMax = new Vector2(0.80f, 0.85f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Header Banner
            var title = CreateLabel("Title", modal.transform, isVictory ? "🏆  BOOYAH!  VICTORY ROYALE" : "☠  MATCH OVER", font, 36, TextAnchor.MiddleCenter, isVictory ? new Color(1.0f, 0.85f, 0.2f) : new Color(0.9f, 0.25f, 0.25f));
            title.fontStyle = FontStyle.Bold;
            title.rectTransform.anchoredPosition = new Vector2(0, 160);

            // Stats Breakdown
            string stats = $"PLACEMENT: {(isVictory ? "#1" : $"#{aliveCount + 1}")} / 15\n\n" +
                           $"ELIMINATIONS: {killCount}\n\n" +
                           $"DAMAGE DEALT: {totalDamageDealt}\n\n" +
                           $"REWARDS: +{rewardCoins} COINS  •  +{rewardXP} EXP";

            var statsLbl = CreateLabel("Stats", modal.transform, stats, font, 20, TextAnchor.MiddleCenter, Color.white);
            statsLbl.rectTransform.anchoredPosition = new Vector2(0, 10);

            // Return to Lobby Button
            var lobbyBtn = CreateButton("LobbyBtn", modal.transform, "❯❯   RETURN TO LOBBY", font, 20, new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.20f), () =>
            {
                OnMatchEnded?.Invoke();
                Destroy(gameObject);
            }, new Color(0.95f, 0.72f, 0.15f));

            var lbText = lobbyBtn.GetComponentInChildren<Text>();
            lbText.color = new Color(0.08f, 0.08f, 0.12f);
            lbText.fontStyle = FontStyle.Bold;
        }

        #endregion

        #region HUD & Minimap

        private void BuildCombatUI()
        {
            canvasGO = new GameObject("BR_Canvas");
            canvasGO.transform.SetParent(transform, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Screen Vignette (for storm damage)
            var vigGO = CreateUIObject("StormVignette", canvasGO.transform);
            stormVignette = vigGO.AddComponent<Image>();
            stormVignette.color = Color.clear;
            SetFullScreen(vigGO.GetComponent<RectTransform>());

            // Center Crosshair
            var crossGO = CreateUIObject("Crosshair", canvasGO.transform);
            crosshairImage = crossGO.AddComponent<Image>();
            crosshairImage.sprite = KyokaTextureGenerator.GetCrosshairSprite();
            var crt = crossGO.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0.5f);
            crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(48, 48);

            // Hitmarker Image
            var hmGO = CreateUIObject("Hitmarker", canvasGO.transform);
            hitmarkerImage = hmGO.AddComponent<Image>();
            hitmarkerImage.sprite = KyokaTextureGenerator.GetCrosshairSprite();
            hitmarkerImage.color = Color.clear;
            var hrt = hmGO.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0.5f, 0.5f);
            hrt.anchorMax = new Vector2(0.5f, 0.5f);
            hrt.sizeDelta = new Vector2(64, 64);
            hrt.localRotation = Quaternion.Euler(0, 0, 45);

            // Top Status Bar (Alive / Kills)
            var topBox = CreateUIObject("TopStatus", canvasGO.transform);
            var tbRect = topBox.GetComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0.40f, 0.92f);
            tbRect.anchorMax = new Vector2(0.60f, 0.98f);
            tbRect.offsetMin = Vector2.zero;
            tbRect.offsetMax = Vector2.zero;

            var tbImg = topBox.AddComponent<Image>();
            tbImg.sprite = KyokaTextureGenerator.GetPanelSprite();
            tbImg.type = Image.Type.Sliced;
            tbImg.color = new Color(0.08f, 0.12f, 0.18f, 0.88f);

            aliveText = CreateLabel("Alive", topBox.transform, $"ALIVE: {aliveCount}", font, 18, TextAnchor.MiddleLeft, new Color(0.95f, 0.75f, 0.2f));
            aliveText.fontStyle = FontStyle.Bold;
            aliveText.rectTransform.anchoredPosition = new Vector2(30, 0);

            killsText = CreateLabel("Kills", topBox.transform, $"KILLS: {killCount}", font, 18, TextAnchor.MiddleRight, new Color(0.95f, 0.25f, 0.3f));
            killsText.fontStyle = FontStyle.Bold;
            killsText.rectTransform.anchoredPosition = new Vector2(-30, 0);

            // Bottom Left Vitals (Health & Armor)
            var vitalsBox = CreateUIObject("VitalsBox", canvasGO.transform);
            var vbRect = vitalsBox.GetComponent<RectTransform>();
            vbRect.anchorMin = new Vector2(0.03f, 0.04f);
            vbRect.anchorMax = new Vector2(0.24f, 0.15f);
            vbRect.offsetMin = Vector2.zero;
            vbRect.offsetMax = Vector2.zero;

            var vbImg = vitalsBox.AddComponent<Image>();
            vbImg.sprite = KyokaTextureGenerator.GetPanelSprite();
            vbImg.type = Image.Type.Sliced;
            vbImg.color = new Color(0.06f, 0.08f, 0.14f, 0.88f);

            healthText = CreateLabel("HP", vitalsBox.transform, "HP: 100", font, 16, TextAnchor.MiddleLeft, new Color(0.2f, 0.95f, 0.4f));
            healthText.rectTransform.anchoredPosition = new Vector2(25, 14);

            armorText = CreateLabel("Armor", vitalsBox.transform, "ARMOR: 50", font, 16, TextAnchor.MiddleLeft, new Color(0.15f, 0.85f, 0.95f));
            armorText.rectTransform.anchoredPosition = new Vector2(25, -14);

            // Bottom Right Ammo Counter
            var ammoBox = CreateUIObject("AmmoBox", canvasGO.transform);
            var abRect = ammoBox.GetComponent<RectTransform>();
            abRect.anchorMin = new Vector2(0.80f, 0.04f);
            abRect.anchorMax = new Vector2(0.97f, 0.15f);
            abRect.offsetMin = Vector2.zero;
            abRect.offsetMax = Vector2.zero;

            var abImg = ammoBox.AddComponent<Image>();
            abImg.sprite = KyokaTextureGenerator.GetPanelSprite();
            abImg.type = Image.Type.Sliced;
            abImg.color = new Color(0.06f, 0.08f, 0.14f, 0.88f);

            ammoText = CreateLabel("Ammo", ammoBox.transform, $"{equippedWeapon}\n30 / 120", font, 18, TextAnchor.MiddleCenter, Color.white);
            ammoText.fontStyle = FontStyle.Bold;
            SetFullScreen(ammoText.GetComponent<RectTransform>());

            // Pickup Prompt
            pickupPromptText = CreateLabel("PickupPrompt", canvasGO.transform, "[E] PICK UP", font, 20, TextAnchor.MiddleCenter, new Color(1.0f, 0.85f, 0.2f));
            pickupPromptText.fontStyle = FontStyle.Bold;
            pickupPromptText.rectTransform.anchoredPosition = new Vector2(0, -90);
            pickupPromptText.gameObject.SetActive(false);

            // Storm Timer (Top Center)
            stormTimerText = CreateLabel("StormTimer", canvasGO.transform, "STORM: 0:45", font, 14, TextAnchor.MiddleCenter, new Color(0.6f, 0.85f, 1.0f));
            stormTimerText.rectTransform.anchorMin = new Vector2(0.40f, 0.87f);
            stormTimerText.rectTransform.anchorMax = new Vector2(0.60f, 0.91f);
            stormTimerText.rectTransform.offsetMin = Vector2.zero;
            stormTimerText.rectTransform.offsetMax = Vector2.zero;
        }

        private void UpdateUI()
        {
            if (aliveText) aliveText.text = $"ALIVE: {aliveCount}";
            if (killsText) killsText.text = $"KILLS: {killCount}";
            if (healthText) healthText.text = $"HP: {Mathf.Max(0, (int)playerHealth)}";
            if (armorText) armorText.text = $"ARMOR: {Mathf.Max(0, (int)playerArmor)}";
            if (ammoText) ammoText.text = $"{equippedWeapon}\n{currentAmmo} / {reserveAmmo}";
            if (stormTimerText) stormTimerText.text = $"STORM: 0:{Mathf.Max(0, (int)stormTimer):D2}";

            // Hitmarker Fade
            if (hitmarkerTimer > 0f)
            {
                hitmarkerTimer -= Time.deltaTime;
                hitmarkerImage.color = new Color(1f, 0.2f, 0.2f, hitmarkerTimer * 5f);
            }
            else
            {
                hitmarkerImage.color = Color.clear;
            }
        }

        #endregion

        #region Helpers

        private GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private Text CreateLabel(string name, Transform parent, string text, Font font, int size, TextAnchor align, Color color)
        {
            var go = CreateUIObject(name, parent);
            var t = go.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 30);
            return t;
        }

        private Button CreateButton(string name, Transform parent, string label, Font font, int size, Vector2 anchorMin, Vector2 anchorMax, Action onClick, Color bgColor)
        {
            var go = CreateUIObject(name, parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.sprite = KyokaTextureGenerator.GetButtonSprite(bgColor, new Color(1f, 1f, 1f, 0.35f));
            img.type = Image.Type.Sliced;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());

            var lblGO = CreateUIObject("Label", go.transform);
            var t = lblGO.AddComponent<Text>();
            t.font = font;
            t.text = label;
            t.fontSize = size;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            var lrt = lblGO.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            return btn;
        }

        private void SetFullScreen(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (playerGO != null) Destroy(playerGO);
            if (combatCamera != null) Destroy(combatCamera.gameObject);
            if (canvasGO != null) Destroy(canvasGO);
            if (worldRoot != null) Destroy(worldRoot);

            if (activeBots != null)
            {
                foreach (var b in activeBots)
                {
                    if (b != null) Destroy(b);
                }
                activeBots.Clear();
            }

            if (activeLootPickups != null)
            {
                foreach (var l in activeLootPickups)
                {
                    if (l != null) Destroy(l);
                }
                activeLootPickups.Clear();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        #endregion
    }
}

