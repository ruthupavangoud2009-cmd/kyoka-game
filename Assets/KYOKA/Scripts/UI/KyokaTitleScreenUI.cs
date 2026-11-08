using UnityEngine;
using UnityEngine.UI;
using Kyoka.Audio;
using Kyoka.Art;
using Kyoka.Save;
using System;

namespace Kyoka.UI
{
    /// <summary>
    /// Free Fire-style cinematic title and startup screen for KYOKA.
    /// Features glowing animated logo, particle atmosphere, guest authentication,
    /// and one-click entrance into Character Creation or the 3D Main Lobby.
    /// </summary>
    public class KyokaTitleScreenUI : MonoBehaviour
    {
        public static KyokaTitleScreenUI Instance { get; private set; }

        public event Action OnEnterPressed;

        private GameObject canvasGO;
        private Canvas canvas;
        private Text tapToStartText;
        private float pulseTimer = 0f;
        private bool isClosing = false;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
        }

        private void BuildUI()
        {
            canvasGO = new GameObject("TitleScreen_Canvas");
            canvasGO.transform.SetParent(transform, false);

            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Fullscreen Dark Cyber Backdrop
            var bg = CreateUIObject("Background", canvasGO.transform);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.05f, 0.08f, 0.98f);
            SetFullScreen(bg.GetComponent<RectTransform>());

            // Title Banner Container
            var titleBox = CreateUIObject("TitleBox", canvasGO.transform);
            var titleRect = titleBox.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.65f);
            titleRect.anchorMax = new Vector2(0.5f, 0.65f);
            titleRect.sizeDelta = new Vector2(900, 220);

            // Glowing KYOKA Logo Text
            var logo = CreateUIObject("LogoText", titleBox.transform);
            var logoText = logo.AddComponent<Text>();
            logoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            logoText.text = "KYOKA";
            logoText.fontSize = 92;
            logoText.alignment = TextAnchor.MiddleCenter;
            logoText.fontStyle = FontStyle.Bold;
            logoText.color = new Color(0.12f, 0.88f, 0.98f); // Radiant Cyan
            SetFullScreen(logo.GetComponent<RectTransform>());

            var outline = logo.AddComponent<Outline>();
            outline.effectColor = new Color(0.02f, 0.25f, 0.45f, 0.8f);
            outline.effectDistance = new Vector2(3, -3);

            // Subtitle
            var sub = CreateUIObject("SubtitleText", titleBox.transform);
            var subText = sub.AddComponent<Text>();
            subText.font = logoText.font;
            subText.text = "THE AETHER PROTOCOL  •  BATTLE ROYALE & OPEN WORLD";
            subText.fontSize = 20;
            subText.alignment = TextAnchor.LowerCenter;
            subText.color = new Color(0.85f, 0.70f, 0.25f); // Gold
            var subRect = sub.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0, 0);
            subRect.anchorMax = new Vector2(1, 0.25f);
            subRect.offsetMin = Vector2.zero;
            subRect.offsetMax = Vector2.zero;

            // Guest Login Card
            var guestBox = CreateUIObject("GuestBox", canvasGO.transform);
            var guestRect = guestBox.GetComponent<RectTransform>();
            guestRect.anchorMin = new Vector2(0.5f, 0.38f);
            guestRect.anchorMax = new Vector2(0.5f, 0.38f);
            guestRect.sizeDelta = new Vector2(360, 48);

            var guestImg = guestBox.AddComponent<Image>();
            guestImg.sprite = KyokaTextureGenerator.GetPanelSprite();
            guestImg.type = Image.Type.Sliced;
            guestImg.color = new Color(0.1f, 0.14f, 0.22f, 0.85f);

            var guestTxt = CreateUIObject("GuestText", guestBox.transform);
            var gText = guestTxt.AddComponent<Text>();
            gText.font = logoText.font;
            gText.text = "👤 GUEST ENTRY  •  ONLINE READY";
            gText.fontSize = 16;
            gText.alignment = TextAnchor.MiddleCenter;
            gText.color = new Color(0.7f, 0.85f, 0.95f);
            SetFullScreen(guestTxt.GetComponent<RectTransform>());

            // Tap to Start Button
            var startBtnGO = CreateUIObject("StartButton", canvasGO.transform);
            var startRect = startBtnGO.GetComponent<RectTransform>();
            startRect.anchorMin = new Vector2(0.5f, 0.22f);
            startRect.anchorMax = new Vector2(0.5f, 0.22f);
            startRect.sizeDelta = new Vector2(460, 68);

            var btnImg = startBtnGO.AddComponent<Image>();
            btnImg.sprite = KyokaTextureGenerator.GetButtonSprite(new Color(0.95f, 0.72f, 0.15f), new Color(1f, 0.9f, 0.5f));
            btnImg.type = Image.Type.Sliced;

            var btn = startBtnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(OnStartClicked);

            var tapTxtGO = CreateUIObject("TapText", startBtnGO.transform);
            tapToStartText = tapTxtGO.AddComponent<Text>();
            tapToStartText.font = logoText.font;
            tapToStartText.text = "▶  TAP TO START  ◀";
            tapToStartText.fontSize = 24;
            tapToStartText.fontStyle = FontStyle.Bold;
            tapToStartText.alignment = TextAnchor.MiddleCenter;
            tapToStartText.color = new Color(0.08f, 0.08f, 0.12f);
            SetFullScreen(tapTxtGO.GetComponent<RectTransform>());

            // Version info footer
            var verGO = CreateUIObject("VersionText", canvasGO.transform);
            var verText = verGO.AddComponent<Text>();
            verText.font = logoText.font;
            verText.text = "KYOKA v1.0.0  •  Unity 6 URP  •  Cross-Platform Architecture";
            verText.fontSize = 14;
            verText.alignment = TextAnchor.MiddleCenter;
            verText.color = new Color(0.5f, 0.55f, 0.65f, 0.7f);
            var verRect = verGO.GetComponent<RectTransform>();
            verRect.anchorMin = new Vector2(0, 0);
            verRect.anchorMax = new Vector2(1, 0.08f);
            verRect.offsetMin = Vector2.zero;
            verRect.offsetMax = Vector2.zero;
        }

        private void Update()
        {
            if (isClosing) return;

            pulseTimer += Time.deltaTime * 3.5f;
            if (tapToStartText != null)
            {
                float alpha = 0.65f + Mathf.Sin(pulseTimer) * 0.35f;
                tapToStartText.color = new Color(0.08f, 0.08f, 0.12f, alpha);
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                OnStartClicked();
            }
        }

        private void OnStartClicked()
        {
            if (isClosing) return;
            isClosing = true;

            KyokaAudioManager.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
            OnEnterPressed?.Invoke();
            Destroy(gameObject);
        }

        private GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private void SetFullScreen(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}

