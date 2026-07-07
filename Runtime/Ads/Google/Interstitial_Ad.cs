using System;
using GoogleMobileAds.Api;
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
		string                adType;
		bool                  _loadSettled;

		public Interstitial_Ad(string unitId, string adType = AdMetricsTypes.Interstitial)
		{
			this.unitId = unitId;
			this.adType = adType;
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
				PerformanceMonitor.Instance.RecordAdEvent(adType, AdEventType.ShowSucceeded, unitId);
				EventBus.Publish(new AdShownEvent {
					AdType = adType,
					Placement = unitId,
					Time = DateTime.Now
				});
			}
			else
			{
				PerformanceMonitor.Instance.RecordAdEvent(adType, AdEventType.ShowFailed, unitId);
				SendLog.LogModule(GoogleAdsLog.Module, "Interstitial ad is not ready yet.", LogLevel.Error);
			}
		}

		public void Load()
		{
			_loadSettled = false;
			PerformanceMonitor.Instance.RecordAdEvent(adType, AdEventType.LoadStarted, unitId);
			AdLoadTimeoutWatcher.Watch(adType, unitId, () => _loadSettled);
			var adRequest = new AdRequest();
			InterstitialAd.Load(unitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
			{
				_loadSettled = true;
				if (error != null)
				{
					PerformanceMonitor.Instance.RecordAdEvent(adType, AdEventType.LoadFailed, unitId);
					SendLog.LogModule(GoogleAdsLog.Module, "Interstitial ad failed to load: " + error, LogLevel.Error);
					OnAdLoadFailed?.Invoke();
					return;
				}

				if (ad == null)
				{
					PerformanceMonitor.Instance.RecordAdEvent(adType, AdEventType.LoadFailed, unitId);
					SendLog.LogModule(GoogleAdsLog.Module, "Unexpected error: Interstitial load event fired with null ad and null error.", LogLevel.Error);
					return;
				}

				PerformanceMonitor.Instance.RecordAdEvent(adType, AdEventType.LoadSucceeded, unitId);
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
