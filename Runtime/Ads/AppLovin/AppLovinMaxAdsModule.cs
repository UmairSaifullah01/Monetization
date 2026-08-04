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

		protected override void OnUpdateModule()
		{
			base.OnUpdateModule();

			var catalog = Context?.Catalog ?? CatalogFactory.Create();
			string sdkKey = catalog?.Resolve(settings.Category, AppLovinMaxSettings.SDK_KEY);
			string adMobAppId = catalog?.Resolve(settings.Category, AppLovinMaxSettings.APP_ID_KEY);

#if UNITY_EDITOR
			AppLovinSettingsSync.ApplySettings(sdkKey, adMobAppId, ModuleName);
#else
			if (string.IsNullOrEmpty(sdkKey))
			{
				SendLog.LogModule(ModuleName, "MaxSdkKey not found in AdKeys JSON.", LogLevel.Warning);
			}

			if (string.IsNullOrEmpty(adMobAppId))
			{
				SendLog.LogModule(ModuleName, "AdMob AppId not found in AdKeys JSON.", LogLevel.Warning);
			}
#endif
		}

		public override IAppAd FetchBanner(string placement = "default") => _service?.Banner;

		public override IAppAd FetchInterstitial(string placement = "default") => _service?.Interstitial;

		public override IAppAd FetchInterstitialVideo(string placement = "default") => _service?.InterstitialVideo;

		public override IAppRewardAd FetchRewarded(string placement = "default") => _service?.Rewarded;

		public override IAppAd FetchAppOpen(string placement = "default") => _service?.AppOpen;
	}
}
