using System;
using System.Collections.Generic;
using UnityEngine;


namespace THEBADDEST.MonetizationApi
{


	public abstract class IAPModule : MonetizationModule, IIAPModule
	{
		[SerializeField] private IAPCatalog catalog = new IAPCatalog();
		public IAPCatalog Catalog => catalog;
		
		protected Dictionary<string, Action> successCallbacks = new Dictionary<string, Action>();
		protected Dictionary<string, Action> failCallbacks = new Dictionary<string, Action>();

		public virtual void Purchase(string productId, Action successCallback, Action failCallback)
		{
			successCallbacks[productId] = successCallback;
			failCallbacks[productId] = failCallback;
		}

        public virtual string GetProductPriceUsd(string productId)
        {
            IAPItem item = catalog.Find(i => i.productId == productId);
            if (item == null)
            {
                SendLog.LogWarning($"[IAP] Product with ID '{productId}' not found in catalog.");
                return "0.00";
            }
            return item.price.ToString("0.00");
        }

        public virtual string GetProductLocalizedPrice(string productId)
        {
            IAPItem item = catalog.Find(i => i.productId == productId);
            if (item == null)
            {
                SendLog.LogWarning($"[IAP] Product with ID '{productId}' not found in catalog.");
                return "0.00";
            }
            return item.price.ToString("0.00");
        }

        public float GetLocalUsdConversion(string productId)
        {
            IAPItem item = catalog.Find(i => i.productId == productId);
            if (item == null)
            {
                SendLog.LogWarning($"[IAP] Product with ID '{productId}' not found in catalog.");
                return 0f;
            }
            GetProductPriceAndCurrencyCode(productId, out string currencyCode, out double price);
            if (item.price <= 0)
            {
                SendLog.LogWarning($"[IAP] Invalid price for product '{productId}'. Cannot calculate conversion.");
                return 0f;
            }
            double d = price / Convert.ToDouble(item.price);
            return (float)d;
        }

        public virtual void GetProductPriceAndCurrencyCode(string productId, out string currencyCode, out double price)
        {
            IAPItem item = catalog.Find(i => i.productId == productId);
            if (item == null)
            {
                SendLog.LogWarning($"[IAP] Product with ID '{productId}' not found in catalog.");
                currencyCode = "USD";
                price = 0.0;
                return;
            }
            currencyCode = "USD";
            price = item.price;
        }

		public abstract void RestorePurchases();

	}


}