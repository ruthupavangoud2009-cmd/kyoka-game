using UnityEngine;
using UnityEngine.UI;
using Kyoka.Character;
using Kyoka.Save;
using Kyoka.Audio;
using Kyoka.Art;
using System;

namespace Kyoka.UI
{
    /// <summary>
    /// Interactive 3D Character Customization Studio for KYOKA.
    /// Features a 360-degree rotating character preview pedestal, camera zoom,
    /// hairstyle selector, vibrant hair & skin color palettes, outfit presets,
    /// and name generator. Saves directly to persistent storage.
    /// </summary>
    public class KyokaCharacterCustomizerUI : MonoBehaviour
    {
        public static KyokaCharacterCustomizerUI Instance { get; private set; }

        public event Action OnCustomizationComplete;

        [Header("3D Studio Elements")]
        private GameObject studioRoot;
        private Camera studioCamera;
        private KyokaCharacterModel characterModel;
        private Transform pedestalTransform;

        [Header("Camera Control")]
        private float currentYaw = 180f;
        private float currentZoom = 2.6f;
        private Vector3 cameraTarget = new Vector3(0, 1.1f, 0);

        [Header("UI Canvas")]
        private GameObject canvasGO;
        private InputField nameInputField;
        private Text nameDisplay;

        // Customization Working Copy
        private string charName = "Kaito";
        private bool isFemale = false;
        private HairStyleType hairStyle = HairStyleType.SpikyAnime;
        private Color hairColor = new Color(0.12f, 0.75f, 0.95f);
        private Color skinColor = new Color(0.96f, 0.82f, 0.73f);
        private Color eyeColor = new Color(0.1f, 0.85f, 0.7f);
        private OutfitPreset outfit = OutfitPreset.UrbanStalker;

        private readonly string[] randomNames = new string[]
        {
            "AetherKaito", "ShadowRen", "ViperBlade", "NeonScout",
            "ZeroStriker", "Kurogane", "BlazeApex", "CyberVanguard",
            "SolRonin", "StormRider", "PhantomAce", "NovaStrike"
        };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            LoadExistingData();
            Build3DStudio();
            BuildUI();
        }

        private void LoadExistingData()
        {
            var save = KyokaSaveSystem.Load();
            if (save != null && save.customization != null)
            {
                var c = save.customization;
                charName = string.IsNullOrEmpty(c.playerName) ? "Kaito" : c.playerName;
                isFemale = c.isFemale;
                hairStyle = (HairStyleType)Mathf.Clamp(c.hairStyle, 0, 3);
                hairColor = new Color(c.hairColorR, c.hairColorG, c.hairColorB);
                skinColor = new Color(c.skinToneR, c.skinToneG, c.skinToneB);
                eyeColor = new Color(c.eyeColorR, c.eyeColorG, c.eyeColorB);
                outfit = (OutfitPreset)Mathf.Clamp(c.outfitPreset, 0, 3);
            }
        }

        private void Build3DStudio()
        {
            studioRoot = new GameObject("Studio_3D_Root");
            studioRoot.transform.SetParent(transform, false);

            // Dedicated Studio Camera
            var camGO = new GameObject("Studio_Camera");
            camGO.transform.SetParent(studioRoot.transform, false);
            studioCamera = camGO.AddComponent<Camera>();
            studioCamera.clearFlags = CameraClearFlags.SolidColor;
            studioCamera.backgroundColor = new Color(0.05f, 0.07f, 0.10f);
            studioCamera.fieldOfView = 45f;
            camGO.AddComponent<AudioListener>();

            // Studio Lighting
            var sunGO = new GameObject("Studio_KeyLight");
            sunGO.transform.SetParent(studioRoot.transform, false);
            var keyLight = sunGO.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.3f;
            keyLight.color = new Color(0.95f, 0.98f, 1.0f);
            sunGO.transform.rotation = Quaternion.Euler(25, -35, 0);

            var rimGO = new GameObject("Studio_RimLight");
            rimGO.transform.SetParent(studioRoot.transform, false);
            var rimLight = rimGO.AddComponent<Light>();
            rimLight.type = LightType.Directional;
            rimLight.intensity = 0.9f;
            rimLight.color = new Color(0.12f, 0.85f, 0.95f); // Cyan rim light
            rimGO.transform.rotation = Quaternion.Euler(15, 145, 0);

            // Rotating Pedestal
            var pedGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedGO.name = "Pedestal";
            pedGO.transform.SetParent(studioRoot.transform, false);
            pedGO.transform.position = new Vector3(0, -0.05f, 0);
            pedGO.transform.localScale = new Vector3(1.6f, 0.10f, 1.6f);
            pedestalTransform = pedGO.transform;
            Destroy(pedGO.GetComponent<Collider>());

            var pedMat = KyokaMaterialFactory.CreateLit(new Color(0.18f, 0.22f, 0.28f), KyokaTextureGenerator.GetCyberGridTexture());
            pedGO.GetComponent<Renderer>().material = pedMat;

            // Character Model
            var charGO = new GameObject("Customizer_Character");
            charGO.transform.SetParent(studioRoot.transform, false);
            characterModel = charGO.AddComponent<KyokaCharacterModel>();
            charGO.AddComponent<KyokaProceduralAnimator>();

            UpdateCharacterModel();
            UpdateCamera();
        }

