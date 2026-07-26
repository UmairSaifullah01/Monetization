using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;
using THEBADDEST.MonetizationApi;
using THEBADDEST.MonetizationApi.Ads;


namespace THEBADDEST.MonetizationApi.Ads
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
		bool                         _loadSettled;

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
				SendLog.LogModule(GoogleAdsLog.Module, "Destroying app open ad instance.");
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
				SendLog.LogModule(GoogleAdsLog.Module, "Showing app open ad.");
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
				SendLog.LogModule(GoogleAdsLog.Module, "App open ad is not ready yet.", LogLevel.Warning);
			}
		}

		public void Load()
		{
			_loadSettled = false;
			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.LoadStarted, unitId);
			AdLoadTimeoutWatcher.Watch(AdMetricsTypes.AppOpen, unitId, () => _loadSettled);
			var adRequest = new AdRequest();

			AppOpenAd.Load(unitId, adRequest, (AppOpenAd ad, LoadAdError error) =>
			{
				_loadSettled = true;
				if (error != null)
				{
					PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.LoadFailed, unitId);
					SendLog.LogModule(GoogleAdsLog.Module, "App open ad failed to load: " + error, LogLevel.Error);
					OnAdLoadFailed?.Invoke();
					return;
				}

				if (ad == null)
				{
					PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.LoadFailed, unitId);
					SendLog.LogModule(GoogleAdsLog.Module, "Unexpected error: App open ad load event fired with null ad and null error.", LogLevel.Error);
					return;
				}

				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.LoadSucceeded, unitId);
				OnAdLoaded?.Invoke();
				SendLog.LogModule(GoogleAdsLog.Module, "App open ad loaded with response : " + ad.GetResponseInfo());
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
