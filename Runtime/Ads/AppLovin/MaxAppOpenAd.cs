using System;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;

namespace THEBADDEST.Advertisement
{
	public class MaxAppOpenAd : IAppAd
	{
		public event Action OnAdLoaded;
		public event Action OnAdLoadFailed;
		public event Action<AdValue> OnAdPaid;

		public object ad => null;

		private readonly string _unitId;
		private int _retryAttempt;
		private bool _loadSettled;

		public MaxAppOpenAd(string unitId)
		{
			_unitId = unitId;

			MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnLoaded;
			MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnLoadFailed;
			MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += OnDisplayed;
			MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnHidden;
			MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnDisplayFailed;
			MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnRevenuePaid;
		}

		public void Create()
		{
			Load();
		}

		public void Destroy()
		{
			// MAX app-open is managed by SDK, no explicit destroy.
		}

		public bool IsLoaded()
		{
			return !string.IsNullOrEmpty(_unitId) && MaxSdk.IsAppOpenAdReady(_unitId);
		}

		public void Show()
		{
			if (!IsLoaded())
			{
				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.ShowFailed, _unitId);
				return;
			}

			MaxSdk.ShowAppOpenAd(_unitId);
		}

		public void Load()
		{
			if (string.IsNullOrEmpty(_unitId))
			{
				return;
			}

			_loadSettled = false;
			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.LoadStarted, _unitId);
			AdLoadTimeoutWatcher.Watch(AdMetricsTypes.AppOpen, _unitId, () => _loadSettled);
			MaxSdk.LoadAppOpenAd(_unitId);
		}

		public void Hide()
		{
			// No-op for app-open.
		}

		private void OnLoaded(string adUnitId, MaxSdkBase.AdInfo _)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			_retryAttempt = 0;
			_loadSettled = true;
			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.LoadSucceeded, _unitId);
			OnAdLoaded?.Invoke();
		}

		private void OnLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo error)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			_loadSettled = true;
			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.LoadFailed, _unitId);
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

			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.ShowSucceeded, _unitId);
			EventBus.Publish(new AdShownEvent { AdType = AdMetricsTypes.AppOpen, Placement = _unitId, Time = DateTime.Now });
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

			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.AppOpen, AdEventType.ShowFailed, _unitId);
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
