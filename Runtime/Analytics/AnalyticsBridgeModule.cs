using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;


namespace THEBADDEST.Analytics
{


	public class AnalyticsBridgeModule : AnalyticsModule
	{
		[SerializeField] string gameAnalyticsKey;
		[SerializeField] string gameAnalyticsSecret;
		
		private bool isInitialized = false;

		internal override void UpdateModule()
		{
			if (!string.IsNullOrEmpty(gameAnalyticsKey) && !string.IsNullOrEmpty(gameAnalyticsSecret))
			{
				GameAnalytics.SettingsGA.SetKeys(gameAnalyticsKey, gameAnalyticsSecret);
			}
		}

		public override async UTask Initialize()
		{
			// Initialize Game Analytics
			GameAnalytics.Initialize();
			isInitialized = true;
			SendLog.Log("[Analytics] Game Analytics initialized successfully.");
		}

		public override void SendEvent(string name)
		{
			if (!isInitialized)
			{
				SendLog.LogWarning("[Analytics] Cannot send event: Game Analytics not initialized.");
				return;
			}

			GameAnalytics.NewDesignEvent(name);
			SendLog.Log($"[Analytics] Event sent: {name}");
		}

		public override void SendEvent(string name, string value)
		{
			if (!isInitialized)
			{
				SendLog.LogWarning("[Analytics] Cannot send event: Game Analytics not initialized.");
				return;
			}

			float customFields;
			if (float.TryParse(value, out customFields))
			{
				GameAnalytics.NewDesignEvent(name, customFields);
				SendLog.Log($"[Analytics] Event sent: {name} with value: {customFields}");
			}
			else
			{
				SendLog.LogWarning($"[Analytics] Cannot convert value to float: {value}");
			}
		}

		public override void SendEvent(ProgressionStatus status, string eventName)
		{
			if (!isInitialized)
			{
				SendLog.LogWarning("[Analytics] Cannot send progression event: Game Analytics not initialized.");
				return;
			}

			GAProgressionStatus gaStatus = ConvertToGAProgressionStatus(status);
			GameAnalytics.NewProgressionEvent(gaStatus, eventName);
			SendLog.Log($"[Analytics] Progression event sent: {status} - {eventName}");
		}

		public override void SendDesignEvent(string category, string subCategory, string outcome, float value)
		{
			if (!isInitialized)
			{
				SendLog.LogWarning("[Analytics] Cannot send design event: Game Analytics not initialized.");
				return;
			}

			// Create a hierarchical event name
			string eventName = $"{category}:{subCategory}:{outcome}";
			GameAnalytics.NewDesignEvent(eventName, value);
			SendLog.Log($"[Analytics] Design event sent: {eventName} with value: {value}");
		}

		public override void SendDesignEvent(string category, string subCategory, string outcome)
		{
			if (!isInitialized)
			{
				SendLog.LogWarning("[Analytics] Cannot send design event: Game Analytics not initialized.");
				return;
			}

			// Create a hierarchical event name
			string eventName = $"{category}:{subCategory}:{outcome}";
			GameAnalytics.NewDesignEvent(eventName);
			SendLog.Log($"[Analytics] Design event sent: {eventName}");
		}

		public override void SendTransaction(string productId, string currencyCode, int quantity, double unitPrice, string receipt, string signature)
		{
			if (!isInitialized)
			{
				SendLog.LogWarning("[Analytics] Cannot send transaction: Game Analytics not initialized.");
				return;
			}

			// Convert to float for Game Analytics
			int amount = (int)(unitPrice * quantity);

			// Send business event for transaction
			GameAnalytics.NewBusinessEvent(currencyCode, amount, productId, quantity.ToString(), receipt, null);
			SendLog.Log($"[Analytics] Transaction sent: {productId} - {amount} {currencyCode}");
		}

		public override void SendEventLog(Dictionary<string, object> eventLog)
		{
			if (!isInitialized)
			{
				SendLog.LogWarning("[Analytics] Cannot send event log: Game Analytics not initialized.");
				return;
			}

			// Convert dictionary to custom event
			foreach (var kvp in eventLog)
			{
				string eventName = kvp.Key;
				object value = kvp.Value;

				if (value is string stringValue)
				{
					//GameAnalytics.NewDesignEvent(eventName, stringValue);
				}
				else if (value is float floatValue)
				{
					GameAnalytics.NewDesignEvent(eventName, floatValue);
				}
				else if (value is int intValue)
				{
					GameAnalytics.NewDesignEvent(eventName, (float)intValue);
				}
				else
				{
					//GameAnalytics.NewDesignEvent(eventName, value.ToString());
				}
			}
			SendLog.Log($"[Analytics] Event log sent with {eventLog.Count} events");
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
			if (!isInitialized)
			{
				SendLog.LogWarning("[Analytics] Cannot set user property: Game Analytics not initialized.");
				return;
			}

			GameAnalytics.SetCustomDimension01(propertyName);
			SendLog.Log($"[Analytics] User property set: {propertyName} = {propertyValue}");
		}
	}

}
