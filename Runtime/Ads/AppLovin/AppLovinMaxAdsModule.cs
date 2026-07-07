using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;
using UnityEngine;

namespace THEBADDEST.Advertisement
{
	public class AppLovinMaxAdsModule : AdsModule
	{
		private const string ModuleLogName = "AppLovinMaxAdsModule";

		[SerializeField] private AppLovinMaxSettings settings = new AppLovinMaxSettings();

		private AppLovinMaxAdsService _service;

		protected override async UTask OnInitialize()
		{
			var configAsset = MonetizationConfig.Instance;
			if (!configAsset.EnableAds)
			{
				SendLog.LogModule(ModuleName, "Ads are disabled by MonetizationConfig.", LogLevel.Warning);
				return;
			}

			_service = new AppLovinMaxAdsService(settings, new JsonPlacementCatalog(), RaiseAdsSdkReady, ModuleLogName);
			await _service.InitializeAsync(configAsset.EnableTestMode);
		}

		public override IAppAd FetchBanner(string placement = "default") => _service?.Banner;

		public override IAppAd FetchInterstitial(string placement = "default") => _service?.Interstitial;

		public override IAppAd FetchInterstitialVideo(string placement = "default") => _service?.InterstitialVideo;

		public override IAppRewardAd FetchRewarded(string placement = "default") => _service?.Rewarded;

		public override IAppAd FetchAppOpen(string placement = "default") => _service?.AppOpen;
	}
}
