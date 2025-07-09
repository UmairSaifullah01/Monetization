using System;
using GoogleMobileAds.Api;
using UnityEngine;


namespace THEBADDEST.Advertisement
{


	public class RewardedVideoAd : IAppRewardAd
	{

		public event Action              OnAdLoaded;
		public event Action OnAdLoadFailed;
		public event Action<AdValue>     OnAdPaid;
		public event Action<object>      OnRewardClaimed;
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
				Debug.Log("Destroying rewarded ad.");
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
				Debug.Log("Showing rewarded ad.");
				rewardedAd.Show((Reward reward) =>
				{
					Debug.Log(String.Format("Rewarded ad granted a reward: {0} {1}",
						reward.Amount,
						reward.Type));
					OnRewardClaimed?.Invoke(reward);
				});
			}
			else
			{
				Debug.LogError("Rewarded ad is not ready yet.");
				
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
					Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
					OnAdLoadFailed?.Invoke();
					return;
				}
				// If the operation failed for unknown reasons.
				// This is an unexpected error, please report this bug if it happens.
				if (ad == null)
				{
					Debug.LogError("Unexpected error: Rewarded load event fired with null ad and null error.");
					return;
				}

				// The operation completed successfully.
				Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
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
				Debug.Log("Showing rewarded ad.");
				rewardedAd.Show((Reward reward) =>
				{
					Debug.Log(String.Format("Rewarded ad granted a reward: {0} {1}",
						reward.Amount,
						reward.Type));
					onRewardClaimed?.Invoke(reward);
				});
			}
			else
			{
				Debug.LogError("Rewarded ad is not ready yet.");
			}
		}

	}


}