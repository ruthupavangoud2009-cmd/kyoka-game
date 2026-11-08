using UnityEngine;
using Kyoka.Player;
using Kyoka.Abilities;
using Kyoka.Combat;
using Kyoka.Economy;
using Kyoka.Missions;
using Kyoka.World;
using Kyoka.Core;
using Kyoka.Save;
using Kyoka.Vehicles;
using Kyoka.Inventory;
using Kyoka.BattleRoyale;

namespace Kyoka.UI
{
    public class KyokaHUD : MonoBehaviour
    {
        public static KyokaHUD Instance { get; private set; }

        private KyokaPlayerController player;
        private KyokaHealth health;
        private KyokaAbilityUser abilityUser;
        private KyokaCombatSystem combatSystem;
        private KyokaVehicleInteractor vehicleInteractor;
        private KyokaInventory inventory;

        // UI Modes
        private bool showMenu = false;
        private bool showSettings = false;
        private bool showInventory = false;
        private bool showMap = false;
        private bool showModes = false;

        // Styles
        private GUIStyle titleStyle;
        private GUIStyle headerStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle statLabelStyle;
        private GUIStyle buttonStyle;
        private GUIStyle activeTabStyle;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            FindPlayerReferences();
        }

        private void FindPlayerReferences()
        {
            player = FindObjectOfType<KyokaPlayerController>();
            if (player != null)
            {
                health = player.GetComponent<KyokaHealth>();
                abilityUser = player.GetComponent<KyokaAbilityUser>();
                combatSystem = player.GetComponent<KyokaCombatSystem>();
                vehicleInteractor = player.GetComponent<KyokaVehicleInteractor>();
                inventory = player.GetComponent<KyokaInventory>();
            }
        }

        public bool showLegacyHUD = false;

        private void OnGUI()
        {
            if (!showLegacyHUD) return;
            if (player == null) FindPlayerReferences();
            InitializeStyles();

            // 1. Top Navigation & Stats Bar
            DrawTopStatsBar();

            // 2. Health & Energy Bars (Bottom Left)
            DrawVitalsHUD();

            // 3. Mission Tracker (Top Right)
            DrawMissionTracker();

            // 4. Mode-specific Overlays (Battle Royale / Vehicle)
            DrawSpecialContextHUD();

            // 5. Modal Windows (Pause Menu, Settings, Inventory, Map, Modes)
            if (showMenu) DrawPauseMenu();
            else if (showSettings) DrawSettingsMenu();
            else if (showInventory) DrawInventoryWindow();
            else if (showMap) DrawMapWindow();
            else if (showModes) DrawModeSelectorWindow();
        }

        private void DrawTopStatsBar()
        {
            float w = Screen.width;
            GUI.Box(new Rect(10, 10, w - 20, 48), "");

            // Game Logo
            GUI.Label(new Rect(20, 14, 110, 36), "KYOKA", titleStyle);

            // Region & Mode
            var wm = KyokaWorldManager.Instance;
            var gm = KyokaGameModeManager.Instance;
            string regionName = wm != null ? wm.CurrentRegion.ToString() : "KYOKA City";
            string modeName = gm != null ? gm.ActiveMode.ToString() : "Open World";
            float timeVal = wm != null ? wm.timeOfDay : 12f;
            int hours = Mathf.FloorToInt(timeVal);
            int minutes = Mathf.FloorToInt((timeVal - hours) * 60f);

            GUI.Label(new Rect(135, 16, 260, 24), $"Region: {regionName} | Mode: {modeName}", bodyStyle);
            GUI.Label(new Rect(135, 34, 160, 20), $"Time: {hours:D2}:{minutes:D2}", smallStyle);

            // Economy Balances
            var eco = KyokaEconomyManager.Instance;
            int credits = eco != null ? eco.KyokaCredits : 0;
            int shards = eco != null ? eco.AetherShards : 0;

            GUI.Label(new Rect(410, 16, 220, 24), $"Credits: ¢{credits}   |   Shards: ◆{shards}", headerStyle);

            // Action Buttons (Top Right)
            float btnX = w - 580;
            if (GUI.Button(new Rect(btnX, 16, 85, 34), "Modes", buttonStyle))
            {
                showModes = !showModes;
                showMenu = showSettings = showInventory = showMap = false;
            }
            if (GUI.Button(new Rect(btnX + 95, 16, 85, 34), "Regions", buttonStyle))
            {
                showMap = !showMap;
                showMenu = showSettings = showInventory = showModes = false;
            }
            if (GUI.Button(new Rect(btnX + 190, 16, 95, 34), "Inventory", buttonStyle))
            {
                showInventory = !showInventory;
                showMenu = showSettings = showMap = showModes = false;
            }
            if (GUI.Button(new Rect(btnX + 295, 16, 85, 34), "Settings", buttonStyle))
            {
                showSettings = !showSettings;
                showMenu = showInventory = showMap = showModes = false;
            }
            if (GUI.Button(new Rect(btnX + 390, 16, 85, 34), "Save", buttonStyle))
            {
                PerformSave();
            }
            if (GUI.Button(new Rect(btnX + 485, 16, 75, 34), showMenu ? "Close" : "Menu", buttonStyle))
            {
                showMenu = !showMenu;
                showSettings = showInventory = showMap = showModes = false;
            }
        }

