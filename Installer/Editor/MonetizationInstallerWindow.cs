using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using THEBADDEST.MonetizationApi;
using THEBADDEST.MonetizationEditor;
using UnityEditor.PackageManager;

namespace Installer
{
    public class MonetizationInstallerWindow : EditorWindow
    {
        private string sourceTgzFolder;
        private string unityPackagePath;
        private string externalPackagesFolder = "ExternalPackages";

        private bool isInstalling = false;
        private float progress = 0f;
        private string progressMessage = "";
        private bool installSuccess = false;

        private Texture2D logoTexture;
        private string configPath;
        private InstallerConfig installerConfig;
        private Dictionary<string, bool> providerSelection = new Dictionary<string, bool>();

        private void OnEnable()
        {
            logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Monetization/Logo/logo.png");
            sourceTgzFolder = Path.Combine(Application.dataPath, "Monetization/Installer/Dependencies");
            unityPackagePath = Path.Combine(Application.dataPath, "Monetization/Installer/MonetizationScripts.unitypackage");
            configPath = Path.Combine(Application.dataPath, "Monetization/Installer/installer_config.json");
            installerConfig = InstallerConfig.Load(configPath);
            RefreshProviderSelection();
        }

        private void RefreshProviderSelection()
        {
            if (installerConfig?.providers == null) return;
            foreach (var key in installerConfig.providers.Keys)
            {
                if (!providerSelection.ContainsKey(key))
                {
                    providerSelection[key] = true;
                }
            }
        }

        [MenuItem("Tools/Monetization/Installer")]
        public static void ShowWindow()
        {
            var window = GetWindow<MonetizationInstallerWindow>("Monetization Installer");
            window.minSize = new Vector2(500, 450);
            window.maxSize = new Vector2(500, 450);
        }

        void OnGUI()
        {
            GUILayout.Space(20);

            // Centered logo and title as a single row
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (logoTexture != null)
            {
                GUILayout.Label(logoTexture, GUILayout.Width(70), GUILayout.Height(70));
                GUILayout.Space(10);
            }
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };
            GUILayout.Label("Monetization Installer", titleStyle, GUILayout.Height(50));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Version/author centered below
            var versionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };
            GUILayout.Label("Version 4.0b - Developed by Umair Saifullah", versionStyle, GUILayout.ExpandWidth(true));

            GUILayout.Space(10);

