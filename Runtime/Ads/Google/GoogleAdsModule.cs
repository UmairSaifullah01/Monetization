using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using THEBADDEST.Tasks;
using UnityEngine;
using THEBADDEST.MonetizationApi;


namespace THEBADDEST.Advertisement
{


	public enum AdSizeType
	{

		Banner = 0,
		MediumRectangle = 1,
		IABBanner = 2,
		Leaderboard = 3,
		SmartBanner = 4,
		AdaptiveBanner = 5,

	}

	[Serializable]
	public struct BannerData
	{
		private const string AD_KEYS_CATEGORY = "AdKeys";

		[SerializeField] private string m_UnitId;
		[SerializeField] private AdSizeType m_Type;
		[SerializeField] private AdPosition m_Position;

		public string unitId => GetUnitId();
		public AdSize size => ConvertTypeToSize();
		public AdPosition position => m_Position;

		private string GetUnitId()
		{
			// If m_UnitId is null or empty, return empty
			if (string.IsNullOrEmpty(m_UnitId))
			{
				return m_UnitId ?? string.Empty;
			}

			// Ensure JsonDataUtility is loaded
			JsonDataUtility.LoadData();

			// Try to load from JSON first - m_UnitId might be a key like "BannerTop" or "BannerBottom"
			string jsonBannerId = JsonDataUtility.GetData(AD_KEYS_CATEGORY, m_UnitId);
			if (!string.IsNullOrEmpty(jsonBannerId))
			{
				return jsonBannerId;
			}

			// Fallback to the serialized value if it's already an ad unit ID (starts with "ca-app-pub-")
			return m_UnitId;
		}

		private AdSize ConvertTypeToSize()
		{
			switch (m_Type)
			{
				case AdSizeType.Banner:
					return AdSize.Banner;

				case AdSizeType.MediumRectangle:
					return AdSize.MediumRectangle;

				case AdSizeType.IABBanner:
					return AdSize.IABBanner;

				case AdSizeType.Leaderboard:
					return AdSize.Leaderboard;

				case AdSizeType.AdaptiveBanner:
					return AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
			}

			return AdSize.Banner;
		}

	}


	public class GoogleAdsModule : AdsModule
	{

		public const string AD_KEYS_CATEGORY = "AdKeys";

		// Fixed key names for JSON lookup
		private const string APP_ID_KEY = "AppId";
		private const string BANNER_ID_KEY = "BannerTop";
		private const string INTERSTITIAL_ID_KEY = "Interstitial";
		private const string INTERSTITIAL_VIDEO_ID_KEY = "InterstitialVideo";
		private const string REWARDED_ID_KEY = "Rewarded";
		private const string APP_OPEN_ID_KEY = "AppOpen";

		
		[Tooltip("Shows all available Ad IDs from JSON.")]
		[JsonDataCategory(AD_KEYS_CATEGORY)]
		[SerializeField] private string adKeysReference = "";

		[Header("Banner Settings")]
		[SerializeField] private BannerData bannerData = new BannerData();

		[Header("Test Devices")]
		[SerializeField]
		private List<string> testDeviceIds = new List<string>()
		{
			AdRequest.TestDeviceSimulator,
#if UNITY_IPHONE
            "96e23e80653bb28980d3f40beb58915c",
#elif UNITY_ANDROID
			"702815ACFC14FF222DA1DC767672A573"
#endif
		};

		// Cached loaded IDs
		private string _cachedAppId;
		private string _cachedBannerId;
		private string _cachedInterstitialId;
		private string _cachedInterstitialVideoId;
		private string _cachedRewardedId;
		private string _cachedAppOpenId;

		IAppAd bannerView;
		IAppAd interstitial;
		IAppAd interstitialVideo;
		IAppRewardAd rewardedVideo;
		IAppAd appOpenAd;

		private bool _sdkInitComplete;

		public bool CanRequestAds => ConsentInformation.CanRequestAds();

