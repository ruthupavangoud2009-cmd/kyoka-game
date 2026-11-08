using UnityEngine;
using UnityEngine.UI;
using Kyoka.Player;
using Kyoka.Abilities;
using Kyoka.Combat;
using Kyoka.Economy;
using Kyoka.Missions;
using Kyoka.World;
using Kyoka.Core;
using Kyoka.Save;
using Kyoka.Inventory;
using Kyoka.Creative;
using Kyoka.Art;
using Kyoka.Audio;

namespace Kyoka.UI
{
    /// <summary>
    /// Modern uGUI Open-World HUD for KYOKA 2.0.
    /// Provides Free Fire-style combat readouts, Minecraft-style 6-slot hotbar,
    /// dynamic health/stamina bars, quest tracker, building overlay, and pause menu.
    /// </summary>
    public class KyokaHUD : MonoBehaviour
    {
        public static KyokaHUD Instance { get; private set; }

        private KyokaPlayerController player;
        private KyokaHealth health;
        private KyokaAbilityUser abilityUser;
        private KyokaCombatSystem combatSystem;
        private KyokaInventory inventory;
        private KyokaCreativeBuilder builder;

        // UI Root Canvas & Components
        private GameObject canvasGO;
        private Image healthFill;
        private Text healthText;
        private Image energyFill;
        private Text energyText;
        private Text ammoText;
        private Text regionText;
        private Text questText;
        private Text buildModeText;
        private GameObject pauseMenuModal;
        private GameObject crosshairGO;
        private Image[] hotbarSlots = new Image[6];
        private Text[] hotbarLabels = new Text[6];
        private int currentSlotIndex = 0;

        private bool isPaused = false;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            BuildModernCanvasHUD();
        }

        private void Start()
        {
            FindPlayerReferences();
        }

