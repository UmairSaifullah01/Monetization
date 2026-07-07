using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace THEBADDEST.MonetizationEditor
{
    public static class ManifestPackageUtility
    {
        public const string ExternalPackagesFolder = "ExternalPackages";

        public static string ManifestPath => Path.Combine(Directory.GetCurrentDirectory(), "Packages", "manifest.json");

        public static string DependenciesFolder =>
            Path.Combine(Application.dataPath, "Monetization/Installer/Dependencies");

        public static string ExternalPackagesPath =>
            Path.Combine(Directory.GetCurrentDirectory(), ExternalPackagesFolder);

        public static bool IsCoreInstalled(InstallerConfig config)
        {
            if (config == null)
            {
                return false;
            }

            var dependencies = ReadDependencies();
            if (dependencies == null)
            {
                return false;
            }

            foreach (var kvp in config.GetCorePackages())
            {
                if (dependencies.ContainsKey(kvp.Key))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsProviderInstalled(InstallerConfig config, string providerKey)
        {
            if (config?.providers == null || !config.providers.TryGetValue(providerKey, out var provider))
            {
                return false;
            }

            var dependencies = ReadDependencies();
            if (dependencies == null)
            {
                return false;
            }

            foreach (var kvp in config.GetProviderPackages(providerKey))
            {
                if (dependencies.ContainsKey(kvp.Key))
                {
                    return true;
                }
            }

            foreach (var kvp in config.GetProviderTgzPackages(providerKey))
            {
                if (dependencies.ContainsKey(kvp.Value))
                {
                    return true;
                }
            }

            return false;
        }

        public static void InstallCorePackages(InstallerConfig config)
        {
            if (config == null)
            {
                return;
            }

            var manifest = ReadManifest();
            var dependencies = GetOrCreateDependencies(manifest);
            foreach (var kvp in config.GetCorePackages())
            {
                dependencies[kvp.Key] = kvp.Value;
            }

            MergeRegistries(manifest, config.registries);
            WriteManifest(manifest);
            ResolvePackages();
        }

        public static void UninstallCorePackages(InstallerConfig config)
        {
            if (config == null)
            {
                return;
            }

            var manifest = ReadManifest();
            var dependencies = GetOrCreateDependencies(manifest);
            foreach (var kvp in config.GetCorePackages())
            {
                dependencies.Remove(kvp.Key);
            }

            WriteManifest(manifest);
            ResolvePackages();
        }

        public static void InstallProvider(InstallerConfig config, string providerKey)
        {
            if (config?.providers == null || !config.providers.TryGetValue(providerKey, out var provider))
            {
                return;
            }

            CopyProviderTgzFiles(provider);
            var manifest = ReadManifest();
            var dependencies = GetOrCreateDependencies(manifest);

            foreach (var kvp in config.GetProviderPackages(providerKey))
            {
                dependencies[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in config.GetProviderTgzPackages(providerKey))
            {
                dependencies[kvp.Value] = $"file:../{ExternalPackagesFolder}/{kvp.Key}";
            }

            MergeRegistries(manifest, config.registries);
            WriteManifest(manifest);
            ResolvePackages();
        }

        public static void UninstallProvider(InstallerConfig config, string providerKey)
        {
            if (config?.providers == null || !config.providers.TryGetValue(providerKey, out var provider))
            {
                return;
            }

            var manifest = ReadManifest();
            var dependencies = GetOrCreateDependencies(manifest);

            foreach (var kvp in config.GetProviderPackages(providerKey))
            {
                dependencies.Remove(kvp.Key);
            }

            foreach (var kvp in config.GetProviderTgzPackages(providerKey))
            {
                dependencies.Remove(kvp.Value);
                DeleteExternalTgz(kvp.Key);
            }

            WriteManifest(manifest);
            ResolvePackages();

            if (providerKey == "analytics_gameanalytics")
            {
                MonetizationLegacyDefineUtility.SetLegacyGameAnalyticsEnabled(false);
            }
        }

        public static void UninstallAllProviders(InstallerConfig config)
        {
            if (config?.providers == null)
            {
                return;
            }

            foreach (var providerKey in config.providers.Keys.ToList())
            {
                UninstallProvider(config, providerKey);
            }
        }

        public static void UninstallAllManifestEntries(InstallerConfig config)
        {
            if (config == null)
            {
                return;
            }

            var manifest = ReadManifest();
            var dependencies = GetOrCreateDependencies(manifest);

            foreach (var kvp in config.GetCorePackages())
            {
                dependencies.Remove(kvp.Key);
            }

            foreach (var kvp in config.GetAllProviderPackages())
            {
                dependencies.Remove(kvp.Key);
            }

            foreach (var kvp in config.GetAllProviderTgzPackages())
            {
                dependencies.Remove(kvp.Value);
            }

            WriteManifest(manifest);
            DeleteAllExternalTgz();
            MonetizationLegacyDefineUtility.SetLegacyGameAnalyticsEnabled(false);
            ResolvePackages();
        }

        public static void UninstallFrameworkAssets()
        {
            string monetizationRoot = Path.Combine(Application.dataPath, "Monetization");
            if (!Directory.Exists(monetizationRoot))
            {
                return;
            }

            foreach (var dir in Directory.GetDirectories(monetizationRoot))
            {
                string dirName = Path.GetFileName(dir).ToLowerInvariant();
                if (dirName != "installer" && dirName != "logo")
                {
                    Directory.Delete(dir, true);
                    string meta = dir + ".meta";
                    if (File.Exists(meta))
                    {
                        File.Delete(meta);
                    }
                }
            }

            AssetDatabase.Refresh();
        }

        private static void CopyProviderTgzFiles(ProviderConfig provider)
        {
            if (provider?.tgzPackages == null || provider.tgzPackages.Count == 0)
            {
                return;
            }

            if (!Directory.Exists(ExternalPackagesPath))
            {
                Directory.CreateDirectory(ExternalPackagesPath);
            }

            foreach (var fileName in provider.tgzPackages.Keys)
            {
                string source = Path.Combine(DependenciesFolder, fileName);
                if (!File.Exists(source))
                {
                    Debug.LogWarning($"[Monetization] Missing dependency archive: {source}");
                    continue;
                }

                string dest = Path.Combine(ExternalPackagesPath, fileName);
                File.Copy(source, dest, true);
            }
        }

        private static void DeleteExternalTgz(string fileName)
        {
            string path = Path.Combine(ExternalPackagesPath, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static void DeleteAllExternalTgz()
        {
            if (!Directory.Exists(ExternalPackagesPath))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(ExternalPackagesPath, "*.tgz"))
            {
                File.Delete(file);
            }
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

        private static void MergeRegistries(Dictionary<string, object> manifest, List<RegistryConfig> registries)
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

        private static void ResolvePackages()
        {
            Client.Resolve();
        }
    }
}
