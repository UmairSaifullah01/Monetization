using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace THEBADDEST.MonetizationEditor
{
	

	public abstract class CustomProfileEditor<T, T1> : Editor where T : Object, IEnumerable<T1> where T1 : ScriptableObject
	{

		protected virtual string collectionPropertyName { get; set; }
		protected virtual string collectionTitle        { get; set; }

		protected virtual string[] propertiesToShow { get; set; }

		protected DrawCollection<T1> drawCollection;

		protected virtual void OnGUIUpdate()
		{
			DrawTitle();
			DrawProperties();
			DrawCollections();
		}

		protected void DrawCollections()
		{
			if (drawCollection == null) drawCollection = new DrawCollection<T1>(collectionPropertyName, collectionTitle, serializedObject, target, typeof(T1));
			drawCollection.OnInspectorGUI();
		}

		protected void DrawProperties()
		{
			if (propertiesToShow == null || propertiesToShow.Length == 0) return;
			EditorTools.DrawLineHelpBox();
			EditorGUILayout.LabelField(new GUIContent("Properties"), EditorStyles.boldLabel);
			foreach (string propertyName in propertiesToShow)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName));
			}
		}

		protected virtual void DrawTitle()
		{
			EditorGUILayout.Space();
			GUILayout.BeginVertical("GroupBox");
			EditorGUILayout.Space();
			GUILayout.Label(typeof(T).Name, new GUIStyle() {normal = {textColor = Color.gray}});
			EditorGUILayout.Space();
			GUILayout.EndVertical();
			EditorGUILayout.Space();
			EditorTools.DrawLineHelpBox();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			OnGUIUpdate();
			serializedObject.ApplyModifiedProperties();
		}

	}


}