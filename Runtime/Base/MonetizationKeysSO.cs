using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace THEBADDEST.MonetizationApi
{
    /// <summary>
    /// ScriptableObject that manages monetization keys loaded from a JSON file.
    /// Provides methods to load and reload keys from JSON.
    /// </summary>
    [CreateAssetMenu(menuName = "THEBADDEST/MonetizationApi/MonetizationKeys", fileName = "MonetizationKeys", order = 2)]
    public class MonetizationKeysSO : ScriptableObject
    {
        [Header("JSON Configuration")]
        [Tooltip("Path to the JSON file containing monetization keys. Can be relative to Assets folder or absolute path.")]
        [TextArea(2, 4)]
        [SerializeField]
        private string jsonFilePath = "Assets/Monetization/Resources/MonetizationKeys.json";

        [Header("Loaded Data")]
        [Tooltip("The deserialized JSON data. This is populated when LoadFromJson() is called.")]
        [SerializeField]
        private MonetizationKeysData keysData = new MonetizationKeysData();

        /// <summary>
        /// Gets the JSON file path.
        /// </summary>
        public string JsonFilePath => jsonFilePath;

        /// <summary>
        /// Gets the loaded keys data.
        /// </summary>
        public MonetizationKeysData KeysData => keysData;

        /// <summary>
        /// Loads keys from the JSON file specified in jsonFilePath.
        /// Uses Json.NET (Newtonsoft.Json) if available, otherwise falls back to Unity's JsonUtility.
        /// </summary>
        /// <returns>True if loading was successful, false otherwise</returns>
        public bool LoadFromJson()
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                SendLog.LogError("JSON file path is not set. Please specify a path in the inspector.");
                return false;
            }

            string fullPath = GetFullPath(jsonFilePath);
            
            if (!File.Exists(fullPath))
            {
                SendLog.LogError($"JSON file not found at path: {fullPath}");
                return false;
            }

            try
            {
                string jsonContent = File.ReadAllText(fullPath);
                
                if (string.IsNullOrEmpty(jsonContent))
                {
                    SendLog.LogError("JSON file is empty.");
                    return false;
                }

                // Try to use Json.NET (Newtonsoft.Json) first
                bool success = TryLoadWithNewtonsoft(jsonContent);
                
                if (!success)
                {
                    // Fallback to Unity's JsonUtility or manual parsing
                    success = TryLoadWithJsonUtility(jsonContent);
                }

                if (success)
                {
                    SendLog.Log($"Successfully loaded monetization keys from: {fullPath}");
                    SendLog.Log($"Loaded {keysData.GetCategoryNames().Count} categories");
                    
#if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
#endif
                    return true;
                }
                else
                {
                    SendLog.LogError("Failed to deserialize JSON. Please check the JSON format.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                SendLog.LogException(ex, $"Error loading JSON file: {fullPath}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to load JSON using Newtonsoft.Json (Json.NET).
        /// </summary>
        private bool TryLoadWithNewtonsoft(string jsonContent)
        {
            try
            {
                // Check if Newtonsoft.Json is available by attempting to use it
                Type newtonsoftType = Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json");
                if (newtonsoftType == null)
                {
                    // Newtonsoft.Json not found, return false to use fallback
                    return false;
                }

                // Use reflection to call JsonConvert.DeserializeObject
                System.Reflection.MethodInfo deserializeMethod = newtonsoftType.GetMethod(
                    "DeserializeObject",
                    new[] { typeof(string) }
                );

                if (deserializeMethod == null)
                {
                    return false;
                }

                // Create a generic method for Dictionary<string, Dictionary<string, string>>
                Type dictType = typeof(Dictionary<string, Dictionary<string, string>>);
                System.Reflection.MethodInfo genericMethod = deserializeMethod.MakeGenericMethod(dictType);
                
                var jsonData = genericMethod.Invoke(null, new object[] { jsonContent }) as Dictionary<string, Dictionary<string, string>>;
                
                if (jsonData != null)
                {
                    keysData = new MonetizationKeysData();
                    keysData.Clear();
                    
                    // Populate categories using SetCategory method
                    foreach (var categoryKvp in jsonData)
                    {
                        keysData.SetCategory(categoryKvp.Key, categoryKvp.Value);
                    }
                    
                    SendLog.LogDebug("Successfully loaded JSON using Newtonsoft.Json");
                    return true;
                }
            }
            catch (Exception ex)
            {
                SendLog.LogWarning($"Newtonsoft.Json deserialization failed: {ex.Message}. Trying fallback method...");
            }
            
            return false;
        }

        /// <summary>
        /// Attempts to load JSON using Unity's JsonUtility or manual parsing.
        /// Since JsonUtility doesn't support Dictionary, we'll use MiniJSON as fallback.
        /// </summary>
        private bool TryLoadWithJsonUtility(string jsonContent)
        {
            try
            {
                // Use MiniJSON for parsing (available in the project)
                var parsedData = MiniJSON.Json.Deserialize(jsonContent) as Dictionary<string, object>;
                
                if (parsedData == null)
                {
                    SendLog.LogError("Failed to parse JSON. Invalid format.");
                    return false;
                }

                keysData = new MonetizationKeysData();
                keysData.Clear();
                
                foreach (var categoryKvp in parsedData)
                {
                    string categoryName = categoryKvp.Key;
                    var categoryValue = categoryKvp.Value as Dictionary<string, object>;
                    
                    if (categoryValue == null)
                    {
                        SendLog.LogWarning($"Category '{categoryName}' does not contain a valid dictionary. Skipping...");
                        continue;
                    }

                    // Convert the category data to Dictionary<string, string>
                    var categoryDict = new Dictionary<string, string>();
                    foreach (var keyKvp in categoryValue)
                    {
                        string keyName = keyKvp.Key;
                        string keyValue = keyKvp.Value?.ToString() ?? string.Empty;
                        categoryDict[keyName] = keyValue;
                    }

                    // Use SetCategory to properly populate the serializable structure
                    keysData.SetCategory(categoryName, categoryDict);
                }

                return true;
            }
            catch (Exception ex)
            {
                SendLog.LogException(ex, "Error parsing JSON with fallback method");
                return false;
            }
        }

        /// <summary>
        /// Gets the full path to the JSON file, handling both relative and absolute paths.
        /// </summary>
        private string GetFullPath(string path)
        {
            // If it's already an absolute path, return as is
            if (Path.IsPathRooted(path))
            {
                return path;
            }
            
            // If path starts with "Assets/", remove "Assets/" prefix
            // Application.dataPath already points to the Assets folder
            if (path.StartsWith("Assets/"))
            {
                string relativePath = path.Substring("Assets/".Length);
                return Path.Combine(Application.dataPath, relativePath);
            }
            
            // Otherwise, treat as relative to Assets folder
            return Path.Combine(Application.dataPath, path);
        }

        /// <summary>
        /// Gets a key value from a specific category.
        /// </summary>
        /// <param name="categoryName">The category name</param>
        /// <param name="keyName">The key name</param>
        /// <returns>The key value, or null if not found</returns>
        public string GetKey(string categoryName, string keyName)
        {
            if (keysData == null)
            {
                SendLog.LogWarning("Keys data is not loaded. Call LoadFromJson() first.");
                return null;
            }

            return keysData.GetKey(categoryName, keyName);
        }

        /// <summary>
        /// Gets all keys from a specific category.
        /// </summary>
        /// <param name="categoryName">The category name</param>
        /// <returns>Dictionary of key-name -> key-value, or null if category not found</returns>
        public Dictionary<string, string> GetCategoryKeys(string categoryName)
        {
            if (keysData == null)
            {
                SendLog.LogWarning("Keys data is not loaded. Call LoadFromJson() first.");
                return null;
            }

            return keysData.GetCategoryKeys(categoryName);
        }

        /// <summary>
        /// Checks if a category exists.
        /// </summary>
        public bool HasCategory(string categoryName)
        {
            return keysData != null && keysData.HasCategory(categoryName);
        }

        /// <summary>
        /// Checks if a key exists in a category.
        /// </summary>
        public bool HasKey(string categoryName, string keyName)
        {
            return keysData != null && keysData.HasKey(categoryName, keyName);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Context menu item to reload keys from JSON.
        /// </summary>
        [ContextMenu("Reload From JSON")]
        private void ReloadFromJsonContextMenu()
        {
            LoadFromJson();
        }
#endif
    }
}
