#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

namespace Kyoka.Editor
{
    public static class KyokaProjectSetup
    {
        [MenuItem("KYOKA/Create / Repair Bootstrap Scene")]
        public static void CreateBootstrapScene()
        {
            string folder = "Assets/KYOKA/Scenes";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                if (!AssetDatabase.IsValidFolder("Assets/KYOKA")) AssetDatabase.CreateFolder("Assets", "KYOKA");
                AssetDatabase.CreateFolder("Assets/KYOKA", "Scenes");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var root = new GameObject("KYOKA_Runtime");
            root.AddComponent<Kyoka.Core.KyokaRuntimeBootstrap>();
            EditorSceneManager.SaveScene(scene, folder + "/KYOKA_Bootstrap.unity");

            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            const string scenePath = "Assets/KYOKA/Scenes/KYOKA_Bootstrap.unity";
            if (!scenes.Exists(s => s.path == scenePath))
                scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();

            AssetDatabase.SaveAssets();
            Debug.Log("KYOKA bootstrap scene created and added to Build Settings.");
        }

        [InitializeOnLoadMethod]
        private static void ScheduleSetup()
        {
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying &&
                    !File.Exists("Assets/KYOKA/Scenes/KYOKA_Bootstrap.unity"))
                {
                    // Run setup from menu to avoid unexpectedly replacing an open scene.
                    Debug.Log("KYOKA: Use menu KYOKA > Create / Repair Bootstrap Scene.");
                }
            };
        }
    }
}
#endif
