using System;
using THEBADDEST.Monetization;
using THEBADDEST.Tasks;
using UnityEngine;


namespace THEBADDEST.Advertisement
{


	[CreateAssetMenu(menuName = "Monotization/AdsModule", fileName = "AdsModule", order = 10)]
	public abstract class AdsModule : MonetizationModule, IAdsModule
	{
		protected bool isInitialized = false;
		public override async UTask Initialize()
		{
			Init();
			await UTask.WaitUntil( () => isInitialized);
		}

		public event Action<bool> OnInitialize;

		public abstract void Init();
		public abstract IAppAd FetchBanner(string placement = "default");
		public abstract IAppAd FetchInterstitial(string placement = "default");
		public abstract IAppAd FetchInterstitialVideo(string placement = "default");
		public abstract IAppRewardAd FetchRewarded(string placement = "default");
		public abstract IAppAd FetchAppOpen(string placement = "default");

	}


}
