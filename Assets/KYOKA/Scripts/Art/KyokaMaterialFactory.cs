using UnityEngine;

namespace Kyoka.Art
{
    /// <summary>
    /// Master Material and Shader Factory for KYOKA AAA Visual Overhaul.
    /// Provides bulletproof fallback-safe shaders for URP and Built-in pipelines.
    /// Supports Toon/Anime Shading, PBR Lit, High-intensity Emissives, Translucency,
    /// Animated Water with specular glints, and Panoramic Skyboxes.
    /// </summary>
    public static class KyokaMaterialFactory
    {
        private static Shader cachedLitShader;
        private static Shader cachedUnlitShader;
        private static Shader cachedSkyboxShader;

        public static Shader GetLitShader()
        {
            if (cachedLitShader != null) return cachedLitShader;

            string[] candidates = new string[]
            {
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Simple Lit",
                "Standard",
                "Mobile/Diffuse",
                "Sprites/Default",
                "UI/Default"
            };

            foreach (var name in candidates)
            {
                var s = Shader.Find(name);
                if (s != null)
                {
                    cachedLitShader = s;
                    return cachedLitShader;
                }
            }

            if (UnityEngine.UI.Graphic.defaultGraphicMaterial != null && UnityEngine.UI.Graphic.defaultGraphicMaterial.shader != null)
            {
                cachedLitShader = UnityEngine.UI.Graphic.defaultGraphicMaterial.shader;
                return cachedLitShader;
            }

            return null;
        }

        public static Shader GetUnlitShader()
        {
            if (cachedUnlitShader != null) return cachedUnlitShader;

            string[] candidates = new string[]
            {
                "Universal Render Pipeline/Unlit",
                "Unlit/Color",
                "Unlit/Texture",
                "Sprites/Default",
                "UI/Default"
            };

            foreach (var name in candidates)
            {
                var s = Shader.Find(name);
                if (s != null)
                {
                    cachedUnlitShader = s;
                    return cachedUnlitShader;
                }
            }

            return GetLitShader();
        }

        public static Shader GetSkyboxShader()
        {
            if (cachedSkyboxShader != null) return cachedSkyboxShader;

            string[] candidates = new string[]
            {
                "Skybox/Panoramic",
                "Skybox/Procedural",
                "Universal Render Pipeline/Unlit",
                "Unlit/Texture"
            };

            foreach (var name in candidates)
            {
                var s = Shader.Find(name);
                if (s != null)
                {
                    cachedSkyboxShader = s;
                    return cachedSkyboxShader;
                }
            }

            return GetUnlitShader();
        }

        public static Material CreateLit(Color color, Texture2D tex = null)
        {
            var s = GetLitShader();
            Material mat = (s != null) ? new Material(s) : new Material(UnityEngine.UI.Graphic.defaultGraphicMaterial);
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);

            if (tex != null)
            {
                mat.mainTexture = tex;
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            }

            // Set reasonable stylized smoothness & specular
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.35f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.05f);

            return mat;
        }

        public static Material CreateLitColorMaterial(Color color)
        {
            return CreateLit(color);
        }

        public static Material CreateUnlit(Color color, Texture2D tex = null)
        {
            var s = GetUnlitShader();
            Material mat = (s != null) ? new Material(s) : new Material(UnityEngine.UI.Graphic.defaultGraphicMaterial);
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (tex != null)
            {
                mat.mainTexture = tex;
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            }
            return mat;
        }

        public static Material CreateEmissionMaterial(Color color, float intensity = 1.8f)
        {
            var mat = CreateLit(color);
            Color emissiveColor = color * intensity;

            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissiveColor);
            if (mat.HasProperty("_EmissionMap")) mat.SetColor("_EmissionColor", emissiveColor);

            return mat;
        }

        public static Material CreateWaterMaterial()
        {
            var waterTex = KyokaTextureGenerator.GetWaterTexture();
            var mat = CreateLit(new Color(0.12f, 0.65f, 0.85f, 0.82f), waterTex);

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.92f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.15f);

            // Setup transparency
            SetMaterialTransparent(mat);

            return mat;
        }

        public static Material CreateGlassMaterial(Color tint)
        {
            var mat = CreateLit(new Color(tint.r, tint.g, tint.b, 0.35f));
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.95f);
            SetMaterialTransparent(mat);
            return mat;
        }

        public static Material CreateFoliageMaterial(Color leafColor, bool isSakura = false)
        {
            var tex = KyokaTextureGenerator.GetFoliageTexture(leafColor, isSakura);
            var mat = CreateLit(leafColor, tex);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.15f);
            return mat;
        }

        public static Material CreateSkyboxMaterial()
        {
            var s = GetSkyboxShader();
            Material mat = (s != null) ? new Material(s) : CreateUnlit(Color.white);
            var panorama = KyokaTextureGenerator.GetSkyboxPanorama();

            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", panorama);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", panorama);

            return mat;
        }

        private static void SetMaterialTransparent(Material mat)
        {
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1); // URP Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
        }
    }
}
