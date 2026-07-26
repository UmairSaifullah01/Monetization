using System;
using System.Collections.Generic;
using THEBADDEST.MonetizationApi.Ads;
using THEBADDEST.Tasks;
using UnityEngine;

namespace THEBADDEST.MonetizationApi
{
	public static class Monetization
	{
		public static event Action<bool> OnInitialize;
		public static event Action<string> OnError;

		private static MonetizationProfile profile;
		private static IModuleContext context;
		private static bool isInitializing = false;
		private static bool isInitialized = false;

		public static bool IsInitialized => isInitialized;
		public static bool IsInitializing => isInitializing;
		public static IModuleContext Context => context;

		public static IReadOnlyList<string> FailedModules =>
			profile != null ? profile.FailedModules : Array.Empty<string>();

		public static T GetModule<T>() where T : class, IModule
		{
			if (!isInitialized)
			{
				SendLog.LogError("Monetization system not initialized. Call Initialize() first.");
				return default;
			}

			if (profile == null)
			{
				SendLog.LogError("MonetizationProfile is null. Initialization may have failed.");
				return default;
			}

			return profile.GetModule<T>();
		}

		public static bool TryGetModule<T>(out T module) where T : class, IModule
		{
			module = default;
			if (!isInitialized)
			{
				SendLog.LogWarning("Monetization system not initialized. Call Initialize() first.");
				return false;
			}

			if (profile == null)
			{
				SendLog.LogWarning("MonetizationProfile is null. Initialization may have failed.");
				return false;
			}

			return profile.TryGetModule(out module);
		}

		public static IReadOnlyList<T> GetModules<T>() where T : class, IModule
		{
			if (!isInitialized)
			{
				SendLog.LogWarning("Monetization system not initialized. Call Initialize() first.");
				return Array.Empty<T>();
			}

			if (profile == null)
			{
				SendLog.LogWarning("MonetizationProfile is null. Initialization may have failed.");
				return Array.Empty<T>();
			}

			return profile.GetModules<T>();
		}

		public static async UTask Initialize(int retryAttempts = 0)
		{
			if (isInitialized)
			{
				SendLog.Log("Monetization already initialized.");
				return;
			}

			if (isInitializing)
			{
				SendLog.LogWarning("Monetization initialization already in progress.");
				await UTask.WaitUntil(() => !isInitializing);
				return;
			}

			isInitializing = true;

			MonetizationProfile settingsForRetry = null;

			try
			{
				var profileObject = Resources.Load<MonetizationProfile>("MonetizationProfile");
				if (profileObject == null)
				{
					throw new InvalidOperationException("MonetizationProfile object is missing in Resources folder.");
				}

				profile = profileObject;
				settingsForRetry = profile;
				profile.ApplySendLogConfiguration();

				var catalog = CatalogFactory.Create();
				var metrics = new AdMetrics(profile);
				PerformanceMonitor.AdMetrics = metrics;
				context = new ModuleContext(profile, catalog, metrics);

				await profile.Initialize(context);

				if (profile.FailedModules.Count > 0)
				{
					string summary = string.Join("; ", profile.FailedModules);
					SendLog.LogBatch(new List<string> { $"Monetization module failures: {summary}" }, LogLevel.Warning);
					OnError?.Invoke(summary);
				}

				isInitialized = true;
				isInitializing = false;
				OnInitialize?.Invoke(true);
				OnInitialize = null;

				SendLog.Log("Monetization system initialized successfully.");
			}
			catch (Exception ex)
			{
				isInitializing = false;
				string errorMessage = $"Monetization initialization failed: {ex.Message}";
				SendLog.LogError(errorMessage);
				OnError?.Invoke(errorMessage);

				int maxRetries = settingsForRetry != null ? settingsForRetry.MaxRetryAttempts : 3;
				float retryDelay = settingsForRetry != null ? settingsForRetry.RetryDelaySeconds : 2f;

				if (retryAttempts < maxRetries)
				{
					SendLog.LogWarning($"Retrying initialization in {retryDelay} seconds... (Attempt {retryAttempts + 1}/{maxRetries})");
					await UTask.Delay(retryDelay);
					await Initialize(retryAttempts + 1);
				}
				else
				{
					OnInitialize?.Invoke(false);
					OnInitialize = null;
					throw;
				}
			}
		}

		public static void Reset()
		{
			isInitialized = false;
			isInitializing = false;
			profile = null;
			context = null;
			PerformanceMonitor.AdMetrics = NullAdMetrics.Instance;
			OnInitialize = null;
			OnError = null;
		}
	}
}
