using System;
using THEBADDEST.MonetizationApi;
using UnityEngine;


namespace THEBADDEST.MonetizationApi.Ads
{


	public abstract class AdsModule : MonetizationModule, IAdsModule
	{
		private const string PrivacyKeysCategory = "PrivacyKeys";
		private const string ProjectKeysCategory = "ProjectKeys";

		[Header("Ads Settings")]
		[Tooltip("Enable test mode for ads (use test ad units / verbose SDK logging).")]
		[SerializeField] protected bool enableTestMode = true;
		[Tooltip("Timeout in seconds for ad load operations.")]
		[SerializeField] protected float adLoadTimeout = 30f;

		[Header("Privacy Policy")]
		[Tooltip("When enabled, ShowPrivacyPolicyPanel() displays the privacy panel from Resources. When disabled, the panel stays hidden.")]
		[SerializeField] protected bool showPrivacyPolicyPanel;

		protected Action<bool> _sdkReadyCallbacks;
		private PrivacyPolicyPanel _privacyPanel;

		public bool EnableTestMode => enableTestMode;
		public float AdLoadTimeout => adLoadTimeout;
		public bool ShowPrivacyPolicyPanelEnabled => showPrivacyPolicyPanel;

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

		public void ShowPrivacyPolicyPanel()
		{
			if (!showPrivacyPolicyPanel)
			{
				return;
			}

			_privacyPanel = PrivacyPolicyPanel.TryGet();
			if (_privacyPanel == null)
			{
				return;
			}

			string privacyUrl = ResolveKey(PrivacyKeysCategory, "PrivacyPolicyUrl");
			string termsUrl = ResolveKey(PrivacyKeysCategory, "TermsOfServiceUrl");
			string version = ResolveKey(ProjectKeysCategory, "Version");
			string bundleCode = ResolveKey(ProjectKeysCategory, "BundleVersionCode");

			if (string.IsNullOrEmpty(version))
			{
				version = Application.version;
			}

			string buildLine = string.IsNullOrEmpty(bundleCode)
				? $"Build Version: {version}"
				: $"Build Version: {version} [{bundleCode}]";

			_privacyPanel.Show(Application.productName, buildLine, termsUrl, privacyUrl);
		}

		public void HidePrivacyPolicyPanel()
		{
			if (_privacyPanel != null)
			{
				_privacyPanel.Hide();
			}
		}

		private static string ResolveKey(string category, string key) =>
			JsonDataUtility.GetData(category, key) ?? string.Empty;

		public abstract IAppAd FetchBanner(string placement = "default");

		public abstract IAppAd FetchInterstitial(string placement = "default");

		public abstract IAppAd FetchInterstitialVideo(string placement = "default");

		public abstract IAppRewardAd FetchRewarded(string placement = "default");

		public abstract IAppAd FetchAppOpen(string placement = "default");

	}


}
