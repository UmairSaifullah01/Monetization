using System;
using System.Collections.Generic;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.Analytics
{
	[Flags]
	public enum AnalyticsProviders
	{
		None = 0,
		GameAnalytics = 1 << 0,
		Firebase = 1 << 1,
		Facebook = 1 << 2,
		Tenjin = 1 << 3,
		All = GameAnalytics | Firebase | Facebook | Tenjin
	}

	public static class Analytics
	{
		public static void SendEvent(string name, AnalyticsProviders targets)
		{
			ForEachTarget(targets, module => module.SendEvent(name));
		}

		public static void SendEvent(string name, string value, AnalyticsProviders targets)
		{
			ForEachTarget(targets, module => module.SendEvent(name, value));
		}

		public static void SendEvent(ProgressionStatus status, string eventName, AnalyticsProviders targets)
		{
			ForEachTarget(targets, module => module.SendEvent(status, eventName));
		}

		public static void SendDesignEvent(string category, string subCategory, string outcome, float value, AnalyticsProviders targets)
		{
			ForEachTarget(targets, module => module.SendDesignEvent(category, subCategory, outcome, value));
		}

		public static void SendDesignEvent(string category, string subCategory, string outcome, AnalyticsProviders targets)
		{
			ForEachTarget(targets, module => module.SendDesignEvent(category, subCategory, outcome));
		}

		public static void SendTransaction(string productId, string currencyCode, int quantity, double unitPrice, string receipt, string signature, AnalyticsProviders targets)
		{
			ForEachTarget(targets, module => module.SendTransaction(productId, currencyCode, quantity, unitPrice, receipt, signature));
		}

		public static void SendEventLog(Dictionary<string, object> eventLog, AnalyticsProviders targets)
		{
			ForEachTarget(targets, module => module.SendEventLog(eventLog));
		}

		private static void ForEachTarget(AnalyticsProviders targets, Action<IAnalyticsModule> action)
		{
			if ((targets & AnalyticsProviders.GameAnalytics) != 0 && Monetization.TryGetModule<IGAAnalyticsModule>(out var ga))
			{
				action(ga);
			}

			if ((targets & AnalyticsProviders.Firebase) != 0 && Monetization.TryGetModule<IFirebaseAnalyticsModule>(out var firebase))
			{
				action(firebase);
			}

			if ((targets & AnalyticsProviders.Facebook) != 0 && Monetization.TryGetModule<IFacebookAnalyticsModule>(out var facebook))
			{
				action(facebook);
			}

			if ((targets & AnalyticsProviders.Tenjin) != 0 && Monetization.TryGetModule<ITenjinAnalyticsModule>(out var tenjin))
			{
				action(tenjin);
			}
		}
	}
}
