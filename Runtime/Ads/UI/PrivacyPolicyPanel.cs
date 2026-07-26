using UnityEngine;
using UnityEngine.UI;

namespace THEBADDEST.MonetizationApi.Ads
{
	/// <summary>
	/// Presenter for the Privacy Policy overlay panel loaded from Resources.
	/// </summary>
	public class PrivacyPolicyPanel : MonoBehaviour
	{
		public const string ResourcePath = "PrivacyPolicyPanel";

		[SerializeField] private GameObject root;
		[SerializeField] private Text appNameLabel;
		[SerializeField] private Text buildVersionLabel;
		[SerializeField] private Button termsButton;
		[SerializeField] private Button privacyButton;
		[SerializeField] private Button closeButton;

		private string _termsUrl;
		private string _privacyUrl;

		private void Awake()
		{
			if (root == null)
			{
				root = gameObject;
			}

			WireButtons();
		}

		private void WireButtons()
		{
			if (termsButton != null)
			{
				termsButton.onClick.RemoveListener(OpenTerms);
				termsButton.onClick.AddListener(OpenTerms);
			}

			if (privacyButton != null)
			{
				privacyButton.onClick.RemoveListener(OpenPrivacy);
				privacyButton.onClick.AddListener(OpenPrivacy);
			}

			if (closeButton != null)
			{
				closeButton.onClick.RemoveListener(Hide);
				closeButton.onClick.AddListener(Hide);
			}
		}

		public void Show(string appName, string buildVersion, string termsUrl, string privacyUrl)
		{
			_termsUrl = termsUrl;
			_privacyUrl = privacyUrl;

			if (appNameLabel != null)
			{
				appNameLabel.text = appName ?? string.Empty;
			}

			if (buildVersionLabel != null)
			{
				buildVersionLabel.text = buildVersion ?? string.Empty;
			}

			if (root != null)
			{
				root.SetActive(true);
			}
			else
			{
				gameObject.SetActive(true);
			}
		}

		public void Hide()
		{
			if (root != null)
			{
				root.SetActive(false);
			}
			else
			{
				gameObject.SetActive(false);
			}
		}

		private void OpenTerms() => OpenUrl(_termsUrl, "TermsOfServiceUrl");

		private void OpenPrivacy() => OpenUrl(_privacyUrl, "PrivacyPolicyUrl");

		private static void OpenUrl(string url, string keyName)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				SendLog.LogWarning($"Privacy panel: {keyName} is empty in MonetizationKeys.json.");
				return;
			}

			Application.OpenURL(url);
		}

		/// <summary>
		/// Loads <see cref="ResourcePath"/> from Resources. Returns null if missing.
		/// </summary>
		public static PrivacyPolicyPanel TryGet()
		{
			var host = UiOverlayHost.Instance;
			var existing = host.Root.GetComponentInChildren<PrivacyPolicyPanel>(true);
			if (existing != null)
			{
				return existing;
			}

			var prefab = Resources.Load<GameObject>(ResourcePath);
			if (prefab == null)
			{
				SendLog.LogWarning($"Privacy panel prefab not found at Resources/{ResourcePath}. Skipping panel.");
				return null;
			}

			var instance = Object.Instantiate(prefab, host.Root, false);
			instance.name = "PrivacyPolicyPanel";
			var panel = instance.GetComponent<PrivacyPolicyPanel>();
			if (panel == null)
			{
				SendLog.LogWarning($"Privacy panel prefab at Resources/{ResourcePath} is missing PrivacyPolicyPanel component. Skipping panel.");
				Object.Destroy(instance);
				return null;
			}

			panel.Hide();
			return panel;
		}
	}
}
