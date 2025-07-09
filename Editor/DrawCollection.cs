using System;
using System.Collections.Generic;
using THEBADDEST.EditorUtls;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace THEBADDEST.EditorUtls
{


	public class EditorData<T>
	{

		public T                reference;
		public bool             folded;
		public SerializedObject serializedObject;

	}

	public class DrawCollection<T> where T : ScriptableObject
	{

		readonly SerializedProperty collectionProperty;
		readonly IEnumerable<Type>  editorTypes;
		readonly SerializedObject   serializedObject;
		readonly Object             target;
		readonly string             title;
		EditorData<T>[]             editorDataContainer;
		int                         oldLength = -1;
		static bool                  hide      = false;
		static bool                 folded;

		public DrawCollection(string collectionPropertyName, string title, SerializedObject serializedObject, Object target, Type collectionParentType)
		{
			this.target           = target;
			this.serializedObject = serializedObject;
			this.title            = title;
			collectionProperty    = this.serializedObject.FindProperty(collectionPropertyName);
			editorTypes           = EditorTools.GetInheritedClasses(collectionParentType);
		}

		public void OnInspectorGUI()
		{
			EditorGUILayout.BeginVertical(EditorTools.Window);
			folded = EditorTools.DrawHeaderFoldoutLessWidthHide(title, folded, OnHide);
			if (!folded)
			{
				EditorGUILayout.EndVertical();
				return;
			}

			using var check = new EditorGUI.ChangeCheckScope();
			EditorGUILayout.Space();
			InitSubEditors();
			for (int i = 0; i < editorDataContainer.Length; i++)
			{
				if (editorDataContainer[i].reference != null)
				{
					EditorGUILayout.BeginVertical(GUI.skin.box);
					int cache = i;
					editorDataContainer[i].folded = EditorTools.DrawHeaderFoldoutLessWithButton(editorDataContainer[i].reference.GetType().Name, editorDataContainer[i].folded, EditorGUIUtility.IconContent("Toolbar Minus"),()=> RemoveType(cache));
					if (editorDataContainer[i].folded)
					{
						EditorTools.DrawScript(editorDataContainer[i].reference);
						EditorTools.DrawAllFields(editorDataContainer[i].reference, editorDataContainer[i].serializedObject, true);
					}

					EditorGUILayout.EndVertical();
				}
			}

			EditorTools.DrawAddRemoveButton(DrawAddMenu, DrawRemoveMenu);
			EditorGUILayout.EndVertical();
		}

		void OnHide()
		{
			Debug.Log("rrrrrrrrrrrrrrrrrrrrr");
			hide = !hide;
			for (int i = 0; i < collectionProperty.arraySize; i++)
			{
				SerializedProperty arrayElementAtIndex = collectionProperty.GetArrayElementAtIndex(i);
				EditorTools.HidFlags(arrayElementAtIndex.objectReferenceValue, hide);
			}
			AssetDatabase.Refresh();
		}

		void DrawAddMenu()
		{
			var menu    = new GenericMenu();
			var typeMap = editorTypes;
			foreach (var kvp in typeMap)
			{
				var  type   = kvp;
				var  title  = new GUIContent(type.Name);
				bool exists = TypeExistInCollection(type);
				if (!exists)
					menu.AddItem(title, false, () => AddType(type));
			}

			menu.ShowAsContext();
		}

		void DrawRemoveMenu()
		{
			var menu = new GenericMenu();
			for (int i = 0; i < collectionProperty.arraySize; i++)
			{
				SerializedProperty arrayElementAtIndex = collectionProperty.GetArrayElementAtIndex(i);
				Type               type                = arrayElementAtIndex.objectReferenceValue.GetType();
				var                title               = new GUIContent(type.Name);
				int                cachedI             = i;
				menu.AddItem(title, false, () => RemoveType(cachedI));
			}

			menu.ShowAsContext();
		}

		bool TypeExistInCollection(Type type)
		{
			for (int i = 0; i < collectionProperty.arraySize; i++)
			{
				SerializedProperty arrayElementAtIndex = collectionProperty.GetArrayElementAtIndex(i);
				if (arrayElementAtIndex.objectReferenceValue.GetType() == type)
				{
					return true;
				}
			}

			return false;
		}

		void AddType(Type type)
		{
			serializedObject.Update();
			var newItem = EditorTools.CreateScriptableInstance<T>(type, target);
			Undo.RegisterCreatedObjectUndo(newItem, "Added New Element");
			collectionProperty.arraySize++;
			var serializedProp = collectionProperty.GetArrayElementAtIndex(collectionProperty.arraySize - 1);
			serializedProp.objectReferenceValue = newItem;
			serializedObject.ApplyModifiedProperties();
			if (EditorUtility.IsPersistent(target))
			{
				EditorUtility.SetDirty(target);
				AssetDatabase.SaveAssets();
			}
		}

		void RemoveType(int id)
		{
			serializedObject.Update();
			var property = collectionProperty.GetArrayElementAtIndex(id);
			var instance = property.objectReferenceValue;
			property.objectReferenceValue = null;
			collectionProperty.DeleteArrayElementAtIndex(id);
			serializedObject.ApplyModifiedProperties();
			Undo.DestroyObjectImmediate(instance);
			EditorUtility.SetDirty(target);
			AssetDatabase.SaveAssets();
		}

		void InitSubEditors()
		{
			int count = collectionProperty.arraySize;
			if (count != oldLength)
			{
				oldLength           = count;
				editorDataContainer = new EditorData<T>[count];
				for (int j = 0; j < count; j++)
				{
					SerializedProperty element = collectionProperty.GetArrayElementAtIndex(j);
					editorDataContainer[j] = new EditorData<T>() { reference = element.objectReferenceValue as T, serializedObject = new SerializedObject(element.objectReferenceValue) };
				}
			}
		}

	}


}