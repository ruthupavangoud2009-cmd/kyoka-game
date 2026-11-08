using UnityEngine;
using System.Collections.Generic;

namespace Kyoka.Art
{
    /// <summary>
    /// Runtime procedural texture and sprite factory for KYOKA.
    /// Produces high-resolution, pixel-crisp UI sprites, cybernetic floor tiles, crosshairs,
    /// and facial features directly into Unity memory with zero disk overhead.
    /// </summary>
    public static class KyokaTextureGenerator
    {
        private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Texture2D> texCache = new Dictionary<string, Texture2D>();

        public static Sprite GetPanelSprite()
        {
            return GetOrCreateSprite("UI_Panel_Rounded", () => CreateRoundedRect(64, 64, 12, new Color(0.06f, 0.08f, 0.12f, 0.88f), new Color(0.2f, 0.5f, 0.8f, 0.9f)));
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

        public static Texture2D GetFaceTexture(Color skinTone, Color eyeColor)
        {
            string key = $"Tex_Face_{ColorUtility.ToHtmlStringRGBA(skinTone)}_{ColorUtility.ToHtmlStringRGBA(eyeColor)}";
            return GetOrCreateTexture(key, () => CreateAnimeFaceTexture(skinTone, eyeColor));
        }

        public static Texture2D GetGliderTexture()
        {
            return GetOrCreateTexture("Tex_Glider", CreateGliderPattern);
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
                    else if (dx <= 2 || dy <= 2)
                    {
                        tex.SetPixel(x, y, border);
                    }
                    else
                    {
                        tex.SetPixel(x, y, fill);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        private static Sprite CreateCircleTexture(int size, Color color)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = size * 0.48f;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius - 1f)
                    {
                        tex.SetPixel(x, y, color);
                    }
                    else if (dist <= radius)
                    {
                        float alpha = 1f - (dist - (radius - 1f));
                        tex.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
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
            Color crossColor = new Color(0.1f, 0.95f, 0.85f, 0.9f);
            Color dotColor = Color.white;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    tex.SetPixel(x, y, clear);
                }
            }

            int center = size / 2;

            // Center dot
            tex.SetPixel(center, center, dotColor);
            tex.SetPixel(center - 1, center, dotColor);
            tex.SetPixel(center + 1, center, dotColor);
            tex.SetPixel(center, center - 1, dotColor);
            tex.SetPixel(center, center + 1, dotColor);

            // 4 ticks (North, South, East, West)
            for (int i = 6; i <= 14; i++)
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
            Color baseColor = new Color(0.08f, 0.10f, 0.14f);
            Color gridLine = new Color(0.15f, 0.75f, 0.95f);
            Color subLine = new Color(0.12f, 0.25f, 0.35f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isMajor = (x % 64 == 0) || (y % 64 == 0);
                    bool isMinor = (x % 16 == 0) || (y % 16 == 0);

                    if (isMajor)
                    {
                        tex.SetPixel(x, y, gridLine);
                    }
                    else if (isMinor)
                    {
                        tex.SetPixel(x, y, subLine);
                    }
                    else
                    {
                        float subtleNoise = (UnityEngine.Random.value * 0.03f);
                        tex.SetPixel(x, y, baseColor + new Color(subtleNoise, subtleNoise, subtleNoise));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateAnimeFaceTexture(Color skinTone, Color eyeColor)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    tex.SetPixel(x, y, skinTone);
                }
            }

            // Head rotated 90 deg has front face at x = 64 (center)
            // Left Eye around x = 50, Right Eye around x = 78, y = 60
            DrawAnimeEye(tex, 50, 60, eyeColor);
            DrawAnimeEye(tex, 78, 60, eyeColor);

            // Eyebrows (y = 78)
            Color browColor = Color.Lerp(skinTone, Color.black, 0.85f);
            for (int x = 42; x <= 56; x++) tex.SetPixel(x, 78 + (x - 42) / 4, browColor);
            for (int x = 72; x <= 86; x++) tex.SetPixel(x, 78 + (86 - x) / 4, browColor);

            // Subtle anime smile (x=58..70, y=36)
            Color mouthColor = new Color(0.85f, 0.4f, 0.45f);
            for (int x = 58; x <= 70; x++)
            {
                int dy = (int)(Mathf.Abs(x - 64) * 0.35f);
                tex.SetPixel(x, 36 + dy, mouthColor);
            }

            // Soft anime cheek blush
            Color blushColor = Color.Lerp(skinTone, new Color(1f, 0.45f, 0.55f), 0.45f);
            for (int dy = -3; dy <= 3; dy++)
            {
                for (int dx = -5; dx <= 5; dx++)
                {
                    if (dx * dx + dy * dy * 2 <= 20)
                    {
                        tex.SetPixel(44 + dx, 48 + dy, blushColor);
                        tex.SetPixel(84 + dx, 48 + dy, blushColor);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static void DrawAnimeEye(Texture2D tex, int cx, int cy, Color irisColor)
        {
            Color sclera = new Color(0.96f, 0.98f, 1.0f);
            Color pupil = Color.black;
            Color highlight = Color.white;
            Color lashColor = new Color(0.1f, 0.1f, 0.12f);

            // Sclera oval
            for (int y = -14; y <= 14; y++)
            {
                for (int x = -10; x <= 10; x++)
                {
                    if ((x * x * 2) + (y * y) <= 180)
                    {
                        tex.SetPixel(cx + x, cy + y, sclera);
                    }
                }
            }

            // Iris
            for (int y = -10; y <= 10; y++)
            {
                for (int x = -7; x <= 7; x++)
                {
                    if ((x * x * 2) + (y * y) <= 80)
                    {
                        float grad = Mathf.Clamp01((y + 10f) / 20f);
                        tex.SetPixel(cx + x, cy + y, Color.Lerp(Color.black, irisColor, grad));
                    }
                }
            }

            // Pupil
            for (int y = -5; y <= 5; y++)
            {
                for (int x = -4; x <= 4; x++)
                {
                    if ((x * x * 2) + (y * y) <= 24)
                    {
                        tex.SetPixel(cx + x, cy + y, pupil);
                    }
                }
            }

            // Anime light reflection highlight
            tex.SetPixel(cx - 3, cy + 5, highlight);
            tex.SetPixel(cx - 2, cy + 5, highlight);
            tex.SetPixel(cx - 3, cy + 4, highlight);
            tex.SetPixel(cx + 3, cy - 3, highlight);

            // Upper Eyelash line
            for (int x = -12; x <= 12; x++)
            {
                int y = 14 - (int)(Mathf.Abs(x) * 0.4f);
                tex.SetPixel(cx + x, cy + y, lashColor);
                tex.SetPixel(cx + x, cy + y + 1, lashColor);
            }
        }

        private static Texture2D CreateGliderPattern()
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color darkCarbon = new Color(0.12f, 0.13f, 0.16f);
            Color neonStripe = new Color(0.15f, 0.85f, 0.95f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool stripe = Mathf.Abs((x - y) % 32) < 4 || (y > 115);
                    tex.SetPixel(x, y, stripe ? neonStripe : darkCarbon);
                }
            }
            tex.Apply();
            return tex;
        }

        #endregion
    }
}

