using System;
using THEBADDEST.MonetizationApi;


namespace THEBADDEST.Advertisement
{


	public static class AdsModuleExtensions
	{

		public static void ShowBanner(this IAdsModule module, string placement = "default")
		{
			module.FetchBanner(placement).Show();
		}

		public static void HideBanner(this IAdsModule module, string placement = "default")
		{
			module.FetchBanner(placement).Hide();
		}

		public static void ShowInterstitial(this IAdsModule module, string placement = "default")
		{
			module.FetchInterstitial(placement).Show();
		}

		public static void LoadInterstitial(this IAdsModule module, string placement = "default")
		{
			module.FetchInterstitial(placement).Load();
		}

		public static void ShowRewarded(this IAdsModule module, string placement = "default", Action<object> onRewarded = null, Action onFailed = null)
		{
			IAppRewardAd appRewardAd = module.FetchRewarded(placement);
			appRewardAd.OnAdLoadFailed+= OnAdLoadFailed;
			void OnAdLoadFailed()
			{
				appRewardAd.OnAdLoadFailed -= OnAdLoadFailed;
				onFailed?.Invoke();
			}
			appRewardAd.Show(x =>
			{
				onRewarded?.Invoke(x);
				appRewardAd.OnAdLoadFailed -= OnAdLoadFailed;
			});
			
		}
		
		public static void LoadRewarded(this IAdsModule module, string placement = "default")
		{
			module.FetchRewarded(placement).Load();
		}

	}
	
	public interface IAdsModule : IModule
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