using System;
using THEBADDEST.MonetizationApi;


namespace THEBADDEST.Advertisement
{
	
	public abstract class AdsModule : MonetizationModule, IAdsModule
	{
        protected Action<bool> initialize;

        public event Action<bool> onInitialize
        {
            add
            {
				initialize += value;
            }
            remove
            {
				initialize -= value;
            }
        }

		public abstract IAppAd FetchBanner(string placement = "default");
		public abstract IAppAd FetchInterstitial(string placement = "default");
		public abstract IAppAd FetchInterstitialVideo(string placement = "default");
		public abstract IAppRewardAd FetchRewarded(string placement = "default");
		public abstract IAppAd FetchAppOpen(string placement = "default");
	}


}
