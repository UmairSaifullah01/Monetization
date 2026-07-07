using THEBADDEST.MonetizationApi;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationEditor
{
    public class PackageManagerModule
    {
        private InstallerConfig _config;
        private Texture2D _logoTexture;
        private Vector2 _scrollPosition;

        public void OnEnable()
        {
            _logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Monetization/Logo/logo.png");
            ReloadConfig();
        }

        public void ReloadConfig()
        {
            _config = InstallerConfig.LoadDefault();
        }

        public void DrawGUI()
        {
            if (_config == null)
            {
                EditorGUILayout.HelpBox(
                    "installer_config.json not found. Run Tools → Monetization → Installer to import the framework.",
                    MessageType.Warning);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawHeader();
            GUILayout.Space(8);
            DrawCoreSection();
            GUILayout.Space(10);
            DrawProvidersSection();
            GUILayout.Space(10);
            DrawBulkActions();
            GUILayout.Space(10);
            DrawProfileWarnings();
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (_logoTexture != null)
            {
                GUILayout.Label(_logoTexture, GUILayout.Width(56), GUILayout.Height(56));
                GUILayout.Space(8);
            }

            GUILayout.Label("Package Manager", new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            }, GUILayout.Height(44));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawCoreSection()
        {
            EditorGUILayout.LabelField("Core Dependencies", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);

            bool coreInstalled = ManifestPackageUtility.IsCoreInstalled(_config);
            EditorGUILayout.LabelField("Status", coreInstalled ? "Installed" : "Not installed");

            if (GUILayout.Button("Retry Core Install", GUILayout.Height(28)))
            {
                ReloadConfig();
                ManifestPackageUtility.InstallCorePackages(_config);
            }

            EditorGUILayout.HelpBox(
                "Core dependencies are installed automatically during framework setup. Use Retry if UTask or registries are missing.",
                MessageType.None);
            EditorGUILayout.EndVertical();
        }

        private void DrawProvidersSection()
        {
            EditorGUILayout.LabelField("SDK Providers", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (_config.providers == null || _config.providers.Count == 0)
            {
                EditorGUILayout.LabelField("No providers defined in installer_config.json.");
                EditorGUILayout.EndVertical();
                return;
            }

            foreach (var kvp in _config.providers)
            {
                DrawProviderRow(kvp.Key, kvp.Value);
                EditorGUILayout.Space(4);
            }

            DrawLegacyGameAnalyticsRow();
            EditorGUILayout.EndVertical();
        }

        private void DrawProviderRow(string providerKey, ProviderConfig provider)
        {
            string label = string.IsNullOrEmpty(provider.label) ? providerKey : provider.label;
            bool installed = ManifestPackageUtility.IsProviderInstalled(_config, providerKey);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(180));
            EditorGUILayout.LabelField(installed ? "Installed" : "Not installed", GUILayout.Width(90));

            EditorGUI.BeginDisabledGroup(installed);
            if (GUILayout.Button("Install", GUILayout.Width(90)))
            {
                ManifestPackageUtility.InstallProvider(_config, providerKey);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!installed);
            if (GUILayout.Button("Uninstall", GUILayout.Width(90)))
            {
                if (EditorUtility.DisplayDialog("Uninstall Provider", $"Remove {label} packages from manifest.json?", "Uninstall", "Cancel"))
                {
                    ManifestPackageUtility.UninstallProvider(_config, providerKey);
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawLegacyGameAnalyticsRow()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Legacy GameAnalytics (.unitypackage)", EditorStyles.miniBoldLabel);
            bool legacyEnabled = MonetizationLegacyDefineUtility.IsLegacyGameAnalyticsEnabled();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scripting define", GUILayout.Width(180));
            EditorGUILayout.LabelField(legacyEnabled ? "Enabled" : "Disabled", GUILayout.Width(90));

            EditorGUI.BeginDisabledGroup(legacyEnabled);
            if (GUILayout.Button("Enable", GUILayout.Width(90)))
            {
                MonetizationLegacyDefineUtility.SetLegacyGameAnalyticsEnabled(true);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!legacyEnabled);
            if (GUILayout.Button("Disable", GUILayout.Width(90)))
            {
                MonetizationLegacyDefineUtility.SetLegacyGameAnalyticsEnabled(false);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox(
                "Use when GameAnalytics is imported via legacy .unitypackage instead of UPM. Import the GA SDK first, then enable this define.",
                MessageType.None);
        }

        private void DrawBulkActions()
        {
            EditorGUILayout.LabelField("Bulk Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (GUILayout.Button("Remove All Provider SDKs", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog(
                        "Remove Provider SDKs",
                        "Remove all provider UPM/tgz entries from manifest.json? Core dependencies and framework code are kept.",
                        "Remove",
                        "Cancel"))
                {
                    ManifestPackageUtility.UninstallAllProviders(_config);
                }
            }

            GUILayout.Space(6);

            if (GUILayout.Button("Uninstall Framework (keep Installer + Logo)", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog(
                        "Uninstall Framework",
                        "Remove all manifest entries and delete Monetization folders except Installer and Logo?",
                        "Uninstall",
                        "Cancel"))
                {
                    ManifestPackageUtility.UninstallAllManifestEntries(_config);
                    ManifestPackageUtility.UninstallFrameworkAssets();
                    EditorUtility.DisplayDialog("Monetization", "Framework uninstalled. Bootstrap installer remains.", "OK");
                }
            }

            EditorGUILayout.EndVertical();
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

            EditorGUILayout.HelpBox(string.Join("\n", warnings), MessageType.Warning);
        }
    }
}
