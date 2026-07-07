using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace THEBADDEST.MonetizationEditor
{


	public class EditorData<T>
	{

		public T reference;
		public bool folded;
		public SerializedObject serializedObject;

	}

	public class DrawCollection<T> where T : ScriptableObject
	{

		readonly SerializedProperty collectionProperty;
		readonly SerializedObject serializedObject;
		readonly Object target;
		readonly string title;
		readonly Type moduleBaseType;
		EditorData<T>[] editorDataContainer;
		int oldLength = -1;
		static bool hide = false;
		static bool folded;

		public DrawCollection(string collectionPropertyName, string title, SerializedObject serializedObject, Object target, Type collectionParentType)
		{
			this.target = target;
			this.serializedObject = serializedObject;
			this.title = title;
			collectionProperty = this.serializedObject.FindProperty(collectionPropertyName);
			moduleBaseType = collectionParentType;
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
				var editorData = editorDataContainer[i];
				if (editorData.reference == null || editorData.serializedObject?.targetObject == null)
				{
					continue;
				}

				EditorGUILayout.BeginVertical(GUI.skin.box);
				int cache = i;
				editorData.folded = EditorTools.DrawHeaderFoldoutLessWithButton(editorData.reference.GetType().Name, editorData.folded, EditorGUIUtility.IconContent("Toolbar Minus"), () => RemoveType(cache));
				if (editorData.folded)
				{
					EditorTools.DrawScript(editorData.reference);
					EditorTools.DrawAllFields(editorData.reference, editorData.serializedObject, false);
				}

				EditorGUILayout.EndVertical();
			}

			EditorTools.DrawAddRemoveButton(DrawAddMenu, DrawRemoveMenu);
			EditorGUILayout.EndVertical();
		}

		void OnHide()
		{
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
			var menu = new GenericMenu();
			int addableCount = 0;
			foreach (var type in EditorTools.GetInheritedClasses(moduleBaseType))
			{
				if (TypeExistInCollection(type))
				{
					continue;
				}

				menu.AddItem(new GUIContent(type.Name), false, () => AddType(type));
				addableCount++;
			}

			if (addableCount == 0)
			{
				menu.AddDisabledItem(new GUIContent("No module types found — install a provider assembly first"));
			}

			menu.ShowAsContext();
		}

		void DrawRemoveMenu()
		{
			var menu = new GenericMenu();
			for (int i = 0; i < collectionProperty.arraySize; i++)
			{
				SerializedProperty arrayElementAtIndex = collectionProperty.GetArrayElementAtIndex(i);
				if (arrayElementAtIndex.objectReferenceValue == null)
				{
					continue;
				}

				Type type = arrayElementAtIndex.objectReferenceValue.GetType();
				var title = new GUIContent(type.Name);
				int cachedI = i;
				menu.AddItem(title, false, () => RemoveType(cachedI));
			}

			menu.ShowAsContext();
		}

		bool TypeExistInCollection(Type type)
		{
			for (int i = 0; i < collectionProperty.arraySize; i++)
			{
				SerializedProperty arrayElementAtIndex = collectionProperty.GetArrayElementAtIndex(i);
				if (arrayElementAtIndex.objectReferenceValue != null &&
				    arrayElementAtIndex.objectReferenceValue.GetType() == type)
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
			InvalidateSubEditors();
			if (EditorUtility.IsPersistent(target))
			{
				EditorUtility.SetDirty(target);
				AssetDatabase.SaveAssets();
			}
		}

		void RemoveType(int id)
		{
			serializedObject.Update();
			if (id < 0 || id >= collectionProperty.arraySize)
			{
				return;
			}

			var property = collectionProperty.GetArrayElementAtIndex(id);
			var instance = property.objectReferenceValue;
			property.objectReferenceValue = null;
			serializedObject.ApplyModifiedProperties();
			collectionProperty.DeleteArrayElementAtIndex(id);
			if (id < collectionProperty.arraySize && collectionProperty.GetArrayElementAtIndex(id).objectReferenceValue == null)
			{
				collectionProperty.DeleteArrayElementAtIndex(id);
			}

			serializedObject.ApplyModifiedProperties();
			if (instance != null)
			{
				Undo.DestroyObjectImmediate(instance);
			}

			InvalidateSubEditors();
			EditorUtility.SetDirty(target);
			AssetDatabase.SaveAssets();
			GUIUtility.ExitGUI();
		}

		void InvalidateSubEditors()
		{
			oldLength = -1;
			editorDataContainer = null;
		}

		void InitSubEditors()
		{
			int count = collectionProperty.arraySize;
			bool needsRebuild = count != oldLength || editorDataContainer == null || editorDataContainer.Length != count;

			if (!needsRebuild)
			{
				for (int j = 0; j < count; j++)
				{
					var element = collectionProperty.GetArrayElementAtIndex(j);
					var obj = element.objectReferenceValue as T;
					if (editorDataContainer[j].reference != obj ||
					    editorDataContainer[j].serializedObject == null ||
					    editorDataContainer[j].serializedObject.targetObject == null)
					{
						needsRebuild = true;
						break;
					}
				}
			}

			if (!needsRebuild)
			{
				return;
			}

			oldLength = count;
			editorDataContainer = new EditorData<T>[count];
			for (int j = 0; j < count; j++)
			{
				SerializedProperty element = collectionProperty.GetArrayElementAtIndex(j);
				var obj = element.objectReferenceValue;
				editorDataContainer[j] = new EditorData<T>()
				{
					reference = obj as T,
					serializedObject = obj != null ? new SerializedObject(obj) : null
				};
			}
		}

	}


}