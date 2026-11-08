using UnityEngine;
using Kyoka.Save;

namespace Kyoka.Character
{
    /// <summary>
    /// Manages the anime/realistic hybrid visual customization for KYOKA characters.
    /// Handles hair styling, coloration, outfit palettes, and persistence.
    /// </summary>
    public class KyokaCharacterCustomization : MonoBehaviour
    {
        [Header("Renderers to Tint")]
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Renderer hairRenderer;
        [SerializeField] private Renderer outfitRenderer;

        [Header("Current Visual Settings")]
        public CharacterCustomizationData data = new CharacterCustomizationData();

        private void Start()
        {
            // Find renderers if not explicitly assigned
            if (bodyRenderer == null) bodyRenderer = GetComponentInChildren<Renderer>();
            ApplyCustomization(data);
        }

        public void SetHairStyle(int index)
        {
            data.hairStyle = index;
            // Additional hair mesh activation can occur here
        }

        public void SetHairColor(Color color)
        {
            data.hairColorR = color.r;
            data.hairColorG = color.g;
            data.hairColorB = color.b;
            if (hairRenderer != null && hairRenderer.material != null)
            {
                hairRenderer.material.color = color;
            }
        }

        public void SetSkinTone(Color color)
        {
            data.skinToneR = color.r;
            data.skinToneG = color.g;
            data.skinToneB = color.b;
            if (bodyRenderer != null && bodyRenderer.material != null)
            {
                bodyRenderer.material.color = color;
            }
        }

        public void SetOutfitColor(Color color)
        {
            data.outfitColorR = color.r;
            data.outfitColorG = color.g;
            data.outfitColorB = color.b;
            if (outfitRenderer != null && outfitRenderer.material != null)
            {
                outfitRenderer.material.color = color;
            }
        }

        public void ApplyCustomization(CharacterCustomizationData incomingData)
        {
            if (incomingData == null) return;
            data = incomingData;

            Color hairCol = new Color(data.hairColorR, data.hairColorG, data.hairColorB);
            Color skinCol = new Color(data.skinToneR, data.skinToneG, data.skinToneB);
            Color outfitCol = new Color(data.outfitColorR, data.outfitColorG, data.outfitColorB);

            if (hairRenderer != null && hairRenderer.material != null) hairRenderer.material.color = hairCol;
            if (bodyRenderer != null && bodyRenderer.material != null) bodyRenderer.material.color = skinCol;
            if (outfitRenderer != null && outfitRenderer.material != null) outfitRenderer.material.color = outfitCol;
        }

        public void SaveTo(KyokaSaveData saveData)
        {
            if (saveData != null)
            {
                saveData.customization = data;
            }
        }

        public void LoadFrom(KyokaSaveData saveData)
        {
            if (saveData != null && saveData.customization != null)
            {
                ApplyCustomization(saveData.customization);
            }
        }
    }
}

