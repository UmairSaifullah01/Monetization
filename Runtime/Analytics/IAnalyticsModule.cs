using System.Collections.Generic;
using THEBADDEST.MonetizationApi;


namespace THEBADDEST.Analytics
{


	public interface IAnalyticsModule : IModule
	{

		public void SendEvent(string name);

		public void SendEvent(string name, string value);

		public void SendEvent(ProgressionStatus status, string eventName);

		public void SendDesignEvent(string category, string subCategory, string outcome, float value);

		public void SendDesignEvent(string category, string subCategory, string outcome);

		public void SendTransaction(string productId, string currencyCode, int quantity, double unitPrice, string receipt, string signature);

		public void SendEventLog(Dictionary<string, object> eventLog);

	}


}