using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;


namespace THEBADDEST.Analytics
{


	public class GAAnalyticsModule : AnalyticsModule
	{

		private const string ANALYTICS_KEYS_CATEGORY = "GameAnalyticsKeys";

		// Fixed key names for JSON lookup
		private static readonly string GAME_ANALYTICS_GAME_KEY = "GameKey";
		private static readonly string GAME_ANALYTICS_SECRET_KEY = "SecretKey";

		[Tooltip("Shows all available Analytics IDs from JSON.")]
		[JsonDataCategory(ANALYTICS_KEYS_CATEGORY)]
		[SerializeField] private string analyticsKeysReference = "";


		// Cached loaded IDs
		private string _cachedGameKey;
		private string _cachedSecretKey;
		private bool _gameAnalyticsReady;

		private void LoadAnalyticsIdsFromJson()
		{
			// Ensure JsonDataUtility is loaded
			JsonDataUtility.LoadData();

			// Load Game Analytics Game Key
			_cachedGameKey = JsonDataUtility.GetData(ANALYTICS_KEYS_CATEGORY, GAME_ANALYTICS_GAME_KEY);

			// Load Game Analytics Secret Key
			_cachedSecretKey = JsonDataUtility.GetData(ANALYTICS_KEYS_CATEGORY, GAME_ANALYTICS_SECRET_KEY);
		}

		public override void UpdateModule()
		{
			// Load IDs from JSON
			LoadAnalyticsIdsFromJson();

			// Set keys if loaded successfully
			if (!string.IsNullOrEmpty(_cachedGameKey) && !string.IsNullOrEmpty(_cachedSecretKey))
			{
				GameAnalytics.SettingsGA.SetKeys(_cachedGameKey, _cachedSecretKey);
				SendLog.LogModule(ModuleName, "Game Analytics keys set successfully.");
			}
			else
			{
				SendLog.LogModule(ModuleName, "Game Analytics keys not found in JSON. Configure keys in MonetizationKeys.json under GameAnalyticsKeys.", LogLevel.Warning);
			}
		}

		protected override async UTask OnInitialize()
		{
			await base.OnInitialize();

			var configAsset = MonetizationConfig.Instance;
			if (!configAsset.EnableAnalytics)
			{
				SendLog.LogModule(ModuleName, "Analytics is disabled by MonetizationConfig.", LogLevel.Warning);
				return;
			}

			if (string.IsNullOrEmpty(_cachedGameKey) || string.IsNullOrEmpty(_cachedSecretKey))
			{
				LoadAnalyticsIdsFromJson();
			}

			GameAnalytics.Initialize();
			_gameAnalyticsReady = true;
			SendLog.LogModule(ModuleName, "Game Analytics initialized successfully.");
		}

		protected override void OnAdShown(AdShownEvent evt)
		{
			if (!_gameAnalyticsReady) return;
			SendAdEvent(evt.AdType, evt.Placement, true);
		}

		public override void SendEvent(string name)
		{
			var configAsset = MonetizationConfig.Instance;
			if (!configAsset.EnableAnalytics)
			{
				SendLog.LogModule(ModuleName, "Analytics is disabled by MonetizationConfig. Event not sent.", LogLevel.Warning);
				return;
			}

			if (!_gameAnalyticsReady)
			{
				SendLog.LogModule(ModuleName, "Cannot send event: Game Analytics not initialized.", LogLevel.Warning);
				return;
			}

			GameAnalytics.NewDesignEvent(name);
			SendLog.LogModule(ModuleName, $"Event sent: {name}");
		}

		public override void SendEvent(string name, string value)
		{
			if (!_gameAnalyticsReady)
			{
				SendLog.LogModule(ModuleName, "Cannot send event: Game Analytics not initialized.", LogLevel.Warning);
				return;
			}

			float customFields;
			if (float.TryParse(value, out customFields))
			{
				GameAnalytics.NewDesignEvent(name, customFields);
				SendLog.LogModule(ModuleName, $"Event sent: {name} with value: {customFields}");
			}
			else
			{
				SendLog.LogModule(ModuleName, $"Cannot convert value to float: {value}", LogLevel.Warning);
			}
		}

