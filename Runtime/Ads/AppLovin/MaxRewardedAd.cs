using System;
using THEBADDEST.MonetizationApi;
using THEBADDEST.MonetizationApi.Ads;
using THEBADDEST.Tasks;

namespace THEBADDEST.MonetizationApi.Ads
{
	public class MaxRewardedAd : IAppRewardAd
	{
		public event Action OnAdLoaded;
		public event Action OnAdLoadFailed;
		public event Action<AdValue> OnAdPaid;
		public event Action<object> OnRewardClaimed;
		public event Action OnRewardFailed;

		public object ad => null;

		private readonly string _unitId;
		private int _retryAttempt;
		private bool _loadSettled;

		private bool _waitingForReward;

		public MaxRewardedAd(string unitId)
		{
			_unitId = unitId;

			MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnLoaded;
			MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnLoadFailed;
			MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnDisplayed;
			MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnHidden;
			MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnDisplayFailed;
			MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnReceivedReward;
			MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRevenuePaid;
		}

		public void Create()
		{
			Load();
		}

		public void Destroy()
		{
			// MAX rewarded is managed by SDK, no explicit destroy.
		}

		public bool IsLoaded()
		{
			return !string.IsNullOrEmpty(_unitId) && MaxSdk.IsRewardedAdReady(_unitId);
		}

		public void Show()
		{
			Show(_ => { });
		}

		public void Show(Action<object> onRewardClaimed)
		{
			if (!IsLoaded())
			{
				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.ShowFailed, _unitId);
				OnRewardFailed?.Invoke();
				return;
			}

			_waitingForReward = true;
			void Handler(object reward)
			{
				OnRewardClaimed -= Handler;
				onRewardClaimed?.Invoke(reward);
			}

			OnRewardClaimed += Handler;
			MaxSdk.ShowRewardedAd(_unitId);
		}

		public void Load()
		{
			if (string.IsNullOrEmpty(_unitId))
			{
				return;
			}

			_loadSettled = false;
			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.LoadStarted, _unitId);
			AdLoadTimeoutWatcher.Watch(AdMetricsTypes.Rewarded, _unitId, () => _loadSettled);
			MaxSdk.LoadRewardedAd(_unitId);
		}

		public void Hide()
		{
			// No-op for rewarded.
		}

		private void OnLoaded(string adUnitId, MaxSdkBase.AdInfo _)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			_retryAttempt = 0;
			_loadSettled = true;
			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.LoadSucceeded, _unitId);
			OnAdLoaded?.Invoke();
		}

		private void OnLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo error)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			_loadSettled = true;
			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.LoadFailed, _unitId);
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

			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.ShowSucceeded, _unitId);
			EventBus.Publish(new AdShownEvent { AdType = AdMetricsTypes.Rewarded, Placement = _unitId, Time = DateTime.Now });
		}

		private void OnHidden(string adUnitId, MaxSdkBase.AdInfo _)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			// Critical fix vs ZPlay: do NOT invoke fail on hidden.
			_waitingForReward = false;
			Load();
		}

		private void OnDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo _, MaxSdkBase.AdInfo __)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			_waitingForReward = false;
			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.ShowFailed, _unitId);
			OnRewardFailed?.Invoke();
			Load();
		}

		private void OnReceivedReward(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo _)
		{
			if (adUnitId != _unitId)
			{
				return;
			}

			_waitingForReward = false;
			OnRewardClaimed?.Invoke(reward);
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
