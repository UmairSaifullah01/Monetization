using System;
using System.Collections.Generic;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;


namespace THEBADDEST.MonetizationApi.Analytics
{


	public abstract class AnalyticsModule : MonetizationModule, IAnalyticsModule
	{
		public event Action<bool> OnSdkReady;

		protected void RaiseSdkInitialized(bool success) => OnSdkReady?.Invoke(success);

		protected override async UTask OnInitialize()
		{
			await UTask.CompletedTask;
		}

		protected override void OnModuleDestroy()
		{
		}

		protected virtual void OnAdShown(AdShownEvent evt)
		{
			// Intentionally empty. Ad event routing is controlled by game-level orchestration.
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
