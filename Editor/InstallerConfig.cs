using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace THEBADDEST.MonetizationApi.Editor
{
    [Serializable]
    public class ProviderConfig
    {
        public string label;
        public Dictionary<string, string> packages = new Dictionary<string, string>();
        public Dictionary<string, string> tgzPackages = new Dictionary<string, string>();
        public string asmdef;
        public string moduleType;
        public string versionDefine;
    }

    [Serializable]
    public class RegistryConfig
    {
        public string name;
        public string url;
        public List<string> scopes = new List<string>();
    }

    [Serializable]
    public class InstallerConfig
    {
        public const string ConfigAssetPath = "Assets/Monetization/Installer/installer_config.json";
        public const string DependenciesFolder = "Assets/Monetization/Installer/Dependencies";

        public Dictionary<string, string> corePackages = new Dictionary<string, string>();
        public Dictionary<string, string> packages = new Dictionary<string, string>();
        public List<RegistryConfig> registries = new List<RegistryConfig>();
        public Dictionary<string, string> tgzPackages = new Dictionary<string, string>();
        public Dictionary<string, ProviderConfig> providers = new Dictionary<string, ProviderConfig>();

        public IEnumerable<KeyValuePair<string, string>> GetCorePackages()
        {
            foreach (var kvp in corePackages)
            {
                yield return kvp;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> GetProviderPackages(string providerKey)
        {
            if (providers == null || !providers.TryGetValue(providerKey, out var provider) || provider?.packages == null)
            {
                yield break;
            }

            foreach (var kvp in provider.packages)
            {
                yield return kvp;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> GetProviderTgzPackages(string providerKey)
        {
            if (providers == null || !providers.TryGetValue(providerKey, out var provider) || provider?.tgzPackages == null)
            {
                yield break;
            }

            foreach (var kvp in provider.tgzPackages)
            {
                yield return kvp;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> GetAllProviderPackages()
        {
            if (providers == null)
            {
                yield break;
            }

            foreach (var provider in providers)
            {
                foreach (var kvp in GetProviderPackages(provider.Key))
                {
                    yield return kvp;
                }
            }
        }

        public IEnumerable<KeyValuePair<string, string>> GetAllProviderTgzPackages()
        {
            if (providers == null)
            {
                yield break;
            }

            foreach (var provider in providers)
            {
                foreach (var kvp in GetProviderTgzPackages(provider.Key))
                {
                    yield return kvp;
                }
            }
        }

        public static InstallerConfig LoadDefault()
        {
            string path = Path.Combine(UnityEngine.Application.dataPath, "Monetization/Installer/installer_config.json");
            return Load(path);
        }

        public static InstallerConfig Load(string path)
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

            var config = new InstallerConfig();
            LoadStringDictionary(dict, "corePackages", config.corePackages);
            LoadStringDictionary(dict, "packages", config.packages);
            LoadStringDictionary(dict, "tgzPackages", config.tgzPackages);

            if (dict.TryGetValue("registries", out var registriesObj) && registriesObj is List<object> regList)
            {
                foreach (var item in regList)
                {
                    if (item is Dictionary<string, object> regDict)
                    {
                        config.registries.Add(new RegistryConfig
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
                        var provider = new ProviderConfig
                        {
                            label = providerDict.TryGetValue("label", out var labelObj) ? labelObj.ToString() : providerPair.Key,
                            asmdef = providerDict.TryGetValue("asmdef", out var asmdefObj) ? asmdefObj.ToString() : null,
                            moduleType = providerDict.TryGetValue("moduleType", out var moduleObj) ? moduleObj.ToString() : null,
                            versionDefine = providerDict.TryGetValue("versionDefine", out var defineObj) ? defineObj.ToString() : null
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
