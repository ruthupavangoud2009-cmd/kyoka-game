using UnityEngine;
using System.Collections.Generic;

namespace Kyoka.Art
{
    /// <summary>
    /// Master procedural texture engine for KYOKA AAA Visual Overhaul.
    /// Synthesizes high-resolution, pixel-crisp anime textures, facial features,
    /// tactical outfit weaves, architectural facades, nature maps, and skyboxes.
    /// </summary>
    public static class KyokaTextureGenerator
    {
        private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Texture2D> texCache = new Dictionary<string, Texture2D>();

        public static Sprite GetPanelSprite()
        {
            return GetOrCreateSprite("UI_Panel_Glass", () => CreateRoundedRect(64, 64, 12, new Color(0.05f, 0.08f, 0.14f, 0.92f), new Color(0.18f, 0.65f, 0.95f, 0.85f)));
        }

        public static Sprite GetButtonSprite(Color baseColor, Color borderColor)
        {
            string key = $"UI_Btn_{ColorUtility.ToHtmlStringRGBA(baseColor)}_{ColorUtility.ToHtmlStringRGBA(borderColor)}";
            return GetOrCreateSprite(key, () => CreateRoundedRect(128, 64, 10, baseColor, borderColor));
        }

        public static Sprite GetCrosshairSprite()
        {
            return GetOrCreateSprite("UI_Crosshair", CreateCrosshairTexture);
        }

        public static Sprite GetCircleSprite(Color color)
        {
            string key = $"UI_Circle_{ColorUtility.ToHtmlStringRGBA(color)}";
            return GetOrCreateSprite(key, () => CreateCircleTexture(64, color));
        }

        public static Texture2D GetCyberGridTexture()
        {
            return GetOrCreateTexture("Tex_CyberGrid", CreateCyberGrid);
        }

        public static Texture2D GetHangarFloorTexture()
        {
            return GetOrCreateTexture("Tex_HangarFloor", CreateHangarFloor);
        }

        public static Texture2D GetFaceTexture(Color skinTone, Color eyeColor, bool isFemale = false)
        {
            string key = $"Tex_Face_{ColorUtility.ToHtmlStringRGBA(skinTone)}_{ColorUtility.ToHtmlStringRGBA(eyeColor)}_{isFemale}";
            return GetOrCreateTexture(key, () => CreateHighResAnimeFace(skinTone, eyeColor, isFemale));
        }

        public static Texture2D GetHairTexture(Color baseColor)
        {
            string key = $"Tex_Hair_{ColorUtility.ToHtmlStringRGBA(baseColor)}";
            return GetOrCreateTexture(key, () => CreateHairTexture(baseColor));
        }

        public static Texture2D GetOutfitTexture(int outfitPreset, bool isFemale = false)
        {
            string key = $"Tex_Outfit_{outfitPreset}_{isFemale}";
            return GetOrCreateTexture(key, () => CreateOutfitTexture(outfitPreset, isFemale));
        }

        public static Texture2D GetSkyscraperTexture(Color wallColor, Color windowLitColor)
        {
            string key = $"Tex_Bldg_{ColorUtility.ToHtmlStringRGBA(wallColor)}_{ColorUtility.ToHtmlStringRGBA(windowLitColor)}";
            return GetOrCreateTexture(key, () => CreateSkyscraperTexture(wallColor, windowLitColor));
        }

        public static Texture2D GetNeonSignTexture(string text, Color neonColor)
        {
            string key = $"Tex_Neon_{text}_{ColorUtility.ToHtmlStringRGBA(neonColor)}";
            return GetOrCreateTexture(key, () => CreateNeonSignTexture(text, neonColor));
        }

        public static Texture2D GetFoliageTexture(Color leafColor, bool isSakura = false)
        {
            string key = $"Tex_Foliage_{ColorUtility.ToHtmlStringRGBA(leafColor)}_{isSakura}";
            return GetOrCreateTexture(key, () => CreateFoliageTexture(leafColor, isSakura));
        }

