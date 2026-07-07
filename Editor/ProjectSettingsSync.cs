using THEBADDEST.MonetizationApi;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationEditor
{
	public static class ProjectSettingsSync
	{
		private const string PROJECT_KEYS_CATEGORY = "ProjectKeys";
		private const string PACKAGE_NAME_KEY = "PackageName";
		private const string VERSION_KEY = "Version";
		private const string BUNDLE_VERSION_CODE_KEY = "BundleVersionCode";
		private const string MIN_API_LEVEL_KEY = "MinApiLevel";
		private const string TARGET_API_LEVEL_KEY = "TargetApiLevel";
		private const string USE_KEY_STORE_KEY = "UseKeyStore";
		private const string KEY_STORE_PATH_KEY = "KeyStorePath";
		private const string KEY_ALIAS_NAME_KEY = "KeyAliasName";
		private const string KEY_STORE_PASSWORD_KEY = "KeyStorePassword";
		private const string KEY_ALIAS_PASSWORD_KEY = "KeyAliasPassword";

		public static void SyncFromJson()
		{
			JsonDataUtility.LoadData();

			string packageName = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, PACKAGE_NAME_KEY) ?? "com.games.gamename";
			string version = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, VERSION_KEY) ?? "1.0";
			int bundleVersionCode = ParseInt(JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, BUNDLE_VERSION_CODE_KEY), 1);
			int minApiLevel = ParseInt(JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, MIN_API_LEVEL_KEY), 22);
			int targetApiLevel = ParseInt(JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, TARGET_API_LEVEL_KEY), 35);
			bool useKeyStore = ParseBool(JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, USE_KEY_STORE_KEY), true);
			string keyStorePath = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, KEY_STORE_PATH_KEY) ?? "Assets/Keystore/user.keystore";
			string keyAliasName = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, KEY_ALIAS_NAME_KEY) ?? "user";
			string keyStorePassword = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, KEY_STORE_PASSWORD_KEY) ?? string.Empty;
			string keyAliasPassword = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, KEY_ALIAS_PASSWORD_KEY) ?? string.Empty;

			PlayerSettings.applicationIdentifier = packageName;
			PlayerSettings.bundleVersion = version;

#if UNITY_ANDROID
			PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
			PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)minApiLevel;
			PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)targetApiLevel;
			PlayerSettings.Android.useCustomKeystore = useKeyStore;
			if (useKeyStore)
			{
				PlayerSettings.Android.keystoreName = keyStorePath;
				PlayerSettings.Android.keyaliasName = keyAliasName;
				if (!string.IsNullOrEmpty(keyStorePassword))
				{
					PlayerSettings.Android.keystorePass = keyStorePassword;
				}
				if (!string.IsNullOrEmpty(keyAliasPassword))
				{
					PlayerSettings.Android.keyaliasPass = keyAliasPassword;
				}
			}
#endif

			AssetDatabase.SaveAssets();
			Debug.Log($"[Monetization] Project settings synced: Package={packageName}, Version={version}, BundleCode={bundleVersionCode}, MinAPI={minApiLevel}, TargetAPI={targetApiLevel}, UseKeyStore={useKeyStore}");
		}

		private static int ParseInt(string value, int fallback)
		{
			return int.TryParse(value, out int result) ? result : fallback;
		}

		private static bool ParseBool(string value, bool fallback)
		{
			return bool.TryParse(value, out bool result) ? result : fallback;
		}
	}
}
