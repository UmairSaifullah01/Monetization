using System;
using System.Collections.Generic;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;


namespace THEBADDEST.Analytics
{


	public abstract class AnalyticsModule : MonetizationModule, IAnalyticsModule
	{
		public event Action<bool> OnSdkReady;

		protected void RaiseSdkInitialized(bool success) => OnSdkReady?.Invoke(success);

		protected override async UTask OnInitialize()
		{
			EventBus.Subscribe<AdShownEvent>(OnAdShown);
			await UTask.CompletedTask;
		}

		protected override void OnModuleDestroy()
		{
			EventBus.Unsubscribe<AdShownEvent>(OnAdShown);
		}

		protected virtual void OnAdShown(AdShownEvent evt)
		{
			SendLog.LogModule(ModuleName, $"Ad shown: Type={evt.AdType}, Placement={evt.Placement}, Time={evt.Time}");
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
