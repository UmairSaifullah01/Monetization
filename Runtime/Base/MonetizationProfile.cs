using System.Collections;
using System.Collections.Generic;
using System.Linq;
using THEBADDEST.Tasks;
using UnityEngine;

namespace THEBADDEST.MonetizationApi
{
	/// <summary>
	/// ScriptableObject that holds and manages all monetization modules.
	/// </summary>
	[CreateAssetMenu(menuName = "THEBADDEST/MonetizationApi/MonetizationProfile", fileName = "MonetizationProfile", order = 0)]
	public class MonetizationProfile : ScriptableObject, IEnumerable<MonetizationModule>
	{
		/// <summary>
		/// List of all modules in this profile. Only one module per type is allowed.
		/// </summary>
		public List<MonetizationModule> modules = new List<MonetizationModule>();

		private bool isInitialized = false;
		private Dictionary<System.Type, IModule> moduleCache = new Dictionary<System.Type, IModule>();

		public bool IsInitialized => isInitialized;

		/// <summary>
		/// Checks if the device has internet connectivity.
		/// </summary>
		public static bool IsInternetAvailable()
		{
#if UNITY_EDITOR
			// In editor, always return true for testing
			return true;
#else
			return Application.internetReachability != NetworkReachability.NotReachable;
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

			var config = MonetizationConfig.Instance;

			config.ApplySendLogConfiguration();

			if (config.CheckInternetBeforeInit && !IsInternetAvailable())
			{
				SendLog.LogError("No internet connection. Initialization aborted.");
				return;
			}

			if (config.ValidateModulesOnStart)
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
				var messages = failedModules.Select(m => $"Module failed to initialize: {m}").ToList();
				messages.Insert(0, $"Some modules failed to initialize ({failedModules.Count}/{modules.Count}):");
				SendLog.LogBatch(messages, LogLevel.Warning);
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
		/// Finds a module without requiring runtime initialization. Use in editor/build flows.
		/// </summary>
		public T FindModule<T>() where T : class, IModule
		{
			foreach (MonetizationModule module in modules)
			{
				if (module is T result)
				{
					return result;
				}
			}

			return default;
		}

		/// <summary>
		/// Gets a module of the specified type.
		/// </summary>
		public T GetModule<T>() where T : class, IModule
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