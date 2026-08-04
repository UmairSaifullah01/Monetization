#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Ads
{
	internal static class GoogleMobileAdsSettingsSync
	{
		public static bool ApplyAppId(string appId, string moduleName)
		{
			if (string.IsNullOrEmpty(appId))
			{
				SendLog.LogModule(moduleName, "Google Ads AppId not found in AdKeys JSON.", LogLevel.Warning);
				return false;
			}

			Type settingsType = FindSettingsType();
			if (settingsType == null)
			{
				SendLog.LogModule(moduleName, "GoogleMobileAdsSettings type not found. Is the Google Mobile Ads plugin installed?", LogLevel.Warning);
				return false;
			}

			object instance = LoadInstance(settingsType);
			if (instance == null)
			{
				SendLog.LogModule(moduleName, "Could not load GoogleMobileAdsSettings instance.", LogLevel.Warning);
				return false;
			}

			PropertyInfo androidProp = settingsType.GetProperty("GoogleMobileAdsAndroidAppId", BindingFlags.Public | BindingFlags.Instance);
			PropertyInfo iosProp = settingsType.GetProperty("GoogleMobileAdsIOSAppId", BindingFlags.Public | BindingFlags.Instance);
			if (androidProp == null && iosProp == null)
			{
				SendLog.LogModule(moduleName, "GoogleMobileAdsSettings App Id properties not found.", LogLevel.Warning);
				return false;
			}

			if (androidProp != null && androidProp.CanWrite)
			{
				androidProp.SetValue(instance, appId);
			}

			if (iosProp != null && iosProp.CanWrite)
			{
				iosProp.SetValue(instance, appId);
			}

			if (instance is UnityEngine.Object unityObj)
			{
				EditorUtility.SetDirty(unityObj);
			}

			SendLog.LogModule(moduleName, "GoogleMobileAdsSettings App Id synced from MonetizationKeys.json.");
			return true;
		}

		private static Type FindSettingsType()
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = null;
				try
				{
					type = assembly.GetType("GoogleMobileAds.Editor.GoogleMobileAdsSettings");
					if (type == null)
					{
						foreach (Type candidate in assembly.GetTypes())
						{
							if (candidate.Name == "GoogleMobileAdsSettings")
							{
								type = candidate;
								break;
							}
						}
					}
				}
				catch (ReflectionTypeLoadException)
				{
					continue;
				}

				if (type != null)
				{
					return type;
				}
			}

			return null;
		}

		private static object LoadInstance(Type settingsType)
		{
			MethodInfo loadInstance = settingsType.GetMethod("LoadInstance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (loadInstance != null)
			{
				return loadInstance.Invoke(null, null);
			}

			string[] guids = AssetDatabase.FindAssets("t:GoogleMobileAdsSettings");
			if (guids != null && guids.Length > 0)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[0]);
				return AssetDatabase.LoadAssetAtPath(path, settingsType);
			}

			return Resources.Load("GoogleMobileAdsSettings");
		}
	}
}
#endif
