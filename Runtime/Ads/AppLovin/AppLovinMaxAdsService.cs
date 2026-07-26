using System;
using THEBADDEST.MonetizationApi;
using THEBADDEST.MonetizationApi.Ads;
using THEBADDEST.Tasks;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Ads
{
	public class AppLovinMaxAdsService
	{
		private readonly AppLovinMaxSettings _settings;
		private readonly IKeyValueCatalog _catalog;
		private readonly Action<bool> _onSdkReady;
		private readonly string _moduleName;

		private bool _sdkInitComplete;
		private bool _sdkInitStarted;

		public IAppAd Banner { get; private set; }
		public IAppAd Interstitial { get; private set; }
		public IAppAd InterstitialVideo { get; private set; }
		public IAppRewardAd Rewarded { get; private set; }
		public IAppAd AppOpen { get; private set; }

		public AppLovinMaxAdsService(AppLovinMaxSettings settings, IKeyValueCatalog catalog, Action<bool> onSdkReady, string moduleName)
		{
			_settings = settings;
			_catalog = catalog;
			_onSdkReady = onSdkReady;
			_moduleName = moduleName;
		}

		public async UTask InitializeAsync(bool enableTestMode)
		{
			if (_sdkInitStarted)
			{
				SendLog.LogModule(_moduleName, "MaxSdk.InitializeSdk already started; skipping duplicate call.", LogLevel.Warning);
				return;
			}

			_sdkInitStarted = true;
			_sdkInitComplete = false;

			string sdkKey = ResolveOrEmpty(AppLovinMaxSettings.SDK_KEY);
			if (string.IsNullOrEmpty(sdkKey))
			{
				SendLog.LogModule(_moduleName, "MaxSdkKey not found in AdKeys JSON.", LogLevel.Error);
				_onSdkReady?.Invoke(false);
				_sdkInitComplete = true;
				return;
			}

			// MAX docs: set SDK key before init.
			MaxSdk.SetSdkKey(sdkKey);
			MaxSdk.SetVerboseLogging(enableTestMode);
			MaxSdk.SetHasUserConsent(true);

			SetupAllAds();

			MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitialized;
			MaxSdk.InitializeSdk();

			await UTask.WaitUntil(() => _sdkInitComplete);
		}

		private void OnSdkInitialized(MaxSdkBase.SdkConfiguration _)
		{
			MaxSdkCallbacks.OnSdkInitializedEvent -= OnSdkInitialized;

			_onSdkReady?.Invoke(true);
			_sdkInitComplete = true;
			SendLog.LogModule(_moduleName, "MAX SDK initialized successfully.");
		}

		private void SetupAllAds()
		{
			string bannerId = ResolveOrEmpty(AppLovinMaxSettings.BANNER_ID_KEY);
			string interstitialId = ResolveOrEmpty(AppLovinMaxSettings.INTERSTITIAL_ID_KEY);
			string interstitialVideoId = ResolveOrEmpty(AppLovinMaxSettings.INTERSTITIAL_VIDEO_ID_KEY);
			string rewardedId = ResolveOrEmpty(AppLovinMaxSettings.REWARDED_ID_KEY);
			string appOpenId = ResolveOrEmpty(AppLovinMaxSettings.APP_OPEN_ID_KEY);

			Banner = new MaxBannerAd(bannerId, _settings.BannerPosition, _settings.EnableAdaptiveBanner);
			Interstitial = new MaxInterstitialAd(interstitialId, AdMetricsTypes.Interstitial);
			InterstitialVideo = new MaxInterstitialAd(interstitialVideoId, AdMetricsTypes.InterstitialVideo);
			Rewarded = new MaxRewardedAd(rewardedId);
			AppOpen = new MaxAppOpenAd(appOpenId);
		}

		private string ResolveOrEmpty(string key)
		{
			return _catalog.Resolve(_settings.Category, key) ?? string.Empty;
		}
	}
}