        private void DrawVitalsHUD()
        {
            float h = Screen.height;
            GUI.Box(new Rect(15, h - 145, 290, 130), "");

            int curHp = health ? health.CurrentHealth : 100;
            int maxHp = health ? health.maxHealth : 100;
            float hpPct = (float)curHp / Mathf.Max(1, maxHp);

            float curEng = abilityUser ? abilityUser.energy : 100;
            float maxEng = abilityUser ? abilityUser.maxEnergy : 100;
            float engPct = curEng / Mathf.Max(1, maxEng);

            // HP Bar Background & Fill
            GUI.Label(new Rect(25, h - 140, 120, 20), $"HEALTH: {curHp}/{maxHp}", statLabelStyle);
            GUI.Box(new Rect(25, h - 120, 260, 18), "");
            DrawColorRect(new Rect(26, h - 119, 258 * hpPct, 16), new Color(0.2f, 0.85f, 0.3f));

            // Energy Bar Background & Fill
            GUI.Label(new Rect(25, h - 96, 120, 20), $"AETHER: {Mathf.RoundToInt(curEng)}/{Mathf.RoundToInt(maxEng)}", statLabelStyle);
            GUI.Box(new Rect(25, h - 76, 260, 18), "");
            DrawColorRect(new Rect(26, h - 75, 258 * engPct, 16), new Color(0.2f, 0.7f, 0.95f));

            // Weapon & Ammo Indicator
            string weaponName = (combatSystem != null && combatSystem.EquippedWeapon != null) ? combatSystem.EquippedWeapon.displayName : "Fists";
            string ammoInfo = (combatSystem != null && combatSystem.EquippedWeapon != null && combatSystem.EquippedWeapon.weaponType != WeaponType.Melee)
                ? $"Ammo: {combatSystem.currentAmmo}" : "Melee Ready";
            GUI.Label(new Rect(25, h - 52, 260, 22), $"{weaponName} | {ammoInfo}", bodyStyle);
        }

        private void DrawMissionTracker()
        {
            var mm = KyokaMissionManager.Instance;
            if (mm == null || mm.ActiveMission == null) return;

            var m = mm.ActiveMission;
            float w = Screen.width;
            GUI.Box(new Rect(w - 275, 68, 265, 105), "");

            GUI.Label(new Rect(w - 265, 72, 245, 22), $"MISSION: {m.title}", headerStyle);
            GUI.Label(new Rect(w - 265, 96, 245, 38), m.description, smallStyle);

            float progPct = (float)m.currentProgress / Mathf.Max(1, m.requiredProgress);
            GUI.Box(new Rect(w - 265, 138, 245, 14), "");
            DrawColorRect(new Rect(w - 264, 139, 243 * progPct, 12), new Color(0.95f, 0.8f, 0.2f));
            GUI.Label(new Rect(w - 265, 153, 245, 18), $"Progress: {m.currentProgress}/{m.requiredProgress} (Reward: ¢{m.rewardCredits})", smallStyle);
        }

