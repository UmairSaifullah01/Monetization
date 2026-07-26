using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
#endif


namespace THEBADDEST.MonetizationApi.Editor
{


	#if UNITY_EDITOR
	public static class EditorTools
	{

		public static GUIStyle BoldFoldout
		{
			get
			{
				var boldFoldout = new GUIStyle(EditorStyles.foldout);
				boldFoldout.fontStyle = FontStyle.Bold;
				return boldFoldout;
			}
		}
		public static GUIStyle Window
		{
			get
			{
				GUIStyle   skinWindow        = GUI.skin.window;
				RectOffset skinWindowPadding = skinWindow.padding;
				skinWindowPadding.top = 5;
				return skinWindow;
			}
		}


		#region Styles

		public static GUIStyle StyleGray   => Style(new Color(0.5f,  0.5f,  0.5f,  0.3f));
		public static GUIStyle StyleBlue   => Style(new Color(0,     0.5f,  1f,    0.3f));
		public static GUIStyle StyleRed    => Style(new Color(1,     0.3f,  0f,    0.3f));
		public static GUIStyle StyleGreen  => Style(new Color(0f,    1f,    0.5f,  0.3f));
		public static GUIStyle StyleOrange => Style(new Color(1f,    0.5f,  0.0f,  0.3f));
		public static GUIStyle Border      => Style(new Color(0,     0.5f,  1f,    0.0f));
		public static GUIStyle FlatBox     => Style(new Color(0.35f, 0.35f, 0.35f, 0.1f));

		#endregion


		public static GUIStyle Style(Color color)
		{
			GUIStyle currentStyle = new GUIStyle(GUI.skin.box) { border = new RectOffset(-1, -1, -1, -1) };
			Color[]  pix          = new Color[1];
			pix[0] = color;
			Texture2D bg = new Texture2D(1, 1);
			bg.SetPixels(pix);
			bg.Apply();
			currentStyle.normal.background = bg;
			#if UNITY_2019_4 || UNITY_2020
			// MW 04-Jul-2020: Check if system supports newer graphics formats used by Unity GUI
			Texture2D bgActual = currentStyle.normal.scaledBackgrounds[0];
			if (SystemInfo.IsFormatSupported(bgActual.graphicsFormat, UnityEngine.Experimental.Rendering.FormatUsage.Sample) == false)
			{
				currentStyle.normal.scaledBackgrounds = new Texture2D[] { }; // This can't be null
			}
			#endif
			return currentStyle;
		}

		public static GUIStyle Style()
		{
			GUIStyle currentStyle = new GUIStyle(GUI.skin.button) { border = new RectOffset(-1, -1, -1, -1) };
			#if UNITY_2019_4 || UNITY_2020
			// MW 04-Jul-2020: Check if system supports newer graphics formats used by Unity GUI
			Texture2D bgActual = currentStyle.normal.scaledBackgrounds[0];
			if (SystemInfo.IsFormatSupported(bgActual.graphicsFormat, UnityEngine.Experimental.Rendering.FormatUsage.Sample) == false)
			{
				currentStyle.normal.scaledBackgrounds = new Texture2D[] { }; // This can't be null
			}
			#endif
			return currentStyle;
		}

		public static string GetPropertyType(SerializedProperty property)
		{
			var type  = property.type;
			var match = Regex.Match(type, @"PPtr<\$(.*?)>");
			if (match.Success)
				type = match.Groups[1].Value;
			return type;
		}

		public static System.Type[] GetTypesByName(string className)
		{
			List<System.Type> returnVal = new List<System.Type>();
			foreach (Assembly a in System.AppDomain.CurrentDomain.GetAssemblies())
			{
				System.Type[] assemblyTypes = a.GetTypes();
				for (int j = 0; j < assemblyTypes.Length; j++)
				{
					if (assemblyTypes[j].Name == className)
					{
						returnVal.Add(assemblyTypes[j]);
					}
				}
			}

			return returnVal.ToArray();
		}

		public static System.Type GetTypeByName(string className)
		{
			return System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).FirstOrDefault(t => t.Name == className);
		}

		public static IEnumerable<Type> GetInheritedClasses(Type givenType)
		{
			return GetAllTypesDerivedFrom(givenType)
				.Where(t => t.IsClass && !t.IsAbstract && givenType.IsAssignableFrom(t));
		}

		public static IEnumerable<Type> GetAllTypesDerivedFrom(Type baseType)
		{
#if UNITY_EDITOR && UNITY_2019_2_OR_NEWER
			return TypeCache.GetTypesDerivedFrom(baseType);
#else
			return GetAllAssemblyTypes().Where(t => t.IsSubclassOf(baseType));
#endif
		}

