using System.Collections;
using System.Collections.Generic;
using THEBADDEST.Tasks;
using UnityEngine;

namespace THEBADDEST.MonetizationApi
{


	/// <summary>
	/// ScriptableObject that holds and manages all monetization modules.
	/// </summary>
	[CreateAssetMenu(menuName = "Monotization/MonotizationProfile", fileName = "MonotizationProfile", order = 0)]
	public class MonetizationProfile : ScriptableObject, IEnumerable<MonetizationModule>
	{
		[SerializeField] bool debugLog = true;
		[SerializeField] bool validateModulesOnStart = true;

		/// <summary>
		/// List of all modules in this profile. Only one module per type is allowed.
		/// </summary>
		public List<MonetizationModule> modules = new List<MonetizationModule>();

		private bool isInitialized = false;
		private Dictionary<System.Type, IModule> moduleCache = new Dictionary<System.Type, IModule>();

		public bool IsInitialized => isInitialized;

		[SerializeField] private string packageName = "com.games.gamename";
		[SerializeField] private string version = "1.0";
		[SerializeField] private int bundleVersionCode = 1;
		[SerializeField] private int minApiLevel = 22;
		[SerializeField] private int targetApiLevel = 35;

		[SerializeField] private bool useKeyStore = true;
		[SerializeField] private string keyStorePath = "Assets/Keystore/user.keystore";
		[SerializeField] private string keyAliasName = "user";
		[SerializeField] private string keyStorePassword = "123456";
		[SerializeField] private string keyAliasPassword = "123456";

		/// <summary>
		/// Updates all project details. Call this from the editor.
		/// </summary>
		public void UpdateProjectDetails()
		{
#if UNITY_EDITOR
			// If all values are empty, fetch from PlayerSettings
			bool allEmpty = string.IsNullOrEmpty(packageName)
				&& string.IsNullOrEmpty(version)
				&& bundleVersionCode == 0
				&& minApiLevel == 0
				&& targetApiLevel == 0
				&& !useKeyStore
				&& string.IsNullOrEmpty(keyStorePath)
				&& string.IsNullOrEmpty(keyAliasName)
				&& string.IsNullOrEmpty(keyStorePassword)
				&& string.IsNullOrEmpty(keyAliasPassword);

			if (allEmpty)
			{
				packageName = UnityEditor.PlayerSettings.applicationIdentifier;
				version = UnityEditor.PlayerSettings.bundleVersion;
#if UNITY_ANDROID
				bundleVersionCode = UnityEditor.PlayerSettings.Android.bundleVersionCode;
				minApiLevel = (int)UnityEditor.PlayerSettings.Android.minSdkVersion;
				targetApiLevel = (int)UnityEditor.PlayerSettings.Android.targetSdkVersion;
				useKeyStore = UnityEditor.PlayerSettings.Android.useCustomKeystore;
				keyStorePath = UnityEditor.PlayerSettings.Android.keystoreName;
				keyAliasName = UnityEditor.PlayerSettings.Android.keyaliasName;
				keyStorePassword = UnityEditor.PlayerSettings.Android.keystorePass;
				keyAliasPassword = UnityEditor.PlayerSettings.Android.keyaliasPass;
#endif
			}
#endif
			SendLog.Log($"Updating project details: Package={packageName}, Version={version}, BundleCode={bundleVersionCode}, MinAPI={minApiLevel}, TargetAPI={targetApiLevel}, UseKeyStore={useKeyStore}");
			if (useKeyStore)
			{
				SendLog.Log($"KeyStore Path={keyStorePath}, Alias={keyAliasName}");
			}
#if UNITY_EDITOR
			var playerSettings = typeof(UnityEditor.PlayerSettings);
			UnityEditor.PlayerSettings.applicationIdentifier = packageName;
			UnityEditor.PlayerSettings.bundleVersion = version;
#if UNITY_ANDROID
			UnityEditor.PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
			UnityEditor.PlayerSettings.Android.minSdkVersion = (UnityEditor.AndroidSdkVersions)minApiLevel;
			UnityEditor.PlayerSettings.Android.targetSdkVersion = (UnityEditor.AndroidSdkVersions)targetApiLevel;
			UnityEditor.PlayerSettings.Android.useCustomKeystore = useKeyStore;
			if (useKeyStore)
			{
				UnityEditor.PlayerSettings.Android.keystoreName = keyStorePath;
				UnityEditor.PlayerSettings.Android.keyaliasName = keyAliasName;
				UnityEditor.PlayerSettings.Android.keystorePass = keyStorePassword;
				UnityEditor.PlayerSettings.Android.keyaliasPass = keyAliasPassword;
			}
#endif
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
#endif
		}

