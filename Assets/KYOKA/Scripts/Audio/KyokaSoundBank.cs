using UnityEngine;
using System;
using System.Collections.Generic;

namespace Kyoka.Audio
{
    /// <summary>
    /// High-performance procedural sound bank and synthesis engine for KYOKA.
    /// Generates punchy, polished sound effects and musical loops directly into Unity AudioClips at runtime.
    /// Completely self-contained, zero external asset dependencies, zero missing audio files.
    /// </summary>
    public static class KyokaSoundBank
    {
        private static readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

        public static AudioClip GetLobbyMusic() => GetOrCreate("Music_Lobby", CreateLobbyTheme);
        public static AudioClip GetCombatMusic() => GetOrCreate("Music_Combat", CreateCombatTheme);
        public static AudioClip GetButtonClick() => GetOrCreate("SFX_ButtonClick", CreateButtonClick);
        public static AudioClip GetButtonHover() => GetOrCreate("SFX_ButtonHover", CreateButtonHover);
        public static AudioClip GetTabSwitch() => GetOrCreate("SFX_TabSwitch", CreateTabSwitch);
        public static AudioClip GetGunshotRifle() => GetOrCreate("SFX_GunshotRifle", CreateGunshotRifle);
        public static AudioClip GetGunshotShotgun() => GetOrCreate("SFX_GunshotShotgun", CreateGunshotShotgun);
        public static AudioClip GetGunshotSniper() => GetOrCreate("SFX_GunshotSniper", CreateGunshotSniper);
        public static AudioClip GetKatanaSlash() => GetOrCreate("SFX_KatanaSlash", CreateKatanaSlash);
        public static AudioClip GetReload() => GetOrCreate("SFX_Reload", CreateReloadSound);
        public static AudioClip GetHitmarker() => GetOrCreate("SFX_Hitmarker", CreateHitmarkerSound);
        public static AudioClip GetHeadshot() => GetOrCreate("SFX_Headshot", CreateHeadshotSound);
        public static AudioClip GetLootPickup() => GetOrCreate("SFX_LootPickup", CreateLootPickupSound);
        public static AudioClip GetGliderWind() => GetOrCreate("SFX_GliderWind", CreateGliderWindSound);
        public static AudioClip GetVictoryFanfare() => GetOrCreate("SFX_VictoryFanfare", CreateVictoryFanfare);

        private static AudioClip GetOrCreate(string key, Func<AudioClip> generator)
        {
            if (clipCache.TryGetValue(key, out var clip) && clip != null)
            {
                return clip;
            }
            clip = generator();
            clipCache[key] = clip;
            return clip;
        }

        #region Sound Effect Synthesizers

        private static AudioClip CreateButtonClick()
        {
            const int sampleRate = 44100;
            const float duration = 0.08f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1f - (t / duration);
                float freq = Mathf.Lerp(1200f, 750f, t / duration);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.45f;
            }

            var clip = AudioClip.Create("SFX_ButtonClick", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateButtonHover()
        {
            const int sampleRate = 44100;
            const float duration = 0.04f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / duration));
                samples[i] = Mathf.Sin(2f * Mathf.PI * 1800f * t) * env * 0.15f;
            }

