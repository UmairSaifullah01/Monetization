using UnityEngine;
using UnityEngine.UI;

namespace THEBADDEST.MonetizationApi
{
	/// <summary>
	/// DontDestroyOnLoad overlay canvas used by monetization panels.
	/// </summary>
	public sealed class UiOverlayHost : MonoBehaviour
	{
		private const string HostName = "[MonetizationUiOverlay]";
		private static UiOverlayHost _instance;
		private Transform _root;

		public static UiOverlayHost Instance
		{
			get
			{
				if (_instance != null)
				{
					return _instance;
				}

				var go = new GameObject(HostName);
				Object.DontDestroyOnLoad(go);
				_instance = go.AddComponent<UiOverlayHost>();
				return _instance;
			}
		}

		public Transform Root
		{
			get
			{
				EnsureCanvas();
				return _root;
			}
		}

		private void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Destroy(gameObject);
				return;
			}

			_instance = this;
			DontDestroyOnLoad(gameObject);
			EnsureCanvas();
		}

		private void EnsureCanvas()
		{
			if (_root != null)
			{
				return;
			}

			var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
			canvasGo.transform.SetParent(transform, false);

			var canvas = canvasGo.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 32000;

			var scaler = canvasGo.GetComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1080, 1920);
			scaler.matchWidthOrHeight = 0.5f;

			_root = canvasGo.transform;
		}
	}
}
