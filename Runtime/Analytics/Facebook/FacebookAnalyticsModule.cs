using System.Collections.Generic;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Analytics
{
	public class FacebookAnalyticsModule : AnalyticsModule, IFacebookAnalyticsModule
	{
		private const string FACEBOOK_KEYS_CATEGORY = "FacebookKeys";
		private const string FACEBOOK_APP_ID = "AppId";
		private const string FACEBOOK_CLIENT_TOKEN = "ClientToken";

		[Tooltip("Shows all available Facebook keys from JSON.")]
		[JsonDataCategory(FACEBOOK_KEYS_CATEGORY)]
		[SerializeField] private string facebookKeysReference = "";

		private FacebookAnalyticsService _service;
		private bool _facebookReady;

		protected override async UTask OnInitialize()
		{
			await base.OnInitialize();

			var catalog = Context?.Catalog ?? CatalogFactory.Create();
			var appId = catalog.Resolve(FACEBOOK_KEYS_CATEGORY, FACEBOOK_APP_ID);
			var clientToken = catalog.Resolve(FACEBOOK_KEYS_CATEGORY, FACEBOOK_CLIENT_TOKEN);

			_service = new FacebookAnalyticsService(ModuleName);
			await _service.InitializeAsync(appId, clientToken);
			_facebookReady = _service.IsReady;
			RaiseSdkInitialized(_facebookReady);
		}

		protected override void OnUpdateModule()
		{
			base.OnUpdateModule();

			var catalog = Context?.Catalog ?? CatalogFactory.Create();
			var appId = catalog.Resolve(FACEBOOK_KEYS_CATEGORY, FACEBOOK_APP_ID);
			var clientToken = catalog.Resolve(FACEBOOK_KEYS_CATEGORY, FACEBOOK_CLIENT_TOKEN);

#if UNITY_EDITOR
			FacebookSettingsSync.ApplyKeys(appId, clientToken, ModuleName);
#else
			if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(clientToken))
			{
				SendLog.LogModule(ModuleName, "Facebook AppId/ClientToken not found in MonetizationKeys.json.", LogLevel.Warning);
			}
#endif
		}

		public override void SendEvent(string name)
		{
			if (!EnsureReady()) return;
			_service.LogEvent(name);
		}

		public override void SendEvent(string name, string value)
		{
			if (!EnsureReady()) return;
			if (float.TryParse(value, out float numericValue))
			{
				_service.LogEvent(name, numericValue);
				return;
			}

			_service.LogEvent(name, new Dictionary<string, object> { { "value", value } });
		}

		public override void SendEvent(ProgressionStatus status, string eventName)
		{
			if (!EnsureReady()) return;
			_service.LogEvent("progression_event", new Dictionary<string, object>
			{
				{ "status", status.ToString() },
				{ "event_name", eventName }
			});
		}

		public override void SendDesignEvent(string category, string subCategory, string outcome, float value)
		{
			if (!EnsureReady()) return;
			var eventName = $"{category}_{subCategory}_{outcome}";
			_service.LogEvent(eventName, value);
		}

		public override void SendDesignEvent(string category, string subCategory, string outcome)
		{
			if (!EnsureReady()) return;
			var eventName = $"{category}_{subCategory}_{outcome}";
			_service.LogEvent(eventName);
		}

		public override void SendTransaction(string productId, string currencyCode, int quantity, double unitPrice, string receipt, string signature)
		{
			if (!EnsureReady()) return;

			LogPurchase((float)(unitPrice * quantity), currencyCode, new Dictionary<string, object>
			{
				{ "product_id", productId },
				{ "quantity", quantity },
				{ "receipt", receipt ?? string.Empty },
				{ "signature", signature ?? string.Empty }
			});
		}

		public override void SendEventLog(Dictionary<string, object> eventLog)
		{
			if (!EnsureReady()) return;
			foreach (var kvp in eventLog)
			{
				_service.LogEvent(kvp.Key, new Dictionary<string, object> { { "value", kvp.Value?.ToString() ?? string.Empty } });
			}
		}

		public void LogPurchase(float amount, string currency, Dictionary<string, object> parameters = null)
		{
			if (!EnsureReady()) return;
			_service.LogPurchase(amount, currency, parameters);
		}

		private bool EnsureReady()
		{
			_facebookReady = _service != null && _service.IsReady;
			if (!_facebookReady)
			{
				SendLog.LogModule(ModuleName, "Facebook Analytics is not initialized yet.", LogLevel.Warning);
				return false;
			}

			return true;
		}
	}
}
