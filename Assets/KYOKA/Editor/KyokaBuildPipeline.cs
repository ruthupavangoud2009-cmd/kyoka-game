#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Kyoka.Editor
{
    public static class KyokaBuildPipeline
    {
        private const string WindowsOutputDir = "Builds/Windows";
        private const string AndroidOutputDir = "Builds/Android";
        private const string BootstrapScenePath = "Assets/KYOKA/Scenes/KYOKA_Bootstrap.unity";

        [MenuItem("KYOKA/Build/Windows (KYOKA.exe)")]
        public static void BuildWindows()
        {
            Debug.Log("[KYOKA Build] Commencing Windows Standalone Build...");

            EnsureBootstrapSceneExists();

            if (!Directory.Exists(WindowsOutputDir))
            {
                Directory.CreateDirectory(WindowsOutputDir);
            }

            string exePath = Path.Combine(WindowsOutputDir, "KYOKA.exe");

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = GetBuildScenes(),
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[KYOKA Build] Windows build SUCCEEDED: {summary.totalSize} bytes at {exePath}");
                GenerateBuildNotes(WindowsOutputDir, "Windows PC 64-bit", exePath);
            }
            else
            {
                Debug.LogError($"[KYOKA Build] Windows build FAILED with {summary.totalErrors} errors.");
            }
        }

        [MenuItem("KYOKA/Build/Android APK (KYOKA.apk)")]
        public static void BuildAndroid()
        {
            Debug.Log("[KYOKA Build] Commencing Android APK Build (ARM64)...");

            EnsureBootstrapSceneExists();

            // Configure Android SDK / NDK / OpenJDK paths if present in Editor folder
            string editorData = EditorApplication.applicationContentsPath;
            string androidPlayer = Path.Combine(editorData, "PlaybackEngines", "AndroidPlayer");
            string sdkPath = Path.Combine(androidPlayer, "SDK");
            string ndkPath = Path.Combine(androidPlayer, "NDK");
            string jdkPath = Path.Combine(androidPlayer, "OpenJDK");

            if (Directory.Exists(sdkPath))
            {
                EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
                Debug.Log($"[KYOKA Build] Configured Android SDK: {sdkPath}");
            }
            if (Directory.Exists(ndkPath))
            {
                EditorPrefs.SetString("AndroidNdkRootR23b", ndkPath);
                EditorPrefs.SetString("AndroidNdkRoot", ndkPath);
                Debug.Log($"[KYOKA Build] Configured Android NDK: {ndkPath}");
            }
            if (Directory.Exists(jdkPath))
            {
                EditorPrefs.SetString("JdkPath", jdkPath);
                Debug.Log($"[KYOKA Build] Configured OpenJDK: {jdkPath}");
            }

            // Configure Android player settings
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
            EditorUserBuildSettings.buildAppBundle = false; // Generate APK

            if (!Directory.Exists(AndroidOutputDir))
            {
                Directory.CreateDirectory(AndroidOutputDir);
            }

            string apkPath = Path.Combine(AndroidOutputDir, "KYOKA.apk");

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = GetBuildScenes(),
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[KYOKA Build] Android APK build SUCCEEDED: {summary.totalSize} bytes at {apkPath}");
                GenerateBuildNotes(AndroidOutputDir, "Android ARM64", apkPath);
            }
            else
            {
                Debug.LogError($"[KYOKA Build] Android build FAILED with {summary.totalErrors} errors.");
            }
        }

        [MenuItem("KYOKA/Build/Validate Project")]
        public static void ValidateProject()
        {
            Debug.Log("[KYOKA Validate] Validating Project configuration, packages, and assets...");

            bool hasScenes = File.Exists(BootstrapScenePath);
            bool hasManifest = File.Exists("Packages/manifest.json");

            Debug.Log($"[KYOKA Validate] Manifest present: {hasManifest}");
            Debug.Log($"[KYOKA Validate] Bootstrap scene present: {hasScenes}");
            Debug.Log($"[KYOKA Validate] Product Name: {PlayerSettings.productName}");
            Debug.Log($"[KYOKA Validate] Bundle Identifier: {PlayerSettings.applicationIdentifier}");
            Debug.Log($"[KYOKA Validate] Android Target Supported: {UnityEditor.BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android)}");

            Debug.Log("[KYOKA Validate] Project validation completed.");
        }

        [MenuItem("KYOKA/Build/Open Build Folder")]
        public static void OpenBuildFolder()
        {
            string fullPath = Path.GetFullPath("Builds");
            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
            EditorUtility.RevealInFinder(fullPath);
        }

        private static string[] GetBuildScenes()
        {
            var scenes = new List<string>();
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (s.enabled && File.Exists(s.path))
                {
                    scenes.Add(s.path);
                }
            }

            if (scenes.Count == 0 && File.Exists(BootstrapScenePath))
            {
                scenes.Add(BootstrapScenePath);
            }

            return scenes.ToArray();
        }

        private static void EnsureBootstrapSceneExists()
        {
            if (!File.Exists(BootstrapScenePath))
            {
                KyokaProjectSetup.CreateBootstrapScene();
            }
        }

        private static void GenerateBuildNotes(string folder, string platform, string outputBinary)
        {
            string docDir = Path.Combine(folder, "Documentation");
            if (!Directory.Exists(docDir)) Directory.CreateDirectory(docDir);

            string info = $"# KYOKA {platform} Build Notes\n\n" +
                          $"Date: {System.DateTime.UtcNow:u}\n" +
                          $"Binary: {Path.GetFileName(outputBinary)}\n" +
                          $"Architecture: ARM64 / x64\n" +
                          $"Version: {PlayerSettings.bundleVersion}\n";

            File.WriteAllText(Path.Combine(docDir, "BUILD_INFO.txt"), info);
        }
    }
}
#endif

