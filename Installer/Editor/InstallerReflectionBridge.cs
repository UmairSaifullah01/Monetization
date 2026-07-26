using System;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Installer
{
    public static class InstallerReflectionBridge
    {
        private const string EditorBridgeType = "THEBADDEST.MonetizationApi.Editor.MonetizationInstallerBridge, THEBADDEST.Monetization.Editor";

        public static void TryOpenPackageManagerWindow()
        {
            try
            {
                Type bridgeType = Type.GetType(EditorBridgeType);
                if (bridgeType == null)
                {
                    return;
                }

                bridgeType.GetMethod("OpenPackageManagerWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    ?.Invoke(null, null);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Monetization Installer] Package Manager window not available yet: " + ex.Message);
            }
        }

        public static bool IsEditorBridgeAvailable()
        {
            return Type.GetType(EditorBridgeType) != null;
        }
    }
}
