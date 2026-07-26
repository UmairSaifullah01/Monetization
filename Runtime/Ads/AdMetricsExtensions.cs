using System;
using THEBADDEST.MonetizationApi;
using THEBADDEST.MonetizationApi.Ads;


namespace THEBADDEST.MonetizationApi.Ads
{


	public static class AdMetricsExtensions
	{
		public static AdMetricSnapshot GetInterstitialMetrics(this IAdsModule _)
			=> PerformanceMonitor.Instance.GetAdMetrics(AdMetricsTypes.Interstitial);

		public static int GetInterstitialShowCount(this IAdsModule _)
			=> PerformanceMonitor.Instance.GetAdMetrics(AdMetricsTypes.Interstitial).ShowCount;

		public static DateTime? GetLastInterstitialShowTime(this IAdsModule _)
			=> PerformanceMonitor.Instance.GetAdMetrics(AdMetricsTypes.Interstitial).LastShownUtc;
	}


}
