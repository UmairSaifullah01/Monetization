#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Analytics
{
	internal static class FacebookSettingsSync
	{
		public static bool ApplyKeys(string appId, string clientToken, string moduleName)
		{
			if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(clientToken))
			{
				SendLog.LogModule(moduleName, "Facebook AppId/ClientToken not found in MonetizationKeys.json.", LogLevel.Warning);
				return false;
			}

			Type settingsType = FindFacebookSettingsType();
			if (settingsType == null)
			{
				SendLog.LogModule(moduleName, "FacebookSettings type not found. Is the Facebook SDK installed?", LogLevel.Warning);
				return false;
			}

			PropertyInfo appIdsProp = settingsType.GetProperty("AppIds", BindingFlags.Public | BindingFlags.Static);
			PropertyInfo clientTokensProp = settingsType.GetProperty("ClientTokens", BindingFlags.Public | BindingFlags.Static);
			if (appIdsProp == null || clientTokensProp == null || !appIdsProp.CanWrite || !clientTokensProp.CanWrite)
			{
				SendLog.LogModule(moduleName, "FacebookSettings.AppIds/ClientTokens properties not found.", LogLevel.Warning);
				return false;
			}

			appIdsProp.SetValue(null, new List<string> { appId });
			clientTokensProp.SetValue(null, new List<string> { clientToken });

			object instance = GetSettingsInstance(settingsType);
			if (instance is UnityEngine.Object unityObj)
			{
				EditorUtility.SetDirty(unityObj);
			}

			SendLog.LogModule(moduleName, "FacebookSettings AppId/ClientToken synced from MonetizationKeys.json.");
			return true;
		}

		private static Type FindFacebookSettingsType()
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = null;
				try
				{
					type = assembly.GetType("Facebook.Unity.Settings.FacebookSettings")
						?? assembly.GetType("Facebook.Unity.FacebookSettings");
					if (type == null)
					{
						foreach (Type candidate in assembly.GetTypes())
						{
							if (candidate.Name == "FacebookSettings")
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
			PropertyInfo instanceProp = settingsType.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (instanceProp != null)
			{
				object instance = instanceProp.GetValue(null);
				if (instance != null)
				{
					return instance;
				}
			}

			string[] guids = AssetDatabase.FindAssets("t:FacebookSettings");
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