            var clip = AudioClip.Create("SFX_ButtonHover", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateTabSwitch()
        {
            const int sampleRate = 44100;
            const float duration = 0.12f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / duration));
                float freq = Mathf.Lerp(400f, 900f, t / duration);
                samples[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) + (UnityEngine.Random.value * 2f - 1f) * 0.2f) * env * 0.35f;
            }

            var clip = AudioClip.Create("SFX_TabSwitch", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateGunshotRifle()
        {
            const int sampleRate = 44100;
            const float duration = 0.28f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float noise = (UnityEngine.Random.value * 2f - 1f);
                float crack = Mathf.Sin(2f * Mathf.PI * 180f * t) * Mathf.Exp(-t * 22f);
                float sub = Mathf.Sin(2f * Mathf.PI * 75f * t) * Mathf.Exp(-t * 14f);
                float decay = Mathf.Exp(-t * 18f);

                samples[i] = (noise * 0.6f + crack * 0.4f + sub * 0.5f) * decay * 0.85f;
            }

            var clip = AudioClip.Create("SFX_GunshotRifle", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateGunshotShotgun()
        {
            const int sampleRate = 44100;
            const float duration = 0.42f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float noise = (UnityEngine.Random.value * 2f - 1f);
                float sub = Mathf.Sin(2f * Mathf.PI * 55f * t) * Mathf.Exp(-t * 8f);
                float decay = Mathf.Exp(-t * 10f);

                samples[i] = (noise * 0.75f + sub * 0.7f) * decay * 0.95f;
            }

            var clip = AudioClip.Create("SFX_GunshotShotgun", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateGunshotSniper()
        {
            const int sampleRate = 44100;
            const float duration = 0.65f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float noise = (UnityEngine.Random.value * 2f - 1f);
                float crack = Mathf.Sin(2f * Mathf.PI * 340f * t) * Mathf.Exp(-t * 30f);
                float boom = Mathf.Sin(2f * Mathf.PI * 60f * t) * Mathf.Exp(-t * 5f);
                float decay = Mathf.Exp(-t * 6f);

                samples[i] = (crack * 0.6f + boom * 0.8f + noise * 0.4f) * decay;
            }

            var clip = AudioClip.Create("SFX_GunshotSniper", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateKatanaSlash()
        {
            const int sampleRate = 44100;
            const float duration = 0.22f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / duration));
                float freq = Mathf.Lerp(1400f, 400f, t / duration);
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.35f;
                samples[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) + noise) * env * 0.65f;
            }

            var clip = AudioClip.Create("SFX_KatanaSlash", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateReloadSound()
        {
            const int sampleRate = 44100;
            const float duration = 0.35f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float click1 = (t > 0.05f && t < 0.10f) ? (UnityEngine.Random.value * 2f - 1f) * 0.7f : 0f;
                float click2 = (t > 0.20f && t < 0.28f) ? (UnityEngine.Random.value * 2f - 1f) * 0.9f : 0f;
                samples[i] = click1 + click2;
            }

            var clip = AudioClip.Create("SFX_Reload", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateHitmarkerSound()
        {
            const int sampleRate = 44100;
            const float duration = 0.06f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1f - (t / duration);
                samples[i] = Mathf.Sin(2f * Mathf.PI * 2400f * t) * env * 0.5f;
            }

            var clip = AudioClip.Create("SFX_Hitmarker", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateHeadshotSound()
        {
            const int sampleRate = 44100;
            const float duration = 0.16f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1f - (t / duration);
                float tone1 = Mathf.Sin(2f * Mathf.PI * 1800f * t);
                float tone2 = Mathf.Sin(2f * Mathf.PI * 3200f * t);
                samples[i] = (tone1 * 0.5f + tone2 * 0.5f) * env * 0.65f;
            }

            var clip = AudioClip.Create("SFX_Headshot", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateLootPickupSound()
        {
            const int sampleRate = 44100;
            const float duration = 0.18f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1f - (t / duration);
                float freq = Mathf.Lerp(660f, 1320f, t / duration);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.5f;
            }

            var clip = AudioClip.Create("SFX_LootPickup", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateGliderWindSound()
        {
            const int sampleRate = 44100;
            const float duration = 2.0f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            float lastSample = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                float white = (UnityEngine.Random.value * 2f - 1f);
                // Low-pass filter for rushing wind
                lastSample = Mathf.Lerp(lastSample, white, 0.08f);
                samples[i] = lastSample * 0.35f;
            }

            var clip = AudioClip.Create("SFX_GliderWind", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateVictoryFanfare()
        {
            const int sampleRate = 44100;
            const float duration = 2.4f;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            // Triumphant chord progression notes: C5, E5, G5, C6
            float[] notes = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f };
            float noteDur = 0.45f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                int noteIdx = Mathf.Clamp((int)(t / noteDur), 0, notes.Length - 1);
                float noteT = t - (noteIdx * noteDur);
                float env = Mathf.Clamp01(1f - (noteT / (noteIdx == notes.Length - 1 ? 1.0f : noteDur)));
                float freq = notes[noteIdx];

                float wave = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f +
                             Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.25f;

                samples[i] = wave * env * 0.6f;
            }

            var clip = AudioClip.Create("SFX_VictoryFanfare", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        #endregion

        #region Music Loop Synthesizers

        private static AudioClip CreateLobbyTheme()
        {
            const int sampleRate = 44100;
            const float duration = 8.0f; // 8-second seamless looping synthwave groove
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            // Chords in progression: Am, F, C, G
            float[][] chordProgression = new float[][]
            {
                new float[] { 220.00f, 261.63f, 329.63f }, // Am
                new float[] { 174.61f, 220.00f, 261.63f }, // F
                new float[] { 130.81f, 164.81f, 196.00f }, // C
                new float[] { 196.00f, 246.94f, 293.66f }  // G
            };

            float chordDuration = duration / chordProgression.Length;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                int chordIdx = (int)(t / chordDuration) % chordProgression.Length;
                float[] chord = chordProgression[chordIdx];

                // Bass arpeggio (16th notes)
                float bassStep = (t * 8f) % 1f;
                float bassFreq = chord[0] * 0.5f;
                float bassEnv = Mathf.Clamp01(1f - bassStep * 1.5f);
                float bass = Mathf.Sin(2f * Mathf.PI * bassFreq * t) * bassEnv * 0.35f;

                // Pad harmony
                float pad = 0f;
                for (int c = 0; c < chord.Length; c++)
                {
                    pad += Mathf.Sin(2f * Mathf.PI * chord[c] * t) * 0.08f;
                }

                // Cyber pulse percussion (Kick on 1 & 3, Snare on 2 & 4)
                float beatTime = (t * 2f) % 1f;
                float kick = (beatTime < 0.12f) ? Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(120f, 40f, beatTime / 0.12f) * beatTime) * 0.4f : 0f;

                samples[i] = (bass + pad + kick) * 0.65f;
            }

            var clip = AudioClip.Create("Music_Lobby", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateCombatTheme()
        {
            const int sampleRate = 44100;
            const float duration = 6.0f; // 6-second high-intensity combat loop
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;

                // Driving 130 BPM tempo
                float beat = (t * 4.33f) % 1f;
                float kick = (beat < 0.1f) ? Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(140f, 45f, beat / 0.1f) * beat) * 0.5f : 0f;
                float subBass = Mathf.Sin(2f * Mathf.PI * 65f * t) * 0.25f;
                float hihat = ((t * 8.66f) % 1f < 0.04f) ? (UnityEngine.Random.value * 2f - 1f) * 0.18f : 0f;

                samples[i] = (kick + subBass + hihat) * 0.7f;
            }

            var clip = AudioClip.Create("Music_Combat", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        #endregion
    }
}

