using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kyoka.Save
{
    [Serializable]
    public class SavedItemStack
    {
        public string itemId;
        public int quantity;
    }

    [Serializable]
    public class CharacterCustomizationData
    {
        public string playerName = "Kaito";
        public bool isFemale = false;
        public int outfitPreset = 0;
        public string equippedWeaponId = "AssaultRifle";
        public bool hasCreatedCharacter = false;
        public int hairStyle = 0;
        public float hairColorR = 0.12f, hairColorG = 0.75f, hairColorB = 0.95f;
        public float eyeColorR = 0.1f, eyeColorG = 0.85f, eyeColorB = 0.7f;
        public float outfitColorR = 0.15f, outfitColorG = 0.2f, outfitColorB = 0.35f;
        public float skinToneR = 0.96f, skinToneG = 0.82f, skinToneB = 0.73f;
    }

    [Serializable]
    public class SavedSettingsData
    {
        public int graphicsQualityIndex = 1; // 0=Low, 1=Medium, 2=High
        public float renderScale = 1.0f;
        public int targetFrameRate = 60;
        public float masterVolume = 0.8f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.8f;
        public float mouseSensitivity = 2.0f;
        public float touchSensitivity = 1.5f;
        public bool invertY = false;
        public bool fullscreen = true;
    }

    [Serializable]
    public class SavedMissionProgress
    {
        public string missionId;
        public int currentProgress;
        public bool isCompleted;
    }

    [Serializable]
    public class KyokaSaveData
    {
        // Player Transform & State
        public float posX;
        public float posY;
        public float posZ;
        public float rotY;
        public bool hasSavedPosition;
        public int currentHealth = 100;
        public int maxHealth = 100;
        public float currentEnergy = 100;

        // Progression & Economy
        public int playerLevel = 1;
        public int currentXP = 0;
        public int kyokaCredits = 500;
        public int aetherShards = 10;

        // Customization
        public CharacterCustomizationData customization = new CharacterCustomizationData();

        // Inventory
        public List<SavedItemStack> inventoryItems = new List<SavedItemStack>();

        // Missions
        public List<SavedMissionProgress> missions = new List<SavedMissionProgress>();

        // Settings
        public SavedSettingsData settings = new SavedSettingsData();

        // Game Mode & Region
        public int lastSelectedGameMode = 0; // 0=OpenWorld, 1=BattleRoyale, etc.
        public int lastWorldRegion = 0;     // 0=KYOKACity, 1=Wildlands, etc.
    }
}
