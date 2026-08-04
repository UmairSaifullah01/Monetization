#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Ads
{
	internal static class AppLovinSettingsSync
	{
		private static readonly string[] SdkKeyPropertyNames = { "SdkKey", "SDKKey" };
		private static readonly string[] AdMobAndroidPropertyNames =
		{
			"AdMobAndroidAppId",
			"AdMobAndroidAppID",
			"GoogleAndroidAppId"
		};
		private static readonly string[] AdMobIosPropertyNames =
		{
			"AdMobIosAppId",
			"AdMobIOSAppId",
			"AdMobIosAppID",
			"GoogleIosAppId",
			"GoogleIOSAppId"
		};

		public static bool ApplySettings(string sdkKey, string adMobAppId, string moduleName)
		{
			Type settingsType = FindAppLovinSettingsType();
			if (settingsType == null)
			{
				SendLog.LogModule(moduleName, "AppLovinSettings type not found. Is the AppLovin MAX plugin installed?", LogLevel.Warning);
				return false;
			}

			object instance = GetSettingsInstance(settingsType);
			if (instance == null)
			{
				SendLog.LogModule(moduleName, "Could not load AppLovinSettings instance.", LogLevel.Warning);
				return false;
			}

			bool wroteAnything = false;

			if (!string.IsNullOrEmpty(sdkKey))
			{
				if (TrySetMember(instance, settingsType, SdkKeyPropertyNames, sdkKey))
				{
					wroteAnything = true;
				}
				else
				{
					SendLog.LogModule(moduleName, "AppLovinSettings.SdkKey property not found.", LogLevel.Warning);
				}
			}
			else
			{
				SendLog.LogModule(moduleName, "MaxSdkKey not found in AdKeys JSON.", LogLevel.Warning);
			}

			if (!string.IsNullOrEmpty(adMobAppId))
			{
				bool wroteAndroid = TrySetMember(instance, settingsType, AdMobAndroidPropertyNames, adMobAppId);
				bool wroteIos = TrySetMember(instance, settingsType, AdMobIosPropertyNames, adMobAppId);
				if (wroteAndroid || wroteIos)
				{
					wroteAnything = true;
				}
				else
				{
					SendLog.LogModule(moduleName, "AppLovinSettings AdMob App Id properties not found.", LogLevel.Warning);
				}
			}
			else
			{
				SendLog.LogModule(moduleName, "AdMob AppId not found in AdKeys JSON.", LogLevel.Warning);
			}

			if (!wroteAnything)
			{
				return false;
			}

			if (instance is UnityEngine.Object unityObj)
			{
				EditorUtility.SetDirty(unityObj);
			}

			SendLog.LogModule(moduleName, "AppLovinSettings SDK key and AdMob App Id synced from MonetizationKeys.json.");
			return true;
		}

		private static bool TrySetMember(object instance, Type settingsType, string[] names, string value)
		{
			foreach (string name in names)
			{
				PropertyInfo prop = settingsType.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (prop != null && prop.CanWrite && prop.PropertyType == typeof(string))
				{
					prop.SetValue(instance, value);
					return true;
				}

				FieldInfo field = settingsType.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (field != null && field.FieldType == typeof(string))
				{
					field.SetValue(instance, value);
					return true;
				}

				// Serialized private fields often use camelCase (adMobAndroidAppId).
				string camel = char.ToLowerInvariant(name[0]) + name.Substring(1);
				field = settingsType.GetField(camel, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (field != null && field.FieldType == typeof(string))
				{
					field.SetValue(instance, value);
					return true;
				}
			}

			return false;
		}

		private static Type FindAppLovinSettingsType()
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = null;
				try
				{
					type = assembly.GetType("AppLovinSettings");
					if (type == null)
					{
						foreach (Type candidate in assembly.GetTypes())
						{
							if (candidate.Name == "AppLovinSettings")
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

		private static object GetSettingsInstance(Type settingsType)
		{
			PropertyInfo instanceProp = settingsType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
			if (instanceProp != null)
			{
				object instance = instanceProp.GetValue(null);
				if (instance != null)
				{
					return instance;
				}
			}

			string[] guids = AssetDatabase.FindAssets("t:AppLovinSettings");
			if (guids != null && guids.Length > 0)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[0]);
				return AssetDatabase.LoadAssetAtPath(path, settingsType);
			}

			return null;
		}
	}
}
#endif
