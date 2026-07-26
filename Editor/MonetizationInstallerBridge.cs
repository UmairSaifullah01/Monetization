using UnityEditor;

namespace THEBADDEST.MonetizationApi.Editor
{
    [InitializeOnLoad]
    public static class MonetizationInstallerBridge
    {
        private const string EditorPrefsShowPackageManager = "MonetizationInstaller_ShowPackageManager";

        static MonetizationInstallerBridge()
        {
            EditorApplication.delayCall += TryOpenFromEditorPrefs;
        }

        public static void OpenPackageManagerWindow()
        {
            MonetizationInstallerEditorWindow.Open();
        }

        private static void TryOpenFromEditorPrefs()
        {
            if (!EditorPrefs.GetBool(EditorPrefsShowPackageManager, false))
            {
                return;
            }

            EditorPrefs.DeleteKey(EditorPrefsShowPackageManager);
            OpenPackageManagerWindow();
        }
    }
}
