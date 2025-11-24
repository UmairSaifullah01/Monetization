using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.MonetizationEditor
{
    [CustomEditor(typeof(JsonDataSO))]
    public class JsonDataSOEditor : Editor
    {
        private const float HEADER_HEIGHT = 24f;
        private const float LINE_HEIGHT = 20f;
        private const float COLUMN_SPACING = 10f;

        // Colors based on the reference image
        private readonly Color KeyColor = new Color(0.0f, 0.8f, 0.8f); // Cyan/Teal
        private readonly Color ValueColor = new Color(0.3f, 0.6f, 1.0f); // Light Blue
        private readonly Color LineColor = new Color(0.3f, 0.3f, 0.3f); // Dark Grey separator
        private readonly Color HeaderBgColor = new Color(0.2f, 0.2f, 0.2f); // Darker background for header
        private readonly Color HeaderTextColor = new Color(0.8f, 0.8f, 0.8f);

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Reload JSON Data"))
            {
                JsonDataUtility.Reload();
            }

            EditorGUILayout.Space(10);

            // Ensure data is loaded
            JsonDataUtility.LoadData();
            var categories = JsonDataUtility.GetAllCategories();

            if (categories.Count == 0)
            {
                EditorGUILayout.HelpBox("No categories found in JSON data.", MessageType.Info);
                return;
            }

            foreach (var category in categories)
            {
                DrawCategory(category);
                EditorGUILayout.Space(5);
            }
        }

        private void DrawCategory(string category)
        {
            var data = JsonDataUtility.GetCategory(category);
            if (data == null) return;

            // 1. Draw Header (Category Name)
            Rect headerRect = EditorGUILayout.GetControlRect(false, HEADER_HEIGHT);
            EditorGUI.DrawRect(headerRect, HeaderBgColor);
            
            var headerStyle = new GUIStyle(EditorStyles.boldLabel) 
            { 
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = HeaderTextColor },
                padding = new RectOffset(5, 0, 0, 0)
            };
            EditorGUI.LabelField(headerRect, $"{category}", headerStyle);

            // 2. Draw Content
            if (data.Count == 0)
            {
                EditorGUILayout.LabelField("No keys found.", EditorStyles.miniLabel);
            }
            else
            {
                // Styles
                var keyStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = KeyColor } };
                var valueStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = ValueColor }, wordWrap = false };

                foreach (var kvp in data)
                {
                    Rect lineRect = EditorGUILayout.GetControlRect(false, LINE_HEIGHT);
                    
                    // Draw Separator Line (Top of the row)
                    Rect separatorRect = new Rect(lineRect.x, lineRect.y, lineRect.width, 1f);
                   // EditorGUI.DrawRect(separatorRect, LineColor);

                    float keyWidth = lineRect.width * 0.35f;
                    float valueWidth = lineRect.width * 0.65f - COLUMN_SPACING;

                    // Draw Key
                    Rect keyRect = new Rect(lineRect.x, lineRect.y + 2, keyWidth, LINE_HEIGHT - 2);
                    EditorGUI.LabelField(keyRect, kvp.Key, keyStyle);

                    // Draw Value
                    Rect valueRect = new Rect(lineRect.x + keyWidth + COLUMN_SPACING, lineRect.y + 2, valueWidth, LINE_HEIGHT - 2);
                    EditorGUI.LabelField(valueRect, kvp.Value, valueStyle);
                }
                
                // Draw Bottom Line for the last item
                Rect bottomLineRect = EditorGUILayout.GetControlRect(false, 1f);
                EditorGUI.DrawRect(bottomLineRect, LineColor);
            }
        }
    }
}
