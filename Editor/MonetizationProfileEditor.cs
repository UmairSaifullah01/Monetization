using THEBADDEST.MonetizationApi;
using UnityEditor;
using UnityEngine;


namespace THEBADDEST.MonetizationApi.Editor
{


	[CustomEditor(typeof(MonetizationProfile))]
	public class MonetizationProfileEditor : CustomProfileEditor<MonetizationProfile, MonetizationModule>
	{

		protected override string collectionTitle => "Modules";
		protected override string collectionPropertyName => "modules";

		private static readonly Color SyncButtonColor = new Color(0.22f, 0.45f, 0.85f);

		private Texture2D logoTexture;

		private void OnEnable()
		{
			logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Monetization/Logo/logo.png");
		}

		protected override void DrawTitle()
		{
			EditorGUILayout.Space(4);
			GUILayout.BeginVertical(EditorTools.Window);
			EditorGUILayout.Space(4);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (logoTexture != null)
			{
				GUILayout.Label(logoTexture, GUILayout.Width(56), GUILayout.Height(56));
				GUILayout.Space(8);
			}

			var titleStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = 24,
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.MiddleLeft,
				normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
			};
			GUILayout.Label("Monetization Profile", titleStyle, GUILayout.Height(44));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Label(
				"Version 4.1 - Developed by Umair Saifullah",
				new GUIStyle
				{
					alignment = TextAnchor.LowerRight,
					fontStyle = FontStyle.Italic,
					normal = { textColor = Color.gray }
				});
			EditorGUILayout.Space(2);
			GUILayout.EndVertical();
			EditorGUILayout.Space(4);
		}

		private void DrawSettingsSections()
		{
			EditorTools.DrawSectionTitle("Logs");
			EditorTools.DrawToggleRow("console.infoicon.sml", "Enable Debug Logs", FindProp("enableDebugLogs"));
			EditorTools.DrawPropertyRow("UnityEditor.ConsoleWindow", "Log Level", FindProp("logLevel"));
			EditorTools.DrawToggleRow("Profiler.UI", "Performance Logging", FindProp("enablePerformanceLogging"));

			EditorTools.DrawSectionTitle("Initialization");
			EditorTools.DrawPropertyRow("Refresh", "Max Retry Attempts", FindProp("maxRetryAttempts"));
			EditorTools.DrawPropertyRow("TestStopwatch", "Retry Delay Seconds", FindProp("retryDelaySeconds"));
			EditorTools.DrawToggleRow("BuildSettings.Web.Small", "Internet Before Init", FindProp("checkInternetBeforeInit"));
			EditorTools.DrawToggleRow("TestPassed", "Validate On Start", FindProp("validateModulesOnStart"));

			EditorTools.DrawSectionTitle("Build");
			EditorTools.DrawToggleRow("AssemblyLock", "Apply Keystore", FindProp("useKeyStore"));
		}

		private SerializedProperty FindProp(string propertyName)
		{
			return serializedObject.FindProperty(propertyName);
		}

		protected override void OnGUIUpdate()
		{
			serializedObject.Update();
			DrawTitle();
			DrawSettingsSections();

			var profile = target as MonetizationProfile;
			var warnings = ProviderProfileValidator.Validate(profile);
			if (warnings.Count > 0)
			{
				EditorTools.DrawSectionTitle("Validation");
				EditorGUILayout.HelpBox(string.Join("\n", warnings), MessageType.Warning);
			}

			EditorTools.DrawSectionTitle("Modules");
			DrawCollections();

			EditorTools.DrawSectionTitle("Setup");
			EditorTools.DrawAccentButton("SYNC PROJECT", SyncButtonColor, () =>
			{
				serializedObject.ApplyModifiedProperties();
				ProjectSettingsSync.SyncFromJson(profile);
			});

			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(profile);
		}

	}


}
