using System;
using GoogleMobileAds.Api;
using UnityEngine;
using THEBADDEST.MonetizationApi;


namespace THEBADDEST.Advertisement
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
				SendLog.Log("Destroying rewarded ad instance.");
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
				SendLog.Log("Showing rewarded ad.");
				rewardedAd.Show((Reward reward) =>
				{
					SendLog.Log($"Rewarded ad granted a reward: {reward.Amount} {reward.Type}");
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
				SendLog.LogError("Rewarded ad is not ready yet.");
				OnRewardFailed?.Invoke();
			}
		}

		public void Load()
		{
			PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.LoadStarted, unitId);
			var adRequest = new AdRequest();

			RewardedAd.Load(unitId, adRequest, (RewardedAd ad, LoadAdError error) =>
			{
				if (error != null)
				{
					PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.LoadFailed, unitId);
					SendLog.LogError("Rewarded ad failed to load: " + error);
					OnAdLoadFailed?.Invoke();
					OnRewardFailed?.Invoke();
					return;
				}

				if (ad == null)
				{
					PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.LoadFailed, unitId);
					OnRewardFailed?.Invoke();
					SendLog.LogError("Unexpected error: Rewarded load event fired with null ad and null error.");
					return;
				}

				PerformanceMonitor.Instance.RecordAdEvent(AdMetricsTypes.Rewarded, AdEventType.LoadSucceeded, unitId);
				SendLog.Log("Rewarded ad loaded successfully. Response: " + ad.GetResponseInfo());
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
				SendLog.Log("Showing rewarded ad.");
				rewardedAd.Show((Reward reward) =>
				{
					SendLog.Log($"Rewarded ad granted a reward: {reward.Amount} {reward.Type}");
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
				SendLog.LogError("Rewarded ad is not ready yet.");
			}
		}

	}


}
