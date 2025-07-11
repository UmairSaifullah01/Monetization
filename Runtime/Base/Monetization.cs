using System;
using THEBADDEST.Tasks;
using UnityEngine;


namespace THEBADDEST.MonetizationApi
{


	public static class Monetization
	{

		public static event Action<bool> OnInitialize;
		public static event Action<string> OnError;

		private static MonetizationProfile profile;
		private static bool isInitializing = false;
		private static bool isInitialized = false;
		private static int maxRetryAttempts = 3;
		private static float retryDelaySeconds = 2f;

		public static bool IsInitialized => isInitialized;
		public static bool IsInitializing => isInitializing;

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

			try
			{
				var profileObject = Resources.Load<MonetizationProfile>("MonetizationProfile");
				if (profileObject == null)
				{
					throw new InvalidOperationException("MonetizationProfile object is missing in Resources folder.");
				}

				profile = profileObject;
				await profile.Initialize();

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

				// Retry logic
				if (retryAttempts < maxRetryAttempts)
				{
					SendLog.LogWarning($"Retrying initialization in {retryDelaySeconds} seconds... (Attempt {retryAttempts + 1}/{maxRetryAttempts})");
					await UTask.Delay((retryDelaySeconds));
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
			OnInitialize = null;
			OnError = null;
		}
	}


}