        private void DrawSpecialContextHUD()
        {
            // Battle Royale Stats
            var br = KyokaMatchController.Instance;
            if (br != null && br.State == KyokaMatchState.InProgress)
            {
                float w = Screen.width;
                GUI.Box(new Rect(w * 0.5f - 140, 68, 280, 50), "");
                GUI.Label(new Rect(w * 0.5f - 130, 72, 260, 22), $"ALIVE: {br.alivePlayers} | KILLS: {br.playerKills}", headerStyle);
                int mins = Mathf.FloorToInt(br.RemainingSeconds / 60f);
                int secs = Mathf.FloorToInt(br.RemainingSeconds % 60f);
                GUI.Label(new Rect(w * 0.5f - 130, 94, 260, 20), $"Safe Zone Shrinking in: {mins:D2}:{secs:D2}", smallStyle);
            }

            // Vehicle HUD
            if (vehicleInteractor != null && vehicleInteractor.IsInVehicle)
            {
                var v = vehicleInteractor.ActiveVehicle;
                float w = Screen.width;
                float h = Screen.height;
                GUI.Box(new Rect(w * 0.5f - 110, h - 90, 220, 75), "");
                GUI.Label(new Rect(w * 0.5f - 100, h - 85, 200, 22), v.vehicleName, headerStyle);
                GUI.Label(new Rect(w * 0.5f - 100, h - 63, 200, 20), $"Vehicle Health: {v.currentHealth}/{v.maxHealth}", smallStyle);
                GUI.Label(new Rect(w * 0.5f - 100, h - 43, 200, 20), $"Fuel: {Mathf.RoundToInt(v.fuel)}% | [E] Exit Vehicle", bodyStyle);
            }
        }

        #region Modal Windows

        private void DrawPauseMenu()
        {
            float cx = Screen.width * 0.5f - 175;
            float cy = Screen.height * 0.5f - 180;
            GUI.Box(new Rect(cx, cy, 350, 360), "");

            GUI.Label(new Rect(cx + 20, cy + 20, 310, 35), "PAUSE MENU", titleStyle);

            if (GUI.Button(new Rect(cx + 35, cy + 70, 280, 42), "Resume Game", buttonStyle))
            {
                showMenu = false;
            }
            if (GUI.Button(new Rect(cx + 35, cy + 122, 280, 42), "Save Game", buttonStyle))
            {
                PerformSave();
            }
            if (GUI.Button(new Rect(cx + 35, cy + 174, 280, 42), "Settings & Presets", buttonStyle))
            {
                showSettings = true;
                showMenu = false;
            }
            if (GUI.Button(new Rect(cx + 35, cy + 226, 280, 42), "Change Game Mode", buttonStyle))
            {
                showModes = true;
                showMenu = false;
            }
            if (GUI.Button(new Rect(cx + 35, cy + 278, 280, 42), "Travel Regions", buttonStyle))
            {
                showMap = true;
                showMenu = false;
            }
        }

