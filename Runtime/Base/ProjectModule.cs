using THEBADDEST.Tasks;
using UnityEngine;

namespace THEBADDEST.MonetizationApi
{
	/// <summary>
	/// Module that manages Unity project settings from JSON configuration.
	/// </summary>
	[CreateAssetMenu(menuName = "THEBADDEST/MonetizationApi/ProjectModule", fileName = "ProjectModule", order = 0)]
	public class ProjectModule : MonetizationModule
	{
		private const string PROJECT_KEYS_CATEGORY = "ProjectKeys";

		// Fixed key names for JSON lookup
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
		
		[Tooltip("Shows all available Project Settings from JSON.")]
		[JsonDataCategory(PROJECT_KEYS_CATEGORY)]
		[SerializeField] private string projectKeysReference = "";

		// Cached loaded values
		private string _cachedPackageName;
		private string _cachedVersion;
		private int _cachedBundleVersionCode;
		private int _cachedMinApiLevel;
		private int _cachedTargetApiLevel;
		private bool _cachedUseKeyStore;
		private string _cachedKeyStorePath;
		private string _cachedKeyAliasName;
		private string _cachedKeyStorePassword;
		private string _cachedKeyAliasPassword;

		/// <summary>
		/// Loads all project settings from JSON.
		/// </summary>
		private void LoadProjectSettingsFromJson()
		{
			// Ensure JsonDataUtility is loaded
			JsonDataUtility.LoadData();

			// Load Package Name
			_cachedPackageName = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, PACKAGE_NAME_KEY);
			if (string.IsNullOrEmpty(_cachedPackageName))
			{
				_cachedPackageName = "com.games.gamename";
			}

			// Load Version
			_cachedVersion = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, VERSION_KEY);
			if (string.IsNullOrEmpty(_cachedVersion))
			{
				_cachedVersion = "1.0";
			}

			// Load Bundle Version Code
			string bundleVersionCodeStr = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, BUNDLE_VERSION_CODE_KEY);
			if (!string.IsNullOrEmpty(bundleVersionCodeStr) && int.TryParse(bundleVersionCodeStr, out int bundleVersionCode))
			{
				_cachedBundleVersionCode = bundleVersionCode;
			}
			else
			{
				_cachedBundleVersionCode = 1;
			}

			// Load Min API Level
			string minApiLevelStr = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, MIN_API_LEVEL_KEY);
			if (!string.IsNullOrEmpty(minApiLevelStr) && int.TryParse(minApiLevelStr, out int minApiLevel))
			{
				_cachedMinApiLevel = minApiLevel;
			}
			else
			{
				_cachedMinApiLevel = 22;
			}

			// Load Target API Level
			string targetApiLevelStr = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, TARGET_API_LEVEL_KEY);
			if (!string.IsNullOrEmpty(targetApiLevelStr) && int.TryParse(targetApiLevelStr, out int targetApiLevel))
			{
				_cachedTargetApiLevel = targetApiLevel;
			}
			else
			{
				_cachedTargetApiLevel = 35;
			}

			// Load Use Key Store
			string useKeyStoreStr = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, USE_KEY_STORE_KEY);
			if (!string.IsNullOrEmpty(useKeyStoreStr) && bool.TryParse(useKeyStoreStr, out bool useKeyStore))
			{
				_cachedUseKeyStore = useKeyStore;
			}
			else
			{
				_cachedUseKeyStore = true;
			}

			// Load Key Store Path
			_cachedKeyStorePath = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, KEY_STORE_PATH_KEY);
			if (string.IsNullOrEmpty(_cachedKeyStorePath))
			{
				_cachedKeyStorePath = "Assets/Keystore/user.keystore";
			}

			// Load Key Alias Name
			_cachedKeyAliasName = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, KEY_ALIAS_NAME_KEY);
			if (string.IsNullOrEmpty(_cachedKeyAliasName))
			{
				_cachedKeyAliasName = "user";
			}

			// Load Key Store Password
			_cachedKeyStorePassword = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, KEY_STORE_PASSWORD_KEY);
			if (string.IsNullOrEmpty(_cachedKeyStorePassword))
			{
				_cachedKeyStorePassword = "123456";
			}

			// Load Key Alias Password
			_cachedKeyAliasPassword = JsonDataUtility.GetData(PROJECT_KEYS_CATEGORY, KEY_ALIAS_PASSWORD_KEY);
			if (string.IsNullOrEmpty(_cachedKeyAliasPassword))
			{
				_cachedKeyAliasPassword = "123456";
			}
		}

		/// <summary>
		/// Initializes the module by loading settings from JSON.
		/// </summary>
		protected override async UTask OnInitialize()
		{
			LoadProjectSettingsFromJson();
			SendLog.Log($"[ProjectModule] Project settings loaded from JSON.");
			await UTask.CompletedTask;
		}

		/// <summary>
		/// Updates the module by syncing settings to Unity PlayerSettings.
		/// </summary>
		protected override void OnUpdateModule()
		{
#if UNITY_EDITOR
			// Reload settings from JSON
			LoadProjectSettingsFromJson();

			// Sync to Unity PlayerSettings
			UnityEditor.PlayerSettings.applicationIdentifier = _cachedPackageName;
			UnityEditor.PlayerSettings.bundleVersion = _cachedVersion;

#if UNITY_ANDROID
			UnityEditor.PlayerSettings.Android.bundleVersionCode = _cachedBundleVersionCode;
			UnityEditor.PlayerSettings.Android.minSdkVersion = (UnityEditor.AndroidSdkVersions)_cachedMinApiLevel;
			UnityEditor.PlayerSettings.Android.targetSdkVersion = (UnityEditor.AndroidSdkVersions)_cachedTargetApiLevel;
			UnityEditor.PlayerSettings.Android.useCustomKeystore = _cachedUseKeyStore;
			if (_cachedUseKeyStore)
			{
				UnityEditor.PlayerSettings.Android.keystoreName = _cachedKeyStorePath;
				UnityEditor.PlayerSettings.Android.keyaliasName = _cachedKeyAliasName;
				UnityEditor.PlayerSettings.Android.keystorePass = _cachedKeyStorePassword;
				UnityEditor.PlayerSettings.Android.keyaliasPass = _cachedKeyAliasPassword;
			}
#endif

			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();

			SendLog.Log($"[ProjectModule] Project settings synced: Package={_cachedPackageName}, Version={_cachedVersion}, BundleCode={_cachedBundleVersionCode}, MinAPI={_cachedMinApiLevel}, TargetAPI={_cachedTargetApiLevel}, UseKeyStore={_cachedUseKeyStore}");
			if (_cachedUseKeyStore)
			{
				SendLog.Log($"[ProjectModule] KeyStore Path={_cachedKeyStorePath}, Alias={_cachedKeyAliasName}");
			}
#endif
		}
	}
}

