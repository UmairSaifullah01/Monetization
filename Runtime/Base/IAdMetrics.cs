using System;
using System.Collections.Generic;

namespace THEBADDEST.MonetizationApi.Ads
{
	public static class AdMetricsTypes
	{
		public const string Interstitial = "Interstitial";
		public const string InterstitialVideo = "InterstitialVideo";
		public const string Rewarded = "Rewarded";
		public const string Banner = "Banner";
		public const string AppOpen = "AppOpen";
	}

	public enum AdEventType
	{
		LoadStarted,
		LoadSucceeded,
		LoadFailed,
		ShowSucceeded,
		ShowFailed
	}

	public struct AdMetricSnapshot
	{
		public int ShowCount;
		public int ShowFailCount;
		public int LoadCount;
		public int LoadSuccessCount;
		public int LoadFailCount;
		public DateTime? LastShownUtc;
		public DateTime? LastLoadedUtc;
		public float SecondsSinceLastShow;
	}

	public interface IAdMetrics
	{
		void RecordAdEvent(string adType, AdEventType eventType, string placement = null);
		AdMetricSnapshot GetAdMetrics(string adType);
		IReadOnlyCollection<string> GetTrackedAdTypes();
		void ResetAdMetrics(string adType = null);
	}
}
