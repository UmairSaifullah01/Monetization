using System;
using THEBADDEST.MonetizationApi;


namespace THEBADDEST.MonetizationApi.Ads
{


	public static class AdsModuleExtensions
	{

		public static void ShowBanner(this IAdsModule module, string placement = "default")
		{
			if (module == null) { SendLog.LogWarning("Ads module not available."); return; }
			module.FetchBanner(placement).Show();
		}

		public static void HideBanner(this IAdsModule module, string placement = "default")
		{
			if (module == null) { SendLog.LogWarning("Ads module not available."); return; }
			module.FetchBanner(placement).Hide();
		}

		public static void ShowInterstitial(this IAdsModule module, string placement = "default")
		{
			if (module == null) { SendLog.LogWarning("Ads module not available."); return; }
			module.FetchInterstitial(placement).Show();
		}

		public static void LoadInterstitial(this IAdsModule module, string placement = "default")
		{
			if (module == null) { SendLog.LogWarning("Ads module not available."); return; }
			module.FetchInterstitial(placement).Load();
		}

		public static void ShowRewarded(this IAdsModule module, string placement = "default", Action<object> onRewarded = null, Action onFailed = null)
		{
			if (module == null) { SendLog.LogWarning("Ads module not available."); onFailed?.Invoke(); return; }
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
			if (module == null) { SendLog.LogWarning("Ads module not available."); return; }
			module.FetchRewarded(placement).Load();
		}

	}
	
	public interface IAdsModule : IModule
	{

		event Action<bool> OnSdkReady;

		[Obsolete("Use OnSdkReady. Will be removed in a future version.")]
		event Action<bool> onInitialize;

		bool EnableTestMode { get; }

		float AdLoadTimeout { get; }

		IAppAd FetchBanner(string placement = "default");


		IAppAd FetchInterstitial(string placement = "default");


		IAppAd FetchInterstitialVideo(string placement = "default");


		IAppRewardAd FetchRewarded(string placement = "default");

		IAppAd FetchAppOpen(string placement = "default");

	}


}