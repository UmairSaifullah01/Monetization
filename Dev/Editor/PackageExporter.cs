using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationDev
{
    /// <summary>
    /// Maintainer-only utility to export the two shipping packages:
    /// - MonetizationScripts.unitypackage (framework content imported by the installer)
    /// - Monetization Installer bootstrap package (installer editor scripts + Logo + unitypackage)
    /// Include lists mirror Installer/EXPORT.md.
    /// </summary>
    public static class PackageExporter
    {
        private const string MonetizationRoot = "Assets/Monetization";
        private const string ScriptsPackagePath = "Assets/Monetization/Installer/MonetizationScripts.unitypackage";

        // Content package: everything an end user needs after the bootstrap runs.
        private static readonly string[] ScriptsPackageAssets =
        {
            "Assets/Monetization/Runtime",
            "Assets/Monetization/JsonDataUtility",
            "Assets/Monetization/Editor",
            "Assets/Monetization/Resources",
            "Assets/Monetization/Content",
            "Assets/Monetization/Demo",
            "Assets/Monetization/Installer/installer_config.json",
            "Assets/Monetization/Installer/Dependencies"
        };

        // Bootstrap package: only the files needed to run the installer in an empty project.
        private static readonly string[] BootstrapPackageAssets =
        {
            "Assets/Monetization/Installer/Editor/MonetizationInstallerWindow.cs",
            "Assets/Monetization/Installer/Editor/FrameworkInstallModule.cs",
            "Assets/Monetization/Installer/Editor/BootstrapManifestUtility.cs",
            "Assets/Monetization/Installer/Editor/BootstrapInstallerConfig.cs",
            "Assets/Monetization/Installer/Editor/InstallerReflectionBridge.cs",
            "Assets/Monetization/Installer/Editor/MiniJSON.cs",
            "Assets/Monetization/Installer/Editor/THEBADDEST.Monetization.Bootstrap.Installer.Editor.asmdef",
            ScriptsPackagePath,
            "Assets/Monetization/Logo"
        };

        [MenuItem("Tools/Monetization Dev/Export MonetizationScripts.unitypackage", priority = 20)]
        public static void ExportScriptsPackage()
        {
            ExportScriptsPackageInternal();
        }

        [MenuItem("Tools/Monetization Dev/Export Installer Bootstrap Package", priority = 21)]
        public static void ExportBootstrapPackage()
        {
            if (!File.Exists(ScriptsPackagePath))
            {
                bool exportNow = EditorUtility.DisplayDialog(
                    "Missing MonetizationScripts.unitypackage",
                    "The bootstrap package embeds MonetizationScripts.unitypackage, which was not found.\n\nExport it now first?",
                    "Export it first",
                    "Cancel");

                if (!exportNow)
                {
                    return;
                }

                if (!ExportScriptsPackageInternal())
                {
                    return;
                }
            }

            ExportBootstrapPackageInternal();
        }

        [MenuItem("Tools/Monetization Dev/Export All Packages", priority = 22)]
        public static void ExportAllPackages()
        {
            if (ExportScriptsPackageInternal())
            {
                ExportBootstrapPackageInternal();
            }
        }

        private static bool ExportScriptsPackageInternal()
        {
            var assets = ResolveExistingAssets(ScriptsPackageAssets, out var missing);
            if (assets.Count == 0)
            {
                EditorUtility.DisplayDialog("Export Failed", "No content assets found to export.", "OK");
                return false;
            }

            AssetDatabase.ExportPackage(
                assets.ToArray(),
                ScriptsPackagePath,
                ExportPackageOptions.Recurse);

            // Import the freshly written unitypackage so the bootstrap export can embed it.
            AssetDatabase.ImportAsset(ScriptsPackagePath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            LogResult("MonetizationScripts.unitypackage", ScriptsPackagePath, assets, missing);
            return true;
        }

        private static bool ExportBootstrapPackageInternal()
        {
            string outputPath = EditorUtility.SaveFilePanel(
                "Export Installer Bootstrap Package",
                Directory.GetCurrentDirectory(),
                "MonetizationInstaller",
                "unitypackage");

            if (string.IsNullOrEmpty(outputPath))
            {
                return false;
            }

            var assets = ResolveExistingAssets(BootstrapPackageAssets, out var missing);
            if (assets.Count == 0)
            {
                EditorUtility.DisplayDialog("Export Failed", "No installer assets found to export.", "OK");
                return false;
            }

            AssetDatabase.ExportPackage(
                assets.ToArray(),
                outputPath,
                ExportPackageOptions.Recurse);

            LogResult("Installer bootstrap package", outputPath, assets, missing);
            return true;
        }

        private static List<string> ResolveExistingAssets(IEnumerable<string> candidates, out List<string> missing)
        {
            var found = new List<string>();
            missing = new List<string>();

            foreach (var path in candidates)
            {
                bool exists = File.Exists(path) || Directory.Exists(path);
                if (exists)
                {
                    found.Add(path);
                }
                else
                {
                    missing.Add(path);
                }
            }

            return found;
        }

        private static void LogResult(string label, string outputPath, List<string> included, List<string> missing)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[Monetization Dev] Exported {label}");
            sb.AppendLine($"Output: {outputPath}");
            sb.AppendLine($"Included ({included.Count}):");
            foreach (var asset in included)
            {
                sb.AppendLine($"  + {asset}");
            }

            if (missing.Count > 0)
            {
                sb.AppendLine($"Skipped (not found) ({missing.Count}):");
                foreach (var asset in missing)
                {
                    sb.AppendLine($"  - {asset}");
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}