        public static Texture2D GetBarkTexture(Color barkColor)
        {
            string key = $"Tex_Bark_{ColorUtility.ToHtmlStringRGBA(barkColor)}";
            return GetOrCreateTexture(key, () => CreateBarkTexture(barkColor));
        }

        public static Texture2D GetRockTexture(Color rockColor)
        {
            string key = $"Tex_Rock_{ColorUtility.ToHtmlStringRGBA(rockColor)}";
            return GetOrCreateTexture(key, () => CreateRockTexture(rockColor));
        }

        public static Texture2D GetWaterTexture()
        {
            return GetOrCreateTexture("Tex_WaterRipples", CreateWaterTexture);
        }

        public static Texture2D GetGliderTexture()
        {
            return GetOrCreateTexture("Tex_Glider_HiRes", CreateGliderPattern);
        }

        public static Texture2D GetSkyboxPanorama()
        {
            return GetOrCreateTexture("Tex_SkyboxPanorama", CreateSkyboxPanorama);
        }

        #region Creators

        private static Sprite GetOrCreateSprite(string key, System.Func<Sprite> creator)
        {
            if (spriteCache.TryGetValue(key, out var s) && s != null) return s;
            s = creator();
            spriteCache[key] = s;
            return s;
        }

        private static Texture2D GetOrCreateTexture(string key, System.Func<Texture2D> creator)
        {
            if (texCache.TryGetValue(key, out var t) && t != null) return t;
            t = creator();
            texCache[key] = t;
            return t;
        }

        private static Sprite CreateRoundedRect(int width, int height, int radius, Color fill, Color border)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int dx = Mathf.Min(x, width - 1 - x);
                    int dy = Mathf.Min(y, height - 1 - y);

                    float dist = 0f;
                    if (dx < radius && dy < radius)
                    {
                        float cx = radius - dx;
                        float cy = radius - dy;
                        dist = Mathf.Sqrt(cx * cx + cy * cy) - radius;
                    }

