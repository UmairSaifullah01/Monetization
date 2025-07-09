using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Serialization;


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

		[SerializeField] private string m_UnitId;
		[SerializeField] private AdSizeType m_Type;
		[SerializeField] private AdPosition m_Position;

		public string unitId => m_UnitId;
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
					;
			}

			return AdSize.Banner;
		}

	}


	[Serializable]
	public class GoogleAdsConfig
	{

		[Header("Ads IDs")] 
		[SerializeField] string appId = "ca-app-pub-3940256099942544~3347511713";
		[SerializeField] BannerData bannerData = new BannerData();
		[SerializeField] string interstitialId = "ca-app-pub-3940256099942544/1033173712";
		[SerializeField] string interstitialVideoId = "ca-app-pub-3940256099942544/8691691433";
		[SerializeField] string rewardedId = "ca-app-pub-3940256099942544/5224354917";
		[SerializeField] string appOpenId = "ca-app-pub-3940256099942544/5662855259";
		[FormerlySerializedAs("TestDeviceIds")]
		[Header("Test Devices")]
		[SerializeField]
		List<string> testDeviceIds = new List<string>()
		{
			AdRequest.TestDeviceSimulator,
#if UNITY_IPHONE
            "96e23e80653bb28980d3f40beb58915c",
#elif UNITY_ANDROID
			"702815ACFC14FF222DA1DC767672A573"
#endif
		};
		
		
		public string AppId => appId;
		public BannerData BannerData => bannerData;
		public string InterstitialId => interstitialId;
		public string InterstitialVideoId => interstitialVideoId;
		public string RewardedId => rewardedId;
		public string AppOpenId => appOpenId;
		public List<string> TestDeviceIds()
		{
			return testDeviceIds;
		}

	}


}