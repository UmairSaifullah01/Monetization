using System.Collections.Generic;


namespace THEBADDEST.Analytics
{

	
	public class AnalyticsBridgeModule : AnalyticsModule
	{

		public override void SendEvent(string name)
		{
			throw new System.NotImplementedException();
		}

		public override void SendEvent(string name, string value)
		{
			throw new System.NotImplementedException();
		}

		public override void SendEvent(ProgressionStatus status, string eventName)
		{
			throw new System.NotImplementedException();
		}

		public override void SendDesignEvent(string category, string subCategory, string outcome, float value)
		{
			throw new System.NotImplementedException();
		}

		public override void SendDesignEvent(string category, string subCategory, string outcome)
		{
			throw new System.NotImplementedException();
		}

		public override void SendTransaction(string productId, string currencyCode, int quantity, double unitPrice, string receipt, string signature)
		{
			throw new System.NotImplementedException();
		}

		public override void SendEventLog(Dictionary<string, object> eventLog)
		{
			throw new System.NotImplementedException();
		}

	}

}
