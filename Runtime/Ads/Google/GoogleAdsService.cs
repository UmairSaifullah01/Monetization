using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;
using UnityEngine;

namespace THEBADDEST.Advertisement
{
	public class GoogleAdsSettings
	{
		public BannerData BannerData { get; set; }
		public List<string> TestDeviceIds { get; set; } = new List<string>();
		public string AppId { get; set; }
		public string BannerId { get; set; }
		public string InterstitialId { get; set; }
		public string InterstitialVideoId { get; set; }
		public string RewardedId { get; set; }
		public string AppOpenId { get; set; }
	}

	public class GoogleAdsService
	{
		private readonly GoogleAdsSettings _settings;
		private readonly IPlacementCatalog _catalog;
		private readonly Action<bool> _onSdkReady;
		private readonly string _moduleName;

		private bool _sdkInitComplete;
		private bool _adsSdkInitializationStarted;

		public IAppAd BannerView { get; private set; }
		public IAppAd Interstitial { get; private set; }
		public IAppAd InterstitialVideo { get; private set; }
		public IAppRewardAd RewardedVideo { get; private set; }
		public IAppAd AppOpenAd { get; private set; }

		public bool CanRequestAds => ConsentInformation.CanRequestAds();

		public GoogleAdsService(GoogleAdsSettings settings, IPlacementCatalog catalog, Action<bool> onSdkReady, string moduleName)
		{
			_settings = settings;
			_catalog = catalog;
			_onSdkReady = onSdkReady;
			_moduleName = moduleName;
		}

		public void LoadAdIdsFromJson()
		{
			_settings.AppId = ResolveOrFallback(GoogleAdsModule.APP_ID_KEY, "ca-app-pub-3940256099942544~3347511713");
			_settings.BannerId = ResolveOrFallback(GoogleAdsModule.BANNER_ID_KEY, "ca-app-pub-3940256099942544/6300978111");
			_settings.InterstitialId = ResolveOrFallback(GoogleAdsModule.INTERSTITIAL_ID_KEY, "ca-app-pub-3940256099942544/1033173712");
			_settings.InterstitialVideoId = ResolveOrFallback(GoogleAdsModule.INTERSTITIAL_VIDEO_ID_KEY, "ca-app-pub-3940256099942544/8691691433");
			_settings.RewardedId = ResolveOrFallback(GoogleAdsModule.REWARDED_ID_KEY, "ca-app-pub-3940256099942544/5224354917");
			_settings.AppOpenId = ResolveOrFallback(GoogleAdsModule.APP_OPEN_ID_KEY, "ca-app-pub-3940256099942544/5662855259");

			if (string.IsNullOrEmpty(_settings.BannerData.UnitIdKey))
			{
				var bannerData = _settings.BannerData;
				bannerData.SetUnitIdKey(GoogleAdsModule.BANNER_ID_KEY);
				_settings.BannerData = bannerData;
			}
		}

		private string ResolveOrFallback(string key, string fallback)
		{
			string value = _catalog.Resolve(GoogleAdsModule.AD_KEYS_CATEGORY, key);
			return string.IsNullOrEmpty(value) ? fallback : value;
		}

		public async UTask InitializeAsync(bool enableTestMode)
		{
			LoadAdIdsFromJson();

			if (enableTestMode)
			{
				SendLog.LogModule(_moduleName, "Test mode enabled by MonetizationConfig.");
			}

			SetupAllAds();
			MobileAds.RaiseAdEventsOnUnityMainThread = true;

			_sdkInitComplete = false;
			if (CheckInternetState())
			{
				RequestConsent();
			}
			else
			{
				_onSdkReady?.Invoke(false);
				_sdkInitComplete = true;
			}

			await UTask.WaitUntil(() => _sdkInitComplete);
		}

		private bool CheckInternetState()
		{
			return Application.internetReachability != NetworkReachability.NotReachable;
		}

		private void RequestConsent()
		{
#if UNITY_EDITOR
			InitializeAds();
			return;
#endif
			ConsentRequestParameters request = new ConsentRequestParameters
			{
				TagForUnderAgeOfConsent = false,
				ConsentDebugSettings = new ConsentDebugSettings
				{
					DebugGeography = DebugGeography.EEA,
					TestDeviceHashedIds = _settings.TestDeviceIds
				}
			};
			ConsentInformation.Update(request, OnConsentInfoUpdated);
		}

		private void OnConsentInfoUpdated(FormError consentError)
		{
			if (consentError != null)
			{
				InitializeAds();
				SendLog.LogModule(_moduleName, $"Consent error: {consentError.Message}", LogLevel.Error);
				return;
			}

			if (ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required &&
			    ConsentInformation.ConsentStatus != ConsentStatus.Obtained)
			{
				SendLog.LogModule(_moduleName, "Obtaining Consent...");
				ConsentForm.LoadAndShowConsentFormIfRequired(_ =>
				{
					PlayerPrefs.SetString("isPolicyAgreed", "yes");
					InitializeAds();
				});
			}
			else
			{
				InitializeAds();
				SendLog.LogModule(_moduleName, "Consent Not Required");
			}
		}

		private void InitializeAds()
		{
			if (_adsSdkInitializationStarted)
			{
				SendLog.LogModule(_moduleName, "MobileAds.Initialize already started; skipping duplicate call.", LogLevel.Warning);
				return;
			}

			_adsSdkInitializationStarted = true;
			SendLog.LogModule(_moduleName, $"Initializing MobileAds (AppId={_settings.AppId}).");
			MobileAds.Initialize(OnInitComplete);
		}

		private void OnInitComplete(InitializationStatus status)
		{
			if (_sdkInitComplete)
			{
				SendLog.LogModule(_moduleName, "Duplicate MobileAds init callback received; ignoring.", LogLevel.Warning);
				return;
			}

			bool failed = status == null;
			_onSdkReady?.Invoke(!failed);
			SendLog.LogModule(_moduleName, failed ? "Error Initialization Status..." : "Initialization Status... Success");
			_sdkInitComplete = true;
		}

		private void SetupAllAds()
		{
			if (string.IsNullOrEmpty(_settings.BannerData.UnitIdKey))
			{
				var bannerData = _settings.BannerData;
				bannerData.SetUnitIdKey(GoogleAdsModule.BANNER_ID_KEY);
				_settings.BannerData = bannerData;
			}

			BannerView = new BannerAd(_settings.BannerData);
			Interstitial = new Interstitial_Ad(_settings.InterstitialId, AdMetricsTypes.Interstitial);
			InterstitialVideo = new Interstitial_Ad(_settings.InterstitialVideoId, AdMetricsTypes.InterstitialVideo);
			RewardedVideo = new RewardedVideoAd(_settings.RewardedId);
			AppOpenAd = new AppOpenAdGoogle(_settings.AppOpenId);
		}
	}
}
