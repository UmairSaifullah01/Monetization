using System;
using THEBADDEST.MonetizationApi;


namespace THEBADDEST.Advertisement
{


	public abstract class AdsModule : MonetizationModule, IAdsModule
	{

		protected Action<bool> _sdkReadyCallbacks;

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