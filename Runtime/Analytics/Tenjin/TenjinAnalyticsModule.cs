using System.Collections.Generic;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Analytics
{
	public class TenjinAnalyticsModule : AnalyticsModule, ITenjinAnalyticsModule
	{
		private const string TENJIN_KEYS_CATEGORY = "TenjinKeys";
		private const string TENJIN_ANDROID_API_KEY = "AndroidApiKey";
		private const string TENJIN_IOS_API_KEY = "IosApiKey";

		[Tooltip("Shows all available Tenjin keys from JSON.")]
		[JsonDataCategory(TENJIN_KEYS_CATEGORY)]
		[SerializeField] private string tenjinKeysReference = "";

		private TenjinAnalyticsService _service;

		protected override async UTask OnInitialize()
		{
			await base.OnInitialize();

			var apiKey = ResolveApiKey();

			_service = new TenjinAnalyticsService(ModuleName);
			_service.Initialize(apiKey);
			RaiseSdkInitialized(_service.IsReady);
		}

		public override void SendEvent(string name)
		{
			if (!EnsureReady()) return;
			_service.SendEvent(name);
		}

		public override void SendEvent(string name, string value)
		{
			if (!EnsureReady()) return;
			_service.SendEvent($"{name}:{value}");
		}

		public override void SendEvent(ProgressionStatus status, string eventName)
		{
			if (!EnsureReady()) return;
			_service.SendEvent($"progression:{status}:{eventName}");
		}

		public override void SendDesignEvent(string category, string subCategory, string outcome, float value)
		{
			if (!EnsureReady()) return;
			_service.SendEvent($"{category}:{subCategory}:{outcome}:{value}");
		}

		public override void SendDesignEvent(string category, string subCategory, string outcome)
		{
			if (!EnsureReady()) return;
			_service.SendEvent($"{category}:{subCategory}:{outcome}");
		}

		public override void SendTransaction(string productId, string currencyCode, int quantity, double unitPrice, string receipt, string signature)
		{
			if (!EnsureReady()) return;
			_service.SendPurchase(productId, currencyCode, quantity, unitPrice, receipt, signature);
		}

		public override void SendEventLog(Dictionary<string, object> eventLog)
		{
			if (!EnsureReady()) return;
			foreach (var kvp in eventLog)
			{
				_service.SendEvent($"{kvp.Key}:{kvp.Value}");
			}
		}

		public void SendAdImpression(string adNetwork, double revenueUsd, string placement)
		{
			if (!EnsureReady()) return;
			_service.SendAdImpression(adNetwork, revenueUsd, placement);
		}

		public void SendPurchase(string productId, string currency, int quantity, double unitPrice)
		{
			if (!EnsureReady()) return;
			_service.SendPurchase(productId, currency, quantity, unitPrice, string.Empty, string.Empty);
		}

		private string ResolveApiKey()
		{
			var catalog = Context?.Catalog ?? CatalogFactory.Create();
#if UNITY_ANDROID
			return catalog.Resolve(TENJIN_KEYS_CATEGORY, TENJIN_ANDROID_API_KEY);
#elif UNITY_IOS
			return catalog.Resolve(TENJIN_KEYS_CATEGORY, TENJIN_IOS_API_KEY);
#else
			var androidKey = catalog.Resolve(TENJIN_KEYS_CATEGORY, TENJIN_ANDROID_API_KEY);
			if (!string.IsNullOrEmpty(androidKey))
			{
				return androidKey;
			}

			return catalog.Resolve(TENJIN_KEYS_CATEGORY, TENJIN_IOS_API_KEY);
#endif
		}

		private bool EnsureReady()
		{
			if (_service == null || !_service.IsReady)
			{
				SendLog.LogModule(ModuleName, "Tenjin Analytics is not initialized.", LogLevel.Warning);
				return false;
			}

			return true;
		}
	}
}
