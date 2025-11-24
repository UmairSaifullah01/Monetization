using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using THEBADDEST.Tasks;
using UnityEngine;
using THEBADDEST.MonetizationApi;


namespace THEBADDEST.RemoteConfigSystem
{


	public class FirebaseRemoteConfig : RemoteConfigModule
	{

		[SerializeField] RemoteVariablesMapper m_VariablesMapper;
		public override IVariablesMapper variablesMapper => m_VariablesMapper;

		Firebase.RemoteConfig.FirebaseRemoteConfig firebaseRemoteConfig;
		private object cachedConfig = null;
		private DateTime lastFetchTime = DateTime.MinValue;


		public override async UTask Initialize()
		{
			var configAsset = THEBADDEST.MonetizationApi.MonetizationConfig.Instance;
			if (!configAsset.EnableRemoteConfig)
			{
				SendLog.LogWarning("[RemoteConfig] Remote Config is disabled by MonetizationConfig.");
				IsInitialized = false;
				return;
			}
			// Use configAsset.ConfigFetchTimeout and configAsset.EnableConfigCaching for fetch logic
			await base.Initialize();
			firebaseRemoteConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;
			if (firebaseRemoteConfig == null)
			{
				OnInitializeCompleted(IsInitialized);
				return;
			}

			firebaseRemoteConfig.SetDefaultsAsync(variablesMapper.GetDefaultValues()).ContinueWithOnMainThread(task =>
			{
				IsInitialized = true;
				OnInitializeCompleted(IsInitialized);
			});
		}

		public override void Load()
		{
			if (firebaseRemoteConfig == null || variablesMapper == null)
			{
				SendLog.LogWarning("[RemoteConfig] Cannot load: Firebase Remote Config or Variables Mapper is null.");
				return;
			}

			IDictionary<string, object> newDictionary = new Dictionary<string, object>();
			foreach (var pair in variablesMapper.GetDefaultValues())
			{
				newDictionary.Add(pair.Key, firebaseRemoteConfig.GetValue(pair.Key));
			}

			variablesMapper.SetValues(newDictionary);
			OnDataLoadCompleted();
		}

		public override void FetchConfig(Action<object> config)
		{
			var configAsset = THEBADDEST.MonetizationApi.MonetizationConfig.Instance;
			if (configAsset.EnableConfigCaching && cachedConfig != null && (DateTime.Now - lastFetchTime).TotalSeconds < configAsset.ConfigFetchTimeout)
			{
				SendLog.LogInfo("[RemoteConfig] Returning cached config.");
				config?.Invoke(cachedConfig);
				return;
			}

			if (!IsInitialized || firebaseRemoteConfig == null)
			{
				SendLog.LogWarning("[RemoteConfig] Cannot fetch: Remote Config is not initialized.");
				return;
			}
			var fetchTask = firebaseRemoteConfig.FetchAsync(TimeSpan.FromSeconds(configAsset.ConfigFetchTimeout));
			fetchTask.ContinueWithOnMainThread(FetchComplete);

			async void FetchComplete(System.Threading.Tasks.Task _fetchTask)
			{
				if (_fetchTask.IsCanceled)
				{
					SendLog.LogWarning("Fetch canceled.");
				}
				else if (_fetchTask.IsFaulted)
				{
					SendLog.LogError("Fetch encountered an error.");
				}
				else if (_fetchTask.IsCompleted)
				{
					SendLog.Log("Fetch completed successfully!");
				}

				var info = firebaseRemoteConfig.Info;
				switch (info.LastFetchStatus)
				{
					case LastFetchStatus.Success:
						firebaseRemoteConfig.ActivateAsync().ContinueWithOnMainThread(task =>
						{
							SendLog.Log($"Remote data loaded and ready (last fetch time {info.FetchTime}).");
							// Load Data
							Load();
							cachedConfig = variablesMapper.GetDefaultValues();
							lastFetchTime = DateTime.Now;
							config?.Invoke(cachedConfig);
						});
						break;

					case LastFetchStatus.Failure:
						switch (info.LastFetchFailureReason)
						{
							case FetchFailureReason.Error:
								SendLog.LogError("Fetch failed for unknown reason");
								break;

							case FetchFailureReason.Throttled:
								SendLog.LogWarning("Fetch throttled until " + info.ThrottledEndTime);
								break;
						}

						break;

					case LastFetchStatus.Pending:
						SendLog.LogWarning("Latest Fetch call still pending.");
						break;
				}
			}
		}

	}


}