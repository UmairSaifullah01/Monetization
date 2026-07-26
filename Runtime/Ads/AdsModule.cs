using System;
using THEBADDEST.MonetizationApi;
using UnityEngine;


namespace THEBADDEST.MonetizationApi.Ads
{


	public abstract class AdsModule : MonetizationModule, IAdsModule
	{
		[Header("Ads Settings")]
		[Tooltip("Enable test mode for ads (use test ad units / verbose SDK logging).")]
		[SerializeField] protected bool enableTestMode = true;
		[Tooltip("Timeout in seconds for ad load operations.")]
		[SerializeField] protected float adLoadTimeout = 30f;

		protected Action<bool> _sdkReadyCallbacks;

		public bool EnableTestMode => enableTestMode;
		public float AdLoadTimeout => adLoadTimeout;

		public event Action<bool> OnSdkReady
		{
			add => _sdkReadyCallbacks += value;
			remove => _sdkReadyCallbacks -= value;
		}

		[Obsolete("Use OnSdkReady. Will be removed in a future version.")]
		public event Action<bool> onInitialize
		{
			add => OnSdkReady += value;
			remove => OnSdkReady -= value;
		}

		protected void RaiseAdsSdkReady(bool success) => _sdkReadyCallbacks?.Invoke(success);

		public abstract IAppAd FetchBanner(string placement = "default");

		public abstract IAppAd FetchInterstitial(string placement = "default");

		public abstract IAppAd FetchInterstitialVideo(string placement = "default");

		public abstract IAppRewardAd FetchRewarded(string placement = "default");

		public abstract IAppAd FetchAppOpen(string placement = "default");

	}


}
