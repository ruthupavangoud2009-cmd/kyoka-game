using UnityEngine;
using UnityEngine.UI;
using Kyoka.Character;
using Kyoka.Save;
using Kyoka.Audio;
using Kyoka.Art;
using System;

namespace Kyoka.UI
{
    public enum LobbyGameMode
    {
        BattleRoyale = 0,
        OpenWorldCity = 1,
        AnimeRealm = 2
    }

    /// <summary>
    /// Free Fire-style AAA 3D Main Lobby for KYOKA.
    /// Features a 3D animated hangar, live customized player character with equipped weapon,
    /// top currency/level bar, game mode selector, Wardrobe, Armory, Vehicles, Shop,
    /// and a prominent glowing START button that launches real gameplay.
    /// </summary>
    public class KyokaLobbyUI : MonoBehaviour
    {
        public static KyokaLobbyUI Instance { get; private set; }

        public event Action<LobbyGameMode> OnStartGameRequested;

        [Header("3D Lobby Stage")]
        private GameObject lobbyStageRoot;
        private Camera lobbyCamera;
        private KyokaCharacterModel lobbyCharacter;
        private Transform stagePedestal;

        [Header("Active State")]
        public LobbyGameMode selectedMode = LobbyGameMode.BattleRoyale;
        public string selectedTeam = "SOLO";
        private KyokaSaveData activeSave;

        [Header("UI Containers")]
        private GameObject canvasGO;
        private Text playerNameText;
        private Text playerLevelText;
        private Text coinsText;
        private Text diamondsText;
        private Text modeTitleText;
        private Text modeDescText;
        private GameObject activeModal = null;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            LoadSaveData();
            Build3DStage();
            BuildLobbyHUD();

            KyokaAudioManager.Instance?.PlayLobbyMusic();
        }

        private void LoadSaveData()
        {
            activeSave = KyokaSaveSystem.Load();
            if (activeSave == null)
            {
                activeSave = new KyokaSaveData();
                KyokaSaveSystem.Save(activeSave);
            }
        }

        private void Build3DStage()
        {
            lobbyStageRoot = new GameObject("Lobby_3D_Stage");
            lobbyStageRoot.transform.SetParent(transform, false);

            // Lobby Camera
            var camGO = new GameObject("Lobby_Camera");
            camGO.transform.SetParent(lobbyStageRoot.transform, false);
            lobbyCamera = camGO.AddComponent<Camera>();
            lobbyCamera.clearFlags = CameraClearFlags.SolidColor;
            lobbyCamera.backgroundColor = new Color(0.04f, 0.06f, 0.09f);
            lobbyCamera.fieldOfView = 50f;
            camGO.AddComponent<AudioListener>();

            camGO.transform.position = new Vector3(-0.35f, 1.25f, -3.2f);
            camGO.transform.LookAt(new Vector3(-0.1f, 1.05f, 0));

            // Lighting Setup
            var keyGO = new GameObject("Lobby_KeyLight");
            keyGO.transform.SetParent(lobbyStageRoot.transform, false);
            var keyLight = keyGO.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.35f;
            keyLight.color = new Color(1.0f, 0.96f, 0.90f);
            keyGO.transform.rotation = Quaternion.Euler(30, -35, 0);

            var rimGO = new GameObject("Lobby_CyanRim");
            rimGO.transform.SetParent(lobbyStageRoot.transform, false);
            var rimLight = rimGO.AddComponent<Light>();
            rimLight.type = LightType.Directional;
            rimLight.intensity = 1.1f;
            rimLight.color = new Color(0.12f, 0.85f, 0.95f);
            rimGO.transform.rotation = Quaternion.Euler(15, 140, 0);

            // Cyber Hangar Floor
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Hangar_Floor";
            floor.transform.SetParent(lobbyStageRoot.transform, false);
            floor.transform.position = new Vector3(0, -0.1f, 0);
            floor.transform.localScale = new Vector3(14, 0.2f, 14);
            Destroy(floor.GetComponent<Collider>());

            var floorMat = KyokaMaterialFactory.CreateLit(new Color(0.16f, 0.18f, 0.22f), KyokaTextureGenerator.GetCyberGridTexture());
            floorMat.mainTextureScale = new Vector2(4, 4);
            floor.GetComponent<Renderer>().material = floorMat;

            // Background Neon Light Pillars
            for (int i = -3; i <= 3; i++)
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.transform.SetParent(lobbyStageRoot.transform, false);
                pillar.transform.position = new Vector3(i * 1.8f, 3.0f, 4.5f);
                pillar.transform.localScale = new Vector3(0.08f, 3.2f, 0.08f);
                Destroy(pillar.GetComponent<Collider>());

                Color pColor = (Mathf.Abs(i) % 2 == 0) ? new Color(0.12f, 0.85f, 0.95f) : new Color(0.95f, 0.72f, 0.15f);
                pillar.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateUnlit(pColor);
            }

