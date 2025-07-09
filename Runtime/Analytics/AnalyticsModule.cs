using System.Collections.Generic;
using THEBADDEST.Tasks;

public enum ProgressionStatus
{

	//Undefined progression
	Undefined = 0,
	// User started progression
	Start = 1,
	// User succesfully ended a progression
	Complete = 2,
	// User failed a progression
	Fail = 3

}

public abstract class AnalyticsModule : MonotizationModule, IAnalyticsModule
{

	public override async UTask Initialize()
	{
	}

	public abstract void SendEvent(string name);

	public abstract void SendEvent(string name, string value);

	public abstract void SendEvent(ProgressionStatus status, string eventName);

	public abstract void SendDesignEvent(string category, string subCategory, string outcome, float value);

	public abstract void SendDesignEvent(string category, string subCategory, string outcome);

	public abstract void SendTransaction(string productId, string currencyCode, int quantity, double unitPrice, string receipt, string signature);

	public abstract void SendEventLog(Dictionary<string, object> eventLog);

}