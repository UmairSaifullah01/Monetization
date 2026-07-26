using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using THEBADDEST.MonetizationApi;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Editor
{
	public static class ProviderProfileValidator
	{
		public static List<string> Validate(MonetizationProfile profile)
		{
			var warnings = new List<string>();
			if (profile == null)
			{
				warnings.Add("MonetizationProfile is null.");
				return warnings;
			}

			var configPath = Path.Combine(Application.dataPath, "Monetization/Installer/installer_config.json");
			if (!File.Exists(configPath))
			{
				warnings.Add("installer_config.json not found.");
				return warnings;
			}

			var configDict = MiniJSON.Json.Deserialize(File.ReadAllText(configPath)) as Dictionary<string, object>;
			if (configDict == null || !configDict.TryGetValue("providers", out var providersObj))
			{
				return warnings;
			}

			var providers = providersObj as Dictionary<string, object>;
			if (providers == null)
			{
				return warnings;
			}

			foreach (var module in profile.modules)
			{
				if (module == null)
				{
					warnings.Add("Profile contains a null module reference.");
					continue;
				}

				string moduleType = module.GetType().FullName;
				KeyValuePair<string, Dictionary<string, object>>? matchedProvider = null;

				foreach (var providerPair in providers)
				{
					if (providerPair.Value is Dictionary<string, object> providerDict &&
					    providerDict.TryGetValue("moduleType", out var moduleTypeObj) &&
					    string.Equals(moduleTypeObj?.ToString(), moduleType, StringComparison.Ordinal))
					{
						matchedProvider = new KeyValuePair<string, Dictionary<string, object>>(providerPair.Key, providerDict);
						break;
					}
				}

				if (matchedProvider == null)
				{
					continue;
				}

				var provider = matchedProvider.Value;
				if (provider.Value.TryGetValue("asmdef", out var asmdefObj))
				{
					string asmdefName = asmdefObj?.ToString();
					if (!string.IsNullOrEmpty(asmdefName) && !AsmdefExists(asmdefName))
					{
						warnings.Add($"Module '{module.ModuleName}' ({provider.Key}) references missing assembly '{asmdefName}'. Install the provider or remove the module from the profile.");
					}
				}
			}

			return warnings;
		}

		public static void DrawWarnings(MonetizationProfile profile)
		{
			var warnings = Validate(profile);
			if (warnings.Count == 0)
			{
				return;
			}

			EditorGUILayout.Space(5);
			EditorGUILayout.HelpBox(string.Join("\n", warnings), MessageType.Warning);
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