		public IEnumerator<MonetizationModule> GetEnumerator()
		{
			return modules.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Initializes all modules in the profile asynchronously.
		/// </summary>
		public async UTask Initialize()
		{
			if (isInitialized)
			{
				SendLog.Log("MonetizationProfile already initialized.");
				return;
			}

			SendLog.Enabled = debugLog;

			if (validateModulesOnStart)
			{
				RemoveDuplicateModules();
			}

			var initializationTasks = new List<UTask>();
			var failedModules = new List<string>();

			foreach (MonetizationModule module in modules)
			{
				if (module == null)
				{
					SendLog.LogError("Null module found in MonetizationProfile. Skipping...");
					continue;
				}

				try
				{
					var task = module.Initialize();
					initializationTasks.Add(task);
				}
				catch (System.Exception ex)
				{
					failedModules.Add(module.GetType().Name);
					SendLog.LogError($"Failed to initialize {module.GetType().Name}: {ex.Message}");
				}
			}

			// Wait for all modules to initialize
			await UTask.WhenAll(initializationTasks.ToArray());

			// Build module cache for faster lookups
			BuildModuleCache();

			isInitialized = true;

			if (failedModules.Count > 0)
			{
				SendLog.LogWarning($"Some modules failed to initialize: {string.Join(", ", failedModules)}");
			}
			else
			{
				SendLog.Log($"All {modules.Count} modules initialized successfully.");
			}
		}

		private void RemoveDuplicateModules()
		{
			var typeToModule = new Dictionary<System.Type, MonetizationModule>();
			var toRemove = new List<MonetizationModule>();
			foreach (var module in modules)
			{
				if (module == null) continue;
				var type = module.GetType();
				if (typeToModule.ContainsKey(type))
				{
					SendLog.LogWarning($"Duplicate module of type {type.Name} found. Removing duplicate.");
					toRemove.Add(module);
				}
				else
				{
					typeToModule[type] = module;
				}
			}
			foreach (var module in toRemove)
			{
				modules.Remove(module);
			}
		}

		private void ValidateModules()
		{
			var moduleTypes = new HashSet<System.Type>();
			var duplicates = new List<string>();

			foreach (var module in modules)
			{
				if (module == null) continue;

				var moduleType = module.GetType();
				if (moduleTypes.Contains(moduleType))
				{
					duplicates.Add(moduleType.Name);
				}
				else
				{
					moduleTypes.Add(moduleType);
				}
			}

			if (duplicates.Count > 0)
			{
				SendLog.LogWarning($"Duplicate modules found: {string.Join(", ", duplicates)}");
			}
		}

		private void BuildModuleCache()
		{
			moduleCache.Clear();
			foreach (var module in modules)
			{
				if (module != null)
				{
					moduleCache[module.GetType()] = module as IModule;
				}
			}
		}

		/// <summary>
		/// Gets a module of the specified type.
		/// </summary>
		internal T GetModule<T>() where T : class, IModule
		{
			if (!isInitialized)
			{
				SendLog.LogError("MonetizationProfile not initialized. Call Initialize() first.");
				return default;
			}

			// Try cache first for better performance
			if (moduleCache.TryGetValue(typeof(T), out var cachedModule))
			{
				return cachedModule as T;
			}

			// Fallback to linear search
			foreach (MonetizationModule module in modules)
			{
				if (module is T result)
				{
					return result;
				}
			}

			SendLog.LogWarning($"Module of type {typeof(T).Name} not found in profile.");
			return default;
		}

		/// <summary>
		/// Updates all modules in the profile.
		/// </summary>
		public void UpdateModules()
		{
			foreach (MonetizationModule module in modules)
			{
				if (module != null)
				{
					module.UpdateModule();
				}
			}
		}

		/// <summary>
		/// Resets the profile and all module caches.
		/// </summary>
		public void Reset()
		{
			isInitialized = false;
			moduleCache.Clear();
		}
	}
}