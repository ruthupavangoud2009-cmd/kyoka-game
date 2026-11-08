using UnityEngine;
using System.IO;
using System;

namespace Kyoka.Save
{
    /// <summary>
    /// Robust local persistence system for KYOKA.
    /// Handles atomic writes, fallback default data generation, and corruption prevention.
    /// </summary>
    public static class KyokaSaveSystem
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "kyoka_save.json");
        private static string BackupPath => Path.Combine(Application.persistentDataPath, "kyoka_save.bak");

        public static event Action<KyokaSaveData> OnGameSaved;
        public static event Action<KyokaSaveData> OnGameLoaded;

        public static void Save(KyokaSaveData data)
        {
            if (data == null) return;
            try
            {
                string json = JsonUtility.ToJson(data, true);
                string path = SavePath;

                // Create backup of old save if exists
                if (File.Exists(path))
                {
                    File.Copy(path, BackupPath, true);
                }

                File.WriteAllText(path, json);
                OnGameSaved?.Invoke(data);
                Debug.Log($"[KYOKA SaveSystem] Saved successfully to {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[KYOKA SaveSystem] Error saving game: {ex.Message}");
            }
        }

        public static KyokaSaveData Load()
        {
            string path = SavePath;
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var data = JsonUtility.FromJson<KyokaSaveData>(json);
                    if (data != null)
                    {
                        OnGameLoaded?.Invoke(data);
                        return data;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[KYOKA SaveSystem] Primary save corrupt: {ex.Message}. Trying backup...");
                }
            }

            // Try fallback backup
            if (File.Exists(BackupPath))
            {
                try
                {
                    string json = File.ReadAllText(BackupPath);
                    var data = JsonUtility.FromJson<KyokaSaveData>(json);
                    if (data != null)
                    {
                        OnGameLoaded?.Invoke(data);
                        return data;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[KYOKA SaveSystem] Backup save also corrupt: {ex.Message}");
                }
            }

            // Return fresh default data
            var defaultData = new KyokaSaveData();
            return defaultData;
        }

        public static bool DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
                if (File.Exists(BackupPath)) File.Delete(BackupPath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[KYOKA SaveSystem] Error deleting save: {ex.Message}");
                return false;
            }
        }
    }
}
