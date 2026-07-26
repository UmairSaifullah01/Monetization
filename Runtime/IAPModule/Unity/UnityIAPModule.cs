using System;
using THEBADDEST.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace THEBADDEST.MonetizationApi
{
    public class UnityIAPModule : IAPModule, IDetailedStoreListener
    {
        private UnityIAPService _service;

        protected override async UTask OnInitialize()
        {
            _service = new UnityIAPService(Catalog, ModuleName);
            await _service.InitializeAsync();
        }

        public override void Purchase(string productId, Action successCallback, Action failCallback)
        {
            if (!IsInitialized || _service == null || !_service.IsReady)
            {
                failCallback?.Invoke();
                SendLog.LogModule(ModuleName, "Purchase failed: IAP Service not initialized yet!", LogLevel.Error);
                return;
            }

            Product product = _service.Controller.products.WithID(productId);
            if (!product?.availableToPurchase ?? true)
            {
                failCallback?.Invoke();
                SendLog.LogModule(ModuleName, $"Purchase failed: Product '{productId}' is not available for purchase.", LogLevel.Error);
                return;
            }

            if (product.hasReceipt && product.definition.type == ProductType.NonConsumable)
            {
                SendLog.LogModule(ModuleName, $"Purchase skipped: Product '{productId}' is already purchased (non-consumable).");
                successCallback?.Invoke();
                return;
            }

            _service.RegisterPurchaseCallbacks(productId, successCallback, failCallback);
            _service.Purchase(productId);
        }

        public override void RestorePurchases()
        {
            if (!IsInitialized || _service == null)
            {
                SendLog.LogModule(ModuleName, "Cannot restore purchases: IAP Service not initialized.", LogLevel.Error);
                return;
            }

            _service.RestorePurchases();
        }

        public override string GetProductPriceUsd(string productId) => _service?.GetProductPriceUsd(productId) ?? base.GetProductPriceUsd(productId);

        public override string GetProductLocalizedPrice(string productId) => _service?.GetProductLocalizedPrice(productId) ?? base.GetProductLocalizedPrice(productId);

        public override void GetProductPriceAndCurrencyCode(string productId, out string currencyCode, out double price)
        {
            if (_service != null)
            {
                _service.GetProductPriceAndCurrencyCode(productId, out currencyCode, out price);
                return;
            }

            base.GetProductPriceAndCurrencyCode(productId, out currencyCode, out price);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions) => _service?.OnInitialized(controller, extensions);

        public void OnInitializeFailed(InitializationFailureReason error) => _service?.OnInitializeFailed(error);

        public void OnInitializeFailed(InitializationFailureReason error, string message) => _service?.OnInitializeFailed(error, message);

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent) =>
            _service?.ProcessPurchase(purchaseEvent) ?? PurchaseProcessingResult.Pending;

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) => _service?.OnPurchaseFailed(product, failureReason);

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription) => _service?.OnPurchaseFailed(product, failureDescription);
    }
}