                    if (dist > 1f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (dist > -1f)
                    {
                        float alpha = Mathf.Clamp01(1f - dist);
                        tex.SetPixel(x, y, new Color(border.r, border.g, border.b, border.a * alpha));
                    }
                    else if (dx < 2 || dy < 2)
                    {
                        tex.SetPixel(x, y, border);
                    }
                    else
                    {
                        // Subtle vertical gloss gradient
                        float vGrad = (float)y / height * 0.12f;
                        Color c = fill + new Color(vGrad, vGrad, vGrad, 0);
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        private static Sprite CreateCircleTexture(int size, Color color)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float r = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(r, r));
                    if (d < r - 1f)
                    {
                        tex.SetPixel(x, y, color);
                    }
                    else if (d < r)
                    {
                        float a = 1f - (d - (r - 1f));
                        tex.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * a));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Sprite CreateCrosshairTexture()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color clear = Color.clear;
            Color crossColor = new Color(0.15f, 0.95f, 0.90f, 0.95f);
            Color dotColor = Color.white;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    tex.SetPixel(x, y, clear);
                }
            }

            int center = size / 2;

            // Center precision dot
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    tex.SetPixel(center + dx, center + dy, dotColor);

            // 4 tactical ticks
            for (int i = 5; i <= 15; i++)
            {
                tex.SetPixel(center, center + i, crossColor);
                tex.SetPixel(center, center - i, crossColor);
                tex.SetPixel(center + i, center, crossColor);
                tex.SetPixel(center - i, center, crossColor);
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Texture2D CreateCyberGrid()
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color baseColor = new Color(0.08f, 0.10f, 0.15f);
            Color gridLine = new Color(0.12f, 0.75f, 0.95f);
            Color subLine = new Color(0.14f, 0.28f, 0.42f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isMajor = (x % 64 == 0) || (y % 64 == 0);
                    bool isMinor = (x % 16 == 0) || (y % 16 == 0);

                    if (isMajor) tex.SetPixel(x, y, gridLine);
                    else if (isMinor) tex.SetPixel(x, y, subLine);
                    else
                    {
                        float subtleNoise = (Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.04f);
                        tex.SetPixel(x, y, baseColor + new Color(subtleNoise, subtleNoise, subtleNoise));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateHangarFloor()
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color darkMetal = new Color(0.07f, 0.08f, 0.11f);
            Color panelEdge = new Color(0.16f, 0.18f, 0.24f);
            Color neonTrim = new Color(0.12f, 0.85f, 0.95f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isTileBorder = (x % 128 < 3) || (y % 128 < 3);
                    bool isCenterAcc = (x % 128 >= 60 && x % 128 <= 68) && (y % 128 >= 60 && y % 128 <= 68);

                    if (isCenterAcc) tex.SetPixel(x, y, neonTrim);
                    else if (isTileBorder) tex.SetPixel(x, y, panelEdge);
                    else
                    {
                        // Carbon weave micro-texture
                        bool weave = ((x / 4) + (y / 4)) % 2 == 0;
                        float brightness = weave ? 0.02f : -0.01f;
                        tex.SetPixel(x, y, darkMetal + new Color(brightness, brightness, brightness));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// High-resolution anime face texture with expressive gradient eyes,
        /// multiple specular highlights, sharp eyelashes, subtle nose bridge, and styled lips.
        /// </summary>
        private static Texture2D CreateHighResAnimeFace(Color skinTone, Color eyeColor, bool isFemale)
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color shadowSkin = Color.Lerp(skinTone, new Color(0.72f, 0.48f, 0.42f), 0.25f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Gentle ambient gradient on face
                    float gradY = (float)y / size;
                    Color c = Color.Lerp(shadowSkin, skinTone, Mathf.Clamp01(gradY * 1.2f));
                    tex.SetPixel(x, y, c);
                }
            }

            // Eyes: Left at x = 96, Right at x = 160, y = 135
            DrawHighResAnimeEye(tex, 96, 135, eyeColor, isFemale, false);
            DrawHighResAnimeEye(tex, 160, 135, eyeColor, isFemale, true);

            // Eyebrows (y = 175)
            Color browColor = Color.Lerp(skinTone, Color.black, 0.88f);
            for (int x = 76; x <= 112; x++)
            {
                int dy = (int)(Mathf.Sin((x - 76f) / 36f * Mathf.PI) * 4f);
                for (int t = 0; t <= 2; t++) tex.SetPixel(x, 175 + dy + t, browColor);
            }
            for (int x = 144; x <= 180; x++)
            {
                int dy = (int)(Mathf.Sin((180f - x) / 36f * Mathf.PI) * 4f);
                for (int t = 0; t <= 2; t++) tex.SetPixel(x, 175 + dy + t, browColor);
            }

            // Anime Nose bridge & tip (x = 127..129, y = 105..110)
            Color noseColor = shadowSkin;
            tex.SetPixel(127, 108, noseColor);
            tex.SetPixel(128, 108, noseColor);
            tex.SetPixel(128, 107, shadowSkin);

            // Anime Mouth / Gentle Smile (x = 114..142, y = 72)
            Color lipColor = isFemale ? new Color(0.92f, 0.45f, 0.52f) : new Color(0.82f, 0.48f, 0.48f);
            for (int x = 114; x <= 142; x++)
            {
                int dy = (int)(Mathf.Abs(x - 128) * 0.28f);
                tex.SetPixel(x, 72 + dy, lipColor);
                if (isFemale) tex.SetPixel(x, 71 + dy, new Color(lipColor.r, lipColor.g, lipColor.b, 0.8f));
            }

            // Anime Cheek Blush
            Color blushColor = Color.Lerp(skinTone, new Color(1.0f, 0.40f, 0.55f), isFemale ? 0.45f : 0.28f);
            for (int dy = -6; dy <= 6; dy++)
            {
                for (int dx = -12; dx <= 12; dx++)
                {
                    if (dx * dx * 0.4f + dy * dy <= 30)
                    {
                        float alpha = 1f - (dx * dx * 0.4f + dy * dy) / 30f;
                        Color curL = tex.GetPixel(82 + dx, 110 + dy);
                        Color curR = tex.GetPixel(174 + dx, 110 + dy);
                        tex.SetPixel(82 + dx, 110 + dy, Color.Lerp(curL, blushColor, alpha * 0.55f));
                        tex.SetPixel(174 + dx, 110 + dy, Color.Lerp(curR, blushColor, alpha * 0.55f));
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static void DrawHighResAnimeEye(Texture2D tex, int cx, int cy, Color irisColor, bool isFemale, bool isRight)
        {
            Color sclera = new Color(0.96f, 0.98f, 1.0f);
            Color darkPupil = new Color(0.04f, 0.05f, 0.08f);
            Color highlight1 = Color.white;
            Color highlight2 = new Color(0.85f, 0.95f, 1.0f, 0.85f);
            Color lashColor = new Color(0.08f, 0.08f, 0.10f);

            // 1. Sclera
            for (int y = -24; y <= 24; y++)
            {
                for (int x = -18; x <= 18; x++)
                {
                    if ((x * x * 2.2f) + (y * y) <= 480)
                    {
                        tex.SetPixel(cx + x, cy + y, sclera);
                    }
                }
            }

            // 2. Iris gradient (Deep at top, radiant vibrant at bottom)
            for (int y = -18; y <= 18; y++)
            {
                for (int x = -13; x <= 13; x++)
                {
                    if ((x * x * 2.2f) + (y * y) <= 240)
                    {
                        float t = Mathf.Clamp01((y + 18f) / 36f);
                        Color irisSample = Color.Lerp(darkPupil, irisColor, t * 1.3f);
                        // Bottom luminance glow
                        if (y < -4)
                        {
                            irisSample = Color.Lerp(irisSample, Color.white, (-(y + 4f) / 14f) * 0.45f);
                        }
                        tex.SetPixel(cx + x, cy + y, irisSample);
                    }
                }
            }

            // 3. Pupil
            for (int y = -8; y <= 8; y++)
            {
                for (int x = -6; x <= 6; x++)
                {
                    if ((x * x * 2.2f) + (y * y) <= 60)
                    {
                        tex.SetPixel(cx + x, cy + y, darkPupil);
                    }
                }
            }

            // 4. Large specular highlight (Top Left)
            int hx = isRight ? cx - 5 : cx - 6;
            int hy = cy + 8;
            for (int dy = -3; dy <= 3; dy++)
            {
                for (int dx = -3; dx <= 3; dx++)
                {
                    if (dx * dx + dy * dy <= 8)
                        tex.SetPixel(hx + dx, hy + dy, highlight1);
                }
            }

            // 5. Small secondary highlight (Bottom Right)
            int sx = isRight ? cx + 6 : cx + 5;
            int sy = cy - 6;
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    tex.SetPixel(sx + dx, sy + dy, highlight2);

            // 6. Upper Eyelash line & Wing
            for (int x = -22; x <= 22; x++)
            {
                float arch = Mathf.Cos(x / 22f * Mathf.PI * 0.5f);
                int ey = (int)(cy + 18 + arch * 6f);
                for (int th = 0; th <= (isFemale ? 4 : 2); th++)
                {
                    tex.SetPixel(cx + x, ey + th, lashColor);
                }
            }
        }

        private static Texture2D CreateHairTexture(Color baseColor)
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            Color tipColor = Color.Lerp(baseColor, Color.white, 0.25f);
            Color shadowColor = Color.Lerp(baseColor, Color.black, 0.45f);

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / size;
                // Vertical gradient with anime specular "angel ring" sheen around y = 60%
                float sheen = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(v - 0.62f) * 6f), 2f) * 0.35f;

                for (int x = 0; x < size; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.15f, y * 0.03f) * 0.08f;
                    Color c = Color.Lerp(shadowColor, tipColor, v) + new Color(sheen + noise, sheen + noise, sheen + noise);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateOutfitTexture(int preset, bool isFemale)
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

            Color primary = new Color(0.12f, 0.14f, 0.18f); // Urban Dark
            Color secondary = new Color(0.22f, 0.26f, 0.34f);
            Color accent = new Color(0.12f, 0.85f, 0.95f); // Neon Cyan

            if (preset == 1) // Cyber Ronin
            {
                primary = new Color(0.10f, 0.10f, 0.12f);
                secondary = new Color(0.72f, 0.12f, 0.16f); // Crimson
                accent = new Color(0.95f, 0.78f, 0.22f);   // Gold
            }
            else if (preset == 2) // Desert Commando
            {
                primary = new Color(0.48f, 0.44f, 0.36f); // Tan
                secondary = new Color(0.32f, 0.30f, 0.24f);
                accent = new Color(0.85f, 0.55f, 0.20f);
            }
            else if (preset == 3) // Aether Vanguard
            {
                primary = new Color(0.90f, 0.92f, 0.96f); // White armor
                secondary = new Color(0.20f, 0.22f, 0.30f);
                accent = new Color(0.20f, 0.85f, 0.95f); // Aether Cyan
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isStrap = (x >= 40 && x <= 56) || (x >= 200 && x <= 216) || (y >= 120 && y <= 136);
                    bool isArmorPlate = (y >= 160 && y <= 230 && x >= 70 && x <= 186);
                    bool isNeonTrim = (y == 138 || y == 118 || (isArmorPlate && (x == 70 || x == 186 || y == 160 || y == 230)));

                    if (isNeonTrim) tex.SetPixel(x, y, accent);
                    else if (isArmorPlate) tex.SetPixel(x, y, secondary);
                    else if (isStrap) tex.SetPixel(x, y, new Color(0.08f, 0.08f, 0.10f));
                    else
                    {
                        // Tactical fabric weave
                        bool weave = ((x / 2) + (y / 2)) % 2 == 0;
                        float w = weave ? 0.02f : -0.02f;
                        tex.SetPixel(x, y, primary + new Color(w, w, w));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateSkyscraperTexture(Color wallColor, Color windowLitColor)
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color darkGlass = new Color(0.08f, 0.12f, 0.18f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isFloorBeam = (y % 32 < 4);
                    bool isColBeam = (x % 32 < 4);

                    if (isFloorBeam || isColBeam)
                    {
                        tex.SetPixel(x, y, wallColor);
                    }
                    else
                    {
                        // Check if window is lit
                        int windowIdx = (x / 32) + (y / 32) * 8;
                        bool isLit = ((windowIdx * 37 + 13) % 5 == 0) || ((windowIdx * 19 + 7) % 7 == 0);
                        tex.SetPixel(x, y, isLit ? windowLitColor : darkGlass);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateNeonSignTexture(string text, Color neonColor)
        {
            const int width = 256;
            const int height = 128;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            Color bg = new Color(0.05f, 0.06f, 0.09f, 0.95f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = (x < 6 || x >= width - 6 || y < 6 || y >= height - 6);
                    if (isBorder)
                    {
                        tex.SetPixel(x, y, neonColor);
                    }
                    else
                    {
                        // Glowing center pill
                        float cx = (float)x / width;
                        float cy = (float)y / height;
                        float glow = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(cx, cy), new Vector2(0.5f, 0.5f)) * 2f);
                        tex.SetPixel(x, y, Color.Lerp(bg, neonColor, glow * 0.4f));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateFoliageTexture(Color leafColor, bool isSakura)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color shadowLeaf = Color.Lerp(leafColor, Color.black, 0.35f);
            Color highlightLeaf = isSakura ? new Color(1.0f, 0.85f, 0.92f) : Color.Lerp(leafColor, Color.yellow, 0.25f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(size * 0.5f, size * 0.5f)) / (size * 0.5f);
                    float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);

                    if (d + noise * 0.3f < 0.85f)
                    {
                        Color c = Color.Lerp(shadowLeaf, highlightLeaf, noise);
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateBarkTexture(Color barkColor)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color darkCrevice = Color.Lerp(barkColor, Color.black, 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Vertical grain streaks
                    float grain = Mathf.PerlinNoise(x * 0.25f, y * 0.04f);
                    Color c = Color.Lerp(darkCrevice, barkColor, grain);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateRockTexture(Color rockColor)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color mossColor = new Color(0.24f, 0.42f, 0.18f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                    float mossNoise = Mathf.PerlinNoise(x * 0.04f + 20f, y * 0.04f + 20f);

                    Color c = Color.Lerp(rockColor * 0.7f, rockColor * 1.2f, noise);
                    if (mossNoise > 0.65f)
                    {
                        c = Color.Lerp(c, mossColor, (mossNoise - 0.65f) * 3f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateWaterTexture()
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color deepWater = new Color(0.06f, 0.35f, 0.65f, 0.88f);
            Color shallowWater = new Color(0.15f, 0.75f, 0.85f, 0.82f);
            Color waveFoam = new Color(0.92f, 0.98f, 1.0f, 0.95f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float wave1 = Mathf.Sin((x + y) * 0.12f);
                    float wave2 = Mathf.Cos((x - y) * 0.08f);
                    float combined = (wave1 + wave2) * 0.5f;

                    if (combined > 0.82f)
                    {
                        tex.SetPixel(x, y, waveFoam);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.Lerp(deepWater, shallowWater, combined * 0.5f + 0.5f));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateGliderPattern()
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color carbon = new Color(0.08f, 0.10f, 0.14f);
            Color cyberGlow = new Color(0.12f, 0.85f, 0.95f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isChevrons = Mathf.Abs(x - 128) == y || Mathf.Abs(x - 128) == y - 40;
                    if (isChevrons && y > 30) tex.SetPixel(x, y, cyberGlow);
                    else tex.SetPixel(x, y, carbon);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateSkyboxPanorama()
        {
            const int width = 512;
            const int height = 256;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color zenith = new Color(0.14f, 0.42f, 0.82f);   // Deep Anime Blue
            Color midSky = new Color(0.38f, 0.68f, 0.95f);   // Bright Sky Cyan
            Color horizon = new Color(0.96f, 0.84f, 0.72f);  // Peachy Warm Horizon
            Color ground = new Color(0.18f, 0.22f, 0.28f);   // Dark Haze Base
            Color cloudColor = new Color(1.0f, 0.98f, 0.96f, 0.90f);

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                Color skyCol;
                if (v < 0.45f)
                {
                    skyCol = Color.Lerp(ground, horizon, v / 0.45f);
                }
                else if (v < 0.7f)
                {
                    skyCol = Color.Lerp(horizon, midSky, (v - 0.45f) / 0.25f);
                }
                else
                {
                    skyCol = Color.Lerp(midSky, zenith, (v - 0.7f) / 0.3f);
                }

                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    // Stylized anime cloud band between v = 0.45 and 0.65
                    if (v > 0.45f && v < 0.68f)
                    {
                        float cloudNoise = Mathf.PerlinNoise(u * 6f, v * 8f);
                        if (cloudNoise > 0.58f)
                        {
                            float alpha = Mathf.Clamp01((cloudNoise - 0.58f) * 3f);
                            skyCol = Color.Lerp(skyCol, cloudColor, alpha * 0.75f);
                        }
                    }
                    tex.SetPixel(x, y, skyCol);
                }
            }
            tex.Apply();
            return tex;
        }

        #endregion
    }
}
