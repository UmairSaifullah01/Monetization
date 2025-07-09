using System;


namespace THEBADDEST.Advertisement
{


	public interface IAdsModule
	{

		event Action<bool> OnInitialize;
		
		void   Init();
		IAppAd FetchBanner(string placement = "default");


		IAppAd FetchInterstitial(string placement = "default");


		IAppAd FetchInterstitialVideo(string placement = "default");


		IAppRewardAd FetchRewarded(string placement = "default");

		IAppAd FetchAppOpen(string placement = "default");

	}


}