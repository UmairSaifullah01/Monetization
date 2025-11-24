using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using THEBADDEST.MonetizationApi;

namespace THEBADDEST.MonetizationEditor
{
    [CustomPropertyDrawer(typeof(JsonDataCategoryAttribute))]
    public class JsonDataCategoryDrawer : PropertyDrawer
    {
        private const float HEADER_HEIGHT = 24f;
        private const float LINE_HEIGHT = 20f;
        private const float PADDING = 5f;
        private const float COLUMN_SPACING = 10f;

        // Colors based on the reference image
        private readonly Color KeyColor = new Color(0.0f, 0.8f, 0.8f); // Cyan/Teal
        private readonly Color ValueColor = new Color(0.3f, 0.6f, 1.0f); // Light Blue
        private readonly Color LineColor = new Color(0.3f, 0.3f, 0.3f); // Dark Grey separator

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [JsonDataCategory] with strings.");
                return;
            }

            JsonDataCategoryAttribute attr = (JsonDataCategoryAttribute)attribute;
            string category = attr.CategoryName;

            // Ensure data is loaded
            JsonDataUtility.LoadData();
            var data = JsonDataUtility.GetCategory(category);

            EditorGUI.BeginProperty(position, label, property);

            // 1. Draw Header (Category Name)
            Rect headerRect = new Rect(position.x, position.y, position.width, HEADER_HEIGHT);
            EditorGUI.DrawRect(headerRect, new Color(0.2f, 0.2f, 0.2f)); // Darker background for header
            
            var headerStyle = new GUIStyle(EditorStyles.boldLabel) 
            { 
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.8f, 0.8f, 0.8f) },
                padding = new RectOffset(5, 0, 0, 0)
            };
            EditorGUI.LabelField(headerRect, $" {category}", headerStyle);

            // 2. Draw Content
            float currentY = position.y + HEADER_HEIGHT;

            if (data == null || data.Count == 0)
            {
                Rect msgRect = new Rect(position.x, currentY, position.width, LINE_HEIGHT);
                EditorGUI.LabelField(msgRect, "No keys found.", EditorStyles.miniLabel);
            }
            else
            {
                // Styles
                var keyStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = KeyColor } };
                var valueStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = ValueColor }, wordWrap = false };

                float keyWidth = position.width * 0.35f; // 35% for key
                float valueWidth = position.width * 0.65f - COLUMN_SPACING; // Remaining for value

                foreach (var kvp in data)
                {
                    // Draw Separator Line
                    Rect lineRect = new Rect(position.x, currentY, position.width, 1f);
                    //EditorGUI.DrawRect(lineRect, LineColor);

                    // Draw Key
                    Rect keyRect = new Rect(position.x, currentY + 2, keyWidth, LINE_HEIGHT - 2);
                    EditorGUI.LabelField(keyRect, kvp.Key, keyStyle);

                    // Draw Value
                    Rect valueRect = new Rect(position.x + keyWidth + COLUMN_SPACING, currentY + 2, valueWidth, LINE_HEIGHT - 2);
                    EditorGUI.LabelField(valueRect, kvp.Value, valueStyle);

                    currentY += LINE_HEIGHT;
                }
                
                // Draw Bottom Line
                Rect bottomLineRect = new Rect(position.x, currentY, position.width, 1f);
                EditorGUI.DrawRect(bottomLineRect, LineColor);
                string combinedValues = "";

                if (data.Count > 0)
                {
                    List<string> allValues = new List<string>(data.Values);
                    combinedValues = string.Join(",", allValues);
                }

                // Write back into the SerializedProperty
                property.stringValue = combinedValues;
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            JsonDataCategoryAttribute attr = (JsonDataCategoryAttribute)attribute;
            string category = attr.CategoryName;
            
            JsonDataUtility.LoadData();
            var data = JsonDataUtility.GetCategory(category);
            
            float contentHeight = 0f;
            if (data != null && data.Count > 0)
            {
                contentHeight = data.Count * LINE_HEIGHT + 2f; // +2 for bottom line
            }
            else
            {
                contentHeight = LINE_HEIGHT;
            }

            return HEADER_HEIGHT + contentHeight;
        }
    }
}
