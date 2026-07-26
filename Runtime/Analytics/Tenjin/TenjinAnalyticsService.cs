using System;
using System.Collections.Generic;
using System.Reflection;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.MonetizationApi.Analytics
{
	public class TenjinAnalyticsService
	{
		private readonly string _moduleName;
		private object _tenjinInstance;
		private MethodInfo _sendEventMethod;
		private MethodInfo _transactionMethod;
		private MethodInfo _connectMethod;

		public bool IsReady { get; private set; }

		public TenjinAnalyticsService(string moduleName)
		{
			_moduleName = moduleName;
		}

		public void Initialize(string apiKey)
		{
			if (string.IsNullOrEmpty(apiKey))
			{
				SendLog.LogModule(_moduleName, "Tenjin API key is missing in MonetizationKeys.json.", LogLevel.Warning);
				IsReady = false;
				return;
			}

			var tenjinType = Type.GetType("TenjinSDK.BaseTenjin, Tenjin");
			if (tenjinType == null)
			{
				SendLog.LogModule(_moduleName, "Tenjin SDK type not found. Install Tenjin package first.", LogLevel.Error);
				IsReady = false;
				return;
			}

			var getInstanceMethod = tenjinType.GetMethod("getInstance", BindingFlags.Public | BindingFlags.Static);
			if (getInstanceMethod == null)
			{
				SendLog.LogModule(_moduleName, "Tenjin getInstance method was not found.", LogLevel.Error);
				IsReady = false;
				return;
			}

			_tenjinInstance = getInstanceMethod.Invoke(null, new object[] { apiKey });
			if (_tenjinInstance == null)
			{
				SendLog.LogModule(_moduleName, "Failed to create Tenjin instance.", LogLevel.Error);
				IsReady = false;
				return;
			}

			_sendEventMethod = tenjinType.GetMethod("SendEvent", new[] { typeof(string) });
			_transactionMethod = tenjinType.GetMethod("Transaction", new[]
			{
				typeof(string), typeof(string), typeof(int), typeof(double), typeof(string), typeof(string)
			});
			_connectMethod = tenjinType.GetMethod("Connect", Type.EmptyTypes);
			_connectMethod?.Invoke(_tenjinInstance, null);
			IsReady = true;
			SendLog.LogModule(_moduleName, "Tenjin Analytics initialized.");
		}

		public void SendEvent(string name)
		{
			if (!IsReady || _sendEventMethod == null) return;
			_sendEventMethod.Invoke(_tenjinInstance, new object[] { name });
		}

		public void SendPurchase(string productId, string currency, int quantity, double unitPrice, string receipt, string signature)
		{
			if (!IsReady || _transactionMethod == null) return;
			_transactionMethod.Invoke(_tenjinInstance, new object[] { productId, currency, quantity, unitPrice, receipt, signature });
		}

		public void SendAdImpression(string adNetwork, double revenueUsd, string placement)
		{
			if (!IsReady)
			{
				return;
			}

			var payload = new Dictionary<string, string>
			{
				{ "network", adNetwork ?? string.Empty },
				{ "revenue_usd", revenueUsd.ToString("F6") },
				{ "placement", placement ?? string.Empty }
			};

			SendEvent($"ad_impression:{payload["network"]}:{payload["placement"]}:{payload["revenue_usd"]}");
		}
	}
}
