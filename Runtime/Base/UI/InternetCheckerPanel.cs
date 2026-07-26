using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;


namespace THEBADDEST.MonetizationApi
{
	/// <summary>
	/// Presenter for the No Internet overlay panel loaded from Resources.
	/// </summary>
	public class InternetCheckerPanel : MonoBehaviour
	{
		public const string ResourcePath = "InternetCheckerPanel";

		[SerializeField] private Button retryButton;
		[SerializeField] private GameObject root;

		private Action _onRetry;

		private void Awake()
		{
			if (root == null)
			{
				root = gameObject;
			}

			WireRetryButton();
		}

		public void BindRetryButton(Button button)
		{
			retryButton = button;
			WireRetryButton();
		}

		private void WireRetryButton()
		{
			if (retryButton == null)
			{
				return;
			}

			retryButton.onClick.RemoveListener(HandleRetry);
			retryButton.onClick.AddListener(HandleRetry);
		}

		private void HandleRetry()
		{
			_onRetry?.Invoke();
		}

		public void Show(Action onRetry)
		{
			_onRetry = onRetry;
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
			_onRetry = null;
			if (root != null)
			{
				root.SetActive(false);
			}
			else
			{
				gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Loads <see cref="ResourcePath"/> from Resources. Returns null if missing.
		/// </summary>
		public static InternetCheckerPanel TryGet()
		{
			var host = UiOverlayHost.Instance;
			var existing = host.Root.GetComponentInChildren<InternetCheckerPanel>(true);
			if (existing != null)
			{
				return existing;
			}

			var prefab = Resources.Load<GameObject>(ResourcePath);
			if (prefab == null)
			{
				SendLog.LogWarning($"Internet panel prefab not found at Resources/{ResourcePath}. Skipping panel.");
				return null;
			}

			var instance = Object.Instantiate(prefab, host.Root, false);
			instance.name = "InternetCheckerPanel";
			var panel = instance.GetComponent<InternetCheckerPanel>();
			if (panel == null)
			{
				SendLog.LogWarning($"Internet panel prefab at Resources/{ResourcePath} is missing InternetCheckerPanel component. Skipping panel.");
				Object.Destroy(instance);
				return null;
			}

			panel.Hide();
			return panel;
		}
	}
}
