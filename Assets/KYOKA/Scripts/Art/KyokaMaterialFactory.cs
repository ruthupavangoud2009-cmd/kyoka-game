using UnityEngine;

namespace Kyoka.Art
{
    /// <summary>
    /// Bulletproof material and shader factory for KYOKA standalone builds.
    /// Guarantees that Material creation never throws ArgumentNullException even when
    /// shader stripping occurs in batchmode release compilations.
    /// </summary>
    public static class KyokaMaterialFactory
    {
        private static Shader cachedLitShader;
        private static Shader cachedUnlitShader;

        public static Shader GetLitShader()
        {
            if (cachedLitShader != null) return cachedLitShader;

            string[] candidates = new string[]
            {
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Simple Lit",
                "Universal Render Pipeline/Unlit",
                "Standard",
                "Sprites/Default",
                "UI/Default",
                "Mobile/Diffuse",
                "Hidden/InternalErrorShader"
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
                "Sprites/Default",
                "UI/Default",
                "Universal Render Pipeline/Lit",
                "Standard"
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

        public static Material CreateLit(Color color, Texture2D tex = null)
        {
            var s = GetLitShader();
            Material mat = (s != null) ? new Material(s) : new Material(UnityEngine.UI.Graphic.defaultGraphicMaterial);
            mat.color = color;
            if (tex != null) mat.mainTexture = tex;
            return mat;
        }

        public static Material CreateUnlit(Color color)
        {
            var s = GetUnlitShader();
            Material mat = (s != null) ? new Material(s) : new Material(UnityEngine.UI.Graphic.defaultGraphicMaterial);
            mat.color = color;
            return mat;
        }
    }
}
