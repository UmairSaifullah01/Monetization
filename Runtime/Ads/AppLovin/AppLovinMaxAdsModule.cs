using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Ads
{
	public class AppLovinMaxAdsModule : AdsModule
	{
		[SerializeField] private AppLovinMaxSettings settings = new AppLovinMaxSettings();

		private AppLovinMaxAdsService _service;

		protected override async UTask OnInitialize()
		{
			_service = new AppLovinMaxAdsService(settings, Context.Catalog, RaiseAdsSdkReady, ModuleName);
			await _service.InitializeAsync(EnableTestMode);
		}

		public override IAppAd FetchBanner(string placement = "default") => _service?.Banner;

		public override IAppAd FetchInterstitial(string placement = "default") => _service?.Interstitial;

		public override IAppAd FetchInterstitialVideo(string placement = "default") => _service?.InterstitialVideo;

		public override IAppRewardAd FetchRewarded(string placement = "default") => _service?.Rewarded;

		public override IAppAd FetchAppOpen(string placement = "default") => _service?.AppOpen;
	}
}