        private void DrawSettingsMenu()
        {
            float cx = Screen.width * 0.5f - 225;
            float cy = Screen.height * 0.5f - 210;
            GUI.Box(new Rect(cx, cy, 450, 420), "");

            var uiMgr = KyokaUIManager.Instance;
            GUI.Label(new Rect(cx + 20, cy + 20, 410, 35), "GRAPHICS & SETTINGS", titleStyle);

            // Quality Presets
            GUI.Label(new Rect(cx + 30, cy + 65, 390, 24), "Graphics Quality Preset (Android / PC):", headerStyle);
            int currentPreset = uiMgr ? uiMgr.graphicsPreset : 1;

            if (GUI.Button(new Rect(cx + 30, cy + 95, 120, 38), "LOW (Fast)", currentPreset == 0 ? activeTabStyle : buttonStyle))
            {
                if (uiMgr) uiMgr.ApplyQualityPreset(0);
            }
            if (GUI.Button(new Rect(cx + 165, cy + 95, 120, 38), "MEDIUM (Bal)", currentPreset == 1 ? activeTabStyle : buttonStyle))
            {
                if (uiMgr) uiMgr.ApplyQualityPreset(1);
            }
            if (GUI.Button(new Rect(cx + 300, cy + 95, 120, 38), "HIGH (Ultra)", currentPreset == 2 ? activeTabStyle : buttonStyle))
            {
                if (uiMgr) uiMgr.ApplyQualityPreset(2);
            }

            // Target FPS
            GUI.Label(new Rect(cx + 30, cy + 145, 390, 24), "Target Framerate:", headerStyle);
            if (GUI.Button(new Rect(cx + 30, cy + 172, 120, 34), "30 FPS", buttonStyle))
            {
                if (uiMgr) uiMgr.SetTargetFPS(30);
            }
            if (GUI.Button(new Rect(cx + 165, cy + 172, 120, 34), "60 FPS", buttonStyle))
            {
                if (uiMgr) uiMgr.SetTargetFPS(60);
            }
            if (GUI.Button(new Rect(cx + 300, cy + 172, 120, 34), "120 FPS", buttonStyle))
            {
                if (uiMgr) uiMgr.SetTargetFPS(120);
            }

            // Audio Master Volume
            GUI.Label(new Rect(cx + 30, cy + 220, 390, 24), $"Master Volume: {Mathf.RoundToInt((uiMgr ? uiMgr.masterVolume : 0.8f) * 100)}%", headerStyle);
            float currentVol = uiMgr ? uiMgr.masterVolume : 0.8f;
            float newVol = GUI.HorizontalSlider(new Rect(cx + 30, cy + 248, 390, 20), currentVol, 0f, 1f);
            if (uiMgr && Mathf.Abs(newVol - currentVol) > 0.01f)
            {
                uiMgr.SetMasterVolume(newVol);
            }

            // Mobile Touch Controls Toggle
            var touchControls = KyokaTouchControls.Instance;
            if (touchControls != null)
            {
                string touchStatus = touchControls.forceEnableOnDesktop ? "ENABLED (ON)" : "AUTO (OFF ON PC)";
                if (GUI.Button(new Rect(cx + 30, cy + 285, 390, 38), $"Touchscreen Controls Overlay: {touchStatus}", buttonStyle))
                {
                    touchControls.forceEnableOnDesktop = !touchControls.forceEnableOnDesktop;
                }
            }

            if (GUI.Button(new Rect(cx + 140, cy + 355, 170, 42), "Back to Game", buttonStyle))
            {
                showSettings = false;
            }
        }

        private void DrawInventoryWindow()
        {
            float cx = Screen.width * 0.5f - 240;
            float cy = Screen.height * 0.5f - 200;
            GUI.Box(new Rect(cx, cy, 480, 400), "");

            GUI.Label(new Rect(cx + 20, cy + 20, 440, 35), "PLAYER INVENTORY", titleStyle);

            if (inventory != null)
            {
                var items = inventory.Items;
                int startY = (int)cy + 70;
                for (int i = 0; i < items.Count && i < 6; i++)
                {
                    var item = items[i];
                    GUI.Box(new Rect(cx + 25, startY + i * 46, 430, 40), "");
                    GUI.Label(new Rect(cx + 35, startY + i * 46 + 10, 220, 24), $"{item.itemId.ToUpper()} (Qty: {item.quantity})", bodyStyle);

                    if (GUI.Button(new Rect(cx + 280, startY + i * 46 + 6, 80, 28), "Use", buttonStyle))
                    {
                        inventory.UseItem(item.itemId);
                    }
                    if (GUI.Button(new Rect(cx + 370, startY + i * 46 + 6, 75, 28), "Drop", buttonStyle))
                    {
                        inventory.Remove(item.itemId, 1);
                    }
                }
            }

            if (GUI.Button(new Rect(cx + 170, cy + 345, 140, 38), "Close", buttonStyle))
            {
                showInventory = false;
            }
        }

        private void DrawMapWindow()
        {
            float cx = Screen.width * 0.5f - 250;
            float cy = Screen.height * 0.5f - 210;
            GUI.Box(new Rect(cx, cy, 500, 420), "");

            GUI.Label(new Rect(cx + 20, cy + 20, 460, 35), "KYOKA WORLD REGIONS", titleStyle);
            GUI.Label(new Rect(cx + 20, cy + 55, 460, 22), "Select a major region to stream and travel to:", bodyStyle);

            var wm = KyokaWorldManager.Instance;
            KyokaRegionId[] regions = (KyokaRegionId[])System.Enum.GetValues(typeof(KyokaRegionId));

            int startY = (int)cy + 85;
            for (int i = 0; i < regions.Length; i++)
            {
                KyokaRegionId r = regions[i];
                bool isCurrent = (wm != null && wm.CurrentRegion == r);

                if (GUI.Button(new Rect(cx + 30, startY + i * 48, 440, 40),
                               $"{r} {(isCurrent ? "(ACTIVE REGION)" : "")}",
                               isCurrent ? activeTabStyle : buttonStyle))
                {
                    if (wm != null)
                    {
                        wm.LoadRegion(r);
                        TeleportPlayerToRegion(r);
                        showMap = false;
                    }
                }
            }

            if (GUI.Button(new Rect(cx + 180, cy + 370, 140, 38), "Close Map", buttonStyle))
            {
                showMap = false;
            }
        }