        private void UpdateCharacterModel()
        {
            if (characterModel == null) return;
            characterModel.isFemale = isFemale;
            characterModel.BuildHumanoidMesh();
            characterModel.ApplyCustomization(hairStyle, hairColor, skinColor, eyeColor, outfit);
        }

        private void Update()
        {
            // Drag to rotate character
            if (Input.GetMouseButton(0))
            {
                // Only rotate if not over right panel (mouse x < 65% of screen)
                if (Input.mousePosition.x < Screen.width * 0.65f)
                {
                    float rot = Input.GetAxis("Mouse X") * 6.0f;
                    currentYaw -= rot;
                }
            }

            // Scroll to zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                currentZoom = Mathf.Clamp(currentZoom - scroll * 2.0f, 1.4f, 3.5f);
            }

            UpdateCamera();
        }

        private void UpdateCamera()
        {
            if (studioCamera == null) return;

            // Camera focuses slightly left of center so right panel doesn't obscure character
            Vector3 offsetTarget = cameraTarget + new Vector3(0.25f, 0, 0);
            Quaternion rot = Quaternion.Euler(8f, currentYaw, 0);
            Vector3 dir = rot * Vector3.forward;

            studioCamera.transform.position = offsetTarget - dir * currentZoom;
            studioCamera.transform.LookAt(offsetTarget);
        }

        private void BuildUI()
        {
            canvasGO = new GameObject("Customizer_Canvas");
            canvasGO.transform.SetParent(transform, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Right Customization Panel
            var panel = CreateUIObject("RightPanel", canvasGO.transform);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.66f, 0.04f);
            panelRect.anchorMax = new Vector2(0.97f, 0.96f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelImg = panel.AddComponent<Image>();
            panelImg.sprite = KyokaTextureGenerator.GetPanelSprite();
            panelImg.type = Image.Type.Sliced;
            panelImg.color = new Color(0.06f, 0.09f, 0.14f, 0.92f);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Header Title
            var header = CreateLabel("Header", panel.transform, "CHARACTER CREATION", font, 24, TextAnchor.MiddleCenter, new Color(0.15f, 0.85f, 0.95f), new Vector2(0.05f, 0.93f), new Vector2(0.95f, 0.98f));

            // Name Field & Randomizer
            var nameBox = CreateUIObject("NameBox", panel.transform);
            var nbRect = nameBox.GetComponent<RectTransform>();
            nbRect.anchorMin = new Vector2(0.06f, 0.85f);
            nbRect.anchorMax = new Vector2(0.94f, 0.915f);
            nbRect.offsetMin = Vector2.zero;
            nbRect.offsetMax = Vector2.zero;

            var nbImg = nameBox.AddComponent<Image>();
            nbImg.sprite = KyokaTextureGenerator.GetPanelSprite();
            nbImg.type = Image.Type.Sliced;
            nbImg.color = new Color(0.12f, 0.16f, 0.24f);

            nameDisplay = CreateLabel("NameDisplay", nameBox.transform, $"AGENT: {charName}", font, 18, TextAnchor.MiddleLeft, Color.white, new Vector2(0.06f, 0), new Vector2(0.70f, 1));

            CreateButton("RandomBtn", nameBox.transform, "🎲 NEW", font, 14, new Vector2(0.72f, 0.1f), new Vector2(0.96f, 0.9f), () =>
            {
                charName = randomNames[UnityEngine.Random.Range(0, randomNames.Length)];
                nameDisplay.text = $"AGENT: {charName}";
                KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
            }, new Color(0.2f, 0.6f, 0.8f));

            // Gender Selection
            CreateLabel("GenderLbl", panel.transform, "GENDER IDENTITY", font, 14, TextAnchor.MiddleLeft, new Color(0.85f, 0.70f, 0.25f), new Vector2(0.06f, 0.80f), new Vector2(0.94f, 0.835f));

            CreateButton("MaleBtn", panel.transform, "♂ MALE", font, 15, new Vector2(0.06f, 0.74f), new Vector2(0.48f, 0.79f), () =>
            {
                isFemale = false;
                UpdateCharacterModel();
                KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
            }, isFemale ? new Color(0.15f, 0.2f, 0.3f) : new Color(0.12f, 0.6f, 0.9f));

            CreateButton("FemaleBtn", panel.transform, "♀ FEMALE", font, 15, new Vector2(0.52f, 0.74f), new Vector2(0.94f, 0.79f), () =>
            {
                isFemale = true;
                UpdateCharacterModel();
                KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
            }, isFemale ? new Color(0.85f, 0.3f, 0.55f) : new Color(0.15f, 0.2f, 0.3f));

            // Hairstyle Buttons
            CreateLabel("HairLbl", panel.transform, "HAIRSTYLE", font, 14, TextAnchor.MiddleLeft, new Color(0.85f, 0.70f, 0.25f), new Vector2(0.06f, 0.685f), new Vector2(0.94f, 0.72f));

            string[] hairNames = new string[] { "Spiky Anime", "Cyber Fade", "Samurai Ponytail", "Tactical Buzz" };
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                float yMin = 0.625f - (i * 0.05f);
                float yMax = yMin + 0.044f;
                CreateButton($"Hair_{i}", panel.transform, hairNames[i], font, 13, new Vector2(0.06f, yMin), new Vector2(0.94f, yMax), () =>
                {
                    hairStyle = (HairStyleType)idx;
                    UpdateCharacterModel();
                    KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
                }, new Color(0.14f, 0.18f, 0.28f));
            }

            // Hair Color Palette
            CreateLabel("ColorLbl", panel.transform, "HAIR COLOR PALETTE", font, 14, TextAnchor.MiddleLeft, new Color(0.85f, 0.70f, 0.25f), new Vector2(0.06f, 0.425f), new Vector2(0.94f, 0.46f));

            Color[] colors = new Color[]
            {
                new Color(0.12f, 0.75f, 0.95f), // Cyan
                new Color(0.95f, 0.22f, 0.28f), // Crimson
                new Color(0.96f, 0.85f, 0.22f), // Blonde
                new Color(0.85f, 0.88f, 0.95f), // Silver
                new Color(0.12f, 0.12f, 0.14f), // Jet Black
                new Color(0.72f, 0.25f, 0.92f)  // Violet
            };

            for (int i = 0; i < colors.Length; i++)
            {
                Color col = colors[i];
                float xMin = 0.06f + (i * 0.15f);
                float xMax = xMin + 0.12f;
                CreateButton($"Color_{i}", panel.transform, "●", font, 22, new Vector2(xMin, 0.355f), new Vector2(xMax, 0.41f), () =>
                {
                    hairColor = col;
                    UpdateCharacterModel();
                    KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
                }, col);
            }

            // Outfit Presets
            CreateLabel("OutfitLbl", panel.transform, "BATTLE OUTFIT PRESET", font, 14, TextAnchor.MiddleLeft, new Color(0.85f, 0.70f, 0.25f), new Vector2(0.06f, 0.305f), new Vector2(0.94f, 0.34f));

            string[] outfitNames = new string[] { "Urban Stalker", "Cyber Ronin", "Desert Commando", "Aether Vanguard" };
            for (int i = 0; i < 4; i++)
            {
                int oIdx = i;
                float col = (i % 2 == 0) ? 0.06f : 0.52f;
                float colW = 0.42f;
                float row = (i < 2) ? 0.235f : 0.175f;

                CreateButton($"Outfit_{i}", panel.transform, outfitNames[i], font, 12, new Vector2(col, row), new Vector2(col + colW, row + 0.048f), () =>
                {
                    outfit = (OutfitPreset)oIdx;
                    UpdateCharacterModel();
                    KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
                }, new Color(0.15f, 0.20f, 0.30f));
            }

            // Confirm & Enter Lobby Button
            var confirmBtn = CreateButton("ConfirmBtn", panel.transform, "✔  CONFIRM & ENTER LOBBY", font, 18, new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.11f), OnConfirmClicked, new Color(0.95f, 0.72f, 0.15f));
            confirmBtn.GetComponentInChildren<Text>().color = new Color(0.08f, 0.08f, 0.12f);
            confirmBtn.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        }

        private void OnConfirmClicked()
        {
            // Save to persistent storage
            var save = KyokaSaveSystem.Load() ?? new KyokaSaveData();
            save.customization.playerName = charName;
            save.customization.isFemale = isFemale;
            save.customization.hairStyle = (int)hairStyle;
            save.customization.hairColorR = hairColor.r;
            save.customization.hairColorG = hairColor.g;
            save.customization.hairColorB = hairColor.b;
            save.customization.skinToneR = skinColor.r;
            save.customization.skinToneG = skinColor.g;
            save.customization.skinToneB = skinColor.b;
            save.customization.eyeColorR = eyeColor.r;
            save.customization.eyeColorG = eyeColor.g;
            save.customization.eyeColorB = eyeColor.b;
            save.customization.outfitPreset = (int)outfit;
            save.customization.hasCreatedCharacter = true;

            KyokaSaveSystem.Save(save);

            KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.RewardChime);
            OnCustomizationComplete?.Invoke();
            Destroy(gameObject);
        }

        private GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private Text CreateLabel(string name, Transform parent, string text, Font font, int size, TextAnchor align, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = CreateUIObject(name, parent);
            var t = go.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
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
            img.sprite = KyokaTextureGenerator.GetButtonSprite(bgColor, new Color(1f, 1f, 1f, 0.4f));
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

