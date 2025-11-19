using System.Collections.Generic;
using System.Linq;
using THEBADDEST.MonetizationApi;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationEditor
{
    /// <summary>
    /// Custom editor for MonetizationKeysSO ScriptableObject.
    /// Displays categories and keys in a beautiful, organized way with foldouts and search functionality.
    /// </summary>
    [CustomEditor(typeof(MonetizationKeysSO))]
    public class MonetizationKeysSOEditor : Editor
    {
        private MonetizationKeysSO keysSO;
        private Dictionary<string, bool> categoryFoldouts = new Dictionary<string, bool>();
        private string searchFilter = "";
        private Vector2 scrollPosition;

        private void OnEnable()
        {
            keysSO = (MonetizationKeysSO)target;
            InitializeFoldouts();
        }

        /// <summary>
        /// Initializes foldout states for all categories.
        /// </summary>
        private void InitializeFoldouts()
        {
            if (keysSO == null || keysSO.KeysData == null)
                return;

            var categoryNames = keysSO.KeysData.GetCategoryNames();
            foreach (var categoryName in categoryNames)
            {
                if (!categoryFoldouts.ContainsKey(categoryName))
                {
                    categoryFoldouts[categoryName] = true; // Default to expanded
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            keysSO = (MonetizationKeysSO)target;

            // Draw title header
            DrawHeader();

            EditorGUILayout.Space(10);

            // Draw JSON file path section
            EditorTools.EditorWindowWithHeader("JSON Configuration");
            
            SerializedProperty jsonFilePathProp = serializedObject.FindProperty("jsonFilePath");
            EditorGUILayout.PropertyField(jsonFilePathProp, new GUIContent("JSON File Path"), true);
            
            EditorGUILayout.Space(5);
            
            // Draw action buttons
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Load From JSON", GUILayout.Width(150), GUILayout.Height(30)))
            {
                LoadFromJson();
            }
            
            GUI.enabled = keysSO.KeysData != null && keysSO.KeysData.CategoryCount > 0;
            if (GUILayout.Button("Reload From JSON", GUILayout.Width(150), GUILayout.Height(30)))
            {
                ReloadFromJson();
            }
            GUI.enabled = true;
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorTools.EditorWindowClose();
            
            EditorGUILayout.Space(10);

            // Draw loaded data section
            if (keysSO.KeysData != null && keysSO.KeysData.CategoryCount > 0)
            {
                DrawLoadedData();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "No keys loaded. Click 'Load From JSON' to load keys from the JSON file.",
                    MessageType.Info
                );
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the header with title and version info.
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.Space();
            GUILayout.BeginVertical(EditorTools.Window);
            EditorGUILayout.Space();
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };
            
            GUILayout.Label("📂 Monetization Keys", titleStyle, GUILayout.Height(40));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the loaded data with categories and keys.
        /// </summary>
        private void DrawLoadedData()
        {
            EditorTools.EditorWindowWithHeader("Loaded Keys");
            
            // Draw search bar
            DrawSearchBar();
            
            EditorGUILayout.Space(5);
            
            // Draw statistics
            DrawStatistics();
            
            EditorGUILayout.Space(10);
            
            // Draw categories and keys
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            var categoryNames = keysSO.KeysData.GetCategoryNames();
            bool hasVisibleCategories = false;
            
            foreach (var categoryName in categoryNames.OrderBy(c => c))
            {
                if (ShouldShowCategory(categoryName))
                {
                    hasVisibleCategories = true;
                    DrawCategory(categoryName);
                }
            }
            
            if (!hasVisibleCategories && !string.IsNullOrEmpty(searchFilter))
            {
                EditorGUILayout.HelpBox($"No categories or keys match the search filter: '{searchFilter}'", MessageType.Info);
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorTools.EditorWindowClose();
        }

        /// <summary>
        /// Draws the search bar.
        /// </summary>
        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Search icon
            GUILayout.Label("🔍", GUILayout.Width(20));
            
            // Search field
            string newSearchFilter = EditorGUILayout.TextField("Search", searchFilter, GUILayout.ExpandWidth(true));
            
            if (newSearchFilter != searchFilter)
            {
                searchFilter = newSearchFilter;
                // Auto-expand categories that match the search
                UpdateFoldoutsForSearch();
            }
            
            // Clear search button
            if (!string.IsNullOrEmpty(searchFilter))
            {
                if (GUILayout.Button("✖", GUILayout.Width(25)))
                {
                    searchFilter = "";
                    GUI.FocusControl(null);
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws statistics about loaded keys.
        /// </summary>
        private void DrawStatistics()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            int categoryCount = keysSO.KeysData.CategoryCount;
            int totalKeyCount = keysSO.KeysData.TotalKeyCount;
            
            GUILayout.Label($"📊 Categories: {categoryCount}", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"🔑 Total Keys: {totalKeyCount}", EditorStyles.miniLabel);
            
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws a category with its keys.
        /// </summary>
        private void DrawCategory(string categoryName)
        {
            var category = keysSO.KeysData.GetCategory(categoryName);
            if (category == null)
                return;

            // Initialize foldout state if not exists
            if (!categoryFoldouts.ContainsKey(categoryName))
            {
                categoryFoldouts[categoryName] = true;
            }

            // Draw category header with foldout
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel = 0;
            
            // Category foldout
            bool wasExpanded = categoryFoldouts[categoryName];
            categoryFoldouts[categoryName] = EditorGUILayout.Foldout(
                categoryFoldouts[categoryName],
                $"📂 {categoryName} ({category.keys.Count} keys)",
                EditorTools.BoldFoldout
            );
            
            // If category was just expanded, scroll to it
            if (categoryFoldouts[categoryName] && !wasExpanded)
            {
                EditorGUILayout.Space(0);
            }
            
            // Draw keys if expanded
            if (categoryFoldouts[categoryName])
            {
                EditorGUI.indentLevel = 1;
                EditorGUILayout.Space(5);
                
                // Draw keys
                bool hasVisibleKeys = false;
                foreach (var key in category.keys.OrderBy(k => k.keyName))
                {
                    if (ShouldShowKey(key.keyName, key.keyValue))
                    {
                        hasVisibleKeys = true;
                        DrawKey(key.keyName, key.keyValue);
                    }
                }
                
                if (!hasVisibleKeys && !string.IsNullOrEmpty(searchFilter))
                {
                    EditorGUILayout.HelpBox($"No keys in this category match the search filter.", MessageType.Info);
                }
                
                EditorGUILayout.Space(5);
            }
            
            EditorGUI.indentLevel = 0;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        /// <summary>
        /// Draws a single key-value pair.
        /// </summary>
        private void DrawKey(string keyName, string keyValue)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Key name (left side)
            EditorGUILayout.LabelField(keyName, EditorStyles.boldLabel, GUILayout.Width(200));
            
            GUILayout.Space(10);
            
            // Key value (right side - selectable)
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(keyValue, GUILayout.ExpandWidth(true));
            EditorGUI.EndDisabledGroup();
            
            // Copy button
            if (GUILayout.Button("📋", GUILayout.Width(30)))
            {
                EditorGUIUtility.systemCopyBuffer = keyValue;
                Debug.Log($"Copied key value to clipboard: {keyValue}");
            }
            
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Checks if a category should be shown based on search filter.
        /// </summary>
        private bool ShouldShowCategory(string categoryName)
        {
            if (string.IsNullOrEmpty(searchFilter))
                return true;

            string filter = searchFilter.ToLower();
            
            // Show if category name matches
            if (categoryName.ToLower().Contains(filter))
                return true;
            
            // Show if any key in category matches
            var category = keysSO.KeysData.GetCategory(categoryName);
            if (category != null)
            {
                foreach (var key in category.keys)
                {
                    if (ShouldShowKey(key.keyName, key.keyValue))
                        return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Checks if a key should be shown based on search filter.
        /// </summary>
        private bool ShouldShowKey(string keyName, string keyValue)
        {
            if (string.IsNullOrEmpty(searchFilter))
                return true;

            string filter = searchFilter.ToLower();
            return keyName.ToLower().Contains(filter) || keyValue.ToLower().Contains(filter);
        }

        /// <summary>
        /// Updates foldout states based on search filter.
        /// </summary>
        private void UpdateFoldoutsForSearch()
        {
            if (string.IsNullOrEmpty(searchFilter))
                return;

            var categoryNames = keysSO.KeysData.GetCategoryNames();
            foreach (var categoryName in categoryNames)
            {
                if (ShouldShowCategory(categoryName))
                {
                    // Auto-expand categories that match search
                    if (!categoryFoldouts.ContainsKey(categoryName))
                    {
                        categoryFoldouts[categoryName] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Loads keys from JSON file.
        /// </summary>
        private void LoadFromJson()
        {
            if (keysSO == null)
                return;

            bool success = keysSO.LoadFromJson();
            
            if (success)
            {
                InitializeFoldouts();
                EditorUtility.DisplayDialog(
                    "Success",
                    $"Successfully loaded monetization keys from JSON file.\n\nCategories: {keysSO.KeysData.CategoryCount}\nTotal Keys: {keysSO.KeysData.TotalKeyCount}",
                    "OK"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    "Failed to load keys from JSON file. Please check the console for details.",
                    "OK"
                );
            }
        }

        /// <summary>
        /// Reloads keys from JSON file.
        /// </summary>
        private void ReloadFromJson()
        {
            LoadFromJson();
        }
    }
}
