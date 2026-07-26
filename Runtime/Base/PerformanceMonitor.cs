using System;
using System.Collections.Generic;
using UnityEngine;
using THEBADDEST.Tasks;
using THEBADDEST.MonetizationApi.Ads;

namespace THEBADDEST.MonetizationApi
{
	/// <summary>
	/// Singleton for monitoring performance of operations.
	/// Ad metrics are recorded via <see cref="IAdMetrics"/> on <see cref="IModuleContext"/>.
	/// </summary>
	public class PerformanceMonitor
	{
		private static PerformanceMonitor instance;
		public static PerformanceMonitor Instance => instance ??= new PerformanceMonitor();

		/// <summary>
		/// Active ad metrics sink set during Monetization.Initialize.
		/// </summary>
		public static IAdMetrics AdMetrics { get; set; } = NullAdMetrics.Instance;

		private Dictionary<string, float> operationStartTimes = new Dictionary<string, float>();
		private Dictionary<string, List<float>> operationDurations = new Dictionary<string, List<float>>();
		private Dictionary<string, int> operationCounts = new Dictionary<string, int>();
		private Dictionary<string, int> errorCounts = new Dictionary<string, int>();

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
			AdMetrics.RecordAdEvent(adType, eventType, placement);
		}

		public AdMetricSnapshot GetAdMetrics(string adType) => AdMetrics.GetAdMetrics(adType);

		public IReadOnlyCollection<string> GetTrackedAdTypes() => AdMetrics.GetTrackedAdTypes();

		public void ResetAdMetrics(string adType = null) => AdMetrics.ResetAdMetrics(adType);

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

			var tracked = AdMetrics.GetTrackedAdTypes();
			if (tracked.Count > 0)
			{
				SendLog.LogInfo("=== Ad Metrics ===");
				foreach (string adType in tracked)
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
			AdMetrics.ResetAdMetrics();
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
