using System;


namespace THEBADDEST.MonetizationApi
{


	public interface IIAPModule : IModule
	{
		IAPCatalog Catalog { get; }

		void Purchase(string productId, Action successCallback, Action failCallback);

		string GetProductPriceUsd(string productId);

		string GetProductLocalizedPrice(string productId);

		float GetLocalUsdConversion(string productId);

		void RestorePurchases();

	}


}