        private void DrawModeSelectorWindow()
        {
            float cx = Screen.width * 0.5f - 250;
            float cy = Screen.height * 0.5f - 210;
            GUI.Box(new Rect(cx, cy, 500, 420), "");

            GUI.Label(new Rect(cx + 20, cy + 20, 460, 35), "KYOKA GAME MODES", titleStyle);
            GUI.Label(new Rect(cx + 20, cy + 55, 460, 22), "Switch active gameplay rules, objectives, and loop:", bodyStyle);

            var gm = KyokaGameModeManager.Instance;
            KyokaGameMode[] modes = (KyokaGameMode[])System.Enum.GetValues(typeof(KyokaGameMode));

            int startY = (int)cy + 85;
            for (int i = 0; i < modes.Length; i++)
            {
                KyokaGameMode m = modes[i];
                bool isCurrent = (gm != null && gm.ActiveMode == m);

                if (GUI.Button(new Rect(cx + 30, startY + i * 48, 440, 40),
                               $"{m} {(isCurrent ? "(CURRENT MODE)" : "")}",
                               isCurrent ? activeTabStyle : buttonStyle))
                {
                    if (gm != null)
                    {
                        gm.SetGameMode(m);
                        showModes = false;
                    }
                }
            }

            if (GUI.Button(new Rect(cx + 180, cy + 370, 140, 38), "Close", buttonStyle))
            {
                showModes = false;
            }
        }

        #endregion

        private void TeleportPlayerToRegion(KyokaRegionId r)
        {
            if (player == null) return;
            switch (r)
            {
                case KyokaRegionId.KYOKACity:
                    player.Teleport(new Vector3(0, 2, 0));
                    break;
                case KyokaRegionId.Wildlands:
                    player.Teleport(new Vector3(250, 2, 0));
                    break;
                case KyokaRegionId.OceanRealm:
                    player.Teleport(new Vector3(0, 2, 250));
                    break;
                case KyokaRegionId.AnimeRealm:
                    player.Teleport(new Vector3(-250, 18, 0));
                    break;
                case KyokaRegionId.BattleIslands:
                    player.Teleport(new Vector3(0, 2, -250));
                    break;
                case KyokaRegionId.CreativeWorld:
                    player.Teleport(new Vector3(0, 502, 0));
                    break;
            }
        }

        private void PerformSave()
        {
            var data = new KyokaSaveData();
            if (player != null)
            {
                data.posX = player.transform.position.x;
                data.posY = player.transform.position.y;
                data.posZ = player.transform.position.z;
                data.rotY = player.transform.eulerAngles.y;
                data.hasSavedPosition = true;
            }
            if (health != null) data.currentHealth = health.CurrentHealth;
            if (abilityUser != null) data.currentEnergy = abilityUser.energy;

            var eco = KyokaEconomyManager.Instance;
            if (eco != null) eco.SaveTo(data);

            var mm = KyokaMissionManager.Instance;
            if (mm != null) mm.SaveTo(data);

            var inv = player != null ? player.GetComponent<KyokaInventory>() : null;
            if (inv != null) inv.SaveTo(data);

            KyokaSaveSystem.Save(data);
        }

        private void DrawColorRect(Rect r, Color c)
        {
            var prevColor = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = prevColor;
        }

        private void InitializeStyles()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
                titleStyle.normal.textColor = Color.white;

                headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold };
                headerStyle.normal.textColor = new Color(0.95f, 0.95f, 0.95f);

                bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
                bodyStyle.normal.textColor = Color.white;

                smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
                smallStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

                statLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = FontStyle.Bold };
                statLabelStyle.normal.textColor = Color.white;

                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 13, fontStyle = FontStyle.Bold };

                activeTabStyle = new GUIStyle(buttonStyle);
                activeTabStyle.normal.textColor = Color.cyan;
            }
        }
    }
}
