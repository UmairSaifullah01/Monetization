using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using THEBADDEST.Tasks;
using UnityEngine;

namespace THEBADDEST.MonetizationApi
{
	/// <summary>
	/// ScriptableObject that holds settings and all monetization modules.
	/// </summary>
	[CreateAssetMenu(menuName = "THEBADDEST/MonetizationApi/MonetizationProfile", fileName = "MonetizationProfile", order = 0)]
	public class MonetizationProfile : ScriptableObject, IEnumerable<MonetizationModule>, IMonetizationSettings
	{
		[Header("General Settings")]
		[SerializeField] private bool enableDebugLogs = true;
		[SerializeField] private LogLevel logLevel = LogLevel.Info;
		[SerializeField] private bool enablePerformanceLogging = false;
		[SerializeField] private int maxRetryAttempts = 3;
		[SerializeField] private float retryDelaySeconds = 2f;
		[Tooltip("If enabled, checks for internet connectivity before initializing modules.")]
		[SerializeField] private bool checkInternetBeforeInit = true;
		[Tooltip("If enabled, validates and removes duplicate modules on start.")]
		[SerializeField] private bool validateModulesOnStart = true;
		[Tooltip("If enabled, Sync Project applies Android custom keystore settings from ProjectKeys.")]
		[SerializeField] private bool useKeyStore = true;

		/// <summary>
		/// List of all modules in this profile. Only one module per type is allowed.
		/// </summary>
		public List<MonetizationModule> modules = new List<MonetizationModule>();

		private bool isInitialized = false;
		private readonly ModuleRegistry moduleRegistry = new ModuleRegistry();
		private readonly List<string> failedModules = new List<string>();
		private IModuleContext moduleContext;

		public bool IsInitialized => isInitialized;
		public IReadOnlyList<string> FailedModules => failedModules;
		public IModuleContext Context => moduleContext;

		public bool EnableDebugLogs => enableDebugLogs;
		public LogLevel LogLevel => logLevel;
		public bool EnablePerformanceLogging => enablePerformanceLogging;
		public int MaxRetryAttempts => maxRetryAttempts;
		public float RetryDelaySeconds => retryDelaySeconds;
		public bool CheckInternetBeforeInit => checkInternetBeforeInit;
		public bool ValidateModulesOnStart => validateModulesOnStart;
		public bool UseKeyStore => useKeyStore;

		public static bool IsInternetAvailable() => InternetChecker.IsAvailable();

		public void ApplySendLogConfiguration()
		{
			SendLog.Enabled = enableDebugLogs;
			SendLog.CurrentLogLevel = logLevel;
		}

		public IEnumerator<MonetizationModule> GetEnumerator()
		{
			return modules.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public async UTask Initialize(IModuleContext context = null)
		{
			if (isInitialized)
			{
				SendLog.Log("MonetizationProfile already initialized.");
				return;
			}

			moduleContext = context ?? new ModuleContext(this, NullKeyValueCatalog.Instance, NullAdMetrics.Instance);
			ApplySendLogConfiguration();

			if (CheckInternetBeforeInit && !InternetChecker.IsAvailable())
			{
				await InternetChecker.WaitUntilAvailableAsync();
			}

			if (ValidateModulesOnStart)
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

				if (!module.IsEnabled)
				{
					SendLog.LogModule(module.ModuleName, "Module is disabled. Skipping initialization.", LogLevel.Warning);
					continue;
				}

				module.BindContext(moduleContext);
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
				else if (module.IsEnabled)
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

		public bool TryGetModule<T>(out T module) where T : class, IModule
		{
			module = default;
			if (!isInitialized)
			{
				return false;
			}

			return moduleRegistry.TryGet(out module);
		}

		public IReadOnlyList<T> GetModules<T>() where T : class, IModule
		{
			if (!isInitialized)
			{
				return Array.Empty<T>();
			}

			return moduleRegistry.GetAll<T>();
		}

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

		public void Reset()
		{
			isInitialized = false;
			failedModules.Clear();
			moduleRegistry.Clear();
			moduleContext = null;
		}
	}
}
