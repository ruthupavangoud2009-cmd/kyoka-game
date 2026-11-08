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
    /// and built-in procedural synthesis so the game has rich audio feedback at ₹0 cost.
    /// Uses dedicated audio channels to prevent DSP filter conflicts and maximize performance.
    /// </summary>
    public class KyokaAudioManager : MonoBehaviour
    {
        public static KyokaAudioManager Instance { get; private set; }

        [Header("Volume Buses")]
        [Range(0f, 1f)] public float masterVolume = 0.8f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float musicVolume = 0.6f;

        [Header("Audio Channels")]
        private AudioSource musicSource;
        private AudioSource sfxSource;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
        }

        private void Start()
        {
            PlayLobbyMusic();
        }

        private void InitializeAudioSources()
        {
            // Dedicated child for Music
            var musicGO = new GameObject("MusicChannel");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.playOnAwake = false;

            // Dedicated child for SFX
            var sfxGO = new GameObject("SFXChannel");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        public void PlayLobbyMusic()
        {
            if (musicSource == null) return;
            var clip = KyokaSoundBank.GetLobbyMusic();
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }

        public void PlayCombatMusic()
        {
            if (musicSource == null) return;
            var clip = KyokaSoundBank.GetCombatMusic();
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null) musicSource.Stop();
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, volumeScale * sfxVolume * masterVolume);
        }

        public void PlaySFX(SoundEffectType type, Vector3? position = null)
        {
            PlaySoundEffect(type, position);
        }

        public void PlaySoundEffect(SoundEffectType type, Vector3? position = null)
        {
            AudioClip clip = null;
            switch (type)
            {
                case SoundEffectType.ButtonClick:
                    clip = KyokaSoundBank.GetButtonClick();
                    break;
                case SoundEffectType.MeleeSlash:
                    clip = KyokaSoundBank.GetKatanaSlash();
                    break;
                case SoundEffectType.RangedShot:
                    clip = KyokaSoundBank.GetGunshotRifle();
                    break;
                case SoundEffectType.HitImpact:
                    clip = KyokaSoundBank.GetHitmarker();
                    break;
                case SoundEffectType.RewardChime:
                    clip = KyokaSoundBank.GetLootPickup();
                    break;
                case SoundEffectType.Footstep:
                case SoundEffectType.Jump:
                case SoundEffectType.AbilityCast:
                case SoundEffectType.VehicleEngine:
                case SoundEffectType.Explosion:
                default:
                    clip = KyokaSoundBank.GetButtonClick();
                    break;
            }

            if (clip != null)
            {
                PlaySFX(clip);
            }
        }

        public void SetMasterVolume(float vol)
        {
            masterVolume = Mathf.Clamp01(vol);
            UpdateVolumes();
        }

        public void SetMusicVolume(float vol)
        {
            musicVolume = Mathf.Clamp01(vol);
            UpdateVolumes();
        }

        public void SetSFXVolume(float vol)
        {
            sfxVolume = Mathf.Clamp01(vol);
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            if (musicSource != null)
            {
                musicSource.volume = musicVolume * masterVolume;
            }
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume * masterVolume;
            }
        }
    }
}