		public static IEnumerable<Type> GetAllTypesDerivedFrom<TT>()
		{
			return GetAllTypesDerivedFrom(typeof(TT));
		}

		public static T CreateScriptableInstance<T>(Type type, Object parent = null, bool hide = false) where T : ScriptableObject
		{
			var instance = (T)ScriptableObject.CreateInstance(type);
			instance.name = type.Name;
			if (parent)
			{
				SetScriptableParent(instance, parent, hide);
			}

			return instance;
		}

		public static void SetScriptableParent<T>(T instance, Object parent, bool hide) where T : ScriptableObject
		{
			if (EditorUtility.IsPersistent(parent))
			{
				AssetDatabase.AddObjectToAsset(instance, parent);
				if (hide)
					instance.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
			}
		}

		public static void HidFlags<T>(T instance, bool hide) where T : Object
		{
			if (hide)
				instance.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
			else
				instance.hideFlags = HideFlags.None;
		}

		public static IEnumerable<Type> GetAllAssemblyTypes()
		{
			var assemblyTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t =>
			{
				var innerTypes = Type.EmptyTypes;
				try
				{
					innerTypes = t.GetTypes();
				}
				catch
				{
					// ignored
				}

				return innerTypes;
			});
			return assemblyTypes;
		}

		public static void DrawHeader(string title)
		{
			var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);
			var labelRect      = backgroundRect;
			labelRect.xMin += 16f;
			labelRect.xMax -= 20f;
			var foldoutRect = backgroundRect;
			foldoutRect.y      += 1f;
			foldoutRect.width  =  13f;
			foldoutRect.height =  13f;

			// Background rect should be full-width
			backgroundRect.xMin  =  0f;
			backgroundRect.width += 4f;

			// Background
			float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
			EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

			// Title
			EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);
			EditorGUILayout.Space();
		}

		public static void DrawSplitter()
		{
			EditorGUILayout.Space();
			var rect = GUILayoutUtility.GetRect(1f, 1f);

			// Splitter rect should be full-width
			rect.xMin  =  20f;
			rect.width += 4f;
			if (Event.current.type != EventType.Repaint)
				return;
			EditorGUI.DrawRect(rect, !EditorGUIUtility.isProSkin ? new Color(0.6f, 0.6f, 0.6f, 1.333f) : new Color(0.12f, 0.12f, 0.12f, 1.333f));
		}

		public static bool DrawHeaderFoldout(string title, bool state)
		{
			var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);
			var labelRect      = backgroundRect;
			labelRect.xMin += 16f;
			labelRect.xMax -= 20f;
			var foldoutRect = backgroundRect;
			foldoutRect.y      += 1f;
			foldoutRect.width  =  13f;
			foldoutRect.height =  13f;

			// Background rect should be full-width
			backgroundRect.xMin  =  0f;
			backgroundRect.width += 4f;

			// Background
			float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
			EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

			// Title
			EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);

			// Active checkbox
			state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);
			var e = Event.current;
			if (e.type == EventType.MouseDown && backgroundRect.Contains(e.mousePosition) && e.button == 0)
			{
				state = !state;
				e.Use();
			}

			return state;
		}

		public static bool DrawHeaderFoldoutLessWidth(string title, bool state)
		{
			var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);
			var labelRect      = backgroundRect;
			labelRect.xMin += 16f;
			labelRect.xMax -= 20f;
			var foldoutRect = backgroundRect;
			foldoutRect.y      += 1f;
			foldoutRect.width  =  13f;
			foldoutRect.height =  13f;

			// Background
			// float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
			// EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

			// Title
			EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);

			// Active checkbox
			state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);
			var e = Event.current;
			if (e.type == EventType.MouseDown && backgroundRect.Contains(e.mousePosition) && e.button == 0)
			{
				state = !state;
				e.Use();
			}

			return state;
		}

		public static bool DrawHeaderFoldoutWithEnableAndRemove(string title, bool state, bool enabled, out bool newEnabled, Action onRemove)
		{
			var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);
			var foldoutRect = backgroundRect;
			foldoutRect.y += 1f;
			foldoutRect.width = 13f;
			foldoutRect.height = 13f;

			float removeSize = 30f;
			float enableWidth = 55f;
			var removeRect = backgroundRect;
			removeRect.x = backgroundRect.xMax - removeSize;
			removeRect.width = removeSize;
			removeRect.height = backgroundRect.height;

			var enableRect = backgroundRect;
			enableRect.x = removeRect.x - enableWidth - 2f;
			enableRect.width = enableWidth;
			enableRect.height = backgroundRect.height;

			var labelRect = backgroundRect;
			labelRect.xMin += 16f;
			labelRect.xMax = enableRect.x - 4f;

			newEnabled = GUI.Toggle(enableRect, enabled, enabled ? "On" : "Off", EditorStyles.miniButton);
			if (GUI.Button(removeRect, EditorGUIUtility.IconContent("Toolbar Minus"), EditorStyles.miniButton))
			{
				onRemove?.Invoke();
			}

			EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);
			state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);
			var e = Event.current;
			if (e.type == EventType.MouseDown && labelRect.Contains(e.mousePosition) && e.button == 0)
			{
				state = !state;
				e.Use();
			}

			return state;
		}

		public static bool DrawHeaderFoldoutLessWithButton(string title, bool state, GUIContent buttonTitle, Action onButtonClick)
		{
			var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);
			var labelRect      = backgroundRect;
			labelRect.xMin += 16f;
			labelRect.xMax -= 20f;
			var foldoutRect = backgroundRect;
			foldoutRect.y      += 1f;
			foldoutRect.width  =  13f;
			foldoutRect.height =  13f;
			float buttonSize = 30f;
			var   buttonRect = backgroundRect;
			buttonRect.x      = backgroundRect.xMax - buttonSize;
			buttonRect.width  = buttonSize;
			buttonRect.height = backgroundRect.height;
			if (GUI.Button(buttonRect, buttonTitle, EditorStyles.miniButton))
			{
				onButtonClick?.Invoke();
			}
			// Background
			// float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
			// EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

			// Title
			EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);

			// Active checkbox
			state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);
			var e = Event.current;
			if (e.type == EventType.MouseDown && backgroundRect.Contains(e.mousePosition) && e.button == 0)
			{
				state = !state;
				e.Use();
			}

			return state;
		}

		public static void EditorWindowWithHeader(string title)
		{
			EditorGUILayout.Space(10);
			GUILayout.BeginVertical(EditorTools.Window);
			EditorGUILayout.Space();
			var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);
			var labelRect      = backgroundRect;
			labelRect.xMin += 6f;
			labelRect.xMax -= 20f;
			var foldoutRect = backgroundRect;
			foldoutRect.y      += 1f;
			foldoutRect.width  =  13f;
			foldoutRect.height =  13f;
			
			// Title
			EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);
		}

		public static void EditorWindowClose()
		{
			EditorGUILayout.Space();
			GUILayout.EndVertical();
			EditorGUILayout.Space();
		}
		public static bool DrawHeaderFoldoutLessWidthHide(string title, bool state, Action onHide)
		{
			var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);
			var labelRect      = backgroundRect;
			labelRect.xMin += 16f;
			labelRect.xMax -= 20f;
			var foldoutRect = backgroundRect;
			foldoutRect.y      += 1f;
			foldoutRect.width  =  13f;
			foldoutRect.height =  13f;

			// Background
			// float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
			// EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

			// Title
			EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);

			// Active checkbox
			state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);
			var e = Event.current;
			if (e.type == EventType.MouseDown && backgroundRect.Contains(e.mousePosition) && e.button == 0)
			{
				state = !state;
				e.Use();
			}

			if (e.type == EventType.KeyDown && backgroundRect.Contains(e.mousePosition) && e.keyCode == KeyCode.H)
			{
				onHide?.Invoke();
				e.Use();
			}

			return state;
		}

		public static Rect DrawBox(float height = 20f)
		{
			var backgroundRect = GUILayoutUtility.GetRect(1f, height);
			var labelRect      = backgroundRect;
			labelRect.xMin += 16f;
			labelRect.xMax -= 20f;
			// Background rect should be full-width
			backgroundRect.xMin  =  0f;
			backgroundRect.width += 4f;

			// Background
			float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
			EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));
			return backgroundRect;
		}

		public static void DrawLineHelpBox()
		{
			EditorGUILayout.BeginVertical(EditorStyles.textField);
			EditorGUILayout.EndVertical();
		}

		public static void DrawScript(Object target)
		{
			EditorGUI.BeginDisabledGroup(true);

			// It can be a MonoBehaviour or a ScriptableObject
			var monoScript = (target as MonoBehaviour) != null ? MonoScript.FromMonoBehaviour((MonoBehaviour)target) : MonoScript.FromScriptableObject((ScriptableObject)target);
			EditorGUILayout.ObjectField("Script", monoScript, target.GetType(), false);
			EditorGUI.EndDisabledGroup();
			// EditorGUI.BeginDisabledGroup(true);
			// EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
			// EditorGUI.EndDisabledGroup();
		}
		
		public static void DrawAllFields(object target, SerializedObject serializedObject, bool includeBaseTypeFields = false, string[] skipFieldNames = null)
		{
			if (target == null || serializedObject == null || serializedObject.targetObject == null)
			{
				return;
			}

			serializedObject.Update();

			var shownFields = new HashSet<string>();
			var skip = skipFieldNames != null ? new HashSet<string>(skipFieldNames) : null;

			if (includeBaseTypeFields)
			{
				var typeChain = new List<Type>();
				for (Type type = target.GetType().BaseType;
				     type != null && type != typeof(ScriptableObject) && type != typeof(UnityEngine.Object) && type != typeof(object);
				     type = type.BaseType)
				{
					typeChain.Add(type);
				}

				// Draw from root base downward (e.g. MonetizationModule → AdsModule)
				for (int i = typeChain.Count - 1; i >= 0; i--)
				{
					DrawSerializedFieldsOfType(typeChain[i], serializedObject, shownFields, skip);
				}
			}

			DrawSerializedFieldsOfType(target.GetType(), serializedObject, shownFields, skip);

			if (serializedObject.targetObject != null)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}

		private static void DrawSerializedFieldsOfType(Type type, SerializedObject serializedObject, HashSet<string> shownFields, HashSet<string> skip)
		{
			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
				.Where(t => (t.IsPublic && !Attribute.IsDefined(t, typeof(NonSerializedAttribute)) && !Attribute.IsDefined(t, typeof(HideInInspector))) ||
				            Attribute.IsDefined(t, typeof(SerializeField)));

			foreach (var field in fields)
			{
				if (shownFields.Contains(field.Name)) continue;
				if (skip != null && skip.Contains(field.Name)) continue;

				SerializedProperty property = serializedObject.FindProperty(field.Name);
				if (property == null) continue;

				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(10);
				EditorGUILayout.PropertyField(property, true);
				EditorGUILayout.EndHorizontal();
				shownFields.Add(field.Name);
			}
		}

		public static void DrawAddRemoveButton(Action addEvent, Action removeEvent)
		{
			var   backgroundRect = GUILayoutUtility.GetRect(1f, 20f);
			float buttonSize     = 30f;
			float buttonHeight   = backgroundRect.height - 2f;
			float buttonSpace    = 5f;
			// Add Button
			var addButtonRect = backgroundRect;
			addButtonRect.x      =  backgroundRect.xMax - buttonSize * 2 - buttonSpace;
			addButtonRect.width  =  buttonSize;
			addButtonRect.y      += 1f;
			addButtonRect.height =  buttonHeight;

			// Remove Button
			var removeButtonRect = backgroundRect;
			removeButtonRect.x      =  backgroundRect.xMax - buttonSize;
			removeButtonRect.width  =  buttonSize;
			removeButtonRect.y      += 1f;
			removeButtonRect.height =  buttonHeight;
			if (GUI.Button(removeButtonRect, EditorGUIUtility.IconContent("Toolbar Plus More")))
			{
				addEvent?.Invoke();
			}

			// if (GUI.Button(removeButtonRect, EditorGUIUtility.IconContent("Toolbar Minus")))
			// {
			// 	removeEvent?.Invoke();
			// }
		}

		public static void DrawDescription(string v)
		{
			EditorGUILayout.BeginVertical(StyleBlue);
			EditorGUILayout.HelpBox(v, MessageType.None);
			EditorGUILayout.EndVertical();
		}

		public static void BoolButton(SerializedProperty prop, GUIContent content)
		{
			prop.boolValue = GUILayout.Toggle(prop.boolValue, content, EditorStyles.miniButton);
		}

		public static void Arrays(SerializedProperty prop, GUIContent content = null)
		{
			EditorGUI.indentLevel++;
			if (content != null)
				EditorGUILayout.PropertyField(prop, content, true);
			else
				EditorGUILayout.PropertyField(prop, true);
			EditorGUI.indentLevel--;
		}

		public static bool Foldout(SerializedProperty prop, string name, bool bold = false)
		{
			EditorGUI.indentLevel++;
			if (bold)
				prop.boolValue = EditorGUILayout.Foldout(prop.boolValue, name, BoldFoldout);
			else
				prop.boolValue = EditorGUILayout.Foldout(prop.boolValue, name);
			EditorGUI.indentLevel--;
			return prop.boolValue;
		}

	}
	#endif


}