        private void Update()
        {
            if (player == null) FindPlayerReferences();

            UpdateVitals();
            UpdateHotbarInput();
            UpdateCombatAndBuildingReadouts();

            // Toggle Pause Menu with Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePauseMenu();
            }
        }

        private void FindPlayerReferences()
        {
            player = FindObjectOfType<KyokaPlayerController>();
            if (player != null)
            {
                health = player.GetComponent<KyokaHealth>();
                abilityUser = player.GetComponent<KyokaAbilityUser>();
                combatSystem = player.GetComponent<KyokaCombatSystem>();
                inventory = player.GetComponent<KyokaInventory>();
                builder = player.GetComponent<KyokaCreativeBuilder>();
            }
        }

        private void BuildModernCanvasHUD()
        {
            canvasGO = new GameObject("Kyoka_Modern_HUD_Canvas");
            canvasGO.transform.SetParent(transform, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf") ?? Font.CreateDynamicFontFromOSFont("Arial", 14);

            // ================= TOP REGION BAR =================
            var topBar = CreateUIObject("TopBar", canvasGO.transform);
            var tbRect = topBar.GetComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0.35f, 0.93f);
            tbRect.anchorMax = new Vector2(0.65f, 0.99f);
            tbRect.offsetMin = Vector2.zero;
            tbRect.offsetMax = Vector2.zero;

            var tbImg = topBar.AddComponent<Image>();
            tbImg.color = new Color(0.06f, 0.10f, 0.16f, 0.85f);

            regionText = CreateText(topBar.transform, "KYOKA OPEN WORLD", font, 16, TextAnchor.MiddleCenter, Color.cyan);
            var regRect = regionText.GetComponent<RectTransform>();
            regRect.anchorMin = Vector2.zero; regRect.anchorMax = Vector2.one;
            regRect.offsetMin = Vector2.zero; regRect.offsetMax = Vector2.zero;

            // ================= RETICLE / CROSSHAIR =================
            crosshairGO = CreateUIObject("Crosshair", canvasGO.transform);
            var chRect = crosshairGO.GetComponent<RectTransform>();
            chRect.anchorMin = new Vector2(0.5f, 0.5f);
            chRect.anchorMax = new Vector2(0.5f, 0.5f);
            chRect.sizeDelta = new Vector2(12, 12);
            var chImg = crosshairGO.AddComponent<Image>();
            chImg.color = new Color(1f, 1f, 1f, 0.75f);

            // ================= BOTTOM-LEFT VITALS (HEALTH & ENERGY) =================
            var vitalsPanel = CreateUIObject("VitalsPanel", canvasGO.transform);
            var vpRect = vitalsPanel.GetComponent<RectTransform>();
            vpRect.anchorMin = new Vector2(0.03f, 0.04f);
            vpRect.anchorMax = new Vector2(0.24f, 0.13f);
            vpRect.offsetMin = Vector2.zero;
            vpRect.offsetMax = Vector2.zero;
            var vpImg = vitalsPanel.AddComponent<Image>();
            vpImg.color = new Color(0.06f, 0.10f, 0.16f, 0.85f);

            // Health Bar
            var hpBg = CreateUIObject("HP_Bg", vitalsPanel.transform);
            var hpBgRect = hpBg.GetComponent<RectTransform>();
            hpBgRect.anchorMin = new Vector2(0.06f, 0.55f);
            hpBgRect.anchorMax = new Vector2(0.94f, 0.88f);
            hpBgRect.offsetMin = Vector2.zero; hpBgRect.offsetMax = Vector2.zero;
            hpBg.AddComponent<Image>().color = new Color(0.18f, 0.08f, 0.08f, 0.9f);

            var hpFillGO = CreateUIObject("HP_Fill", hpBg.transform);
            var hpFillRect = hpFillGO.GetComponent<RectTransform>();
            hpFillRect.anchorMin = Vector2.zero; hpFillRect.anchorMax = Vector2.one;
            hpFillRect.offsetMin = Vector2.zero; hpFillRect.offsetMax = Vector2.zero;
            healthFill = hpFillGO.AddComponent<Image>();
            healthFill.color = new Color(0.95f, 0.22f, 0.22f);
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
            healthFill.fillAmount = 1.0f;

            healthText = CreateText(hpBg.transform, "HP: 100 / 100", font, 14, TextAnchor.MiddleCenter, Color.white);
            var htRect = healthText.GetComponent<RectTransform>();
            htRect.anchorMin = Vector2.zero; htRect.anchorMax = Vector2.one;
            htRect.offsetMin = Vector2.zero; htRect.offsetMax = Vector2.zero;

            // Energy / Stamina Bar
            var nrgBg = CreateUIObject("NRG_Bg", vitalsPanel.transform);
            var nrgBgRect = nrgBg.GetComponent<RectTransform>();
            nrgBgRect.anchorMin = new Vector2(0.06f, 0.12f);
            nrgBgRect.anchorMax = new Vector2(0.94f, 0.45f);
            nrgBgRect.offsetMin = Vector2.zero; nrgBgRect.offsetMax = Vector2.zero;
            nrgBg.AddComponent<Image>().color = new Color(0.18f, 0.15f, 0.05f, 0.9f);

            var nrgFillGO = CreateUIObject("NRG_Fill", nrgBg.transform);
            var nrgFillRect = nrgFillGO.GetComponent<RectTransform>();
            nrgFillRect.anchorMin = Vector2.zero; nrgFillRect.anchorMax = Vector2.one;
            nrgFillRect.offsetMin = Vector2.zero; nrgFillRect.offsetMax = Vector2.zero;
            energyFill = nrgFillGO.AddComponent<Image>();
            energyFill.color = new Color(0.95f, 0.78f, 0.15f);
            energyFill.type = Image.Type.Filled;
            energyFill.fillMethod = Image.FillMethod.Horizontal;
            energyFill.fillAmount = 1.0f;

            energyText = CreateText(nrgBg.transform, "ENERGY: 100", font, 13, TextAnchor.MiddleCenter, Color.white);
            var ntRect = energyText.GetComponent<RectTransform>();
            ntRect.anchorMin = Vector2.zero; ntRect.anchorMax = Vector2.one;
            ntRect.offsetMin = Vector2.zero; ntRect.offsetMax = Vector2.zero;

            // ================= BOTTOM-CENTER MINECRAFT-STYLE HOTBAR =================
            var hotbarPanel = CreateUIObject("HotbarPanel", canvasGO.transform);
            var hbRect = hotbarPanel.GetComponent<RectTransform>();
            hbRect.anchorMin = new Vector2(0.32f, 0.03f);
            hbRect.anchorMax = new Vector2(0.68f, 0.11f);
            hbRect.offsetMin = Vector2.zero;
            hbRect.offsetMax = Vector2.zero;

            string[] defaultSlotNames = new string[] { "1: Rifle", "2: Shotgun", "3: Sniper", "4: Katana", "5: Pickaxe", "6: Medkit" };
            for (int i = 0; i < 6; i++)
            {
                float xMin = (float)i / 6.0f + 0.01f;
                float xMax = (float)(i + 1) / 6.0f - 0.01f;

                var slotGO = CreateUIObject($"Slot_{i}", hotbarPanel.transform);
                var sRect = slotGO.GetComponent<RectTransform>();
                sRect.anchorMin = new Vector2(xMin, 0.05f);
                sRect.anchorMax = new Vector2(xMax, 0.95f);
                sRect.offsetMin = Vector2.zero; sRect.offsetMax = Vector2.zero;

                var sImg = slotGO.AddComponent<Image>();
                sImg.color = (i == 0) ? new Color(0.2f, 0.75f, 0.95f, 0.85f) : new Color(0.10f, 0.14f, 0.20f, 0.85f);
                hotbarSlots[i] = sImg;

                var lbl = CreateText(slotGO.transform, defaultSlotNames[i], font, 11, TextAnchor.MiddleCenter, Color.white);
                var lRect = lbl.GetComponent<RectTransform>();
                lRect.anchorMin = Vector2.zero; lRect.anchorMax = Vector2.one;
                lRect.offsetMin = Vector2.zero; lRect.offsetMax = Vector2.zero;
                hotbarLabels[i] = lbl;
            }

            // ================= BOTTOM-RIGHT COMBAT / BUILDING READOUT =================
            var actionPanel = CreateUIObject("ActionPanel", canvasGO.transform);
            var apRect = actionPanel.GetComponent<RectTransform>();
            apRect.anchorMin = new Vector2(0.74f, 0.04f);
            apRect.anchorMax = new Vector2(0.97f, 0.13f);
            apRect.offsetMin = Vector2.zero;
            apRect.offsetMax = Vector2.zero;
            var apImg = actionPanel.AddComponent<Image>();
            apImg.color = new Color(0.06f, 0.10f, 0.16f, 0.85f);

            ammoText = CreateText(actionPanel.transform, "AMMO: 30 / 120\n[L-Click] Attack  [R-Click] ADS", font, 13, TextAnchor.MiddleCenter, new Color(0.95f, 0.75f, 0.15f));
            var atRect = ammoText.GetComponent<RectTransform>();
            atRect.anchorMin = Vector2.zero; atRect.anchorMax = Vector2.one;
            atRect.offsetMin = Vector2.zero; atRect.offsetMax = Vector2.zero;

            // ================= BUILDING MODE NOTIFICATION BANNER =================
            var buildBanner = CreateUIObject("BuildBanner", canvasGO.transform);
            var bbRect = buildBanner.GetComponent<RectTransform>();
            bbRect.anchorMin = new Vector2(0.25f, 0.13f);
            bbRect.anchorMax = new Vector2(0.75f, 0.18f);
            bbRect.offsetMin = Vector2.zero; bbRect.offsetMax = Vector2.zero;
            buildModeText = CreateText(buildBanner.transform, "[B] BUILD MODE: Press 'B' to toggle base construction | [R] Rotate piece", font, 14, TextAnchor.MiddleCenter, new Color(0.2f, 0.85f, 0.45f));
            var bbtRect = buildModeText.GetComponent<RectTransform>();
            bbtRect.anchorMin = Vector2.zero; bbtRect.anchorMax = Vector2.one;
            bbtRect.offsetMin = Vector2.zero; bbtRect.offsetMax = Vector2.zero;

            // ================= TOP-RIGHT QUEST TRACKER =================
            var questPanel = CreateUIObject("QuestPanel", canvasGO.transform);
            var qpRect = questPanel.GetComponent<RectTransform>();
            qpRect.anchorMin = new Vector2(0.74f, 0.78f);
            qpRect.anchorMax = new Vector2(0.97f, 0.95f);
            qpRect.offsetMin = Vector2.zero; qpRect.offsetMax = Vector2.zero;
            var qpImg = questPanel.AddComponent<Image>();
            qpImg.color = new Color(0.06f, 0.10f, 0.16f, 0.85f);

            questText = CreateText(questPanel.transform, "QUEST: The Aether Protocol\n• Explore Wildlands\n• Mine 5x Stone & Chop 5x Wood\n• Press [B] to build an Outpost", font, 12, TextAnchor.MiddleLeft, Color.white);
            var qtRect = questText.GetComponent<RectTransform>();
            qtRect.anchorMin = new Vector2(0.08f, 0.08f); qtRect.anchorMax = new Vector2(0.92f, 0.92f);
            qtRect.offsetMin = Vector2.zero; qtRect.offsetMax = Vector2.zero;

            // Build Pause Menu Modal
            BuildPauseMenu(font);
        }

        private void BuildPauseMenu(Font font)
        {
            pauseMenuModal = CreateUIObject("PauseMenuModal", canvasGO.transform);
            var pmRect = pauseMenuModal.GetComponent<RectTransform>();
            pmRect.anchorMin = new Vector2(0.35f, 0.25f);
            pmRect.anchorMax = new Vector2(0.65f, 0.75f);
            pmRect.offsetMin = Vector2.zero; pmRect.offsetMax = Vector2.zero;

            var pmImg = pauseMenuModal.AddComponent<Image>();
            pmImg.color = new Color(0.04f, 0.07f, 0.12f, 0.95f);

            var title = CreateText(pauseMenuModal.transform, "GAME PAUSED", font, 24, TextAnchor.MiddleCenter, Color.cyan);
            var tRect = title.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.1f, 0.82f); tRect.anchorMax = new Vector2(0.9f, 0.95f);
            tRect.offsetMin = Vector2.zero; tRect.offsetMax = Vector2.zero;

            // Resume Button
            CreateMenuButton("Btn_Resume", pauseMenuModal.transform, "RESUME", font, 16, new Vector2(0.15f, 0.60f), new Vector2(0.85f, 0.74f), () =>
            {
                TogglePauseMenu();
            });

            // Save Game Button
            CreateMenuButton("Btn_Save", pauseMenuModal.transform, "SAVE GAME", font, 16, new Vector2(0.15f, 0.42f), new Vector2(0.85f, 0.56f), () =>
            {
                SaveCurrentGame();
            });

            // Return to Lobby Button
            CreateMenuButton("Btn_Lobby", pauseMenuModal.transform, "RETURN TO LOBBY", font, 16, new Vector2(0.15f, 0.24f), new Vector2(0.85f, 0.38f), () =>
            {
                ReturnToLobby();
            });

            pauseMenuModal.SetActive(false);
        }

        private void TogglePauseMenu()
        {
            isPaused = !isPaused;
            pauseMenuModal.SetActive(isPaused);
            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
        }

        private void SaveCurrentGame()
        {
            var save = KyokaSaveSystem.Load() ?? new KyokaSaveData();
            if (player != null)
            {
                save.posX = player.transform.position.x;
                save.posY = player.transform.position.y;
                save.posZ = player.transform.position.z;
                save.rotY = player.transform.eulerAngles.y;
                save.hasSavedPosition = true;
            }
            KyokaSaveSystem.Save(save);
            KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.LootPickup);
            Debug.Log("[KYOKA Save] Game state saved successfully!");
        }

        private void ReturnToLobby()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var controller = KyokaMasterGameController.Instance;
            if (controller != null)
            {
                controller.ShowMainLobby();
            }
            Destroy(transform.root.gameObject);
        }

        private void UpdateVitals()
        {
            if (health != null && healthFill != null)
            {
                float fill = Mathf.Clamp01((float)health.currentHealth / Mathf.Max(1, health.maxHealth));
                healthFill.fillAmount = fill;
                healthText.text = $"HP: {health.currentHealth} / {health.maxHealth}";
            }

            if (abilityUser != null && energyFill != null)
            {
                float fill = Mathf.Clamp01(abilityUser.energy / Mathf.Max(1, abilityUser.maxEnergy));
                energyFill.fillAmount = fill;
                energyText.text = $"ENERGY: {Mathf.RoundToInt(abilityUser.energy)} / {Mathf.RoundToInt(abilityUser.maxEnergy)}";
            }
        }

        private void UpdateHotbarInput()
        {
            for (int i = 0; i < 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectHotbarSlot(i);
                }
            }
        }

        private void SelectHotbarSlot(int slot)
        {
            currentSlotIndex = slot;
            for (int i = 0; i < 6; i++)
            {
                hotbarSlots[i].color = (i == currentSlotIndex) ? new Color(0.2f, 0.75f, 0.95f, 0.85f) : new Color(0.10f, 0.14f, 0.20f, 0.85f);
            }

            if (player != null)
            {
                switch (slot)
                {
                    case 0: player.SwitchWeapon("AssaultRifle"); break;
                    case 1: player.SwitchWeapon("Shotgun"); break;
                    case 2: player.SwitchWeapon("Sniper"); break;
                    case 3: player.SwitchWeapon("Katana"); break;
                    case 4: player.SwitchWeapon("Pickaxe"); break;
                    case 5:
                        if (health != null) health.Heal(25);
                        KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.LootPickup);
                        break;
                }
            }
        }

        private void UpdateCombatAndBuildingReadouts()
        {
            if (builder != null && builder.isBuildMode)
            {
                buildModeText.text = $"[B] BUILD MODE ACTIVE: {builder.selectedPiece} | [R] Rotate 90° | [Tab] Next Piece | [L-Click] Place | [R-Click] Remove";
                buildModeText.color = new Color(0.2f, 0.95f, 0.45f);
            }
            else
            {
                buildModeText.text = "[B] BUILD MODE: Press 'B' to toggle base construction | [R] Rotate piece";
                buildModeText.color = new Color(0.6f, 0.75f, 0.85f);
            }

            if (combatSystem != null && ammoText != null)
            {
                ammoText.text = $"AMMO: {combatSystem.currentAmmo} / 120\n[L-Click] Attack  [R-Click] ADS";
            }
        }

        private GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            if (go.GetComponent<RectTransform>() == null) go.AddComponent<RectTransform>();
            return go;
        }

        private Text CreateText(Transform parent, string content, Font font, int size, TextAnchor anchor, Color color)
        {
            var go = CreateUIObject("Text", parent);
            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = font;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            return text;
        }

        private void CreateMenuButton(string name, Transform parent, string label, Font font, int size, Vector2 anchorMin, Vector2 anchorMax, System.Action onClick)
        {
            var btnGO = CreateUIObject(name, parent);
            var rect = btnGO.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;

            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.14f, 0.20f, 0.30f);

            var btn = btnGO.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            var txt = CreateText(btnGO.transform, label, font, size, TextAnchor.MiddleCenter, Color.white);
            var tRect = txt.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero; tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero; tRect.offsetMax = Vector2.zero;
        }
    }
}
