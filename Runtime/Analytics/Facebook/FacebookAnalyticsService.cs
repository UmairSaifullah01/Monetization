using System.Collections.Generic;
using Facebook.Unity;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;

namespace THEBADDEST.Analytics
{
	public class FacebookAnalyticsService
	{
		private readonly string _moduleName;
		private bool _isInitialized;
		private bool _initComplete;

		public bool IsReady => _isInitialized && FB.IsInitialized;

		public FacebookAnalyticsService(string moduleName)
		{
			_moduleName = moduleName;
		}

		public async UTask InitializeAsync(string appId, string clientToken)
		{
			if (_isInitialized && FB.IsInitialized)
			{
				return;
			}

			_initComplete = false;
			FB.Init(() =>
			{
				if (!FB.IsInitialized)
				{
					_isInitialized = false;
					_initComplete = true;
					SendLog.LogModule(_moduleName, "Facebook SDK failed to initialize.", LogLevel.Error);
					return;
				}

				if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(clientToken))
				{
					SendLog.LogModule(_moduleName, "Facebook AppId/ClientToken not found in MonetizationKeys.json. Using SDK editor settings.", LogLevel.Warning);
				}

				FB.ActivateApp();
				_isInitialized = true;
				_initComplete = true;
				SendLog.LogModule(_moduleName, "Facebook Analytics initialized.");
			});

			await UTask.WaitUntil(() => _initComplete);
		}

		public void LogEvent(string name)
		{
			if (!IsReady) return;
			FB.LogAppEvent(name);
		}

		public void LogEvent(string name, float valueToSum)
		{
			if (!IsReady) return;
			FB.LogAppEvent(name, valueToSum);
		}

		public void LogEvent(string name, Dictionary<string, object> parameters)
		{
			if (!IsReady) return;
			FB.LogAppEvent(name, null, parameters);
		}

		public void LogPurchase(float amount, string currency, Dictionary<string, object> parameters = null)
		{
			if (!IsReady) return;
			FB.LogPurchase(amount, currency, parameters);
		}
	}
}
