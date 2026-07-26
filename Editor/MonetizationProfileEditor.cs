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

		bool generalSettingsFoldout = true;
		private Texture2D logoTexture;
		private void OnEnable()
		{
			logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Monetization/Logo/logo.png");
		}

		protected override void DrawTitle()
		{
			EditorGUILayout.Space();
			GUILayout.BeginVertical(EditorTools.Window);
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (logoTexture != null)
			{
				GUILayout.Label(logoTexture, GUILayout.Width(70), GUILayout.Height(70));
				GUILayout.Space(10);
			}
			var titleStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = 28,
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.MiddleLeft,
				normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
			};
			GUILayout.Label("Monetization Profile", titleStyle, GUILayout.Height(50));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			GUILayout.Label("Version 4.0b - Developed by Umair Saifullah", new GUIStyle() { alignment = TextAnchor.LowerRight, fontStyle = FontStyle.Italic, normal = { textColor = Color.gray } });
			EditorGUILayout.Space();
			GUILayout.EndVertical();
			EditorGUILayout.Space(10);
		}

		private void DrawGeneralSettings()
		{
			EditorGUILayout.BeginVertical(EditorTools.Window);
			generalSettingsFoldout = EditorGUILayout.Foldout(generalSettingsFoldout, "General Settings", true, EditorTools.BoldFoldout);
			if (generalSettingsFoldout)
			{
				EditorGUI.indentLevel++;
				DrawProp("enableDebugLogs");
				DrawProp("logLevel");
				DrawProp("enablePerformanceLogging");
				DrawProp("maxRetryAttempts");
				DrawProp("retryDelaySeconds");
				DrawProp("checkInternetBeforeInit");
				DrawProp("validateModulesOnStart");
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space(8);
		}

		private void DrawProp(string propertyName)
		{
			var prop = serializedObject.FindProperty(propertyName);
			if (prop != null)
			{
				EditorGUILayout.PropertyField(prop, true);
			}
		}

		protected override void OnGUIUpdate()
		{
			serializedObject.Update();
			DrawTitle();
			DrawGeneralSettings();
			ProviderProfileValidator.DrawWarnings(target as MonetizationProfile);
			DrawCollections();
			EditorGUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			if (GUILayout.Button("Sync Project", GUILayout.Width(200), GUILayout.Height(40)))
			{
				serializedObject.ApplyModifiedProperties();
				ProjectSettingsSync.SyncFromJson(target as MonetizationProfile);
			}

			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target as MonetizationProfile);
		}

	}


}
