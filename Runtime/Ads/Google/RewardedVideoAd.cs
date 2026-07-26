using System;
using GoogleMobileAds.Api;
using UnityEngine;
using THEBADDEST.MonetizationApi;
using THEBADDEST.MonetizationApi.Ads;


namespace THEBADDEST.MonetizationApi.Ads
{


	public class RewardedVideoAd : IAppRewardAd
	{

		public event Action              OnAdLoaded;
		public event Action OnAdLoadFailed;
		public event Action<AdValue>     OnAdPaid;
		public event Action<object>      OnRewardClaimed;
		public event Action      OnRewardFailed;
		public RewardedAd                ad => rewardedAd;
		object IAppAd.                   ad => ad;
		private RewardedAd               rewardedAd;
		
		
		string                unitId;
		bool                  _loadSettled;

		public RewardedVideoAd(string unitId)
		{
			this.unitId = unitId;
		}
		
		public void Create()
		{
			if(rewardedAd==null){Load();}
		}

		public void Destroy()
		{
			if (rewardedAd != null)
			{
				SendLog.LogModule(GoogleAdsLog.Module, "Destroying rewarded ad instance.");
				rewardedAd.Destroy();
				rewardedAd = null;
			}
		}

		public bool IsLoaded()
		{
			return rewardedAd != null && rewardedAd.CanShowAd();
		}

		public void Show()
		{
			if (rewardedAd != null && rewardedAd.CanShowAd())
			{
				SendLog.LogModule(GoogleAdsLog.Module, "Showing rewarded ad.");
				rewardedAd.Show((Reward reward) =>
				{
					SendLog.LogModule(GoogleAdsLog.Module, $"Rewarded ad granted a reward: {reward.Amount} {reward.Type}");
					OnRewardClaimed?.Invoke(reward);
				});
				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.ShowSucceeded, unitId);
				EventBus.Publish(new AdShownEvent {
					AdType = AdMetricsTypes.Rewarded,
					Placement = unitId,
					Time = DateTime.Now
				});
			}
			else
			{
				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.ShowFailed, unitId);
				SendLog.LogModule(GoogleAdsLog.Module, "Rewarded ad is not ready yet.", LogLevel.Error);
				OnRewardFailed?.Invoke();
			}
		}

		public void Load()
		{
			_loadSettled = false;
			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.LoadStarted, unitId);
			AdLoadTimeoutWatcher.Watch(AdMetricsTypes.Rewarded, unitId, () => _loadSettled);
			var adRequest = new AdRequest();

			RewardedAd.Load(unitId, adRequest, (RewardedAd ad, LoadAdError error) =>
			{
				_loadSettled = true;
				if (error != null)
				{
					PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.LoadFailed, unitId);
					SendLog.LogModule(GoogleAdsLog.Module, "Rewarded ad failed to load: " + error, LogLevel.Error);
					OnAdLoadFailed?.Invoke();
					OnRewardFailed?.Invoke();
					return;
				}

				if (ad == null)
				{
					PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.LoadFailed, unitId);
					OnRewardFailed?.Invoke();
					SendLog.LogModule(GoogleAdsLog.Module, "Unexpected error: Rewarded load event fired with null ad and null error.", LogLevel.Error);
					return;
				}

				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.LoadSucceeded, unitId);
				SendLog.LogModule(GoogleAdsLog.Module, "Rewarded ad loaded successfully. Response: " + ad.GetResponseInfo());
				rewardedAd = ad;
				
			});
		}

		public void Hide()
		{
			Destroy();
		}

		public void Show(Action<object> onRewardClaimed)
		{
			if (rewardedAd != null && rewardedAd.CanShowAd())
			{
				SendLog.LogModule(GoogleAdsLog.Module, "Showing rewarded ad.");
				rewardedAd.Show((Reward reward) =>
				{
					SendLog.LogModule(GoogleAdsLog.Module, $"Rewarded ad granted a reward: {reward.Amount} {reward.Type}");
					onRewardClaimed?.Invoke(reward);
				});
				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.ShowSucceeded, unitId);
				EventBus.Publish(new AdShownEvent {
					AdType = AdMetricsTypes.Rewarded,
					Placement = unitId,
					Time = DateTime.Now
				});
			}
			else
			{
				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.ShowFailed, unitId);
				OnRewardFailed?.Invoke();
				SendLog.LogModule(GoogleAdsLog.Module, "Rewarded ad is not ready yet.", LogLevel.Error);
			}
		}

	}


}
