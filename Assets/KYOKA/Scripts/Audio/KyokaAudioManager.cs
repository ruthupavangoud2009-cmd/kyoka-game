using UnityEngine;
using System;
using System.Collections.Generic;
using Kyoka.World;

namespace Kyoka.Audio
{
    public enum SoundEffectType
    {
        Footstep,
        Jump,
        MeleeSlash,
        RangedShot,
        AbilityCast,
        HitImpact,
        VehicleEngine,
        ButtonClick,
        RewardChime,
        Explosion
    }

    /// <summary>
    /// Master audio manager for KYOKA. Provides spatial SFX, regional background music,
    /// and built-in procedural synthesis so the game has rich audio feedback at ₹0 cost
    /// even prior to importing external WAV/MP3 audio clips.
    /// </summary>
    public class KyokaAudioManager : MonoBehaviour
    {
        public static KyokaAudioManager Instance { get; private set; }

        [Header("Volume Buses")]
        [Range(0f, 1f)] public float masterVolume = 0.8f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float musicVolume = 0.6f;

        [Header("Audio Sources")]
        private AudioSource musicSource;
        private AudioSource sfxSource;
        private readonly Queue<AudioSource> sfxPool = new Queue<AudioSource>();

        [Header("Procedural Synthesizer Settings")]
        private bool isSynthesizingMusic = true;
        private float synthPhase = 0f;
        private float currentToneFreq = 220f; // A3
        private float synthTimer = 0f;

        // Regional Musical Scales (Frequencies in Hz)
        private readonly float[] cityScale = new float[] { 220f, 246.94f, 277.18f, 329.63f, 369.99f }; // A Minor Pentatonic
        private readonly float[] wildlandsScale = new float[] { 196f, 220f, 261.63f, 293.66f, 329.63f }; // G Major Pentatonic
        private readonly float[] animeScale = new float[] { 261.63f, 293.66f, 329.63f, 392f, 440f, 493.88f }; // Lydian / Anime
        private readonly float[] battleScale = new float[] { 146.83f, 174.61f, 196f, 220f, 246.94f }; // D Minor

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
        }

        private void InitializeAudioSources()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.playOnAwake = false;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume * masterVolume;

            // SFX Voice Pool
            for (int i = 0; i < 6; i++)
            {
                var child = new GameObject($"SFX_Voice_{i}");
                child.transform.SetParent(transform);
                var src = child.AddComponent<AudioSource>();
                src.spatialBlend = 0f; // 2D by default
                sfxPool.Enqueue(src);
            }
        }

        private void Update()
        {
            // Procedural ambient music progression
            if (isSynthesizingMusic)
            {
                synthTimer += Time.deltaTime;
                if (synthTimer > 1.8f)
                {
                    synthTimer = 0f;
                    PickNextAmbientNote();
                }
            }
        }

        private void PickNextAmbientNote()
        {
            var wm = KyokaWorldManager.Instance;
            float[] activeScale = cityScale;

            if (wm != null)
            {
                switch (wm.CurrentRegion)
                {
                    case KyokaRegionId.KYOKACity: activeScale = cityScale; break;
                    case KyokaRegionId.Wildlands: activeScale = wildlandsScale; break;
                    case KyokaRegionId.AnimeRealm: activeScale = animeScale; break;
                    case KyokaRegionId.BattleIslands: activeScale = battleScale; break;
                    default: activeScale = cityScale; break;
                }
            }

            int noteIdx = UnityEngine.Random.Range(0, activeScale.Length);
            currentToneFreq = activeScale[noteIdx];
        }

        /// <summary>
        /// Real-time audio buffer filter producing procedural ambient harmonies and effects.
        /// </summary>
        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!isSynthesizingMusic || musicVolume <= 0.001f) return;

            float sampleRate = AudioSettings.outputSampleRate;
            float phaseIncrement = (2f * Mathf.PI * currentToneFreq) / sampleRate;
            float gain = musicVolume * masterVolume * 0.04f; // Soft ambient background pad

            for (int i = 0; i < data.Length; i += channels)
            {
                synthPhase += phaseIncrement;
                if (synthPhase > 2f * Mathf.PI) synthPhase -= 2f * Mathf.PI;

                // Warm sine + soft harmonic
                float sample = (Mathf.Sin(synthPhase) + Mathf.Sin(synthPhase * 2f) * 0.25f) * gain;

                for (int c = 0; c < channels; c++)
                {
                    data[i + c] += sample;
                }
            }
        }

        public void PlaySFX(SoundEffectType type, Vector3? position = null)
        {
            if (sfxPool.Count == 0) return;
            var src = sfxPool.Dequeue();
            sfxPool.Enqueue(src);

            if (position.HasValue)
            {
                src.transform.position = position.Value;
                src.spatialBlend = 1.0f; // 3D Audio
                src.minDistance = 2f;
                src.maxDistance = 35f;
            }
            else
            {
                src.spatialBlend = 0f; // 2D Stereo
            }

            src.volume = sfxVolume * masterVolume;
            src.pitch = UnityEngine.Random.Range(0.92f, 1.08f);

            // Generate synthetic audio clip for immediate playback
            AudioClip clip = GenerateSyntheticClip(type);
            if (clip != null)
            {
                src.PlayOneShot(clip);
            }
        }

        private AudioClip GenerateSyntheticClip(SoundEffectType type)
        {
            int sampleRate = 44100;
            float duration = 0.15f;
            float baseFreq = 440f;

            switch (type)
            {
                case SoundEffectType.Footstep:
                    duration = 0.08f;
                    baseFreq = 80f;
                    break;
                case SoundEffectType.MeleeSlash:
                    duration = 0.18f;
                    baseFreq = 380f;
                    break;
                case SoundEffectType.RangedShot:
                    duration = 0.22f;
                    baseFreq = 650f;
                    break;
                case SoundEffectType.AbilityCast:
                    duration = 0.35f;
                    baseFreq = 520f;
                    break;
                case SoundEffectType.ButtonClick:
                    duration = 0.04f;
                    baseFreq = 880f;
                    break;
                case SoundEffectType.RewardChime:
                    duration = 0.45f;
                    baseFreq = 1046.5f; // C6
                    break;
                case SoundEffectType.Explosion:
                    duration = 0.5f;
                    baseFreq = 65f;
                    break;
            }

            int numSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / numSamples; // Linear decay

                if (type == SoundEffectType.Explosion || type == SoundEffectType.Footstep)
                {
                    // Noise + low frequency thud
                    float noise = UnityEngine.Random.Range(-1f, 1f) * 0.4f;
                    float tone = Mathf.Sin(2f * Mathf.PI * baseFreq * t) * 0.6f;
                    samples[i] = (noise + tone) * envelope;
                }
                else if (type == SoundEffectType.MeleeSlash)
                {
                    // Pitch sweep down
                    float sweepFreq = Mathf.Lerp(baseFreq * 2f, baseFreq * 0.4f, (float)i / numSamples);
                    samples[i] = Mathf.Sin(2f * Mathf.PI * sweepFreq * t) * envelope;
                }
                else
                {
                    // Clean tone with harmonic
                    samples[i] = (Mathf.Sin(2f * Mathf.PI * baseFreq * t) + Mathf.Sin(2f * Mathf.PI * baseFreq * 2f * t) * 0.3f) * envelope;
                }
            }

            var clip = AudioClip.Create($"Synth_{type}", numSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public void SetMasterVolume(float vol)
        {
            masterVolume = Mathf.Clamp01(vol);
            AudioListener.volume = masterVolume;
        }
    }
}
