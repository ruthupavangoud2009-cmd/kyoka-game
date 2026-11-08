using UnityEngine;
using UnityEngine.EventSystems;
using Kyoka.UI;
using Kyoka.Save;
using Kyoka.Audio;
using Kyoka.BattleRoyale;
using Kyoka.World;
using Kyoka.Player;

namespace Kyoka.Core
{
    /// <summary>
    /// Master coordinator for KYOKA's complete game experience flow:
    /// TITLE SCREEN ❯ CHARACTER CREATION ❯ 3D MAIN LOBBY ❯ BATTLE ROYALE / OPEN WORLD ❯ RESULTS ❯ LOBBY.
    /// Ensures every transition is smooth, persistent, and functional.
    /// </summary>
    public class KyokaMasterGameController : MonoBehaviour
    {
        public static KyokaMasterGameController Instance { get; private set; }

        private KyokaTitleScreenUI activeTitleScreen;
        private KyokaCharacterCustomizerUI activeCustomizer;
        private KyokaLobbyUI activeLobby;
        private KyokaBattleRoyaleExperience activeBR;
        private GameObject activeWorldInstance;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);

            EnsureEventSystem();
            EnsureAudioManager();
        }

        private void Start()
        {
            ShowTitleScreen();

            string[] args = System.Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                if (arg == "-captureScreenshots")
                {
                    StartCoroutine(CaptureScreenshotsCoroutine());
                    break;
                }
            }
        }

        private System.Collections.IEnumerator CaptureScreenshotsCoroutine()
        {
            string[] dirs = new string[]
            {
                System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Screenshots")),
                System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../../Builds/Screenshots")),
                System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Builds/Screenshots")),
                System.IO.Path.GetFullPath("Builds/Screenshots")
            };

            foreach (var d in dirs)
            {
                try { if (!System.IO.Directory.Exists(d)) System.IO.Directory.CreateDirectory(d); } catch { }
            }

            void SaveAll(string filename)
            {
                foreach (var d in dirs)
                {
                    try { ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(d, filename)); } catch { }
                }
            }

            yield return new WaitForSeconds(1.2f);
            SaveAll("1_TitleScreen.png");
            Debug.Log("[KYOKA Capture] Captured 1_TitleScreen.png");

            yield return new WaitForSeconds(0.8f);
            ShowCharacterCustomizer();
            yield return new WaitForSeconds(1.5f);
            SaveAll("2_CharacterCustomizer.png");
            Debug.Log("[KYOKA Capture] Captured 2_CharacterCustomizer.png");

            yield return new WaitForSeconds(0.8f);
            ShowMainLobby();
            yield return new WaitForSeconds(1.5f);
            SaveAll("3_MainLobby.png");
            Debug.Log("[KYOKA Capture] Captured 3_MainLobby.png");

            yield return new WaitForSeconds(0.8f);
            HandleStartGame(LobbyGameMode.BattleRoyale);
            yield return new WaitForSeconds(2.0f);
            SaveAll("4_BattleRoyale.png");
            Debug.Log("[KYOKA Capture] Captured 4_BattleRoyale.png");

            yield return new WaitForSeconds(0.8f);
            HandleStartGame(LobbyGameMode.OpenWorldCity);
            yield return new WaitForSeconds(2.0f);
            SaveAll("5_OpenWorld.png");
            Debug.Log("[KYOKA Capture] Captured 5_OpenWorld.png");

            yield return new WaitForSeconds(1.0f);
            Debug.Log("[KYOKA Capture] All screenshots captured successfully!");
            Application.Quit();
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
                if (esGO.transform.parent == null) DontDestroyOnLoad(esGO);
            }
        }

        private void EnsureAudioManager()
        {
            if (KyokaAudioManager.Instance == null)
            {
                var audioGO = new GameObject("KyokaAudioManager");
                audioGO.AddComponent<KyokaAudioManager>();
            }
        }

        public void ShowTitleScreen()
        {
            CleanupActiveViews();

            var tsGO = new GameObject("TitleScreen_Controller");
            activeTitleScreen = tsGO.AddComponent<KyokaTitleScreenUI>();
            activeTitleScreen.OnEnterPressed += HandleTitleEnter;
        }

        private void HandleTitleEnter()
        {
            var save = KyokaSaveSystem.Load();
            bool hasCustomized = (save != null && save.customization != null && save.customization.hasCreatedCharacter);

            if (!hasCustomized)
            {
                ShowCharacterCustomizer();
            }
            else
            {
                ShowMainLobby();
            }
        }

        public void ShowCharacterCustomizer()
        {
            CleanupActiveViews();

            var ccGO = new GameObject("CharacterCustomizer_Controller");
            activeCustomizer = ccGO.AddComponent<KyokaCharacterCustomizerUI>();
            activeCustomizer.OnCustomizationComplete += () =>
            {
                ShowMainLobby();
            };
        }

        public void ShowMainLobby()
        {
            CleanupActiveViews();

            var lobbyGO = new GameObject("MainLobby_Controller");
            activeLobby = lobbyGO.AddComponent<KyokaLobbyUI>();
            activeLobby.OnStartGameRequested += HandleStartGame;
        }

        private void HandleStartGame(LobbyGameMode mode)
        {
            CleanupActiveViews();

            if (mode == LobbyGameMode.BattleRoyale)
            {
                LaunchBattleRoyale();
            }
            else
            {
                LaunchOpenWorld();
            }
        }

        private void LaunchBattleRoyale()
        {
            var brGO = new GameObject("BattleRoyale_Match");
            activeBR = brGO.AddComponent<KyokaBattleRoyaleExperience>();
            activeBR.OnMatchEnded += () =>
            {
                ShowMainLobby();
            };
        }

        private void LaunchOpenWorld()
        {
            activeWorldInstance = new GameObject("OpenWorld_Instance");
            var wm = activeWorldInstance.AddComponent<KyokaWorldManager>();
            var wb = activeWorldInstance.AddComponent<KyokaWorldBootstrap>();
            var pb = activeWorldInstance.AddComponent<KyokaPlayerBootstrap>();
            var hud = activeWorldInstance.AddComponent<KyokaHUD>();
        }

        private void CleanupActiveViews()
        {
            if (activeTitleScreen != null) Destroy(activeTitleScreen.gameObject);
            if (activeCustomizer != null) Destroy(activeCustomizer.gameObject);
            if (activeLobby != null) Destroy(activeLobby.gameObject);
            if (activeBR != null) Destroy(activeBR.gameObject);
            if (activeWorldInstance != null) Destroy(activeWorldInstance);

            activeTitleScreen = null;
            activeCustomizer = null;
            activeLobby = null;
            activeBR = null;
            activeWorldInstance = null;
        }
    }
}

