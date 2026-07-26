using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using THEBADDEST.MonetizationApi;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Editor
{
	public static class HotSwapValidator
	{
		[MenuItem("Tools/Monetization/Validate Hot-Swap Readiness")]
		public static void ValidateHotSwapReadiness()
		{
			var report = BuildReport();
			Debug.Log(report);
			EditorUtility.DisplayDialog("Hot-Swap Validation", report, "OK");
		}

		public static string BuildReport()
		{
			var sb = new StringBuilder();
			sb.AppendLine("=== Monetization Hot-Swap Validation ===");

			var configPath = Path.Combine(Application.dataPath, "Monetization/Installer/installer_config.json");
			if (!File.Exists(configPath))
			{
				sb.AppendLine("FAIL: installer_config.json not found.");
				return sb.ToString();
			}

			var configDict = MiniJSON.Json.Deserialize(File.ReadAllText(configPath)) as Dictionary<string, object>;
			if (configDict == null || !configDict.TryGetValue("providers", out var providersObj))
			{
				sb.AppendLine("FAIL: No providers section in installer_config.json.");
				return sb.ToString();
			}

			var providers = providersObj as Dictionary<string, object>;
			string[] coreAsmdefs =
			{
				"THEBADDEST.Monetization.Core",
				"THEBADDEST.Monetization.Configuration",
				"THEBADDEST.Monetization.Ads.Abstractions",
				"THEBADDEST.Monetization.IAP.Abstractions",
				"THEBADDEST.Monetization.Analytics.Abstractions",
				"THEBADDEST.Monetization.RemoteConfig.Abstractions",
				"THEBADDEST.Monetization.Database.Abstractions",
				"THEBADDEST.Monetization.Storage.Abstractions"
			};

			sb.AppendLine("\nCore + Abstractions (must always compile):");
			foreach (string asmdef in coreAsmdefs)
			{
				sb.AppendLine($"  {(AsmdefExists(asmdef) ? "OK" : "MISSING")}: {asmdef}");
			}

			sb.AppendLine("\nOptional providers:");
			foreach (var providerPair in providers)
			{
				if (providerPair.Value is Dictionary<string, object> providerDict &&
				    providerDict.TryGetValue("asmdef", out var asmdefObj))
				{
					string asmdef = asmdefObj?.ToString();
					bool exists = !string.IsNullOrEmpty(asmdef) && AsmdefExists(asmdef);
					sb.AppendLine($"  {(exists ? "INSTALLED" : "REMOVED")}: {providerPair.Key} ({asmdef})");
				}
			}

			var profile = Resources.Load<MonetizationProfile>("MonetizationProfile");
			if (profile != null)
			{
				sb.AppendLine("\nProfile module warnings:");
				foreach (string warning in ProviderProfileValidator.Validate(profile))
				{
					sb.AppendLine($"  WARN: {warning}");
				}

				if (ProviderProfileValidator.Validate(profile).Count == 0)
				{
					sb.AppendLine("  None â€” profile matches installed providers.");
				}
			}

			sb.AppendLine("\nGame code should use Monetization.TryGetModule<T>() for all provider access.");
			return sb.ToString();
		}

		private static bool AsmdefExists(string asmdefName)
		{
			string[] guids = AssetDatabase.FindAssets($"{asmdefName} t:AssemblyDefinitionAsset");
			return guids.Any(guid =>
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				return Path.GetFileNameWithoutExtension(path) == asmdefName;
			});
		}
	}
}
