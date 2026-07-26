using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Editor
{
    public class MonetizationInstallerEditorWindow : EditorWindow
    {
        private PackageManagerModule _packageManagerModule;

        public static void Open()
        {
            var window = GetWindow<MonetizationInstallerEditorWindow>("Monetization Installer");
            window.minSize = new Vector2(520, 520);
        }

        private void OnEnable()
        {
            _packageManagerModule = new PackageManagerModule();
            _packageManagerModule.OnEnable();
        }

        private void OnGUI()
        {
            _packageManagerModule?.DrawGUI();
        }
    }
}
