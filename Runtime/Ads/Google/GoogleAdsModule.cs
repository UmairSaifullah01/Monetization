using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Ads
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

		[SerializeField] private string m_UnitIdKey;
		[SerializeField] private AdSizeType m_Type;
		[SerializeField] private AdPosition m_Position;

		public string UnitIdKey => m_UnitIdKey;

		public void SetUnitIdKey(string key)
		{
			m_UnitIdKey = key;
		}

		public string unitId
		{
			get
			{
				if (string.IsNullOrEmpty(m_UnitIdKey))
				{
					return string.Empty;
				}

				var catalog = Monetization.Context?.Catalog ?? CatalogFactory.Create();
				string jsonBannerId = catalog?.Resolve(AD_KEYS_CATEGORY, m_UnitIdKey);
				if (!string.IsNullOrEmpty(jsonBannerId))
				{
					return jsonBannerId;
				}

				return m_UnitIdKey.StartsWith("ca-app-pub-") ? m_UnitIdKey : string.Empty;
			}
		}

		public AdSize size => ConvertTypeToSize();
		public AdPosition position => m_Position;

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
		public const string APP_ID_KEY = "AppId";
		public const string BANNER_ID_KEY = "BannerTop";
		public const string INTERSTITIAL_ID_KEY = "Interstitial";
		public const string INTERSTITIAL_VIDEO_ID_KEY = "InterstitialVideo";
		public const string REWARDED_ID_KEY = "Rewarded";
		public const string APP_OPEN_ID_KEY = "AppOpen";

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

		private GoogleAdsService _service;

		public bool CanRequestAds => _service?.CanRequestAds ?? false;

		protected override async UTask OnInitialize()
		{
			var settings = new GoogleAdsSettings
			{
				BannerData = bannerData,
				TestDeviceIds = testDeviceIds
			};

			_service = new GoogleAdsService(settings, Context.Catalog, RaiseAdsSdkReady, ModuleName);
			await _service.InitializeAsync(EnableTestMode);
			bannerData = settings.BannerData;
		}

		public override IAppAd FetchBanner(string placement = "default") => _service?.BannerView;

		public override IAppAd FetchInterstitial(string placement = "default") => _service?.Interstitial;

		public override IAppAd FetchInterstitialVideo(string placement = "default") => _service?.InterstitialVideo;

		public override IAppRewardAd FetchRewarded(string placement = "default") => _service?.RewardedVideo;

		public override IAppAd FetchAppOpen(string placement = "default") => _service?.AppOpenAd;
	}
}
