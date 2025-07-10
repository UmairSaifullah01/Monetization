using THEBADDEST.MonetizationApi;
using UnityEditor;
using UnityEngine;


namespace THEBADDEST.MonetizationEditor
{

	[CustomEditor(typeof(MonetizationProfile))]
	public class MonetizationProfileEditor : CustomProfileEditor<MonetizationProfile, MonetizationModule>
	{
		protected override string              collectionTitle        => "Modules";
		protected override string              collectionPropertyName => "modules";
		DrawCollection<MonetizationModule> drawComponentCollection;
		bool                                   titleFoldout;
		bool                                   settingsFoldout;
		protected override void DrawTitle()
		{
			EditorGUILayout.Space();
			GUILayout.BeginVertical(EditorTools.Window);
			EditorGUILayout.Space();
			GUILayout.Label("Monetization Profile", new GUIStyle {fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = {textColor = Color.gray}});
			EditorGUILayout.Space();
			GUILayout.Label("Version 4.0b - Developed by Umair Saifullah", new GUIStyle() {alignment = TextAnchor.LowerRight, fontStyle = FontStyle.Italic, normal = {textColor = Color.gray}});
			EditorGUILayout.Space();
			GUILayout.EndVertical();
			EditorGUILayout.Space(10);
		}


		protected override void GUI()
		{
			base.GUI();
			EditorGUILayout.Space(10);
			EditorGUILayout.BeginVertical(EditorTools.Window);
			settingsFoldout = EditorTools.DrawHeaderFoldoutLessWidthHide("Settings", settingsFoldout, OnHide);
			if (!settingsFoldout)
			{
				EditorGUILayout.EndVertical();
				return;
			}
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Debug Log");
			if (EditorGUILayout.Toggle(serializedObject.FindProperty("debugLog").boolValue, EditorStyles.toggle))
				serializedObject.FindProperty("debugLog").boolValue = true;
			else
				serializedObject.FindProperty("debugLog").boolValue = false;
			EditorGUILayout.EndHorizontal();
			
			if (GUILayout.Button("Update Project", GUILayout.Width(200)))
			{
				(serializedObject.targetObject as MonetizationProfile)?.UpdateModules();
			}
			
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space(10);
			EditorUtility.SetDirty(target as MonetizationProfile);
		}

		void OnHide()
		{
			
		}

	}
	


}

