using System;
using THEBADDEST.MonetizationApi;
using THEBADDEST.MonetizationApi.Ads;

namespace THEBADDEST.MonetizationApi.Ads
{
	public class MaxBannerAd : IAppAd
	{
		public event Action OnAdLoaded;
		public event Action OnAdLoadFailed;
		public event Action<AdValue> OnAdPaid;

		public object ad => null;

		private readonly string _unitId;
		private readonly MaxSdkBase.BannerPosition _position;
		private readonly bool _adaptive;

		private bool _created;

		public MaxBannerAd(string unitId, MaxSdkBase.BannerPosition position, bool adaptive)
		{
			_unitId = unitId;
			_position = position;
			_adaptive = adaptive;

			MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnLoaded;
			MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnLoadFailed;
			MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnRevenuePaid;
		}

		public void Create()
		{
			if (string.IsNullOrEmpty(_unitId))
			{
				SendLog.LogModule("AppLovinMaxAdsModule", "Banner ad unit id is empty.", LogLevel.Warning);
				return;
			}

			if (_created)
			{
				return;
			}

			_created = true;
			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Banner, AdEventType.LoadStarted, _unitId);

			MaxSdk.CreateBanner(_unitId, _position);
			MaxSdk.SetBannerExtraParameter(_unitId, "adaptive_banner", _adaptive ? "true" : "false");
			MaxSdk.LoadBanner(_unitId);

			AdLoadTimeoutWatcher.Watch(AdMetricsTypes.Banner, _unitId, () => IsLoaded());
		}

		public void Destroy()
		{
			if (!_created || string.IsNullOrEmpty(_unitId))
			{
				return;
			}

			MaxSdk.DestroyBanner(_unitId);
			_created = false;
		}

		public bool IsLoaded()
		{
			return _created && !string.IsNullOrEmpty(_unitId) && MaxSdk.IsBannerReady(_unitId);
		}

		public void Show()
		{
			if (!IsLoaded())
			{
				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Banner, AdEventType.ShowFailed, _unitId);
				return;
			}

			MaxSdk.ShowBanner(_unitId);
			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Banner, AdEventType.ShowSucceeded, _unitId);
			EventBus.Publish(new AdShownEvent { AdType = AdMetricsTypes.Banner, Placement = _unitId, Time = DateTime.Now });
		}

		public void Load()
		{
			Create();
		}

		public void Hide()
		{
			if (!_created || string.IsNullOrEmpty(_unitId))
			{
				return;
			}

			MaxSdk.HideBanner(_unitId);
		}

		private void OnLoaded(string adUnitId, MaxSdkBase.AdInfo _)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Banner, AdEventType.LoadSucceeded, _unitId);
			OnAdLoaded?.Invoke();
		}

		private void OnLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo error)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Banner, AdEventType.LoadFailed, _unitId);
			SendLog.LogModule("AppLovinMaxAdsModule", $"Banner failed to load: {error?.Message}", LogLevel.Warning);
			OnAdLoadFailed?.Invoke();
		}

		private void OnRevenuePaid(string adUnitId, MaxSdkBase.AdInfo adInfo)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			var adValue = new AdValue
			{
				Value = (long)(adInfo.Revenue * 1000000d),
				CurrencyCode = "USD",
				Precision = AdValue.PrecisionType.Estimated
			};
			OnAdPaid?.Invoke(adValue);
		}
	}
}
