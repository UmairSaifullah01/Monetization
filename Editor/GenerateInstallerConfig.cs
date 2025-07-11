using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

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

            string manifestText = File.ReadAllText(manifestPath);
            var manifest = MiniJSON.Json.Deserialize(manifestText) as Dictionary<string, object>;
            var dependencies = manifest["dependencies"] as Dictionary<string, object>;

            // Load existing config if present
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

            // Preserve existing packages and registries
            if (!config.ContainsKey("packages")) config["packages"] = new Dictionary<string, string>();
            if (!config.ContainsKey("registries")) config["registries"] = new List<object>();

            // Build tgzPackages mapping
            var tgzPackages = new Dictionary<string, string>();
            foreach (var file in Directory.GetFiles(tgzFolder, "*.tgz"))
            {
                string fileName = Path.GetFileName(file);
                string fileValue = $"file:../ExternalPackages/{fileName}";
                // Find the key in manifest.json that matches this value
                foreach (var kvp in dependencies)
                {
                    if (kvp.Value.ToString() == fileValue)
                    {
                        tgzPackages[fileName] = kvp.Key;
                        break;
                    }
                }
            }
            config["tgzPackages"] = tgzPackages;

            string json = MiniJSON.Json.Serialize(config);
            File.WriteAllText(configPath, json);

            Debug.Log("Updated installer_config.json with tgzPackages from manifest and .tgz files.");
        }
    }
} 