            // Spawn Customized Player in Center of Hangar
            var charGO = new GameObject("Lobby_Character");
            charGO.transform.SetParent(lobbyStageRoot.transform, false);
            charGO.transform.position = new Vector3(-0.2f, 0, 0);
            charGO.transform.rotation = Quaternion.Euler(0, 165f, 0);

            lobbyCharacter = charGO.AddComponent<KyokaCharacterModel>();
            charGO.AddComponent<KyokaProceduralAnimator>();

            RefreshCharacterVisuals();
        }

        public void RefreshCharacterVisuals()
        {
            if (lobbyCharacter == null) return;
            LoadSaveData();

            var c = activeSave.customization;
            lobbyCharacter.isFemale = c.isFemale;
            lobbyCharacter.BuildHumanoidMesh();
            lobbyCharacter.ApplyCustomization(
                (HairStyleType)c.hairStyle,
                new Color(c.hairColorR, c.hairColorG, c.hairColorB),
                new Color(c.skinToneR, c.skinToneG, c.skinToneB),
                new Color(c.eyeColorR, c.eyeColorG, c.eyeColorB),
                (OutfitPreset)c.outfitPreset
            );

            string weapon = string.IsNullOrEmpty(c.equippedWeaponId) ? "AssaultRifle" : c.equippedWeaponId;
            lobbyCharacter.EquipWeapon(weapon);
        }

        private void BuildLobbyHUD()
        {
            canvasGO = new GameObject("Lobby_Canvas");
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

            // ================= TOP HEADER BAR =================
            var topBar = CreateUIObject("TopBar", canvasGO.transform);
            var tbRect = topBar.GetComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0, 0.90f);
            tbRect.anchorMax = new Vector2(1, 1.0f);
            tbRect.offsetMin = Vector2.zero;
            tbRect.offsetMax = Vector2.zero;

            var tbImg = topBar.AddComponent<Image>();
            tbImg.color = new Color(0.04f, 0.06f, 0.09f, 0.85f);

            // Profile Card (Left)
            var profileBox = CreateUIObject("ProfileBox", topBar.transform);
            var pbRect = profileBox.GetComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(0.02f, 0.15f);
            pbRect.anchorMax = new Vector2(0.28f, 0.85f);
            pbRect.offsetMin = Vector2.zero;
            pbRect.offsetMax = Vector2.zero;

            var pbImg = profileBox.AddComponent<Image>();
            pbImg.sprite = KyokaTextureGenerator.GetPanelSprite();
            pbImg.type = Image.Type.Sliced;
            pbImg.color = new Color(0.10f, 0.14f, 0.22f);

            playerNameText = CreateLabel("PlayerName", profileBox.transform, activeSave.customization.playerName, font, 20, TextAnchor.MiddleLeft, Color.white);
            playerNameText.fontStyle = FontStyle.Bold;
            playerNameText.rectTransform.anchoredPosition = new Vector2(20, 12);

            playerLevelText = CreateLabel("PlayerLevel", profileBox.transform, $"LEVEL {activeSave.playerLevel}  •  {activeSave.currentXP}/2000 EXP", font, 13, TextAnchor.MiddleLeft, new Color(0.85f, 0.70f, 0.25f));
            playerLevelText.rectTransform.anchoredPosition = new Vector2(20, -14);

            // Rank Badge (Center)
            var rankLabel = CreateLabel("RankLabel", topBar.transform, "★ GOLD II  •  SEASON 1", font, 18, TextAnchor.MiddleCenter, new Color(0.95f, 0.75f, 0.20f));
            rankLabel.fontStyle = FontStyle.Bold;
            var rlRect = rankLabel.GetComponent<RectTransform>();
            rlRect.anchorMin = new Vector2(0.40f, 0);
            rlRect.anchorMax = new Vector2(0.60f, 1);
            rlRect.offsetMin = Vector2.zero;
            rlRect.offsetMax = Vector2.zero;

            // Currencies (Right)
            var curBox = CreateUIObject("CurrencyBox", topBar.transform);
            var cbRect = curBox.GetComponent<RectTransform>();
            cbRect.anchorMin = new Vector2(0.70f, 0.15f);
            cbRect.anchorMax = new Vector2(0.98f, 0.85f);
            cbRect.offsetMin = Vector2.zero;
            cbRect.offsetMax = Vector2.zero;

            coinsText = CreateLabel("Coins", curBox.transform, $"🪙 {activeSave.kyokaCredits}", font, 18, TextAnchor.MiddleRight, new Color(1.0f, 0.85f, 0.25f));
            coinsText.fontStyle = FontStyle.Bold;
            coinsText.rectTransform.anchorMin = new Vector2(0, 0);
            coinsText.rectTransform.anchorMax = new Vector2(0.48f, 1);
            coinsText.rectTransform.offsetMin = Vector2.zero;
            coinsText.rectTransform.offsetMax = Vector2.zero;

            diamondsText = CreateLabel("Diamonds", curBox.transform, $"💎 {activeSave.aetherShards}", font, 18, TextAnchor.MiddleRight, new Color(0.2f, 0.85f, 1.0f));
            diamondsText.fontStyle = FontStyle.Bold;
            diamondsText.rectTransform.anchorMin = new Vector2(0.52f, 0);
            diamondsText.rectTransform.anchorMax = new Vector2(1.0f, 1);
            diamondsText.rectTransform.offsetMin = Vector2.zero;
            diamondsText.rectTransform.offsetMax = Vector2.zero;

            // ================= RIGHT GAME MODE SELECTION & START BUTTON =================
            var modeCard = CreateUIObject("ModeCard", canvasGO.transform);
            var mcRect = modeCard.GetComponent<RectTransform>();
            mcRect.anchorMin = new Vector2(0.72f, 0.22f);
            mcRect.anchorMax = new Vector2(0.97f, 0.44f);
            mcRect.offsetMin = Vector2.zero;
            mcRect.offsetMax = Vector2.zero;

            var mcImg = modeCard.AddComponent<Image>();
            mcImg.sprite = KyokaTextureGenerator.GetPanelSprite();
            mcImg.type = Image.Type.Sliced;
            mcImg.color = new Color(0.08f, 0.12f, 0.18f, 0.92f);

            modeTitleText = CreateLabel("ModeTitle", modeCard.transform, "BATTLE ROYALE", font, 24, TextAnchor.MiddleLeft, new Color(0.95f, 0.72f, 0.15f));
            modeTitleText.fontStyle = FontStyle.Bold;
            modeTitleText.rectTransform.anchoredPosition = new Vector2(25, 42);

            modeDescText = CreateLabel("ModeDesc", modeCard.transform, "Battle Island • 15 Survivors\nAether Storm • Tactical Loot Drops", font, 14, TextAnchor.MiddleLeft, new Color(0.75f, 0.85f, 0.95f));
            modeDescText.rectTransform.anchoredPosition = new Vector2(25, -2);

            CreateButton("SwitchModeBtn", modeCard.transform, "CHANGE MODE ❯", font, 13, new Vector2(0.58f, 0.12f), new Vector2(0.95f, 0.38f), OpenModeSelectorModal, new Color(0.18f, 0.25f, 0.38f));

            // START GAME BUTTON (Prominent Glowing Gold Button)
            var startBtn = CreateButton("StartBtn", canvasGO.transform, "▶   START GAME", font, 28, new Vector2(0.72f, 0.08f), new Vector2(0.97f, 0.19f), OnStartClicked, new Color(0.95f, 0.72f, 0.15f));
            var sText = startBtn.GetComponentInChildren<Text>();
            sText.color = new Color(0.08f, 0.08f, 0.12f);
            sText.fontStyle = FontStyle.Bold;

            // ================= LEFT / BOTTOM NAVIGATION TABS =================
            string[] tabs = new string[] { "WARDROBE", "ARMORY", "VEHICLES", "SHOP", "SETTINGS" };
            for (int i = 0; i < tabs.Length; i++)
            {
                string tabName = tabs[i];
                float yMin = 0.65f - (i * 0.09f);
                float yMax = yMin + 0.075f;

                CreateButton($"Tab_{tabName}", canvasGO.transform, $"◧  {tabName}", font, 16, new Vector2(0.03f, yMin), new Vector2(0.18f, yMax), () =>
                {
                    OpenTabModal(tabName);
                }, new Color(0.10f, 0.14f, 0.22f, 0.90f));
            }
        }

        private void OnStartClicked()
        {
            KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
            OnStartGameRequested?.Invoke(selectedMode);
            Destroy(gameObject);
        }

        private void OpenModeSelectorModal()
        {
            CloseModal();
            activeModal = CreateModalWindow("SELECT GAME MODE");
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Mode 1: Battle Royale
            CreateButton("BR_Mode", activeModal.transform, "⚔  BATTLE ROYALE\nBattle Island • 15 Players • Aether Storm", font, 16, new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.82f), () =>
            {
                selectedMode = LobbyGameMode.BattleRoyale;
                modeTitleText.text = "BATTLE ROYALE";
                modeDescText.text = "Battle Island • 15 Survivors\nAether Storm • Tactical Loot Drops";
                CloseModal();
            }, (selectedMode == LobbyGameMode.BattleRoyale) ? new Color(0.95f, 0.72f, 0.15f) : new Color(0.15f, 0.20f, 0.30f));

            // Mode 2: Open World City
            CreateButton("OW_Mode", activeModal.transform, "🏙  OPEN WORLD CITY\nKYOKA City • Drivable Vehicles • Shops • Wanted Levels", font, 16, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.58f), () =>
            {
                selectedMode = LobbyGameMode.OpenWorldCity;
                modeTitleText.text = "OPEN WORLD CITY";
                modeDescText.text = "KYOKA City • Drivable Vehicles\nShops & Missions • Police Wanted System";
                CloseModal();
            }, (selectedMode == LobbyGameMode.OpenWorldCity) ? new Color(0.95f, 0.72f, 0.15f) : new Color(0.15f, 0.20f, 0.30f));

            // Mode 3: Anime Realm
            CreateButton("Anime_Mode", activeModal.transform, "⚡  ANIME REALM\nElemental Powers • Boss Battles • Transformation Quests", font, 16, new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.34f), () =>
            {
                selectedMode = LobbyGameMode.AnimeRealm;
                modeTitleText.text = "ANIME REALM";
                modeDescText.text = "Elemental Powers & Transformations\nAncient Shrines & Colossal Bosses";
                CloseModal();
            }, (selectedMode == LobbyGameMode.AnimeRealm) ? new Color(0.95f, 0.72f, 0.15f) : new Color(0.15f, 0.20f, 0.30f));
        }

        private void OpenTabModal(string tabName)
        {
            CloseModal();
            activeModal = CreateModalWindow(tabName);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            switch (tabName)
            {
                case "WARDROBE":
                    CreateLabel("W_Info", activeModal.transform, "SELECT OUTFIT PRESET FOR YOUR CHARACTER", font, 15, TextAnchor.MiddleCenter, Color.white)
                        .rectTransform.anchoredPosition = new Vector2(0, 110);

                    string[] outfits = new string[] { "Urban Stalker", "Cyber Ronin", "Desert Commando", "Aether Vanguard" };
                    for (int i = 0; i < 4; i++)
                    {
                        int oIdx = i;
                        float y = 0.60f - (i * 0.12f);
                        CreateButton($"O_{i}", activeModal.transform, outfits[i], font, 16, new Vector2(0.15f, y), new Vector2(0.85f, y + 0.10f), () =>
                        {
                            activeSave.customization.outfitPreset = oIdx;
                            KyokaSaveSystem.Save(activeSave);
                            RefreshCharacterVisuals();
                            KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
                        }, new Color(0.18f, 0.25f, 0.35f));
                    }
                    break;

                case "ARMORY":
                    CreateLabel("A_Info", activeModal.transform, "EQUIP ACTIVE PRIMARY WEAPON", font, 15, TextAnchor.MiddleCenter, Color.white)
                        .rectTransform.anchoredPosition = new Vector2(0, 110);

                    string[] weps = new string[] { "AssaultRifle", "Katana", "Shotgun" };
                    string[] wepLabels = new string[] { "Assault Rifle (Rapid DPS)", "High-Frequency Katana (Melee Slash)", "Tactical Shotgun (Close Blast)" };
                    for (int i = 0; i < weps.Length; i++)
                    {
                        string wId = weps[i];
                        float y = 0.55f - (i * 0.14f);
                        CreateButton($"W_{wId}", activeModal.transform, wepLabels[i], font, 16, new Vector2(0.12f, y), new Vector2(0.88f, y + 0.11f), () =>
                        {
                            activeSave.customization.equippedWeaponId = wId;
                            KyokaSaveSystem.Save(activeSave);
                            RefreshCharacterVisuals();
                            KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
                        }, new Color(0.18f, 0.25f, 0.35f));
                    }
                    break;

                case "VEHICLES":
                    CreateLabel("V_Info", activeModal.transform, "GARAGE SHOWCASE", font, 18, TextAnchor.MiddleCenter, Color.white)
                        .rectTransform.anchoredPosition = new Vector2(0, 100);
                    CreateLabel("V_1", activeModal.transform, "🏎  Cyber Cruiser (Top Speed: 180 km/h)  [READY IN CITY]", font, 15, TextAnchor.MiddleCenter, new Color(0.2f, 0.85f, 0.95f))
                        .rectTransform.anchoredPosition = new Vector2(0, 40);
                    CreateLabel("V_2", activeModal.transform, "🚙  Tactical Buggy (Off-road Suspension)  [READY IN WILDLANDS]", font, 15, TextAnchor.MiddleCenter, new Color(0.85f, 0.70f, 0.25f))
                        .rectTransform.anchoredPosition = new Vector2(0, -20);
                    break;

                case "SHOP":
                    CreateLabel("S_Info", activeModal.transform, "KYOKA IN-GAME REWARD SHOP", font, 18, TextAnchor.MiddleCenter, Color.white)
                        .rectTransform.anchoredPosition = new Vector2(0, 100);

                    CreateButton("BuyPack", activeModal.transform, "🎁  CLAIM DAILY SUPPLY CRATE (+250 COINS)", font, 16, new Vector2(0.15f, 0.40f), new Vector2(0.85f, 0.55f), () =>
                    {
                        activeSave.kyokaCredits += 250;
                        KyokaSaveSystem.Save(activeSave);
                        coinsText.text = $"🪙 {activeSave.kyokaCredits}";
                        KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.RewardChime);
                    }, new Color(0.85f, 0.65f, 0.15f));
                    break;

                case "SETTINGS":
                    CreateLabel("Set_G", activeModal.transform, "GRAPHICS PRESET", font, 15, TextAnchor.MiddleLeft, new Color(0.85f, 0.70f, 0.25f))
                        .rectTransform.anchoredPosition = new Vector2(-180, 80);

                    CreateButton("LowQ", activeModal.transform, "LOW", font, 14, new Vector2(0.10f, 0.55f), new Vector2(0.35f, 0.65f), () => QualitySettings.SetQualityLevel(0), new Color(0.2f, 0.3f, 0.4f));
                    CreateButton("MedQ", activeModal.transform, "MEDIUM", font, 14, new Vector2(0.38f, 0.55f), new Vector2(0.63f, 0.65f), () => QualitySettings.SetQualityLevel(1), new Color(0.2f, 0.3f, 0.4f));
                    CreateButton("HighQ", activeModal.transform, "ULTRA 60FPS", font, 14, new Vector2(0.66f, 0.55f), new Vector2(0.90f, 0.65f), () => QualitySettings.SetQualityLevel(2), new Color(0.95f, 0.72f, 0.15f));

                    CreateButton("FullScr", activeModal.transform, "TOGGLE FULLSCREEN", font, 15, new Vector2(0.15f, 0.25f), new Vector2(0.85f, 0.38f), () =>
                    {
                        Screen.fullScreen = !Screen.fullScreen;
                    }, new Color(0.18f, 0.25f, 0.35f));
                    break;
            }
        }

        private GameObject CreateModalWindow(string title)
        {
            var modal = CreateUIObject("ModalWindow", canvasGO.transform);
            var mRect = modal.GetComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0.25f, 0.15f);
            mRect.anchorMax = new Vector2(0.75f, 0.85f);
            mRect.offsetMin = Vector2.zero;
            mRect.offsetMax = Vector2.zero;

            var mImg = modal.AddComponent<Image>();
            mImg.sprite = KyokaTextureGenerator.GetPanelSprite();
            mImg.type = Image.Type.Sliced;
            mImg.color = new Color(0.06f, 0.08f, 0.14f, 0.96f);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Modal Title
            var titleLbl = CreateLabel("Title", modal.transform, title, font, 22, TextAnchor.MiddleCenter, new Color(0.12f, 0.85f, 0.95f));
            titleLbl.fontStyle = FontStyle.Bold;
            var tRect = titleLbl.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.1f, 0.88f);
            tRect.anchorMax = new Vector2(0.9f, 0.98f);
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;

            // Close button (X)
            CreateButton("CloseX", modal.transform, "✕", font, 18, new Vector2(0.90f, 0.90f), new Vector2(0.97f, 0.97f), CloseModal, new Color(0.85f, 0.22f, 0.28f));

            return modal;
        }

        private void CloseModal()
        {
            if (activeModal != null)
            {
                Destroy(activeModal);
                activeModal = null;
                KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
            }
        }

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
            rt.sizeDelta = new Vector2(400, 30);
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
    }
}

