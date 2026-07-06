using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;
using THEBADDEST.MonetizationApi;


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
			if (appOpenAd != null && appOpenAd.CanShowAd())
			{
				SendLog.Log("Showing app open ad.");
				appOpenAd.Show();
				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.ShowSucceeded, unitId);
				EventBus.Publish(new AdShownEvent
				{
					AdType = AdMetricsTypes.AppOpen,
					Placement = unitId,
					Time = DateTime.Now
				});
			}
			else
			{
				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.ShowFailed, unitId);
				SendLog.LogWarning("App open ad is not ready yet.");
			}
		}

		public void Load()
		{
			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.LoadStarted, unitId);
			var adRequest = new AdRequest();

			AppOpenAd.Load(unitId, adRequest, (AppOpenAd ad, LoadAdError error) =>
			{
				if (error != null)
				{
					PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.LoadFailed, unitId);
					SendLog.LogError("App open ad failed to load: " + error);
					OnAdLoadFailed?.Invoke();
					return;
				}

				if (ad == null)
				{
					PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.LoadFailed, unitId);
					Debug.Log("Unexpected error: App open ad load event fired with null ad and null error.");
					return;
				}

				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.LoadSucceeded, unitId);
				OnAdLoaded?.Invoke();
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
