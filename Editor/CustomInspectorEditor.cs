using UnityEditor;
using UnityEngine;
using THEBADDEST.CustomProfileEditor;
using THEBADDEST.EditorUtls;

[CustomEditor(typeof(MonotizationProfile))]
public class CustomInspectorEditor : CustomProfileEditor<MonotizationProfile, MonotizationModule>
{
	protected override string              collectionTitle        => "Modules";
	protected override string              collectionPropertyName => "modules";
	DrawCollection<MonotizationModule> drawComponentCollection;
	bool                                   titleFoldout;

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
		EditorUtility.SetDirty(target as MonotizationProfile);
	}
}