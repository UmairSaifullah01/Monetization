using System.IO;
using UnityEditor;
using UnityEngine;

namespace Installer
{
    public class MonetizationInstallerWindow : EditorWindow
    {
        private Texture2D _logoTexture;
        private string _unityPackagePath;
        private FrameworkInstallModule _installModule;

        [MenuItem("Tools/Monetization/Installer")]
        public static void ShowWindow()
        {
            if (FrameworkInstallModule.IsFrameworkInstalled() && InstallerReflectionBridge.IsEditorBridgeAvailable())
            {
                InstallerReflectionBridge.TryOpenPackageManagerWindow();
                return;
            }

            var window = GetWindow<MonetizationInstallerWindow>("Monetization Installer");
            window.minSize = new Vector2(500, 420);
            window.maxSize = new Vector2(500, 420);
        }

        private void OnEnable()
        {
            _logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Monetization/Logo/logo.png");
            _unityPackagePath = Path.Combine(Application.dataPath, "Monetization/Installer/MonetizationScripts.unitypackage");
            _installModule = new FrameworkInstallModule(_unityPackagePath);
            _installModule.SetOnStateChanged(Repaint);
        }

        private void OnDisable()
        {
            _installModule?.Stop();
        }

        private void OnGUI()
        {
            GUILayout.Space(16);
            DrawHeader();
            GUILayout.Space(10);

            if (FrameworkInstallModule.IsFrameworkInstalled())
            {
                DrawFrameworkInstalledState();
                return;
            }

            DrawFrameworkInstallPanel();
        }

        private void DrawFrameworkInstallPanel()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(420));
            GUILayout.Label(
                "Install imports the Monetization framework, then reads installer_config.json to add core dependencies (UTask).\n\n" +
                "SDK providers are managed from the same window after installation completes.",
                EditorStyles.wordWrappedLabel,
                GUILayout.Height(90));
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(12);

            if (_installModule.Phase != FrameworkInstallModule.InstallPhase.Idle &&
                _installModule.Phase != FrameworkInstallModule.InstallPhase.Complete &&
                _installModule.Phase != FrameworkInstallModule.InstallPhase.Failed)
            {
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(420, 22), _installModule.Progress, _installModule.StatusMessage);
                GUILayout.Space(8);
            }

            if (_installModule.Phase == FrameworkInstallModule.InstallPhase.Failed)
            {
                EditorGUILayout.HelpBox(_installModule.StatusMessage, MessageType.Error);
            }

            if (_installModule.Phase == FrameworkInstallModule.InstallPhase.Complete)
            {
                EditorGUILayout.HelpBox("Installation complete. Opening Package Manager...", MessageType.Info);
            }

            bool isBusy = _installModule.Phase != FrameworkInstallModule.InstallPhase.Idle &&
                          _installModule.Phase != FrameworkInstallModule.InstallPhase.Failed &&
                          _installModule.Phase != FrameworkInstallModule.InstallPhase.Complete;

            EditorGUI.BeginDisabledGroup(isBusy);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Install Monetization", GUILayout.Width(240), GUILayout.Height(40)))
            {
                _installModule.StartInstall();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            if (!File.Exists(_unityPackagePath))
            {
                EditorGUILayout.HelpBox("MonetizationScripts.unitypackage not found at Assets/Monetization/Installer/.", MessageType.Error);
            }
        }

        private void DrawFrameworkInstalledState()
        {
            EditorGUILayout.HelpBox(
                "Monetization framework is installed. Use the Package Manager panel to install SDK providers from installer_config.json.",
                MessageType.Info);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Package Manager", GUILayout.Width(220), GUILayout.Height(36)))
            {
                InstallerReflectionBridge.TryOpenPackageManagerWindow();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (_logoTexture != null)
            {
                GUILayout.Label(_logoTexture, GUILayout.Width(64), GUILayout.Height(64));
                GUILayout.Space(8);
            }

            GUILayout.Label("Monetization Installer", new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            }, GUILayout.Height(48));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Label("Version 4.0b - Developed by Umair Saifullah", new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            });
        }
    }
}
