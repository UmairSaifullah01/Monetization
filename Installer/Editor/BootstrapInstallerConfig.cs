using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Installer
{
    [Serializable]
    public class BootstrapProviderConfig
    {
        public string label;
        public Dictionary<string, string> packages = new Dictionary<string, string>();
        public Dictionary<string, string> tgzPackages = new Dictionary<string, string>();
        public string asmdef;
        public string moduleType;
    }

    [Serializable]
    public class BootstrapRegistryConfig
    {
        public string name;
        public string url;
        public List<string> scopes = new List<string>();
    }

    public class BootstrapInstallerConfig
    {
        public const string ConfigRelativePath = "Monetization/Installer/installer_config.json";

        public Dictionary<string, string> corePackages = new Dictionary<string, string>();
        public List<BootstrapRegistryConfig> registries = new List<BootstrapRegistryConfig>();
        public Dictionary<string, BootstrapProviderConfig> providers = new Dictionary<string, BootstrapProviderConfig>();

        public static string GetConfigPath()
        {
            return Path.Combine(UnityEngine.Application.dataPath, ConfigRelativePath);
        }

        public static BootstrapInstallerConfig LoadDefault()
        {
            return Load(GetConfigPath());
        }

        public static BootstrapInstallerConfig Load(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            string json = File.ReadAllText(path);
            var dict = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
            if (dict == null)
            {
                return null;
            }

            var config = new BootstrapInstallerConfig();
            LoadStringDictionary(dict, "corePackages", config.corePackages);

            if (dict.TryGetValue("registries", out var registriesObj) && registriesObj is List<object> regList)
            {
                foreach (var item in regList)
                {
                    if (item is Dictionary<string, object> regDict)
                    {
                        config.registries.Add(new BootstrapRegistryConfig
                        {
                            name = regDict["name"].ToString(),
                            url = regDict["url"].ToString(),
                            scopes = (regDict["scopes"] as List<object>)?.Select(o => o.ToString()).ToList()
                        });
                    }
                }
            }

            if (dict.TryGetValue("providers", out var providersObj) && providersObj is Dictionary<string, object> providersDict)
            {
                foreach (var providerPair in providersDict)
                {
                    if (providerPair.Value is Dictionary<string, object> providerDict)
                    {
                        var provider = new BootstrapProviderConfig
                        {
                            label = providerDict.TryGetValue("label", out var labelObj) ? labelObj.ToString() : providerPair.Key,
                            asmdef = providerDict.TryGetValue("asmdef", out var asmdefObj) ? asmdefObj.ToString() : null,
                            moduleType = providerDict.TryGetValue("moduleType", out var moduleObj) ? moduleObj.ToString() : null
                        };

                        if (providerDict.TryGetValue("packages", out var packagesObj) && packagesObj is Dictionary<string, object> packagesDict)
                        {
                            foreach (var kvp in packagesDict)
                            {
                                provider.packages[kvp.Key] = kvp.Value.ToString();
                            }
                        }

                        if (providerDict.TryGetValue("tgzPackages", out var tgzObj) && tgzObj is Dictionary<string, object> tgzDict)
                        {
                            foreach (var kvp in tgzDict)
                            {
                                provider.tgzPackages[kvp.Key] = kvp.Value.ToString();
                            }
                        }

                        config.providers[providerPair.Key] = provider;
                    }
                }
            }

            return config;
        }

        private static void LoadStringDictionary(Dictionary<string, object> dict, string key, Dictionary<string, string> target)
        {
            if (dict.TryGetValue(key, out var obj) && obj is Dictionary<string, object> source)
            {
                foreach (var kvp in source)
                {
                    target[kvp.Key] = kvp.Value.ToString();
                }
            }
        }
    }
}
