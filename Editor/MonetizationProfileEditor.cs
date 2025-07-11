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
			DrawTitle();
			EditorTools.EditorWindowWithHeader("Settings");

			SerializedProperty packageNameProp = serializedObject.FindProperty("packageName");
			SerializedProperty versionProp = serializedObject.FindProperty("version");
			SerializedProperty bundleVersionCodeProp = serializedObject.FindProperty("bundleVersionCode");
			SerializedProperty minApiLevelProp = serializedObject.FindProperty("minApiLevel");
			SerializedProperty targetApiLevelProp = serializedObject.FindProperty("targetApiLevel");
			SerializedProperty useKeyStoreProp = serializedObject.FindProperty("useKeyStore");
			SerializedProperty keyStorePathProp = serializedObject.FindProperty("keyStorePath");
			SerializedProperty keyAliasNameProp = serializedObject.FindProperty("keyAliasName");
			SerializedProperty keyStorePasswordProp = serializedObject.FindProperty("keyStorePassword");
			SerializedProperty keyAliasPasswordProp = serializedObject.FindProperty("keyAliasPassword");

			EditorGUILayout.LabelField("Project Info", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(packageNameProp, new GUIContent("Package Name"));
			EditorGUILayout.PropertyField(versionProp, new GUIContent("Version"));
			EditorGUILayout.PropertyField(bundleVersionCodeProp, new GUIContent("Bundle Version Code"));
			EditorGUILayout.PropertyField(minApiLevelProp, new GUIContent("Min API Level"));
			EditorGUILayout.PropertyField(targetApiLevelProp, new GUIContent("Target API Level"));

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Keystore", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(useKeyStoreProp, new GUIContent("Use KeyStore"));
			if (useKeyStoreProp.boolValue)
			{
				EditorGUILayout.PropertyField(keyStorePathProp, new GUIContent("KeyStore Path"));
				EditorGUILayout.PropertyField(keyAliasNameProp, new GUIContent("Alias Name"));
				EditorGUILayout.PropertyField(keyStorePasswordProp, new GUIContent("KeyStore Password"));
				EditorGUILayout.PropertyField(keyAliasPasswordProp, new GUIContent("Alias Password"));
			}

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();

			if (GUILayout.Button("Update Project", GUILayout.Width(200),GUILayout.Height(40)))
			{
				serializedObject.ApplyModifiedProperties();
				(serializedObject.targetObject as MonetizationProfile)?.UpdateProjectDetails();
			}
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			EditorUtility.SetDirty(target as MonetizationProfile);

			EditorTools.EditorWindowClose();

			DrawCollections();
			EditorGUILayout.Space(10);
		}

		void OnHide()
		{
			
		}

	}
	


}

