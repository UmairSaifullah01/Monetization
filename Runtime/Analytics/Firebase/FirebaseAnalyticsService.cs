using System;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using THEBADDEST.Tasks;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.MonetizationApi.Analytics
{
	public class FirebaseAnalyticsService
	{
		private readonly string _moduleName;
		private readonly Action<bool> _onSdkReady;
		private bool _initComplete;

		public bool IsReady { get; private set; }

		public FirebaseAnalyticsService(string moduleName, Action<bool> onSdkReady)
		{
			_moduleName = moduleName;
			_onSdkReady = onSdkReady;
		}

		public async UTask InitializeAsync()
		{
			_initComplete = false;
			FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
			{
				if (task.IsFaulted || task.IsCanceled)
				{
					IsReady = false;
					_onSdkReady?.Invoke(false);
					_initComplete = true;
					return;
				}

				var status = task.Result;
				if (status == DependencyStatus.Available)
				{
					FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
					IsReady = true;
					_onSdkReady?.Invoke(true);
					SendLog.LogModule(_moduleName, "Firebase Analytics initialized.");
				}
				else
				{
					IsReady = false;
					_onSdkReady?.Invoke(false);
					SendLog.LogModule(_moduleName, $"Firebase dependencies unavailable: {status}", LogLevel.Error);
				}

				_initComplete = true;
			});

			await UTask.WaitUntil(() => _initComplete);
		}

		public void LogEvent(string name)
		{
			if (!IsReady) return;
			FirebaseAnalytics.LogEvent(name);
		}

		public void LogEvent(string name, string parameterName, string parameterValue)
		{
			if (!IsReady) return;
			FirebaseAnalytics.LogEvent(name, parameterName, parameterValue);
		}

		public void LogEvent(string name, string parameterName, double parameterValue)
		{
			if (!IsReady) return;
			FirebaseAnalytics.LogEvent(name, parameterName, parameterValue);
		}

		public void LogEvent(string name, Parameter[] parameters)
		{
			if (!IsReady) return;
			FirebaseAnalytics.LogEvent(name, parameters);
		}
	}
}