		private void LoadAdIdsFromJson()
		{
			// Ensure JsonDataUtility is loaded
			JsonDataUtility.LoadData();

			// Load App ID
			_cachedAppId = JsonDataUtility.GetData(AD_KEYS_CATEGORY, APP_ID_KEY);
			if (string.IsNullOrEmpty(_cachedAppId))
			{
				_cachedAppId = "ca-app-pub-3940256099942544~3347511713"; // Fallback to test ID
			}

			// Load Banner ID
			_cachedBannerId = JsonDataUtility.GetData(AD_KEYS_CATEGORY, BANNER_ID_KEY);
			if (string.IsNullOrEmpty(_cachedBannerId))
			{
				_cachedBannerId = "ca-app-pub-3940256099942544/6300978111"; // Fallback to test ID
			}

			// Load Interstitial ID
			_cachedInterstitialId = JsonDataUtility.GetData(AD_KEYS_CATEGORY, INTERSTITIAL_ID_KEY);
			if (string.IsNullOrEmpty(_cachedInterstitialId))
			{
				_cachedInterstitialId = "ca-app-pub-3940256099942544/1033173712"; // Fallback to test ID
			}

			// Load Interstitial Video ID
			_cachedInterstitialVideoId = JsonDataUtility.GetData(AD_KEYS_CATEGORY, INTERSTITIAL_VIDEO_ID_KEY);
			if (string.IsNullOrEmpty(_cachedInterstitialVideoId))
			{
				_cachedInterstitialVideoId = "ca-app-pub-3940256099942544/8691691433"; // Fallback to test ID
			}

			// Load Rewarded ID
			_cachedRewardedId = JsonDataUtility.GetData(AD_KEYS_CATEGORY, REWARDED_ID_KEY);
			if (string.IsNullOrEmpty(_cachedRewardedId))
			{
				_cachedRewardedId = "ca-app-pub-3940256099942544/5224354917"; // Fallback to test ID
			}

			// Load App Open ID
			_cachedAppOpenId = JsonDataUtility.GetData(AD_KEYS_CATEGORY, APP_OPEN_ID_KEY);
			if (string.IsNullOrEmpty(_cachedAppOpenId))
			{
				_cachedAppOpenId = "ca-app-pub-3940256099942544/5662855259"; // Fallback to test ID
			}

			// Update banner data with loaded key if not already set
			if (string.IsNullOrEmpty(GetBannerDataUnitId()))
			{
				SetBannerDataUnitId(BANNER_ID_KEY);
			}
		}

		private string GetBannerDataUnitId()
		{
			var field = typeof(BannerData).GetField("m_UnitId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			return field?.GetValue(bannerData) as string ?? "";
		}

		private void SetBannerDataUnitId(string unitId)
		{
			var field = typeof(BannerData).GetField("m_UnitId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			if (field != null)
			{
				var bannerDataCopy = bannerData;
				field.SetValue(bannerDataCopy, unitId);
				bannerData = bannerDataCopy;
			}
		}

		protected override async UTask OnInitialize()
		{
			var configAsset = MonetizationConfig.Instance;
			if (!configAsset.EnableAds)
			{
				SendLog.LogWarning("[Ads] Ads are disabled by MonetizationConfig.");
				return;
			}

			LoadAdIdsFromJson();

			if (configAsset.EnableTestMode)
			{
				SendLog.LogInfo("[Ads] Test mode enabled by MonetizationConfig.");
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
				initialize?.Invoke(false);
				_sdkInitComplete = true;
			}

			await UTask.WaitUntil(() => _sdkInitComplete);
		}


		bool CheckInternetState()
		{
			return Application.internetReachability != NetworkReachability.NotReachable;
		}

		void RequestConsent()
		{
#if UNITY_EDITOR
			InitializeAds();
			return;
#endif
			ConsentRequestParameters request = new ConsentRequestParameters { TagForUnderAgeOfConsent = false, ConsentDebugSettings = new ConsentDebugSettings() { DebugGeography = DebugGeography.EEA, TestDeviceHashedIds = testDeviceIds }, };
			ConsentInformation.Update(request, OnConsentInfoUpdated);
		}

		void OnConsentInfoUpdated(FormError consentError)
		{
			if (consentError != null)
			{
				InitializeAds();
				SendLog.LogError($"Consent error: {consentError.Message}");
				return;
			}

			if (ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required && ConsentInformation.ConsentStatus != GoogleMobileAds.Ump.Api.ConsentStatus.Obtained)
			{
				SendLog.Log("Obtaining Consent...");
				ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
				{
					if (formError == null)
					{
						PlayerPrefs.SetString("isPolicyAgreed", "yes");
					}

					InitializeAds();
				});
			}
			else
			{
				InitializeAds();
				SendLog.Log("Consent Not Required");
			}
		}

		void InitializeAds()
		{
			// Note: App ID should be configured in GoogleMobileAdsSettings asset
			// The AppId from JSON is available via _cachedAppId if needed
			MobileAds.Initialize(OnInitComplete);
		}

		void OnInitComplete(InitializationStatus status)
		{
			bool failed = status == null;
			initialize?.Invoke(!failed);
			SendLog.Log(failed ? "[ADS] Error Initialization Status..." : "[ADS] Initialization Status... Success");
			_sdkInitComplete = true;
		}

		void SetupAllAds()
		{
			if (string.IsNullOrEmpty(GetBannerDataUnitId()))
			{
				SetBannerDataUnitId(BANNER_ID_KEY);
			}

			bannerView = new BannerAd(bannerData);
			interstitial = new Interstitial_Ad(_cachedInterstitialId, AdMetricsTypes.Interstitial);
			interstitialVideo = new Interstitial_Ad(_cachedInterstitialVideoId, AdMetricsTypes.InterstitialVideo);
			rewardedVideo = new RewardedVideoAd(_cachedRewardedId);
			appOpenAd = new AppOpenAdGoogle(_cachedAppOpenId);
		}

		public override IAppAd FetchBanner(string placement = "default")
		{
			return bannerView;
		}

		public override IAppAd FetchInterstitial(string placement = "default") => interstitial;

		public override IAppAd FetchInterstitialVideo(string placement = "default") => interstitialVideo;

		public override IAppRewardAd FetchRewarded(string placement = "default") => rewardedVideo;

		public override IAppAd FetchAppOpen(string placement = "default") => appOpenAd;

	}


}