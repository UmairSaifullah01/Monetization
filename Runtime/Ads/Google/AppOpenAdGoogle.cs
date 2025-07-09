using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;
using THEBADDEST.Monetization;


namespace THEBADDEST.Advertisement
{


	public class AppOpenAdGoogle : IAppAd
	{

		public event Action          OnAdLoaded;
		public event Action          OnAdLoadFailed;
		public event Action<AdValue> OnAdPaid;
		AppOpenAd                    ad => appOpenAd;
		object IAppAd.               ad => ad;
		public AppOpenAd             appOpenAd;
		string                       unitId;

		public AppOpenAdGoogle(string unitId)
		{
			this.unitId                           =  unitId;
			AppStateEventNotifier.AppStateChanged += OnAppStateChange;
		}

		~AppOpenAdGoogle()
		{
			AppStateEventNotifier.AppStateChanged -= OnAppStateChange;
		}

		void OnAppStateChange(AppState state)
		{
			if (state == AppState.Background)
			{
				Load();
			}
			else
			{
				Show();
			}
		}

		public void Create()
		{
			Load();
		}

		public void Destroy()
		{
			if (appOpenAd != null)
			{
				SendLog.Log("Destroying app open ad instance.");
				appOpenAd.Destroy();
				appOpenAd = null;
			}
		}

		public bool IsLoaded()
		{
			return appOpenAd != null && appOpenAd.CanShowAd();
		}

		public void Show()
		{
			// App open ads can be preloaded for up to 4 hours.
			if (appOpenAd != null && appOpenAd.CanShowAd())
			{
				SendLog.Log("Showing app open ad.");
				appOpenAd.Show();
			}
			else
			{
				SendLog.LogWarning("App open ad is not ready yet.");
			}
		}

		public void Load()
		{
			var adRequest = new AdRequest();

			// Send the request to load the ad.
			AppOpenAd.Load(unitId, adRequest, (AppOpenAd ad, LoadAdError error) =>
			{
				// If the operation failed with a reason.
				if (error != null)
				{
					SendLog.LogError("App open ad failed to load: " + error);
					OnAdLoadFailed?.Invoke();
					return;
				}

				// If the operation failed for unknown reasons.
				// This is an unexpected error, please report this bug if it happens.
				if (ad == null)
				{
					Debug.Log("Unexpected error: App open ad load event fired with " + " null ad and null error.");
					return;
				}

				OnAdLoaded?.Invoke();
				// The operation completed successfully.
				Debug.Log("App open ad loaded with response : " + ad.GetResponseInfo());
				appOpenAd          =  ad;
				appOpenAd.OnAdPaid += info =>
				{
					AdValue adValue = new AdValue { Value = info.Value, CurrencyCode = info.CurrencyCode, Precision = (AdValue.PrecisionType)(int)info.Precision };
					OnAdPaid?.Invoke(adValue);
				};
			});
		}

		public void Hide()
		{
			Destroy();
		}

	}


}