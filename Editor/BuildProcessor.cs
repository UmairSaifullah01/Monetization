using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.MonetizationEditor
{
    public class BuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // Find MonetizationProfile asset
            var profile = AssetDatabase.FindAssets("t:MonetizationProfile")
                .Select(guid => AssetDatabase.LoadAssetAtPath<MonetizationProfile>(AssetDatabase.GUIDToAssetPath(guid)))
                .FirstOrDefault();
            if (profile != null)
            {
                Debug.Log("[Monetization] Updating project details before build...");
                profile.UpdateProjectDetails();
            }
            else
            {
                Debug.LogWarning("[Monetization] MonetizationProfile not found before build.");
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            Debug.Log("[Monetization] Renaming build file after build...");

            string buildPath = report.summary.outputPath;
            string ext = System.IO.Path.GetExtension(buildPath).ToLower();
            if (ext != ".apk" && ext != ".aab")
            {
                Debug.LogWarning($"[Monetization] Build output is not APK or AAB: {buildPath}");
                return;
            }

            // Get info from PlayerSettings
            string gameName = UnityEngine.Application.productName;
            string version = UnityEditor.PlayerSettings.bundleVersion;
            int bundleCode = 1;
#if UNITY_ANDROID
            bundleCode = UnityEditor.PlayerSettings.Android.bundleVersionCode;
#endif
            string date = System.DateTime.Now.ToString("yyyyMMdd_HHmm");

            string dir = System.IO.Path.GetDirectoryName(buildPath);
            string newName = $"{gameName}_v{version}_{bundleCode}_{date}{ext}";
            string newPath = System.IO.Path.Combine(dir, newName);

            try
            {
                System.IO.File.Move(buildPath, newPath);
                Debug.Log($"[Monetization] Renamed build file to: {newName}");
                UnityEditor.EditorUtility.RevealInFinder(newPath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Monetization] Failed to rename build file: {ex.Message}");
            }
        }
    }
} 