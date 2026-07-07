using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Installer
{
    public class GenerateInstallerConfig
    {
        [MenuItem("Tools/Monetization/Generate Installer Config From Manifest")]
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

            string json = MiniJSON.Json.Serialize(config);
            File.WriteAllText(configPath, json);

            Debug.Log("Updated installer_config.json (preserved corePackages, registries, and providers).");
        }

        private static Dictionary<string, string> BuildTgzMapping(Dictionary<string, object> dependencies, string tgzFolder)
        {
            var tgzToPackageId = new Dictionary<string, string>();
            if (!Directory.Exists(tgzFolder) || dependencies == null)
            {
                return tgzToPackageId;
            }

            foreach (var file in Directory.GetFiles(tgzFolder, "*.tgz"))
            {
                string fileName = Path.GetFileName(file);
                string fileValue = $"file:../ExternalPackages/{fileName}";
                foreach (var kvp in dependencies)
                {
                    if (kvp.Value.ToString() == fileValue)
                    {
                        tgzToPackageId[fileName] = kvp.Key;
                        break;
                    }
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
    }
}
