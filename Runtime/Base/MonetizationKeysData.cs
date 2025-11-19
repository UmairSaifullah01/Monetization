using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace THEBADDEST.MonetizationApi
{
    /// <summary>
    /// Serializable class representing a single key-value pair.
    /// </summary>
    [Serializable]
    public class MonetizationKeyValue
    {
        public string keyName;
        public string keyValue;

        public MonetizationKeyValue() { }

        public MonetizationKeyValue(string keyName, string keyValue)
        {
            this.keyName = keyName;
            this.keyValue = keyValue;
        }
    }

    /// <summary>
    /// Serializable class representing a category with multiple key-value pairs.
    /// </summary>
    [Serializable]
    public class MonetizationCategory
    {
        public string categoryName;
        public List<MonetizationKeyValue> keys = new List<MonetizationKeyValue>();

        public MonetizationCategory() { }

        public MonetizationCategory(string categoryName)
        {
            this.categoryName = categoryName;
        }

        /// <summary>
        /// Converts the list of keys to a dictionary for easy access.
        /// </summary>
        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>();
            foreach (var kv in keys)
            {
                if (!string.IsNullOrEmpty(kv.keyName))
                {
                    dict[kv.keyName] = kv.keyValue ?? string.Empty;
                }
            }
            return dict;
        }
    }

    /// <summary>
    /// Serializable class that holds monetization keys data loaded from JSON.
    /// Contains categories, each with multiple key-value pairs.
    /// </summary>
    [Serializable]
    public class MonetizationKeysData
    {
        /// <summary>
        /// List of categories. This is serializable by Unity.
        /// </summary>
        [SerializeField]
        private List<MonetizationCategory> categories = new List<MonetizationCategory>();

        /// <summary>
        /// Runtime cache of categories as dictionary for faster access.
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> categoryCache = null;
        private bool isCacheDirty = true;

        /// <summary>
        /// Gets the categories as a dictionary (category name -> key-name -> key-value).
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Categories
        {
            get
            {
                if (isCacheDirty || categoryCache == null)
                {
                    RebuildCache();
                }
                return categoryCache;
            }
        }

        /// <summary>
        /// Gets the serializable list of categories.
        /// </summary>
        public List<MonetizationCategory> CategoryList => categories;

        /// <summary>
        /// Rebuilds the category cache from the serialized list.
        /// </summary>
        private void RebuildCache()
        {
            categoryCache = new Dictionary<string, Dictionary<string, string>>();
            
            if (categories == null)
            {
                categories = new List<MonetizationCategory>();
                return;
            }

            foreach (var category in categories)
            {
                if (category != null && !string.IsNullOrEmpty(category.categoryName))
                {
                    categoryCache[category.categoryName] = category.ToDictionary();
                }
            }

            isCacheDirty = false;
        }

        /// <summary>
        /// Clears all categories and keys.
        /// </summary>
        public void Clear()
        {
            if (categories != null)
            {
                categories.Clear();
            }
            isCacheDirty = true;
        }

        /// <summary>
        /// Adds or updates a category with key-value pairs from a dictionary.
        /// </summary>
        public void SetCategory(string categoryName, Dictionary<string, string> keys)
        {
            if (categories == null)
            {
                categories = new List<MonetizationCategory>();
            }

            // Remove existing category if it exists
            categories.RemoveAll(c => c != null && c.categoryName == categoryName);

            // Create new category
            var category = new MonetizationCategory(categoryName);
            foreach (var kvp in keys)
            {
                category.keys.Add(new MonetizationKeyValue(kvp.Key, kvp.Value));
            }

            categories.Add(category);
            isCacheDirty = true;
        }

        /// <summary>
        /// Gets a key value from a specific category.
        /// </summary>
        /// <param name="categoryName">The category name (e.g., "AdKeys")</param>
        /// <param name="keyName">The key name (e.g., "BannerTop")</param>
        /// <returns>The key value, or null if not found</returns>
        public string GetKey(string categoryName, string keyName)
        {
            var categoriesDict = Categories;
            if (categoriesDict == null || !categoriesDict.ContainsKey(categoryName))
                return null;

            if (categoriesDict[categoryName] == null || !categoriesDict[categoryName].ContainsKey(keyName))
                return null;

            return categoriesDict[categoryName][keyName];
        }

        /// <summary>
        /// Gets all keys from a specific category.
        /// </summary>
        /// <param name="categoryName">The category name</param>
        /// <returns>Dictionary of key-name -> key-value, or null if category not found</returns>
        public Dictionary<string, string> GetCategoryKeys(string categoryName)
        {
            var categoriesDict = Categories;
            if (categoriesDict == null || !categoriesDict.ContainsKey(categoryName))
                return null;

            return categoriesDict[categoryName];
        }

        /// <summary>
        /// Gets the MonetizationCategory object for a specific category name.
        /// </summary>
        /// <param name="categoryName">The category name</param>
        /// <returns>The MonetizationCategory, or null if not found</returns>
        public MonetizationCategory GetCategory(string categoryName)
        {
            if (categories == null)
                return null;

            return categories.FirstOrDefault(c => c != null && c.categoryName == categoryName);
        }

        /// <summary>
        /// Checks if a category exists.
        /// </summary>
        /// <param name="categoryName">The category name</param>
        /// <returns>True if category exists, false otherwise</returns>
        public bool HasCategory(string categoryName)
        {
            var categoriesDict = Categories;
            return categoriesDict != null && categoriesDict.ContainsKey(categoryName);
        }

        /// <summary>
        /// Checks if a key exists in a category.
        /// </summary>
        /// <param name="categoryName">The category name</param>
        /// <param name="keyName">The key name</param>
        /// <returns>True if key exists, false otherwise</returns>
        public bool HasKey(string categoryName, string keyName)
        {
            return GetKey(categoryName, keyName) != null;
        }

        /// <summary>
        /// Gets all category names.
        /// </summary>
        /// <returns>List of category names</returns>
        public List<string> GetCategoryNames()
        {
            var categoriesDict = Categories;
            if (categoriesDict == null)
                return new List<string>();

            return new List<string>(categoriesDict.Keys);
        }

        /// <summary>
        /// Gets the number of categories.
        /// </summary>
        public int CategoryCount => categories != null ? categories.Count : 0;

        /// <summary>
        /// Gets the total number of keys across all categories.
        /// </summary>
        public int TotalKeyCount
        {
            get
            {
                if (categories == null)
                    return 0;

                return categories.Sum(c => c != null && c.keys != null ? c.keys.Count : 0);
            }
        }
    }
}
