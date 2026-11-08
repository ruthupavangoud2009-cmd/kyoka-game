using UnityEngine;
using System.IO;

namespace Kyoka.Save
{
    public static class KyokaSaveSystem
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "kyoka_save.json");

        public static void Save(KyokaSaveData data)
        {
            if (data == null) return;
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        }

        public static KyokaSaveData Load()
        {
            if (!File.Exists(SavePath)) return new KyokaSaveData();
            try { return JsonUtility.FromJson<KyokaSaveData>(File.ReadAllText(SavePath)); }
            catch { return new KyokaSaveData(); }
        }
    }
}
