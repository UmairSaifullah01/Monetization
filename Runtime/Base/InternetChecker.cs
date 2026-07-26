using THEBADDEST.Tasks;
using UnityEngine;

namespace THEBADDEST.MonetizationApi
{
	/// <summary>
	/// Connectivity checks and init-time wait with No Internet panel from Resources.
	/// </summary>
	public static class InternetChecker
	{
		public static bool IsAvailable()
		{
#if UNITY_EDITOR
			return true;
#else
			return Application.internetReachability != NetworkReachability.NotReachable;
#endif
		}

		/// <summary>
		/// Shows the internet panel while offline and completes once Retry succeeds (online).
		/// If already online, returns immediately. If the Resources panel is missing, logs and returns.
		/// </summary>
		public static async UTask WaitUntilAvailableAsync()
		{
			if (IsAvailable())
			{
				return;
			}

			var panel = InternetCheckerPanel.TryGet();
			if (panel == null)
			{
				return;
			}

			SendLog.LogWarning("No internet connection. Waiting for Retry...");

			var online = false;
			panel.Show(() =>
			{
				if (IsAvailable())
				{
					online = true;
				}
				else
				{
					SendLog.LogWarning("Still offline. Check your connection and try again.");
				}
			});

			await UTask.WaitUntil(() => online);
			panel.Hide();
			SendLog.Log("Internet connection restored.");
		}
	}
}