		public override void SendEvent(ProgressionStatus status, string eventName)
		{
			if (!_gameAnalyticsReady)
			{
				SendLog.LogModule(ModuleName, "Cannot send progression event: Game Analytics not initialized.", LogLevel.Warning);
				return;
			}

			GAProgressionStatus gaStatus = ConvertToGAProgressionStatus(status);
			GameAnalytics.NewProgressionEvent(gaStatus, eventName);
			SendLog.LogModule(ModuleName, $"Progression event sent: {status} - {eventName}");
		}

		public override void SendDesignEvent(string category, string subCategory, string outcome, float value)
		{
			if (!_gameAnalyticsReady)
			{
				SendLog.LogModule(ModuleName, "Cannot send design event: Game Analytics not initialized.", LogLevel.Warning);
				return;
			}

			string eventName = $"{category}:{subCategory}:{outcome}";
			GameAnalytics.NewDesignEvent(eventName, value);
			SendLog.LogModule(ModuleName, $"Design event sent: {eventName} with value: {value}");
		}

		public override void SendDesignEvent(string category, string subCategory, string outcome)
		{
			if (!_gameAnalyticsReady)
			{
				SendLog.LogModule(ModuleName, "Cannot send design event: Game Analytics not initialized.", LogLevel.Warning);
				return;
			}

			string eventName = $"{category}:{subCategory}:{outcome}";
			GameAnalytics.NewDesignEvent(eventName);
			SendLog.LogModule(ModuleName, $"Design event sent: {eventName}");
		}

		public override void SendTransaction(string productId, string currencyCode, int quantity, double unitPrice, string receipt, string signature)
		{
			if (!_gameAnalyticsReady)
			{
				SendLog.LogModule(ModuleName, "Cannot send transaction: Game Analytics not initialized.", LogLevel.Warning);
				return;
			}

			int amount = (int)(unitPrice * quantity);
			GameAnalytics.NewBusinessEvent(currencyCode, amount, productId, quantity.ToString(), receipt, null);
			SendLog.LogModule(ModuleName, $"Transaction sent: {productId} - {amount} {currencyCode}");
		}

		public override void SendEventLog(Dictionary<string, object> eventLog)
		{
			if (!_gameAnalyticsReady)
			{
				SendLog.LogModule(ModuleName, "Cannot send event log: Game Analytics not initialized.", LogLevel.Warning);
				return;
			}

			foreach (var kvp in eventLog)
			{
				string eventName = kvp.Key;
				object value = kvp.Value;

				if (value is float floatValue)
				{
					GameAnalytics.NewDesignEvent(eventName, floatValue);
				}
				else if (value is int intValue)
				{
					GameAnalytics.NewDesignEvent(eventName, (float)intValue);
				}
				else if (value is double doubleValue)
				{
					GameAnalytics.NewDesignEvent(eventName, (float)doubleValue);
				}
			}

			SendLog.LogModule(ModuleName, $"Event log sent with {eventLog.Count} events");
		}

		private GAProgressionStatus ConvertToGAProgressionStatus(ProgressionStatus status)
		{
			switch (status)
			{
				case ProgressionStatus.Start:
					return GAProgressionStatus.Start;

				case ProgressionStatus.Complete:
					return GAProgressionStatus.Complete;

				case ProgressionStatus.Fail:
					return GAProgressionStatus.Fail;

				case ProgressionStatus.Undefined:
				default:
					return GAProgressionStatus.Undefined;
			}
		}

		// Additional helper methods for common analytics patterns
		public void SendLevelStart(int level)
		{
			SendEvent(ProgressionStatus.Start, $"Level_{level}");
		}

		public void SendLevelComplete(int level)
		{
			SendEvent(ProgressionStatus.Complete, $"Level_{level}");
		}

		public void SendLevelFail(int level)
		{
			SendEvent(ProgressionStatus.Fail, $"Level_{level}");
		}

		public void SendAdEvent(string adType, string placement, bool success)
		{
			string outcome = success ? "success" : "failed";
			SendDesignEvent("Ad", adType, outcome);
		}

		public void SendPurchaseEvent(string productId, float amount, string currency)
		{
			SendDesignEvent("Purchase", productId, "completed", amount);
		}

		public void SetUserProperty(string propertyName, string propertyValue)
		{
			if (!_gameAnalyticsReady)
			{
				SendLog.LogModule(ModuleName, "Cannot set user property: Game Analytics not initialized.", LogLevel.Warning);
				return;
			}

			GameAnalytics.SetCustomDimension01(propertyName);
			SendLog.LogModule(ModuleName, $"User property set: {propertyName} = {propertyValue}");
		}

	}


}