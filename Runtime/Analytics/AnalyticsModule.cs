using System.Collections.Generic;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;
using System;


namespace THEBADDEST.Analytics
{


	public abstract class AnalyticsModule : MonetizationModule, IAnalyticsModule
	{

		public override async UTask Initialize()
		{
			EventBus.Subscribe<AdShownEvent>(OnAdShown);
		}

		/// <summary>
		/// Called when an ad is shown. Override to process ad shown events.
		/// </summary>
		/// <param name="evt">The ad shown event.</param>
		protected virtual void OnAdShown(AdShownEvent evt)
		{
			SendLog.Log($"[Analytics] Ad shown: Type={evt.AdType}, Placement={evt.Placement}, Time={evt.Time}");
			// Derived classes can override to send analytics events
		}

		public abstract void SendEvent(string name);

		public abstract void SendEvent(string name, string value);

		public abstract void SendEvent(ProgressionStatus status, string eventName);

		public abstract void SendDesignEvent(string category, string subCategory, string outcome, float value);

		public abstract void SendDesignEvent(string category, string subCategory, string outcome);

		public abstract void SendTransaction(string productId, string currencyCode, int quantity, double unitPrice, string receipt, string signature);

		public abstract void SendEventLog(Dictionary<string, object> eventLog);

	}


}