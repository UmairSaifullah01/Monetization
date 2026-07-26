using System;
using GoogleMobileAds.Api;
using UnityEngine;
using THEBADDEST.MonetizationApi;
using THEBADDEST.MonetizationApi.Ads;


namespace THEBADDEST.MonetizationApi.Ads
{


	public class BannerAd : IAppAd
	{

		public event Action OnAdLoaded;
		public event Action OnAdLoadFailed;
		public event Action<AdValue> OnAdPaid;

		public BannerView ad => bannerView;
		object IAppAd.ad => ad;
		BannerView bannerView;
		BannerData bannerData;
		bool isDisplaying = false;

		public BannerAd(BannerData bannerData)
		{
			this.bannerData = bannerData;
			isDisplaying = false;
		}

		public void Create()
		{
			if (bannerView != null)
			{
				Destroy();
			}

			bannerView = new BannerView(bannerData.unitId, bannerData.size, bannerData.position);
			bannerView.OnBannerAdLoaded += () =>
			{
				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Banner, AdEventType.LoadSucceeded, bannerData.unitId);
				OnAdLoaded?.Invoke();
			};
			bannerView.OnAdPaid += info =>
			{
				AdValue adValue = new AdValue { Value = info.Value, CurrencyCode = info.CurrencyCode, Precision = (AdValue.PrecisionType)(int)info.Precision };
				OnAdPaid?.Invoke(adValue);
			};
			bannerView.OnBannerAdLoadFailed += error =>
			{
				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Banner, AdEventType.LoadFailed, bannerData.unitId);
				OnAdLoadFailed?.Invoke();
			};
		}

		public void Destroy()
		{
			if (bannerView != null)
			{
				bannerView.Destroy();
				bannerView = null;
			}
		}

		public bool IsLoaded()
		{
			return bannerView != null;
		}

		public void Show()
		{
			if (bannerView != null && !isDisplaying)
			{
				SendLog.LogModule(GoogleAdsLog.Module, "Showing banner ad.");
				bannerView.Show();
				isDisplaying = true;
				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Banner, AdEventType.ShowSucceeded, bannerData.unitId);
				EventBus.Publish(new AdShownEvent
				{
					AdType = AdMetricsTypes.Banner,
					Placement = bannerData.unitId,
					Time = DateTime.Now
				});
			}
			else
			{
				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Banner, AdEventType.ShowFailed, bannerData.unitId);
			}
		}

		public void Load()
		{
			if (bannerView == null)
			{
				Create();
			}

			if (Monetization.TryGetModule<IAdsModule>(out var ads) && ads.EnableTestMode)
			{
				SendLog.LogModule(GoogleAdsLog.Module, "Test mode enabled.");
			}

			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Banner, AdEventType.LoadStarted, bannerData.unitId);
			var adRequest = new AdRequest();
			bannerView?.LoadAd(adRequest);
		}

		public void Hide()
		{
			if (bannerView != null)
			{
				bannerView.Hide();
				isDisplaying = false;
			}
		}

	}


}
