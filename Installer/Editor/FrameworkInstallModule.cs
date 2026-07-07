using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Installer
{
    public class FrameworkInstallModule
    {
        public const string CoreAsmdefName = "THEBADDEST.Monetization.Core";
        public const string EditorPrefsShowPackageManager = "MonetizationInstaller_ShowPackageManager";

        public enum InstallPhase
        {
            Idle,
            ImportingFramework,
            WaitingForConfig,
            InstallingCore,
            ResolvingPackages,
            Complete,
            Failed
        }

        public InstallPhase Phase { get; private set; } = InstallPhase.Idle;
        public float Progress { get; private set; }
        public string StatusMessage { get; private set; } = string.Empty;

        private readonly string _unityPackagePath;
        private int _installStep;
        private double _stepStartTime;
        private Action _onStateChanged;

        public FrameworkInstallModule(string unityPackagePath)
        {
            _unityPackagePath = unityPackagePath;
        }

        public void SetOnStateChanged(Action callback)
        {
            _onStateChanged = callback;
        }

        public static bool IsFrameworkInstalled()
        {
            string[] guids = AssetDatabase.FindAssets($"{CoreAsmdefName} t:AssemblyDefinitionAsset");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == CoreAsmdefName)
                {
                    return true;
                }
            }

            return false;
        }

        public void StartInstall()
        {
            if (!File.Exists(_unityPackagePath))
            {
                Phase = InstallPhase.Failed;
                StatusMessage = "MonetizationScripts.unitypackage not found.";
                NotifyChanged();
                return;
            }

            Phase = InstallPhase.ImportingFramework;
            Progress = 0.1f;
            StatusMessage = "Importing Monetization framework...";
            _installStep = 0;
            _stepStartTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += InstallStep;
            NotifyChanged();
        }

        public void Stop()
        {
            EditorApplication.update -= InstallStep;
            if (Phase != InstallPhase.Complete)
            {
                Phase = InstallPhase.Idle;
            }
        }

        private void InstallStep()
        {
            try
            {
                switch (_installStep)
                {
                    case 0:
                        AssetDatabase.ImportPackage(_unityPackagePath, false);
                        Phase = InstallPhase.WaitingForConfig;
                        Progress = 0.35f;
                        StatusMessage = "Waiting for installer_config.json...";
                        _installStep++;
                        _stepStartTime = EditorApplication.timeSinceStartup;
                        break;

                    case 1:
                        if (!File.Exists(BootstrapInstallerConfig.GetConfigPath()))
                        {
                            if (EditorApplication.timeSinceStartup - _stepStartTime > 120d)
                            {
                                Fail("installer_config.json did not appear after unitypackage import.");
                            }
                            return;
                        }

                        Phase = InstallPhase.InstallingCore;
                        Progress = 0.55f;
                        StatusMessage = "Installing core dependencies from installer_config.json...";
                        var config = BootstrapInstallerConfig.LoadDefault();
                        if (config == null || config.corePackages.Count == 0)
                        {
                            Fail("installer_config.json is missing or has no corePackages.");
                            return;
                        }

                        BootstrapManifestUtility.InstallCorePackages(config.corePackages, config.registries);
                        _installStep++;
                        _stepStartTime = EditorApplication.timeSinceStartup;
                        break;

                    case 2:
                        Phase = InstallPhase.ResolvingPackages;
                        Progress = 0.8f;
                        StatusMessage = "Resolving packages and waiting for Core to compile...";

                        var loadedConfig = BootstrapInstallerConfig.LoadDefault();
                        bool coreInManifest = false;
                        if (loadedConfig != null)
                        {
                            foreach (var kvp in loadedConfig.corePackages)
                            {
                                if (BootstrapManifestUtility.IsCorePackageInstalled(kvp.Key))
                                {
                                    coreInManifest = true;
                                    break;
                                }
                            }
                        }

                        if (coreInManifest && IsFrameworkInstalled())
                        {
                            Phase = InstallPhase.Complete;
                            Progress = 1f;
                            StatusMessage = "Installation complete.";
                            EditorPrefs.SetBool(EditorPrefsShowPackageManager, true);
                            EditorApplication.update -= InstallStep;
                            NotifyChanged();
                            InstallerReflectionBridge.TryOpenPackageManagerWindow();
                            return;
                        }

                        if (EditorApplication.timeSinceStartup - _stepStartTime > 180d)
                        {
                            Fail("Timed out waiting for core packages and framework compilation.");
                        }
                        break;
                }

                NotifyChanged();
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
        }

        private void Fail(string message)
        {
            Phase = InstallPhase.Failed;
            StatusMessage = message;
            Debug.LogError("[Monetization Installer] " + message);
            EditorApplication.update -= InstallStep;
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            _onStateChanged?.Invoke();
        }
    }
}
