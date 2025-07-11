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
				EventBus.Publish(new AdShownEvent {
					AdType = "Rewarded",
					Placement = unitId,
					Time = DateTime.Now
				});
			}
			else
			{
				SendLog.LogError("Rewarded ad is not ready yet.");
				OnRewardFailed?.Invoke();
			}
		}

		public void Load()
		{
			var adRequest = new AdRequest();

			// Send the request to load the ad.
			RewardedAd.Load(unitId, adRequest, (RewardedAd ad, LoadAdError error) =>
			{
				// If the operation failed with a reason.
				if (error != null)
				{
					SendLog.LogError("Rewarded ad failed to load: " + error);
					OnAdLoadFailed?.Invoke();
					OnRewardFailed?.Invoke();
					return;
				}
				// If the operation failed for unknown reasons.
				// This is an unexpected error, please report this bug if it happens.
				if (ad == null)
				{
					OnRewardFailed?.Invoke();
					SendLog.LogError("Unexpected error: Rewarded load event fired with null ad and null error.");
					return;
				}

				// The operation completed successfully.
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
			}
			else
			{
				OnRewardFailed?.Invoke();
				SendLog.LogError("Rewarded ad is not ready yet.");
			}
		}

	}


}