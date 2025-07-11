using System;
using System.Collections.Generic;
using UnityEngine;
using THEBADDEST.Tasks;

namespace THEBADDEST.MonetizationApi
{
    public class PerformanceMonitor
    {
        private static PerformanceMonitor instance;
        public static PerformanceMonitor Instance => instance ??= new PerformanceMonitor();

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
        }

        public void Reset()
        {
            operationStartTimes.Clear();
            operationDurations.Clear();
            operationCounts.Clear();
            errorCounts.Clear();
        }

        // Async operation monitoring
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

        // Timeout monitoring
        public async UTask<T> MonitorWithTimeout<T>(string operationName, UTask<T> operation, float timeoutSeconds)
        {
            StartOperation(operationName);
            try
            {
                var timeoutTask = UTask.Delay(TimeSpan.FromSeconds(timeoutSeconds));
                var completedTask = await UTask.WhenAny(operation, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    RecordError(operationName);
                    throw new TimeoutException($"Operation {operationName} timed out after {timeoutSeconds} seconds");
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