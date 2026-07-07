using System;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;

namespace THEBADDEST.Advertisement
{
	public class MaxInterstitialAd : IAppAd
	{
		public event Action OnAdLoaded;
		public event Action OnAdLoadFailed;
		public event Action<AdValue> OnAdPaid;

		public object ad => null;

		private readonly string _unitId;
		private readonly string _adType;

		private int _retryAttempt;
		private bool _loadSettled;

		public MaxInterstitialAd(string unitId, string adType)
		{
			_unitId = unitId;
			_adType = adType;

			MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnLoaded;
			MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnLoadFailed;
			MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnDisplayed;
			MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnHidden;
			MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnDisplayFailed;
			MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnRevenuePaid;
		}

		public void Create()
		{
			Load();
		}

		public void Destroy()
		{
			// MAX interstitials are managed by SDK, no explicit destroy.
		}

		public bool IsLoaded()
		{
			return !string.IsNullOrEmpty(_unitId) && MaxSdk.IsInterstitialReady(_unitId);
		}

		public void Show()
		{
			if (!IsLoaded())
			{
				PerformanceMonitor.Instance.RecordAdEvent(_adType, AdEventType.ShowFailed, _unitId);
				return;
			}

			MaxSdk.ShowInterstitial(_unitId);
		}

		public void Load()
		{
			if (string.IsNullOrEmpty(_unitId))
			{
				return;
			}

			_loadSettled = false;
			PerformanceMonitor.Instance.RecordAdEvent(_adType, AdEventType.LoadStarted, _unitId);
			AdLoadTimeoutWatcher.Watch(_adType, _unitId, () => _loadSettled);

			MaxSdk.LoadInterstitial(_unitId);
		}

		public void Hide()
		{
			// No-op for interstitial.
		}

		private void OnLoaded(string adUnitId, MaxSdkBase.AdInfo _)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			_retryAttempt = 0;
			_loadSettled = true;
			PerformanceMonitor.Instance.RecordAdEvent(_adType, AdEventType.LoadSucceeded, _unitId);
			OnAdLoaded?.Invoke();
		}

		private void OnLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo error)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			_loadSettled = true;
			PerformanceMonitor.Instance.RecordAdEvent(_adType, AdEventType.LoadFailed, _unitId);
			OnAdLoadFailed?.Invoke();

			_retryAttempt++;
			double retryDelay = System.Math.Pow(2, System.Math.Min(6, _retryAttempt));
			RetryLoadAfterDelay((float)retryDelay).Forget();
		}

		private void OnDisplayed(string adUnitId, MaxSdkBase.AdInfo _)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			PerformanceMonitor.Instance.RecordAdEvent(_adType, AdEventType.ShowSucceeded, _unitId);
			EventBus.Publish(new AdShownEvent { AdType = _adType, Placement = _unitId, Time = DateTime.Now });
		}

		private void OnHidden(string adUnitId, MaxSdkBase.AdInfo _)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			Load();
		}

		private void OnDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo _, MaxSdkBase.AdInfo __)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			PerformanceMonitor.Instance.RecordAdEvent(_adType, AdEventType.ShowFailed, _unitId);
			Load();
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

		private async UTask RetryLoadAfterDelay(float seconds)
		{
			await UTask.Delay(seconds);
			Load();
		}
	}
}
