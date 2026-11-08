using UnityEngine;
using System;
using Kyoka.World;
using Kyoka.Core;
using Kyoka.Save;

namespace Kyoka.UI
{
    public enum ActiveScreen
    {
        MainMenu,
        GameplayHUD,
        PauseMenu,
        SettingsMenu,
        InventoryScreen,
        MapScreen,
        GameModeSelector,
        SocialWindow,
        CharacterCustomizer
    }

    /// <summary>
    /// Master UI manager for KYOKA. Controls screen transitions, modal dialogs,
    /// settings adjustments (graphics presets, audio, sensitivity), and platform responsiveness.
    /// </summary>
    public class KyokaUIManager : MonoBehaviour
    {
        public static KyokaUIManager Instance { get; private set; }

        [Header("State")]
        public ActiveScreen currentScreen = ActiveScreen.GameplayHUD;

        [Header("Settings State")]
        public int graphicsPreset = 1; // 0=Low, 1=Medium, 2=High
        public float masterVolume = 0.8f;
        public float sfxVolume = 0.8f;
        public float musicVolume = 0.7f;
        public int targetFps = 60;

        public event Action<ActiveScreen> OnScreenChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
            ApplyQualityPreset(graphicsPreset);
        }

        private void Update()
        {
            var input = KyokaInputBridge.Instance;
            if (input == null) return;

            if (input.PauseTogglePressed)
            {
                ToggleScreen(ActiveScreen.PauseMenu);
            }
            if (input.InventoryTogglePressed)
            {
                ToggleScreen(ActiveScreen.InventoryScreen);
            }
            if (input.MapTogglePressed)
            {
                ToggleScreen(ActiveScreen.MapScreen);
            }
        }

        public void ShowScreen(ActiveScreen screen)
        {
            currentScreen = screen;
            bool isGameplay = (screen == ActiveScreen.GameplayHUD);

            // Unlock cursor when menus are active
            if (!Application.isMobilePlatform)
            {
                Cursor.lockState = isGameplay ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !isGameplay;
            }

            Time.timeScale = (screen == ActiveScreen.PauseMenu) ? 0f : 1f;
            OnScreenChanged?.Invoke(currentScreen);
        }

        public void ToggleScreen(ActiveScreen screen)
        {
            if (currentScreen == screen)
            {
                ShowScreen(ActiveScreen.GameplayHUD);
            }
            else
            {
                ShowScreen(screen);
            }
        }

        public void ResumeGame()
        {
            ShowScreen(ActiveScreen.GameplayHUD);
        }

        #region Settings Application

        public void ApplyQualityPreset(int presetIndex)
        {
            graphicsPreset = Mathf.Clamp(presetIndex, 0, 2);
            switch (graphicsPreset)
            {
                case 0: // Low (Mobile Performance)
                    QualitySettings.SetQualityLevel(0, true);
                    QualitySettings.shadows = ShadowQuality.Disable;
                    Application.targetFrameRate = 30;
                    targetFps = 30;
                    break;
                case 1: // Medium (Balanced)
                    QualitySettings.SetQualityLevel(1, true);
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    Application.targetFrameRate = 60;
                    targetFps = 60;
                    break;
                case 2: // High (PC & Premium Mobile)
                    QualitySettings.SetQualityLevel(2, true);
                    QualitySettings.shadows = ShadowQuality.All;
                    Application.targetFrameRate = 60;
                    targetFps = 60;
                    break;
            }
            Debug.Log($"[KYOKA Settings] Applied Quality Preset: {(graphicsPreset == 0 ? "Low" : graphicsPreset == 1 ? "Medium" : "High")}");
        }

        public void SetTargetFPS(int fps)
        {
            targetFps = fps;
            Application.targetFrameRate = fps;
        }

        public void SetMasterVolume(float vol)
        {
            masterVolume = Mathf.Clamp01(vol);
            AudioListener.volume = masterVolume;
        }

        #endregion
    }
}

