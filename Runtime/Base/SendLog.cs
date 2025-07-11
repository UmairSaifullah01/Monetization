using UnityEngine;
using System;
using System.Collections.Generic;

namespace THEBADDEST.MonetizationApi
{
	public enum LogLevel
	{
		None = 0,
		Error = 1,
		Warning = 2,
		Info = 3,
		Debug = 4
	}

	public static class SendLog
	{
		private const string Tag = "[Monetization]";
		public static bool Enabled = true;
		public static LogLevel CurrentLogLevel = LogLevel.Info;

		private static readonly Dictionary<LogLevel, string> LogColors = new Dictionary<LogLevel, string>
		{
			{ LogLevel.Error, "red" },
			{ LogLevel.Warning, "yellow" },
			{ LogLevel.Info, "green" },
			{ LogLevel.Debug, "cyan" }
		};

		public static void Log(string message, LogLevel level = LogLevel.Info)
		{
			if (!Enabled || level > CurrentLogLevel) return;

			string color = LogColors.TryGetValue(level, out string logColor) ? logColor : "white";
			string levelTag = level == LogLevel.Info ? "" : $"[{level}] ";
			Debug.Log($"<color={color}>{Tag}</color> {levelTag}{message}");
		}

		public static void LogError(string message)
		{
			Log(message, LogLevel.Error);
		}

		public static void LogWarning(string message)
		{
			Log(message, LogLevel.Warning);
		}

		public static void LogDebug(string message)
		{
			Log(message, LogLevel.Debug);
		}

		public static void LogInfo(string message)
		{
			Log(message, LogLevel.Info);
		}

		// Performance logging
		public static void LogPerformance(string operation, float durationMs)
		{
			if (CurrentLogLevel >= LogLevel.Debug)
			{
				LogDebug($"{operation} took {durationMs:F2}ms");
			}
		}

		// Module-specific logging
		public static void LogModule(string moduleName, string message, LogLevel level = LogLevel.Info)
		{
			if (!Enabled || level > CurrentLogLevel) return;

			string color = LogColors.TryGetValue(level, out string logColor) ? logColor : "white";
			string levelTag = level == LogLevel.Info ? "" : $"[{level}] ";
			Debug.Log($"<color={color}>{Tag}[{moduleName}]</color> {levelTag}{message}");
		}

		// Exception logging with stack trace
		public static void LogException(Exception ex, string context = "")
		{
			if (!Enabled) return;

			string contextMessage = string.IsNullOrEmpty(context) ? "" : $"Context: {context}. ";
			Debug.LogError($"<color=red>{Tag}</color> Exception: {contextMessage}{ex.Message}\nStackTrace: {ex.StackTrace}");
		}

		// Batch logging for multiple messages
		public static void LogBatch(IEnumerable<string> messages, LogLevel level = LogLevel.Info)
		{
			if (!Enabled || level > CurrentLogLevel) return;

			foreach (var message in messages)
			{
				Log(message, level);
			}
		}
	}
}