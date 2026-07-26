using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;

namespace THEBADDEST.MonetizationApi.Installer
{
    public static class BootstrapManifestUtility
    {
        public static string ManifestPath => Path.Combine(Directory.GetCurrentDirectory(), "Packages", "manifest.json");

        public static void InstallCorePackages(Dictionary<string, string> corePackages, List<BootstrapRegistryConfig> registries)
        {
            var manifest = ReadManifest();
            var dependencies = GetOrCreateDependencies(manifest);

            foreach (var kvp in corePackages)
            {
                dependencies[kvp.Key] = kvp.Value;
            }

            MergeRegistries(manifest, registries);
            WriteManifest(manifest);
            Client.Resolve();
        }

        public static bool IsCorePackageInstalled(string packageId)
        {
            var dependencies = ReadDependencies();
            return dependencies != null && dependencies.ContainsKey(packageId);
        }

        private static Dictionary<string, object> ReadManifest()
        {
            string text = File.ReadAllText(ManifestPath);
            return MiniJSON.Json.Deserialize(text) as Dictionary<string, object>;
        }

        private static Dictionary<string, object> ReadDependencies()
        {
            var manifest = ReadManifest();
            return manifest?["dependencies"] as Dictionary<string, object>;
        }

        private static Dictionary<string, object> GetOrCreateDependencies(Dictionary<string, object> manifest)
        {
            if (!manifest.TryGetValue("dependencies", out var depsObj) || !(depsObj is Dictionary<string, object> dependencies))
            {
                dependencies = new Dictionary<string, object>();
                manifest["dependencies"] = dependencies;
            }

            return dependencies;
        }

        private static void MergeRegistries(Dictionary<string, object> manifest, List<BootstrapRegistryConfig> registries)
        {
            if (registries == null || registries.Count == 0)
            {
                return;
            }

            if (!manifest.TryGetValue("scopedRegistries", out var registriesObj) || !(registriesObj is List<object> scopedRegistries))
            {
                scopedRegistries = new List<object>();
                manifest["scopedRegistries"] = scopedRegistries;
            }

            foreach (var reg in registries)
            {
                bool found = false;
                foreach (var existing in scopedRegistries)
                {
                    if (existing is Dictionary<string, object> regDict &&
                        regDict.TryGetValue("name", out var nameObj) &&
                        nameObj.ToString() == reg.name)
                    {
                        regDict["url"] = reg.url;
                        regDict["scopes"] = reg.scopes.Cast<object>().ToList();
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    scopedRegistries.Add(new Dictionary<string, object>
                    {
                        { "name", reg.name },
                        { "url", reg.url },
                        { "scopes", reg.scopes.Cast<object>().ToList() }
                    });
                }
            }
        }

        private static void WriteManifest(Dictionary<string, object> manifest)
        {
            string json = MiniJSON.Json.Serialize(manifest);
            File.WriteAllText(ManifestPath, json);
            AssetDatabase.Refresh();
        }
    }
}
