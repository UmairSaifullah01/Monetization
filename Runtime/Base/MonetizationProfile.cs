using System;
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
		private readonly ModuleRegistry moduleRegistry = new ModuleRegistry();
		private readonly List<string> failedModules = new List<string>();

		public bool IsInitialized => isInitialized;
		public IReadOnlyList<string> FailedModules => failedModules;

		/// <summary>
		/// Checks if the device has internet connectivity.
		/// </summary>
		public static bool IsInternetAvailable()
		{
#if UNITY_EDITOR
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

			failedModules.Clear();
			var succeeded = new List<MonetizationModule>();
			var initializationTasks = new List<UTask>();

			foreach (MonetizationModule module in modules)
			{
				if (module == null)
				{
					SendLog.LogError("Null module found in MonetizationProfile. Skipping...");
					continue;
				}

				initializationTasks.Add(InitModuleSafe(module, failedModules, succeeded));
			}

			await UTask.WhenAll(initializationTasks.ToArray());

			BuildModuleCache(succeeded);
			isInitialized = true;

			if (failedModules.Count > 0)
			{
				var messages = failedModules.Select(m => m).ToList();
				messages.Insert(0, $"Some modules failed to initialize ({failedModules.Count}/{modules.Count}):");
				SendLog.LogBatch(messages, LogLevel.Warning);

				if (succeeded.Count == 0)
				{
					SendLog.LogBatch(new List<string> { "All modules failed to initialize. Monetization cache is empty." }, LogLevel.Error);
				}
			}
			else if (succeeded.Count > 0)
			{
				SendLog.Log($"All {succeeded.Count} modules initialized successfully.");
			}
		}

		private static async UTask InitModuleSafe(MonetizationModule module, List<string> failed, List<MonetizationModule> succeeded)
		{
			if (module == null)
			{
				return;
			}

			try
			{
				await module.Initialize();
				if (module.IsInitialized)
				{
					succeeded.Add(module);
				}
				else
				{
					failed.Add($"{module.GetType().Name}: initialization did not complete");
				}
			}
			catch (Exception ex)
			{
				failed.Add($"{module.GetType().Name}: {ex.Message}");
				SendLog.LogModule(module.ModuleName, ex.Message, LogLevel.Error);
			}
		}

		private void RemoveDuplicateModules()
		{
			var typeToModule = new Dictionary<Type, MonetizationModule>();
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

		private void BuildModuleCache(IReadOnlyList<MonetizationModule> succeeded)
		{
			moduleRegistry.Clear();
			foreach (var module in succeeded)
			{
				if (module != null)
				{
					moduleRegistry.Register(module);
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

			var module = moduleRegistry.Get<T>();
			if (module != null)
			{
				return module;
			}

			SendLog.LogWarning($"Module of type {typeof(T).Name} not found in profile.");
			return default;
		}

		/// <summary>
		/// Tries to get a module of the specified type without logging errors.
		/// </summary>
		public bool TryGetModule<T>(out T module) where T : class, IModule
		{
			module = default;
			if (!isInitialized)
			{
				return false;
			}

			return moduleRegistry.TryGet(out module);
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
			failedModules.Clear();
			moduleRegistry.Clear();
		}

	}
}
