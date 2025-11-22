using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace THEBADDEST.MonetizationApi
{
    /// <summary>
    /// Static utility to access monetization keys loaded from JSON in Resources.
    /// </summary>
    public static class JsonDataUtility
    {
        private const string JSON_FILE_NAME = "MonetizationKeys"; // File name in Resources (without .json)
        private static Dictionary<string, Dictionary<string, string>> _categoryCache;
        private static bool _isLoaded = false;

        /// <summary>
        /// Ensures data is loaded.
        /// </summary>
        public static void LoadData()
        {
            if (_isLoaded && _categoryCache != null) return;

            _categoryCache = new Dictionary<string, Dictionary<string, string>>();
            
            TextAsset jsonFile = Resources.Load<TextAsset>(JSON_FILE_NAME);
            if (jsonFile == null)
            {
                Debug.LogError($"[JsonDataUtility] Could not find {JSON_FILE_NAME}.json in Resources folder.");
                return;
            }

            try
            {
                // Using MiniJSON or simple parsing since we want to avoid external dependencies if possible,
                // but assuming the project had a way to parse. 
                // Based on previous file, it used MiniJSON or Newtonsoft. 
                // Let's use a simple Unity JsonUtility wrapper or MiniJSON if available.
                // Since I don't see MiniJSON source, I'll implement a simple dictionary parser using Unity's JsonUtility 
                // isn't great for Dictionaries. 
                // Let's try to use the existing logic from the previous SO but adapted.
                
                // For simplicity and robustness without external libs, let's assume the structure is:
                // { "Category": { "Key": "Value" } }
                // We can use a simple object wrapper for JsonUtility if the structure was known, 
                // but for dynamic dictionary keys, we need a parser.
                // I will assume MiniJSON is available as it was used in the previous code.
                
                var data = MiniJSON.Json.Deserialize(jsonFile.text) as Dictionary<string, object>;
                
                if (data != null)
                {
                    foreach (var categoryPair in data)
                    {
                        string categoryName = categoryPair.Key;
                        var keysObj = categoryPair.Value as Dictionary<string, object>;
                        
                        if (keysObj != null)
                        {
                            var keyDict = new Dictionary<string, string>();
                            foreach (var keyPair in keysObj)
                            {
                                keyDict[keyPair.Key] = keyPair.Value?.ToString() ?? "";
                            }
                            _categoryCache[categoryName] = keyDict;
                        }
                    }
                    _isLoaded = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonDataUtility] Error parsing JSON: {e.Message}");
            }
        }

        /// <summary>
        /// Reloads the data from Resources. Useful for Editor updates.
        /// </summary>
        public static void Reload()
        {
            _isLoaded = false;
            _categoryCache = null;
            LoadData();
        }

        /// <summary>
        /// Gets a specific key value from a category.
        /// </summary>
        public static string GetData(string category, string key)
        {
            LoadData();
            
            if (_categoryCache != null && 
                _categoryCache.TryGetValue(category, out var keys) && 
                keys.TryGetValue(key, out var value))
            {
                return value;
            }
            
            Debug.LogWarning($"[JsonDataUtility] Key not found: {category} -> {key}");
            return null;
        }

        /// <summary>
        /// Gets all keys for a specific category.
        /// </summary>
        public static List<string> GetKeys(string category)
        {
            LoadData();
            
            if (_categoryCache != null && _categoryCache.TryGetValue(category, out var keys))
            {
                return keys.Keys.ToList();
            }
            
            return new List<string>();
        }

        /// <summary>
        /// Gets the full dictionary for a specific category.
        /// </summary>
        public static Dictionary<string, string> GetCategory(string category)
        {
            LoadData();
            
            if (_categoryCache != null && _categoryCache.TryGetValue(category, out var keys))
            {
                return keys;
            }
            
            return null;
        }

        /// <summary>
        /// Gets all category names.
        /// </summary>
        public static List<string> GetAllCategories()
        {
            LoadData();
            
            if (_categoryCache != null)
            {
                return _categoryCache.Keys.ToList();
            }
            
            return new List<string>();
        }

        /// <summary>
        /// Checks if a category exists.
        /// </summary>
        public static bool HasCategory(string category)
        {
            LoadData();
            return _categoryCache != null && _categoryCache.ContainsKey(category);
        }
    }
}
