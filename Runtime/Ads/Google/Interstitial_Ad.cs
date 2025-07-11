using System;
using GoogleMobileAds.Api;
using UnityEngine;
using THEBADDEST.MonetizationApi;


namespace THEBADDEST.Advertisement
{


	public class Interstitial_Ad : IAppAd
	{

		public event Action              OnAdLoaded;
		public event Action OnAdLoadFailed;
		public event Action<AdValue>     OnAdPaid;


		public InterstitialAd ad => interstitialAd;
		object IAppAd.        ad => ad;
		InterstitialAd        interstitialAd;
		string                unitId;

		public Interstitial_Ad(string unitId)
		{
			this.unitId = unitId;
		}

		public void Create()
		{
			Load();
		}

		public void Destroy()
		{
			if (interstitialAd != null)
			{
				interstitialAd.Destroy();
				interstitialAd = null;
			}
		}

		public bool IsLoaded()
		{
			return interstitialAd != null && interstitialAd.CanShowAd();
		}

		public void Show()
		{
			if (interstitialAd != null && interstitialAd.CanShowAd())
			{
				interstitialAd.Show();
				EventBus.Publish(new AdShownEvent {
					AdType = "Interstitial",
					Placement = unitId,
					Time = DateTime.Now
				});
			}
			else
			{
				SendLog.LogError("Interstitial ad is not ready yet.");
			}
		}

		public void Load()
		{
			var adRequest = new AdRequest();
			InterstitialAd.Load(unitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
			{
				if (error != null)
				{
					SendLog.LogError("Interstitial ad failed to load: " + error);
					OnAdLoadFailed?.Invoke();
					return;
				}

				// If the operation failed for unknown reasons.
				// This is an unexpected error, please report this bug if it happens.
				if (ad == null)
				{
					SendLog.LogError("Unexpected error: Interstitial load event fired with null ad and null error.");
					return;
				}

				OnAdLoaded?.Invoke();
				interstitialAd          =  ad;
				interstitialAd.OnAdPaid += info =>
				{
					AdValue adValue = new AdValue { Value = info.Value, CurrencyCode = info.CurrencyCode, Precision = (AdValue.PrecisionType)(int)info.Precision };
					OnAdPaid?.Invoke(adValue);
				};
				interstitialAd.OnAdFullScreenContentClosed += Load;
			});
		}

		public void Hide()
		{
			Destroy();
		}

	}


}