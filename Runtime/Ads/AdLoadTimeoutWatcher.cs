using THEBADDEST.MonetizationApi;
using THEBADDEST.Tasks;

namespace THEBADDEST.Advertisement
{


	public static class AdLoadTimeoutWatcher
	{
		public static async UTask Watch(string adType, string unitId, System.Func<bool> isSettled)
		{
			float timeout = MonetizationConfig.Instance.AdLoadTimeout;
			await UTask.Delay(timeout);
			if (isSettled())
			{
				return;
			}

			PerformanceMonitor.Instance.RecordAdEvent(adType, AdEventType.LoadFailed, unitId);
		}
	}
}
