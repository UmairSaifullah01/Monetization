using System;
using System.Collections.Generic;
using System.Globalization;
using THEBADDEST.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace THEBADDEST.MonetizationApi
{
	public class UnityIAPService : IDetailedStoreListener
	{
		private readonly IAPCatalog _catalog;
		private readonly string _moduleName;
		private readonly Dictionary<string, Action> _successCallbacks = new Dictionary<string, Action>();
		private readonly Dictionary<string, Action> _failCallbacks = new Dictionary<string, Action>();

		private IStoreController _controller;
		private IExtensionProvider _extensions;
		private bool _storeInitComplete;
		private bool _storeInitSuccess;

		public bool IsReady => _storeInitSuccess && _controller != null;
		public IStoreController Controller => _controller;
		public IExtensionProvider Extensions => _extensions;

		public UnityIAPService(IAPCatalog catalog, string moduleName)
		{
			_catalog = catalog;
			_moduleName = moduleName;
		}

		public async UTask InitializeAsync()
		{
			_storeInitComplete = false;
			_storeInitSuccess = false;

			ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
			foreach (var product in _catalog.items)
			{
				builder.AddProduct(product.productId, product.consumable ? ProductType.Consumable : ProductType.NonConsumable);
			}

			UnityPurchasing.Initialize(this, builder);
			await UTask.WaitUntil(() => _storeInitComplete);

			if (!_storeInitSuccess)
			{
				throw new InvalidOperationException("[IAP] Unity IAP failed to initialize.");
			}
		}

		public void RegisterPurchaseCallbacks(string productId, Action successCallback, Action failCallback)
		{
			_successCallbacks[productId] = successCallback;
			_failCallbacks[productId] = failCallback;
		}

		public void Purchase(string productId)
		{
			_controller.InitiatePurchase(productId);
		}

		public void RestorePurchases()
		{
#if UNITY_IOS
			var apple = _extensions?.GetExtension<IAppleExtensions>();
			apple?.RestoreTransactions((success, error) =>
			{
				SendLog.LogModule(_moduleName, success ? "Restore successful." : $"Restore failed: {error}");
			});
#else
			SendLog.LogModule(_moduleName, "RestorePurchases is not supported on this platform.");
#endif
		}

		public string GetProductPriceUsd(string productId)
		{
			var iapProduct = _catalog.Find(x => x.productId.Equals(productId));
			return iapProduct == null ? "$10" : $"${iapProduct.price}";
		}

		public string GetProductLocalizedPrice(string productId)
		{
			if (string.IsNullOrEmpty(productId))
			{
				return GetProductPriceUsd(productId);
			}

			if (_controller == null)
			{
				return GetProductPriceUsd(productId);
			}

			var product = _controller.products.WithID(productId);
			return product?.metadata.localizedPriceString ?? GetProductPriceUsd(productId);
		}

		public void GetProductPriceAndCurrencyCode(string productId, out string currencyCode, out double price)
		{
			string priceString = GetProductLocalizedPrice(productId);
			price = 0.0;
			currencyCode = "";

			if (string.IsNullOrWhiteSpace(priceString))
			{
				return;
			}

			int index = 0;
			while (index < priceString.Length && !char.IsDigit(priceString[index]) && priceString[index] != '.')
			{
				index++;
			}

			currencyCode = priceString.Substring(0, index).Trim();
			string pricePart = priceString.Substring(index).Trim();
			if (!double.TryParse(pricePart, NumberStyles.Currency, CultureInfo.InvariantCulture, out price))
			{
				price = 0.0;
			}
		}

		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			_controller = controller;
			_extensions = extensions;
			_storeInitSuccess = true;
			_storeInitComplete = true;
			SendLog.LogModule(_moduleName, "IAP initialized successfully.");

#if UNITY_ANDROID
			_extensions.GetExtension<IGooglePlayStoreExtensions>()
				.RestoreTransactions((result, error) =>
				{
					if (result)
					{
						SendLog.LogModule(_moduleName, "Purchases already restored for this account.");
					}
				});
#elif UNITY_IOS || UNITY_IPHONE
			_extensions.GetExtension<IAppleExtensions>()
				.RestoreTransactions((result, error) =>
				{
					if (result)
					{
						SendLog.LogModule(_moduleName, "Purchases already restored for this account.");
					}
				});
#endif

			foreach (var item in _catalog.items)
			{
				if (item.consumable) continue;
				Product product = controller.products.WithID(item.productId);
				if (product is { hasReceipt: true } && product.definition.type == ProductType.NonConsumable)
				{
					item.alreadyPurchased = true;
				}
			}
		}

		public void OnInitializeFailed(InitializationFailureReason error)
		{
			SendLog.LogModule(_moduleName, $"Failed to initialize IAP: {error}", LogLevel.Error);
			_storeInitSuccess = false;
			_storeInitComplete = true;
		}

		public void OnInitializeFailed(InitializationFailureReason error, string message)
		{
			SendLog.LogModule(_moduleName, $"Failed to initialize IAP: {error} - {message}", LogLevel.Error);
			_storeInitSuccess = false;
			_storeInitComplete = true;
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
		{
			if (purchaseEvent.purchasedProduct.receipt != null)
			{
				string productId = purchaseEvent.purchasedProduct.definition.id;
				if (_successCallbacks.TryGetValue(productId, out Action callback))
				{
					callback?.Invoke();
					_successCallbacks.Remove(productId);
					_failCallbacks.Remove(productId);
				}

				return PurchaseProcessingResult.Complete;
			}

			return PurchaseProcessingResult.Pending;
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
		{
			HandlePurchaseFailed(product.definition.id, failureReason.ToString());
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
		{
			HandlePurchaseFailed(product.definition.id, failureDescription.reason.ToString());
		}

		private void HandlePurchaseFailed(string productId, string reason)
		{
			if (_failCallbacks.TryGetValue(productId, out Action callback))
			{
				callback?.Invoke();
				_successCallbacks.Remove(productId);
				_failCallbacks.Remove(productId);
			}

			SendLog.LogModule(_moduleName, $"Purchase failed: Product '{productId}' - Reason: {reason}", LogLevel.Error);
		}
	}
}
