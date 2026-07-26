using System;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Ads
{
	[Serializable]
	public class AppLovinMaxSettings
	{
		private const string AD_KEYS_CATEGORY = "AdKeys";

		public const string SDK_KEY = "MaxSdkKey";
		public const string BANNER_ID_KEY = "BannerTop";
		public const string INTERSTITIAL_ID_KEY = "Interstitial";
		public const string INTERSTITIAL_VIDEO_ID_KEY = "InterstitialVideo";
		public const string REWARDED_ID_KEY = "Rewarded";
		public const string APP_OPEN_ID_KEY = "AppOpen";

		[SerializeField] private string adKeysReference = "";
		[SerializeField] private bool enableAdaptiveBanner = true;
		[SerializeField] private MaxSdkBase.BannerPosition bannerPosition = MaxSdkBase.BannerPosition.TopCenter;

		public string AdKeysReference => adKeysReference;
		public bool EnableAdaptiveBanner => enableAdaptiveBanner;
		public MaxSdkBase.BannerPosition BannerPosition => bannerPosition;
		public string Category => AD_KEYS_CATEGORY;
	}
}
