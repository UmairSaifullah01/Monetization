using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;

namespace THEBADDEST.MonetizationApi.Ads
{


	public static class AdLoadTimeoutWatcher
	{
		public static async UTask Watch(string adType, string unitId, System.Func<bool> isSettled)
		{
			float timeout = 30f;
			if (Monetization.TryGetModule<IAdsModule>(out var ads))
			{
				timeout = ads.AdLoadTimeout;
			}

			await UTask.Delay(timeout);
			if (isSettled())
			{
				return;
			}

			PerformanceMonitor.Instance.RecordAdEvent(adType, AdEventType.LoadFailed, unitId);
		}
	}
}
