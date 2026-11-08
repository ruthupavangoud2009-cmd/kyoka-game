using UnityEngine;
using System;
using System.Collections.Generic;

namespace Kyoka.World
{
    public enum WeatherType
    {
        ClearDay,
        SunsetGlow,
        RainStorm,
        DenseFog,
        AetherSakura
    }

    /// <summary>
    /// Master coordinator for the KYOKA open world: manages the 6 distinct regions,
    /// dynamic day/night environmental lighting, and dynamic weather states.
    /// </summary>
    public class KyokaWorldManager : MonoBehaviour
    {
        public static KyokaWorldManager Instance { get; private set; }

        [Header("Active Region")]
        [SerializeField] private KyokaRegionId currentRegion = KyokaRegionId.KYOKACity;
        public KyokaRegionId CurrentRegion => currentRegion;

        [Header("Day / Night Cycle")]
        public Light directionalSun;
        public float dayCycleDurationMinutes = 24.0f;
        [Range(0f, 24f)] public float timeOfDay = 12.0f; // 12 = Noon
        public bool runTimeCycle = true;

        [Header("Weather Controller")]
        public WeatherType activeWeather = WeatherType.ClearDay;

        [Header("Region Root References")]
        public readonly Dictionary<KyokaRegionId, GameObject> regionRoots = new Dictionary<KyokaRegionId, GameObject>();

        public event Action<KyokaRegionId> OnRegionChanged;
        public event Action<WeatherType> OnWeatherChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (directionalSun == null)
            {
                var sunGo = GameObject.Find("KYOKA_Sun");
                if (sunGo != null) directionalSun = sunGo.GetComponent<Light>();
            }
        }

        private void Update()
        {
            if (runTimeCycle)
            {
                // Advance time
                float hoursPerSecond = 24.0f / (dayCycleDurationMinutes * 60f);
                timeOfDay = (timeOfDay + hoursPerSecond * Time.deltaTime) % 24f;

                UpdateSunLighting();
            }
        }

        private void UpdateSunLighting()
        {
            if (!directionalSun) return;

            // Rotation around X axis based on time (0=Midnight, 6=Sunrise, 12=Noon, 18=Sunset)
            float sunAngle = (timeOfDay / 24.0f) * 360f - 90f;
            directionalSun.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f);

            // Adjust intensity and color for night vs day
            if (timeOfDay >= 6f && timeOfDay <= 18f)
            {
                // Day
                float daylightFactor = Mathf.Sin((timeOfDay - 6f) / 12f * Mathf.PI);
                directionalSun.intensity = Mathf.Lerp(0.2f, 1.3f, daylightFactor);
                directionalSun.color = Color.Lerp(new Color(1f, 0.7f, 0.4f), Color.white, daylightFactor);
                RenderSettings.ambientLight = Color.Lerp(new Color(0.2f, 0.25f, 0.35f), new Color(0.55f, 0.58f, 0.62f), daylightFactor);
            }
            else
            {
                // Night
                directionalSun.intensity = 0.15f;
                directionalSun.color = new Color(0.25f, 0.35f, 0.6f);
                RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.22f);
            }
        }

        public void LoadRegion(KyokaRegionId regionId)
        {
            currentRegion = regionId;

            // Toggle regional hierarchies
            foreach (var kvp in regionRoots)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.SetActive(kvp.Key == regionId);
                }
            }

            OnRegionChanged?.Invoke(currentRegion);
            Debug.Log($"[KYOKA World] Switched to Region: {currentRegion}");
        }

        public void SetWeather(WeatherType weather)
        {
            activeWeather = weather;
            OnWeatherChanged?.Invoke(activeWeather);

            switch (weather)
            {
                case WeatherType.ClearDay:
                    RenderSettings.fog = false;
                    break;
                case WeatherType.DenseFog:
                    RenderSettings.fog = true;
                    RenderSettings.fogDensity = 0.035f;
                    RenderSettings.fogColor = new Color(0.6f, 0.65f, 0.75f);
                    break;
                case WeatherType.RainStorm:
                    RenderSettings.fog = true;
                    RenderSettings.fogDensity = 0.02f;
                    RenderSettings.fogColor = new Color(0.35f, 0.38f, 0.45f);
                    break;
                case WeatherType.AetherSakura:
                    RenderSettings.fog = true;
                    RenderSettings.fogDensity = 0.015f;
                    RenderSettings.fogColor = new Color(0.85f, 0.65f, 0.75f);
                    break;
            }
        }
    }
}

