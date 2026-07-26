using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using THEBADDEST.Tasks;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.MonetizationApi.RemoteConfig
{
	public class FirebaseRemoteConfigService
	{
		private readonly IVariablesMapper _variablesMapper;
		private readonly float _configFetchTimeout;
		private readonly bool _enableConfigCaching;
		private readonly string _moduleName;
		private readonly Action<bool> _onSdkReady;
		private readonly Action _onDataLoad;

		private Firebase.RemoteConfig.FirebaseRemoteConfig _firebaseRemoteConfig;
		private object _cachedConfig;
		private DateTime _lastFetchTime = DateTime.MinValue;
		private bool _defaultsSetComplete;

		public FirebaseRemoteConfigService(
			IVariablesMapper variablesMapper,
			float configFetchTimeout,
			bool enableConfigCaching,
			string moduleName,
			Action<bool> onSdkReady,
			Action onDataLoad)
		{
			_variablesMapper = variablesMapper;
			_configFetchTimeout = configFetchTimeout;
			_enableConfigCaching = enableConfigCaching;
			_moduleName = moduleName;
			_onSdkReady = onSdkReady;
			_onDataLoad = onDataLoad;
		}

		public async UTask InitializeAsync()
		{
			_firebaseRemoteConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;
			if (_firebaseRemoteConfig == null || _variablesMapper == null)
			{
				_onSdkReady?.Invoke(false);
				return;
			}

			_defaultsSetComplete = false;
			_firebaseRemoteConfig.SetDefaultsAsync(_variablesMapper.GetDefaultValues()).ContinueWithOnMainThread(task =>
			{
				bool success = task.IsCompleted && !task.IsFaulted && !task.IsCanceled;
				_onSdkReady?.Invoke(success);
				_defaultsSetComplete = true;
			});

			await UTask.WaitUntil(() => _defaultsSetComplete);
		}

		public void Load()
		{
			if (_firebaseRemoteConfig == null || _variablesMapper == null)
			{
				SendLog.LogModule(_moduleName, "Cannot load: Firebase Remote Config or Variables Mapper is null.", LogLevel.Warning);
				return;
			}

			IDictionary<string, object> newDictionary = new Dictionary<string, object>();
			foreach (var pair in _variablesMapper.GetDefaultValues())
			{
				newDictionary.Add(pair.Key, _firebaseRemoteConfig.GetValue(pair.Key));
			}

			_variablesMapper.SetValues(newDictionary);
			_onDataLoad?.Invoke();
		}

		public void FetchConfig(Action<object> config)
		{
			if (_enableConfigCaching && _cachedConfig != null &&
			    (DateTime.Now - _lastFetchTime).TotalSeconds < _configFetchTimeout)
			{
				SendLog.LogModule(_moduleName, "Returning cached config.");
				config?.Invoke(_cachedConfig);
				return;
			}

			if (_firebaseRemoteConfig == null)
			{
				SendLog.LogModule(_moduleName, "Cannot fetch: Remote Config is not initialized.", LogLevel.Warning);
				return;
			}

			var fetchTask = _firebaseRemoteConfig.FetchAsync(TimeSpan.FromSeconds(_configFetchTimeout));
			fetchTask.ContinueWithOnMainThread(FetchComplete);

			void FetchComplete(System.Threading.Tasks.Task _fetchTask)
			{
				if (_fetchTask.IsCanceled)
				{
					SendLog.LogModule(_moduleName, "Fetch canceled.", LogLevel.Warning);
				}
				else if (_fetchTask.IsFaulted)
				{
					SendLog.LogModule(_moduleName, "Fetch encountered an error.", LogLevel.Error);
				}
				else if (_fetchTask.IsCompleted)
				{
					SendLog.LogModule(_moduleName, "Fetch completed successfully!");
				}

				var info = _firebaseRemoteConfig.Info;
				switch (info.LastFetchStatus)
				{
					case LastFetchStatus.Success:
						_firebaseRemoteConfig.ActivateAsync().ContinueWithOnMainThread(_ =>
						{
							SendLog.LogModule(_moduleName, $"Remote data loaded and ready (last fetch time {info.FetchTime}).");
							Load();
							_cachedConfig = _variablesMapper.GetDefaultValues();
							_lastFetchTime = DateTime.Now;
							config?.Invoke(_cachedConfig);
						});
						break;
					case LastFetchStatus.Failure:
						switch (info.LastFetchFailureReason)
						{
							case FetchFailureReason.Error:
								SendLog.LogModule(_moduleName, "Fetch failed for unknown reason", LogLevel.Error);
								break;
							case FetchFailureReason.Throttled:
								SendLog.LogModule(_moduleName, "Fetch throttled until " + info.ThrottledEndTime, LogLevel.Warning);
								break;
						}
						break;
					case LastFetchStatus.Pending:
						SendLog.LogModule(_moduleName, "Latest Fetch call still pending.", LogLevel.Warning);
						break;
				}
			}
		}
	}
}
