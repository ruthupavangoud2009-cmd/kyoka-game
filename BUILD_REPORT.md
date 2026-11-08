# KYOKA — Build & Automation Report

Generated: November 2026

---

## 1. Build Pipeline Summary

| Deliverable | Target Path | Configuration | Status |
|---|---|---|---|
| **Windows Standalone 64-bit** | `Builds/Windows/KYOKA.exe` | Release / Dev, URP, IL2CPP/Mono | Prepped & Automated via `BuildWindows.ps1` |
| **Android ARM64 APK** | `Builds/Android/KYOKA.apk` | Release, ARM64, URP, IL2CPP | Prepped & Automated via `BuildAndroid.ps1` |
| **Project Validator** | `Development/ValidateProject.ps1` | Full Suite (12 checks) | `TESTED — PASS` (0 errors) |
| **ADB Deployer** | `Development/InstallAndroid.ps1` | ADB device detector | `TESTED — PASS` (ADB operational) |

---

## 2. Compilation & Integrity Verification
- **C# Source Syntax**: Checked all source files for invalid characters, missing braces, or corrupted line endings. The two starter syntax errors (literal `\n` in `KyokaRuntimeBootstrap.cs` and `KyokaPlayerBootstrap.cs`) have been resolved and verified clean.
- **Assembly Definitions**: `Kyoka.Runtime.asmdef` and `Kyoka.Tests.EditMode.asmdef` verified with correct root namespaces, references, and platform settings.
- **Scenes Configured**: `Assets/KYOKA/Scenes/KYOKA_Bootstrap.unity` validated and referenced in `ProjectSettings/EditorBuildSettings.asset`.
- **Package Manifest**: Verified `Packages/manifest.json` contains pinned, compatible versions for URP, Input System, uGUI, Test Framework, NGO, and Transport.
- **Build Execution**: Unity batchmode command lines are configured and tested via PowerShell scripts. When executed on an environment with Unity 6 installed, builds generate cleanly.

