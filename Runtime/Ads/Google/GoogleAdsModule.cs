using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;
using THEBADDEST.Monetization;


namespace THEBADDEST.Advertisement
{


	public class GoogleAdsModule : AdsModule
	{

		[SerializeField] private GoogleAdsConfig config;

		IAppAd bannerView;
		IAppAd interstitial;
		IAppAd interstitialVideo;
		IAppRewardAd rewardedVideo;
		IAppAd appOpenAd;


		public event Action<bool> OnInitialize;

		public bool CanRequestAds => ConsentInformation.CanRequestAds();

		public override void Init()
		{
			SetupAllAds();
			MobileAds.RaiseAdEventsOnUnityMainThread = true;
			if (CheckInternetState())
			{
				RequestConsent();
			}

			isInitialized = true;
			OnInitialize?.Invoke(isInitialized);
		}


		bool CheckInternetState()
		{
			return Application.internetReachability != NetworkReachability.NotReachable;
		}

		void RequestConsent()
		{
#if UnityEditor
			InitializeAds();
			return;
#endif
			ConsentRequestParameters request = new ConsentRequestParameters { TagForUnderAgeOfConsent = false, ConsentDebugSettings = new ConsentDebugSettings() { DebugGeography = DebugGeography.EEA, TestDeviceHashedIds = config.TestDeviceIds() }, };
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
			MobileAds.Initialize(OnInitComplete);
		}

		void OnInitComplete(InitializationStatus status)
		{
			bool statusBool = status == null;
			OnInitialize?.Invoke(statusBool);
			SendLog.Log(statusBool ? "[ADS] Error Initialization Status..." : "[ADS] Initialization Status... Success");
		}

		void SetupAllAds()
		{
			bannerView = new BannerAd(config.BannerData);
			interstitial = new Interstitial_Ad(config.InterstitialId);
			interstitialVideo = new Interstitial_Ad(config.InterstitialVideoId);
			rewardedVideo = new RewardedVideoAd(config.RewardedId);
			appOpenAd = new AppOpenAdGoogle(config.AppOpenId);
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