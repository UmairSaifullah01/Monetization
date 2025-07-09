using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using THEBADDEST.Tasks;
using UnityEngine;


namespace THEBADDEST.RemoteConfigSystem
{


	public class FireBaseRemoteConfigModuleModule : RemoteConfigModule
	{

		[SerializeField] RemoteVariablesMapper m_VariablesMapper;
		public override IVariablesMapper variablesMapper => m_VariablesMapper;

		FirebaseRemoteConfig firebaseRemoteConfig;


		public override async UTask Initialize()
		{
			await base.Initialize();
			firebaseRemoteConfig = FirebaseRemoteConfig.DefaultInstance;
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
			if (!IsInitialized) return;
			var fetchTask = firebaseRemoteConfig.FetchAsync(TimeSpan.Zero);
			fetchTask.ContinueWithOnMainThread(FetchComplete);

			void FetchComplete(Task _fetchTask)
			{
				if (_fetchTask.IsCanceled)
				{
					Debug.Log("Fetch canceled.");
				}
				else if (_fetchTask.IsFaulted)
				{
					Debug.Log("Fetch encountered an error.");
				}
				else if (_fetchTask.IsCompleted)
				{
					Debug.Log("Fetch completed successfully!");
				}

				var info = firebaseRemoteConfig.Info;
				switch (info.LastFetchStatus)
				{
					case LastFetchStatus.Success:
						firebaseRemoteConfig.ActivateAsync().ContinueWithOnMainThread(task =>
						{
							Debug.Log($"Remote data loaded and ready (last fetch time {info.FetchTime}).");
							// Load Data
							Load();
							config?.Invoke(variablesMapper.GetDefaultValues());
						});
						break;

					case LastFetchStatus.Failure:
						switch (info.LastFetchFailureReason)
						{
							case FetchFailureReason.Error:
								Debug.Log("Fetch failed for unknown reason");
								break;

							case FetchFailureReason.Throttled:
								Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
								break;
						}

						break;

					case LastFetchStatus.Pending:
						Debug.Log("Latest Fetch call still pending.");
						break;
				}
			}
		}

	}


}