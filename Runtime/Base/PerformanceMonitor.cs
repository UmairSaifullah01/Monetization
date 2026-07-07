using System;
using System.Collections.Generic;
using UnityEngine;
using THEBADDEST.Tasks;


namespace THEBADDEST.MonetizationApi
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
	/// Singleton class for monitoring performance of operations and ad events.
	/// </summary>
	public class PerformanceMonitor
	{

		private static PerformanceMonitor instance;
		public static PerformanceMonitor Instance => instance ??= new PerformanceMonitor();

		private Dictionary<string, float> operationStartTimes = new Dictionary<string, float>();
		private Dictionary<string, List<float>> operationDurations = new Dictionary<string, List<float>>();
		private Dictionary<string, int> operationCounts = new Dictionary<string, int>();
		private Dictionary<string, int> errorCounts = new Dictionary<string, int>();
		private Dictionary<string, AdMetricData> adMetrics = new Dictionary<string, AdMetricData>();

		public void StartOperation(string operationName)
		{
			operationStartTimes[operationName] = Time.realtimeSinceStartup;
		}

		public void EndOperation(string operationName)
		{
			if (operationStartTimes.TryGetValue(operationName, out float startTime))
			{
				float duration = Time.realtimeSinceStartup - startTime;
				if (!operationDurations.ContainsKey(operationName))
				{
					operationDurations[operationName] = new List<float>();
				}

				operationDurations[operationName].Add(duration);
				operationCounts[operationName] = GetOperationCount(operationName) + 1;
				SendLog.LogPerformance(operationName, duration);
				operationStartTimes.Remove(operationName);
			}
		}

		public void RecordError(string operationName)
		{
			errorCounts[operationName] = GetErrorCount(operationName) + 1;
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
					StartOperation(loadOperation);
					break;
				case AdEventType.LoadSucceeded:
					data.LoadSuccessCount++;
					data.LastLoadedUtc = DateTime.UtcNow;
					EndOperation(loadOperation);
					break;
				case AdEventType.LoadFailed:
					data.LoadFailCount++;
					RecordError(loadOperation);
					EndOperation(loadOperation);
					break;
				case AdEventType.ShowSucceeded:
					data.ShowCount++;
					data.LastShownUtc = DateTime.UtcNow;
					break;
				case AdEventType.ShowFailed:
					data.ShowFailCount++;
					break;
			}

			if (MonetizationConfig.Instance.EnablePerformanceLogging)
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

		public float GetAverageDuration(string operationName)
		{
			if (operationDurations.TryGetValue(operationName, out var durations) && durations.Count > 0)
			{
				float sum = 0;
				foreach (float duration in durations)
				{
					sum += duration;
				}

				return sum / durations.Count;
			}

			return 0f;
		}

		public float GetMinDuration(string operationName)
		{
			if (operationDurations.TryGetValue(operationName, out var durations) && durations.Count > 0)
			{
				float min = float.MaxValue;
				foreach (float duration in durations)
				{
					if (duration < min) min = duration;
				}

				return min;
			}

			return 0f;
		}

		public float GetMaxDuration(string operationName)
		{
			if (operationDurations.TryGetValue(operationName, out var durations) && durations.Count > 0)
			{
				float max = 0f;
				foreach (float duration in durations)
				{
					if (duration > max) max = duration;
				}

				return max;
			}

			return 0f;
		}

		public int GetOperationCount(string operationName)
		{
			return operationCounts.TryGetValue(operationName, out int count) ? count : 0;
		}

		public int GetErrorCount(string operationName)
		{
			return errorCounts.TryGetValue(operationName, out int count) ? count : 0;
		}

		public float GetSuccessRate(string operationName)
		{
			int total = GetOperationCount(operationName);
			int errors = GetErrorCount(operationName);
			if (total == 0) return 0f;
			return (float)(total - errors) / total * 100f;
		}

		public void LogPerformanceReport()
		{
			SendLog.LogInfo("=== Performance Report ===");
			foreach (var operation in operationDurations.Keys)
			{
				float avg = GetAverageDuration(operation);
				float min = GetMinDuration(operation);
				float max = GetMaxDuration(operation);
				int count = GetOperationCount(operation);
				int errors = GetErrorCount(operation);
				float successRate = GetSuccessRate(operation);
				SendLog.LogInfo($"{operation}: Avg={avg:F2}ms, Min={min:F2}ms, Max={max:F2}ms, Count={count}, Errors={errors}, Success={successRate:F1}%");
			}

			if (adMetrics.Count > 0)
			{
				SendLog.LogInfo("=== Ad Metrics ===");
				foreach (string adType in adMetrics.Keys)
				{
					var snapshot = GetAdMetrics(adType);
					string lastShown = snapshot.LastShownUtc?.ToString("u") ?? "never";
					SendLog.LogInfo($"{adType}: Shows={snapshot.ShowCount}, ShowFails={snapshot.ShowFailCount}, Loads={snapshot.LoadCount}, LoadSuccess={snapshot.LoadSuccessCount}, LoadFails={snapshot.LoadFailCount}, LastShown={lastShown}");
				}
			}
		}

		public void Reset()
		{
			operationStartTimes.Clear();
			operationDurations.Clear();
			operationCounts.Clear();
			errorCounts.Clear();
			adMetrics.Clear();
		}

		public async UTask MonitorAsyncOperation(string operationName, UTask operation)
		{
			StartOperation(operationName);
			try
			{
				await operation;
				EndOperation(operationName);
			}
			catch (Exception ex)
			{
				RecordError(operationName);
				SendLog.LogException(ex, operationName);
				throw;
			}
		}

		public async UTask<T> MonitorWithTimeout<T>(string operationName, UTask<T> operation, float timeoutSeconds)
		{
			StartOperation(operationName);
			try
			{
				var timeoutTask = UTask.Delay(timeoutSeconds);
				var completedTask = await UTask.WhenAny(operation.ToTask(), timeoutTask);
				if (completedTask == timeoutTask)
				{
					RecordError(operationName);
					SendLog.LogError($"Operation '{operationName}' timed out after {timeoutSeconds} seconds.");
					throw new TimeoutException($"Operation '{operationName}' timed out after {timeoutSeconds} seconds.");
				}
				var result = await operation;
				EndOperation(operationName);
				return result;
			}
			catch (Exception ex)
			{
				RecordError(operationName);
				SendLog.LogException(ex, operationName);
				throw;
			}
		}

	}


}
