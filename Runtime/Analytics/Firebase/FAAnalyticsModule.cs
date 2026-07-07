using System.Collections.Generic;
using Firebase.Analytics;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;

namespace THEBADDEST.Analytics
{
	public class FAAnalyticsModule : AnalyticsModule
	{
		private FirebaseAnalyticsService _service;
		private bool _firebaseReady;

		protected override async UTask OnInitialize()
		{
			await base.OnInitialize();

			var configAsset = MonetizationConfig.Instance;
			if (!configAsset.EnableAnalytics)
			{
				SendLog.LogModule(ModuleName, "Analytics is disabled by MonetizationConfig.", LogLevel.Warning);
				return;
			}

			_service = new FirebaseAnalyticsService(ModuleName, success =>
			{
				_firebaseReady = success;
				RaiseSdkInitialized(success);
			});
			await _service.InitializeAsync();
		}

		protected override void OnAdShown(AdShownEvent evt)
		{
			if (!_firebaseReady) return;
			SendAdEvent(evt.AdType, evt.Placement, true);
		}

		public override void SendEvent(string name)
		{
			if (!EnsureReady()) return;
			_service.LogEvent(name);
			SendLog.LogModule(ModuleName, $"Event sent: {name}");
		}

		public override void SendEvent(string name, string value)
		{
			if (!EnsureReady()) return;
			if (float.TryParse(value, out float numericValue))
			{
				_service.LogEvent(name, "value", numericValue);
			}
			else
			{
				_service.LogEvent(name, "value", value);
			}

			SendLog.LogModule(ModuleName, $"Event sent: {name} = {value}");
		}

		public override void SendEvent(ProgressionStatus status, string eventName)
		{
			if (!EnsureReady()) return;
			_service.LogEvent("progression", new[]
			{
				new Parameter("status", status.ToString()),
				new Parameter("event_name", eventName)
			});
			SendLog.LogModule(ModuleName, $"Progression event sent: {status} - {eventName}");
		}

		public override void SendDesignEvent(string category, string subCategory, string outcome, float value)
		{
			if (!EnsureReady()) return;
			_service.LogEvent($"{category}_{subCategory}_{outcome}", "value", value);
			SendLog.LogModule(ModuleName, $"Design event sent: {category}:{subCategory}:{outcome} = {value}");
		}

		public override void SendDesignEvent(string category, string subCategory, string outcome)
		{
			if (!EnsureReady()) return;
			_service.LogEvent($"{category}_{subCategory}_{outcome}");
			SendLog.LogModule(ModuleName, $"Design event sent: {category}:{subCategory}:{outcome}");
		}

		public override void SendTransaction(string productId, string currencyCode, int quantity, double unitPrice, string receipt, string signature)
		{
			if (!EnsureReady()) return;
			_service.LogEvent(FirebaseAnalytics.EventPurchase, new[]
			{
				new Parameter(FirebaseAnalytics.ParameterItemId, productId),
				new Parameter(FirebaseAnalytics.ParameterCurrency, currencyCode),
				new Parameter(FirebaseAnalytics.ParameterQuantity, quantity),
				new Parameter(FirebaseAnalytics.ParameterValue, unitPrice)
			});
			SendLog.LogModule(ModuleName, $"Transaction sent: {productId}");
		}

		public override void SendEventLog(Dictionary<string, object> eventLog)
		{
			if (!EnsureReady()) return;
			foreach (var kvp in eventLog)
			{
				if (kvp.Value is float floatValue)
				{
					_service.LogEvent(kvp.Key, "value", floatValue);
				}
				else if (kvp.Value is int intValue)
				{
					_service.LogEvent(kvp.Key, "value", intValue);
				}
				else if (kvp.Value is double doubleValue)
				{
					_service.LogEvent(kvp.Key, "value", doubleValue);
				}
				else
				{
					_service.LogEvent(kvp.Key, "value", kvp.Value?.ToString() ?? string.Empty);
				}
			}

			SendLog.LogModule(ModuleName, $"Event log sent with {eventLog.Count} events");
		}

		public void SendAdEvent(string adType, string placement, bool success)
		{
			string outcome = success ? "success" : "failed";
			SendDesignEvent("Ad", adType, outcome);
		}

		private bool EnsureReady()
		{
			var configAsset = MonetizationConfig.Instance;
			if (!configAsset.EnableAnalytics)
			{
				SendLog.LogModule(ModuleName, "Analytics is disabled by MonetizationConfig.", LogLevel.Warning);
				return false;
			}

			if (!_firebaseReady || _service == null || !_service.IsReady)
			{
				SendLog.LogModule(ModuleName, "Cannot send event: Firebase Analytics not initialized.", LogLevel.Warning);
				return false;
			}

			return true;
		}
	}
}
