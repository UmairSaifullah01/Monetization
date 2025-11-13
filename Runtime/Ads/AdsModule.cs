using System;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;


namespace THEBADDEST.Advertisement
{
	
	public abstract class AdsModule : MonetizationModule, IAdsModule
	{
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