            // Description label in a rounded box
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(420));
            var descStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = 14
            };
            GUILayout.Label(
                "A comprehensive Unity monetization framework that provides a modular, extensible architecture for ads, in-app purchases, analytics, and remote configuration.\n\nThis installer will copy all required dependencies, update your manifest, and import all Monetization scripts for you.\n\nGood luck :)",
                descStyle, GUILayout.Height(200), GUILayout.ExpandWidth(true));
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (installerConfig?.providers != null && installerConfig.providers.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(420));
                GUILayout.Label("Optional SDK Providers", EditorStyles.boldLabel);
                foreach (var kvp in installerConfig.providers)
                {
                    if (!providerSelection.ContainsKey(kvp.Key))
                    {
                        providerSelection[kvp.Key] = true;
                    }
                    providerSelection[kvp.Key] = EditorGUILayout.ToggleLeft(
                        string.IsNullOrEmpty(kvp.Value.label) ? kvp.Key : kvp.Value.label,
                        providerSelection[kvp.Key]);
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                DrawProfileWarnings();
                GUILayout.Space(10);
            }

            if (isInstalling)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(200, 20), progress, progressMessage);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
            if (installSuccess)
            {
                EditorGUILayout.HelpBox("Dependencies and scripts installed successfully!\nPlease allow Unity to resolve packages.", MessageType.Info);
            }

            EditorGUI.BeginDisabledGroup(isInstalling);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (!isInstalling && GUILayout.Button("Install", GUILayout.Width(200), GUILayout.Height(40)))
            {
                installSuccess = false;
                isInstalling = true;
                progress = 0f;
                progressMessage = "Starting installation...";
                EditorApplication.update += InstallStep;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            // Uninstall button
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (!isInstalling && GUILayout.Button("Uninstall", GUILayout.Width(200), GUILayout.Height(30)))
            {
                Uninstall();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        private void DrawProfileWarnings()
        {
            var profile = Resources.Load<MonetizationProfile>("MonetizationProfile");
            if (profile == null)
            {
                return;
            }

            var warnings = ProviderProfileValidator.Validate(profile);
            if (warnings.Count == 0)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox(string.Join("\n", warnings), MessageType.Warning, true);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private int installStep = 0;
        private string[] tgzFilesCache;
        private string projectExternalCache;
        private string manifestPathCache;
        private string manifestTextCache;
        private Dictionary<string, object> manifestCache;
        private Dictionary<string, object> dependenciesCache;

        void InstallStep()
        {
            try
            {
                switch (installStep)
                {
                    case 0:
                        progress = 0.1f;
                        progressMessage = "Copying dependencies...";
                        projectExternalCache = Path.Combine(Directory.GetCurrentDirectory(), externalPackagesFolder);
                        if (!Directory.Exists(projectExternalCache)) Directory.CreateDirectory(projectExternalCache);
                        tgzFilesCache = Directory.GetFiles(sourceTgzFolder, "*.tgz");
                        foreach (var file in tgzFilesCache)
                        {
                            string dest = Path.Combine(projectExternalCache, Path.GetFileName(file));
                            File.Copy(file, dest, true);
                        }
                        installStep++;
                        break;
                    case 1:
                        progress = 0.5f;
                        progressMessage = "Updating manifest.json...";
                        manifestPathCache = Path.Combine(Directory.GetCurrentDirectory(), "Packages", "manifest.json");
                        manifestTextCache = File.ReadAllText(manifestPathCache);
                        manifestCache = MiniJSON.Json.Deserialize(manifestTextCache) as Dictionary<string, object>;
                        dependenciesCache = manifestCache["dependencies"] as Dictionary<string, object>;

                        // Add packages from config (core + selected providers)
                        if (installerConfig != null)
                        {
                            foreach (var kvp in installerConfig.GetPackagesForInstall(providerSelection))
                            {
                                dependenciesCache[kvp.Key] = kvp.Value;
                            }
                        }

                        // Add registries from config only
                        List<object> scopedRegistries;
                        if (!manifestCache.ContainsKey("scopedRegistries"))
                        {
                            scopedRegistries = new List<object>();
                            manifestCache["scopedRegistries"] = scopedRegistries;
                        }
                        else
                        {
                            scopedRegistries = manifestCache["scopedRegistries"] as List<object>;
                        }
                        if (installerConfig != null && installerConfig.registries != null)
                        {
                            foreach (var reg in installerConfig.registries)
                            {
                                // Check if already present
                                bool found = false;
                                foreach (var existing in scopedRegistries)
                                {
                                    var regDict = existing as Dictionary<string, object>;
                                    if (regDict != null && regDict.ContainsKey("name") && regDict["name"].ToString() == reg.name)
                                    {
                                        found = true;
                                        regDict["url"] = reg.url;
                                        regDict["scopes"] = reg.scopes.Cast<object>().ToList();
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

                        // Add .tgz packages from config (selected providers)
                        if (installerConfig != null)
                        {
                            foreach (var kvp in installerConfig.GetTgzPackagesForInstall(providerSelection))
                            {
                                string fileName = kvp.Key;
                                string packageName = kvp.Value;
                                string fileValue = $"file:../{externalPackagesFolder}/{fileName}";
                                dependenciesCache[packageName] = fileValue;
                            }
                        }

                        manifestCache["dependencies"] = dependenciesCache;
                        string newManifest = MiniJSON.Json.Serialize(manifestCache);
                        File.WriteAllText(manifestPathCache, newManifest);
                        AssetDatabase.Refresh();
                        Client.Resolve();
                        installStep++;
                        break;
                    case 2:
                        progress = 0.8f;
                        progressMessage = "Importing Monetization scripts...";
                        if (File.Exists(unityPackagePath))
                        {
                            AssetDatabase.ImportPackage(unityPackagePath, false);
                        }
                        else
                        {
                            Debug.LogWarning("Unity package not found: " + unityPackagePath);
                        }
                        installStep++;
                        break;
                    case 3:
                        progress = 1f;
                        progressMessage = "Done!";
                        isInstalling = false;
                        installSuccess = true;
                        installStep = 0;
                        EditorApplication.update -= InstallStep;
                        break;
                }
            }
            catch (System.Exception ex)
            {
                isInstalling = false;
                installSuccess = false;
                installStep = 0;
                EditorApplication.update -= InstallStep;
                Debug.LogError("[Monetization Installer] Error: " + ex.Message);
                EditorUtility.DisplayDialog("Monetization Installer", "Error during installation: " + ex.Message, "OK");
            }
        }

        void Uninstall()
        {
            // Remove dependencies from manifest.json
            string manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages", "manifest.json");
            string manifestText = File.ReadAllText(manifestPath);
            var manifest = MiniJSON.Json.Deserialize(manifestText) as Dictionary<string, object>;
            var dependencies = manifest["dependencies"] as Dictionary<string, object>;

            // Remove all packages from config
            if (installerConfig != null)
            {
                foreach (var kvp in installerConfig.GetAllPackages())
                {
                    if (dependencies.ContainsKey(kvp.Key))
                        dependencies.Remove(kvp.Key);
                }
                foreach (var kvp in installerConfig.GetAllTgzPackages())
                {
                    if (dependencies.ContainsKey(kvp.Value))
                        dependencies.Remove(kvp.Value);
                }
            }

            manifest["dependencies"] = dependencies;
            string newManifest = MiniJSON.Json.Serialize(manifest);
            File.WriteAllText(manifestPath, newManifest);

            // Delete all .tgz files in ExternalPackages but keep the folder if not empty
            string projectExternal = Path.Combine(Directory.GetCurrentDirectory(), externalPackagesFolder);
            if (Directory.Exists(projectExternal))
            {
                var tgzFilesExternal = Directory.GetFiles(projectExternal, "*.tgz");
                foreach (var file in tgzFilesExternal)
                {
                    File.Delete(file);
                }
                if (Directory.GetFiles(projectExternal).Length == 0 && Directory.GetDirectories(projectExternal).Length == 0)
                    Directory.Delete(projectExternal, true);
            }

            // Delete all folders in Monetization except Installer and Logo
            string monetizationRoot = Path.Combine(Application.dataPath, "Monetization");
            if (Directory.Exists(monetizationRoot))
            {
                var dirs = Directory.GetDirectories(monetizationRoot);
                foreach (var dir in dirs)
                {
                    string dirName = Path.GetFileName(dir).ToLowerInvariant();
                    if (dirName != "installer" && dirName != "logo")
                    {
                        Directory.Delete(dir, true);
                    }
                }
            }
            AssetDatabase.Refresh();
            Client.Resolve();
            EditorUtility.DisplayDialog("Monetization Uninstaller", "All monetization dependencies and packages have been removed.", "OK");
        }
    }
}

[Serializable]
    public class ProviderConfig
    {
        public string label;
        public Dictionary<string, string> packages = new Dictionary<string, string>();
        public Dictionary<string, string> tgzPackages = new Dictionary<string, string>();
        public string asmdef;
        public string moduleType;
    }

    [Serializable]
    public class InstallerConfig
    {
        public Dictionary<string, string> corePackages = new Dictionary<string, string>();
        public Dictionary<string, string> packages = new Dictionary<string, string>();
        public List<RegistryConfig> registries = new List<RegistryConfig>();
        public Dictionary<string, string> tgzPackages = new Dictionary<string, string>();
        public Dictionary<string, ProviderConfig> providers = new Dictionary<string, ProviderConfig>();

        public IEnumerable<KeyValuePair<string, string>> GetPackagesForInstall(Dictionary<string, bool> selection)
        {
            foreach (var kvp in corePackages)
            {
                yield return kvp;
            }

            if (providers == null || providers.Count == 0)
            {
                foreach (var kvp in packages)
                {
                    yield return kvp;
                }
                yield break;
            }

            foreach (var provider in providers)
            {
                if (selection != null && selection.TryGetValue(provider.Key, out bool enabled) && !enabled)
                {
                    continue;
                }

                if (provider.Value?.packages == null) continue;
                foreach (var kvp in provider.Value.packages)
                {
                    yield return kvp;
                }
            }
        }

        public IEnumerable<KeyValuePair<string, string>> GetTgzPackagesForInstall(Dictionary<string, bool> selection)
        {
            if (providers == null || providers.Count == 0)
            {
                foreach (var kvp in tgzPackages)
                {
                    yield return kvp;
                }
                yield break;
            }

            foreach (var provider in providers)
            {
                if (selection != null && selection.TryGetValue(provider.Key, out bool enabled) && !enabled)
                {
                    continue;
                }

                if (provider.Value?.tgzPackages == null) continue;
                foreach (var kvp in provider.Value.tgzPackages)
                {
                    yield return kvp;
                }
            }
        }

        public IEnumerable<KeyValuePair<string, string>> GetAllPackages()
        {
            foreach (var kvp in GetPackagesForInstall(null))
            {
                yield return kvp;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> GetAllTgzPackages()
        {
            foreach (var kvp in GetTgzPackagesForInstall(null))
            {
                yield return kvp;
            }
        }

        public static InstallerConfig Load(string path)
        {
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
            var dict = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
            if (dict == null) return null;

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
                        var reg = new RegistryConfig
                        {
                            name = regDict["name"].ToString(),
                            url = regDict["url"].ToString(),
                            scopes = (regDict["scopes"] as List<object>)?.Select(o => o.ToString()).ToList()
                        };
                        config.registries.Add(reg);
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

    [Serializable]
    public class RegistryConfig
    {
        public string name;
        public string url;
        public List<string> scopes = new List<string>();
    } 