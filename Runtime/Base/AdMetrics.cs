using System;
using System.Collections.Generic;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Ads
{
	internal class AdMetricData
	{
		public int ShowCount;
		public int ShowFailCount;
		public int LoadCount;
		public int LoadSuccessCount;
		public int LoadFailCount;
		public DateTime? LastShownUtc;
		public DateTime? LastLoadedUtc;
	}

	/// <summary>
	/// Records ad load/show metrics. Lives in Core to avoid circular asmdef deps with Ads.Abstractions.
	/// </summary>
	public class AdMetrics : IAdMetrics
	{
		private readonly IMonetizationSettings settings;
		private readonly Dictionary<string, float> operationStartTimes = new Dictionary<string, float>();
		private readonly Dictionary<string, AdMetricData> adMetrics = new Dictionary<string, AdMetricData>();

		public AdMetrics(IMonetizationSettings settings)
		{
			this.settings = settings;
		}

		public void RecordAdEvent(string adType, AdEventType eventType, string placement = null)
		{
			if (!adMetrics.TryGetValue(adType, out AdMetricData data))
			{
				data = new AdMetricData();
				adMetrics[adType] = data;
			}

			string loadOperation = $"Ad.{adType}.Load";

			switch (eventType)
			{
				case AdEventType.LoadStarted:
					data.LoadCount++;
					operationStartTimes[loadOperation] = Time.realtimeSinceStartup;
					break;
				case AdEventType.LoadSucceeded:
					data.LoadSuccessCount++;
					data.LastLoadedUtc = DateTime.UtcNow;
					operationStartTimes.Remove(loadOperation);
					break;
				case AdEventType.LoadFailed:
					data.LoadFailCount++;
					operationStartTimes.Remove(loadOperation);
					break;
				case AdEventType.ShowSucceeded:
					data.ShowCount++;
					data.LastShownUtc = DateTime.UtcNow;
					break;
				case AdEventType.ShowFailed:
					data.ShowFailCount++;
					break;
			}

			if (settings != null && settings.EnablePerformanceLogging)
			{
				var snapshot = BuildSnapshot(data);
				string placementInfo = string.IsNullOrEmpty(placement) ? "" : $", placement={placement}";
				SendLog.LogDebug($"Ad {adType} {eventType} (shows={snapshot.ShowCount}, loads={snapshot.LoadCount}{placementInfo})");
			}
		}

		public AdMetricSnapshot GetAdMetrics(string adType)
		{
			if (!adMetrics.TryGetValue(adType, out AdMetricData data))
			{
				return default;
			}

			return BuildSnapshot(data);
		}

		public IReadOnlyCollection<string> GetTrackedAdTypes() => adMetrics.Keys;

		public void ResetAdMetrics(string adType = null)
		{
			if (string.IsNullOrEmpty(adType))
			{
				adMetrics.Clear();
				return;
			}

			adMetrics.Remove(adType);
		}

		private static AdMetricSnapshot BuildSnapshot(AdMetricData data)
		{
			float secondsSinceLastShow = 0f;
			if (data.LastShownUtc.HasValue)
			{
				secondsSinceLastShow = (float)(DateTime.UtcNow - data.LastShownUtc.Value).TotalSeconds;
			}

			return new AdMetricSnapshot
			{
				ShowCount = data.ShowCount,
				ShowFailCount = data.ShowFailCount,
				LoadCount = data.LoadCount,
				LoadSuccessCount = data.LoadSuccessCount,
				LoadFailCount = data.LoadFailCount,
				LastShownUtc = data.LastShownUtc,
				LastLoadedUtc = data.LastLoadedUtc,
				SecondsSinceLastShow = secondsSinceLastShow
			};
		}
	}
}
