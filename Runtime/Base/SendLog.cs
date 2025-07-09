using UnityEngine;


namespace THEBADDEST.Monetization
{


	public static class SendLog
	{
		private const string Tag = "[Monetization]";
		public static bool Enabled = true;

		public static void Log(string message)
		{
			if (!Enabled) return;
			Debug.Log($"<color=green>{Tag}</color> {message}");
		}

		public static void LogError(string message)
		{
			if (!Enabled) return;
			Debug.LogError($"<color=red>{Tag}</color> {message}");
		}

		public static void LogWarning(string message)
		{
			if (!Enabled) return;
			Debug.LogWarning($"<color=yellow>{Tag}</color> {message}");
		}
	}


}