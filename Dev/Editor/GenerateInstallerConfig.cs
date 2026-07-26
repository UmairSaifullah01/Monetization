using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Dev
{
    /// <summary>
    /// Maintainer-only utility. Not shipped in bootstrap or MonetizationScripts.unitypackage.
    /// </summary>
    public static class GenerateInstallerConfig
    {
        private static readonly Regex TgzVersionSuffix = new Regex(@"-\d+\.\d+(\.\d+)?(\.\d+)?$", RegexOptions.Compiled);

        private static readonly Dictionary<string, System.Func<string, bool>> FirebaseProviderFilters =
            new Dictionary<string, System.Func<string, bool>>
            {
                { "remoteconfig_firebase", IsFirebaseRemoteConfigDependency },
                { "analytics_firebase", IsFirebaseAnalyticsDependency },
                { "database_firebase", IsFirebaseDatabaseDependency },
                { "storage_firebase", IsFirebaseStorageDependency }
            };

        [MenuItem("Tools/Monetization Dev/Generate Installer Config From Manifest")]
        public static void GenerateInstallerConfigFile()
        {
            string manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages", "manifest.json");
            string configPath = Path.Combine(Application.dataPath, "Monetization/Installer/installer_config.json");
            string tgzFolder = Path.Combine(Application.dataPath, "Monetization/Installer/Dependencies");

            if (!File.Exists(manifestPath))
            {
                Debug.LogError("manifest.json not found.");
                return;
            }

            string manifestText = File.ReadAllText(manifestPath);
            var manifest = MiniJSON.Json.Deserialize(manifestText) as Dictionary<string, object>;
            var dependencies = manifest["dependencies"] as Dictionary<string, object>;

            Dictionary<string, object> config;
            if (File.Exists(configPath))
            {
                string oldJson = File.ReadAllText(configPath);
                config = MiniJSON.Json.Deserialize(oldJson) as Dictionary<string, object>;
            }
            else
            {
                config = new Dictionary<string, object>();
            }

            if (!config.ContainsKey("corePackages")) config["corePackages"] = new Dictionary<string, string>();
            if (!config.ContainsKey("registries")) config["registries"] = new List<object>();
            if (!config.ContainsKey("providers")) config["providers"] = new Dictionary<string, object>();

            var tgzToPackageId = BuildTgzMapping(dependencies, tgzFolder);
            UpdateProviderTgzPackages(config, tgzToPackageId);
            SeedFirebaseTgzFromDependencies(config, tgzFolder);

            string json = MiniJSON.Json.Serialize(config);
            File.WriteAllText(configPath, json);

            Debug.Log("Updated installer_config.json (preserved corePackages, registries, and providers).");
        }

        private static Dictionary<string, string> BuildTgzMapping(Dictionary<string, object> dependencies, string tgzFolder)
        {
            var tgzToPackageId = new Dictionary<string, string>();
            if (!Directory.Exists(tgzFolder))
            {
                return tgzToPackageId;
            }

            foreach (var file in Directory.GetFiles(tgzFolder, "*.tgz"))
            {
                string fileName = Path.GetFileName(file);
                if (tgzToPackageId.ContainsKey(fileName))
                {
                    continue;
                }

                string fileValue = $"file:../ExternalPackages/{fileName}";
                bool mappedFromManifest = false;
                if (dependencies != null)
                {
                    foreach (var kvp in dependencies)
                    {
                        if (kvp.Value.ToString() == fileValue)
                        {
                            tgzToPackageId[fileName] = kvp.Key;
                            mappedFromManifest = true;
                            break;
                        }
                    }
                }

                if (!mappedFromManifest && TryParsePackageIdFromTgzFileName(fileName, out string packageId))
                {
                    tgzToPackageId[fileName] = packageId;
                }
            }

            return tgzToPackageId;
        }

        private static void UpdateProviderTgzPackages(Dictionary<string, object> config, Dictionary<string, string> tgzToPackageId)
        {
            if (!(config["providers"] is Dictionary<string, object> providers))
            {
                return;
            }

            foreach (var providerPair in providers.ToList())
            {
                if (!(providerPair.Value is Dictionary<string, object> providerDict))
                {
                    continue;
                }

                if (!providerDict.ContainsKey("tgzPackages"))
                {
                    providerDict["tgzPackages"] = new Dictionary<string, string>();
                }

                if (!(providerDict["tgzPackages"] is Dictionary<string, string> providerTgz))
                {
                    providerTgz = new Dictionary<string, string>();
                    providerDict["tgzPackages"] = providerTgz;
                }

                foreach (var tgzPair in tgzToPackageId)
                {
                    if (providerTgz.ContainsKey(tgzPair.Key))
                    {
                        providerTgz[tgzPair.Key] = tgzPair.Value;
                    }
                }
            }
        }

        private static void SeedFirebaseTgzFromDependencies(Dictionary<string, object> config, string tgzFolder)
        {
            if (!Directory.Exists(tgzFolder) || !(config["providers"] is Dictionary<string, object> providers))
            {
                return;
            }

            foreach (var providerFilter in FirebaseProviderFilters)
            {
                if (!providers.TryGetValue(providerFilter.Key, out var providerObj) ||
                    !(providerObj is Dictionary<string, object> providerDict))
                {
                    continue;
                }

                if (!(providerDict["tgzPackages"] is Dictionary<string, string> providerTgz))
                {
                    providerTgz = new Dictionary<string, string>();
                    providerDict["tgzPackages"] = providerTgz;
                }

                foreach (var file in Directory.GetFiles(tgzFolder, "*.tgz"))
                {
                    string fileName = Path.GetFileName(file);
                    if (!providerFilter.Value(fileName))
                    {
                        continue;
                    }

                    if (!TryParsePackageIdFromTgzFileName(fileName, out string packageId))
                    {
                        continue;
                    }

                    providerTgz[fileName] = packageId;
                }
            }
        }

        private static bool IsFirebaseBaseDependency(string fileName)
        {
            return fileName.StartsWith("com.google.external-dependency-manager") ||
                   fileName.StartsWith("com.google.firebase.app-");
        }

        private static bool IsFirebaseRemoteConfigDependency(string fileName)
        {
            return IsFirebaseBaseDependency(fileName) ||
                   fileName.StartsWith("com.google.firebase.remote-config-");
        }

        private static bool IsFirebaseAnalyticsDependency(string fileName)
        {
            return IsFirebaseBaseDependency(fileName) ||
                   fileName.StartsWith("com.google.firebase.analytics-");
        }

        private static bool IsFirebaseDatabaseDependency(string fileName)
        {
            return IsFirebaseBaseDependency(fileName) ||
                   fileName.StartsWith("com.google.firebase.database-");
        }

        private static bool IsFirebaseStorageDependency(string fileName)
        {
            return IsFirebaseBaseDependency(fileName) ||
                   fileName.StartsWith("com.google.firebase.storage-");
        }

        private static bool TryParsePackageIdFromTgzFileName(string fileName, out string packageId)
        {
            packageId = null;
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            if (string.IsNullOrEmpty(baseName))
            {
                return false;
            }

            var match = TgzVersionSuffix.Match(baseName);
            if (!match.Success)
            {
                return false;
            }

            packageId = baseName.Substring(0, match.Index);
            return !string.IsNullOrEmpty(packageId);
        }
    }
}
