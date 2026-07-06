using System;
using System.Globalization;
using THEBADDEST.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;


namespace THEBADDEST.MonetizationApi
{


    public class UnityIAPModule : IAPModule, IDetailedStoreListener
    {
        private IStoreController controller;
        private IExtensionProvider extensions;
        private bool _storeInitComplete;
        private bool _storeInitSuccess;

        protected override async UTask OnInitialize()
        {
            var configAsset = MonetizationConfig.Instance;
            if (!configAsset.EnableIAP)
            {
                SendLog.LogWarning("[IAP] IAP is disabled by MonetizationConfig.");
                return;
            }

            _storeInitComplete = false;
            _storeInitSuccess = false;

            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var product in Catalog.items)
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



        public override void Purchase(string productId, Action successCallback, Action failCallback)
        {
            if (!IsInitialized)
            {
                failCallback?.Invoke();
                SendLog.LogError("[IAP] Purchase failed: IAPService not initialized yet!");
                return;
            }
            if (controller == null)
            {
                failCallback?.Invoke();
                SendLog.LogError("[IAP] Purchase failed: IAP Controller is not initialized.");
                return;
            }
            Product product = controller.products.WithID(productId);
            if (!product?.availableToPurchase ?? true)
            {
                failCallback?.Invoke();
                SendLog.LogError($"[IAP] Purchase failed: Product '{productId}' is not available for purchase.");
                return;
            }
            if (product.hasReceipt && product.definition.type == ProductType.NonConsumable)
            {
                SendLog.LogError($"[IAP] Purchase skipped: Product '{productId}' is already purchased (non-consumable).");
                successCallback?.Invoke();
                return;
            }
            base.Purchase(productId, successCallback, failCallback);
            controller.InitiatePurchase(productId);
        }


        public override void RestorePurchases()
        {
            if (!IsInitialized)
            {
                SendLog.LogError("[IAP] Cannot restore purchases: IAPService not initialized.");
                return;
            }

#if UNITY_IOS
            var apple = extensions?.GetExtension<IAppleExtensions>();
            apple?.RestoreTransactions((success, error) =>
            {
                SendLog.Log(success ? "[IAP] Restore successful." : $"[IAP] Restore failed: {error}");
            });
#else
            SendLog.Log("[IAP] RestorePurchases is not supported on this platform.");
#endif
        }

        public override string GetProductPriceUsd(string productId)
        {
            string amount = "$10";
            var iapProduct = Catalog.Find(x => x.productId.Equals(productId));
            if (iapProduct == null)
            {
                SendLog.Log($"[IAP] GetProductPriceUsd: Product with ID '{productId}' not found in the catalog.");
                return amount;
            }

            amount = $"${iapProduct.price}";
            return amount;
        }

        public override string GetProductLocalizedPrice(string productId)
        {
            string amount = "$10";
            if (string.IsNullOrEmpty(productId))
            {
                SendLog.Log("[IAP] GetProductLocalizedPrice: Product ID is null or empty.");
                return amount;
            }

            amount = GetProductPriceUsd(productId);
            if (controller == null)
            {
                SendLog.Log("[IAP] GetProductLocalizedPrice: IAP Service is not initialized.");
                return amount;
            }
            var product = controller.products.WithID(productId);
            if (product == null)
            {
                SendLog.Log($"[IAP] GetProductLocalizedPrice: Product with ID '{productId}' not found in the store.");
                return amount;
            }
            return product.metadata.localizedPriceString ?? amount;
        }


        public override void GetProductPriceAndCurrencyCode(string productId, out string currencyCode, out double price)
        {
            string priceString = GetProductLocalizedPrice(productId);
            price = 0.0;
            currencyCode = "";

            if (string.IsNullOrWhiteSpace(priceString))
                return;

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
            this.controller = controller;
            this.extensions = extensions;
            _storeInitSuccess = true;
            _storeInitComplete = true;
            SendLog.Log("[IAP] IAP initialized successfully.");
#if UNITY_ANDROID
            this.extensions.GetExtension<IGooglePlayStoreExtensions>()
                .RestoreTransactions((result, error) =>
                {
                    SendLog.Log($"[IAP] Restoring Purchases: Result={{result}}, Error={{error}}");
                    if (result)
                    {
                        SendLog.Log("[IAP] Purchases already restored for this account.");
                    }
                });
#elif UNITY_IOS || UNITY_IPHONE
            this.extensions.GetExtension<IAppleExtensions>()
                .RestoreTransactions((result, error) =>
                {
                    if (result)
                    {
                        SendLog.Log("[IAP] Purchases already restored for this account.");
                    }
                });
#endif

            foreach (var item in Catalog.items)
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
            SendLog.LogError($"[IAP] Failed to initialize IAP: {error}");
            _storeInitSuccess = false;
            _storeInitComplete = true;
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            SendLog.LogError($"[IAP] Failed to initialize IAP: {error} - {message}");
            _storeInitSuccess = false;
            _storeInitComplete = true;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            if (purchaseEvent.purchasedProduct.receipt != null)
            {
                string productId = purchaseEvent.purchasedProduct.definition.id;
                if (successCallbacks.TryGetValue(productId, out Action callback))
                {
                    callback?.Invoke();
                    successCallbacks.Remove(productId);
                    failCallbacks.Remove(productId);
                }

                return PurchaseProcessingResult.Complete;
            }

            return PurchaseProcessingResult.Pending;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            string productId = product.definition.id;
            if (failCallbacks.TryGetValue(productId, out Action callback))
            {
                callback?.Invoke();
                successCallbacks.Remove(productId);
                failCallbacks.Remove(productId);
            }

            SendLog.LogError($"[IAP] Purchase failed: Product '{productId}' - Reason: {failureReason}");
        }
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            string productId = product.definition.id;
            if (failCallbacks.TryGetValue(productId, out Action callback))
            {
                callback?.Invoke();
                successCallbacks.Remove(productId);
                failCallbacks.Remove(productId);
            }

            SendLog.LogError($"[IAP] Purchase failed: Product '{productId}' - Description: {failureDescription.reason}");
        }
    }
}
