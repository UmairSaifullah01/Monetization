using System;
using GoogleMobileAds.Api;
using UnityEngine;
using THEBADDEST.Monetization;


namespace THEBADDEST.Advertisement
{


	public class BannerAd : IAppAd
	{

		public event Action OnAdLoaded
		{
			add => bannerView.OnBannerAdLoaded += value;
			remove => bannerView.OnBannerAdLoaded -= value;
		}
		public event Action OnAdLoadFailed;
		public event Action<AdValue> OnAdPaid;

		public BannerView ad => bannerView;
		object IAppAd.ad => ad;
		BannerView bannerView;
		BannerData bannerData;
		bool isDisplaying = false;
		private bool isBannerVisible = false;
		private bool isBannerLoaded = false;

		public BannerAd(BannerData bannerData)
		{
			this.bannerData = bannerData;
			isDisplaying = false;
		}

		public void Create()
		{
			if (bannerView != null)
			{
				Destroy();
			}

			bannerView = new BannerView(bannerData.unitId, bannerData.size, bannerData.position);
			bannerView.OnAdPaid += info =>
			{
				AdValue adValue = new AdValue { Value = info.Value, CurrencyCode = info.CurrencyCode, Precision = (AdValue.PrecisionType)(int)info.Precision };
				OnAdPaid?.Invoke(adValue);
			};
			bannerView.OnBannerAdLoadFailed += error => { OnAdLoadFailed?.Invoke(); };
		}

		public void Destroy()
		{
			if (bannerView != null)
			{
				bannerView.Destroy();
				bannerView = null;
			}
		}

		public bool IsLoaded()
		{
			return bannerView != null;
		}

		public void Show()
		{
			if (bannerView != null && !isDisplaying)
			{
				SendLog.Log("Showing banner ad.");
				bannerView.Show();
				isDisplaying = true;
			}
		}

		public void Load()
		{
			// Create an instance of a banner view first.
			if (bannerView == null)
			{
				Create();
			}

			// Create our request used to load the ad.
			var adRequest = new AdRequest();
			bannerView?.LoadAd(adRequest);
			isDisplaying = true;
		}

		public void Hide()
		{
			if (bannerView != null)
			{
				bannerView.Hide();
				isDisplaying = false;
			}
		}

	}


}