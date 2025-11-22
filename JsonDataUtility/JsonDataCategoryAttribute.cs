using UnityEngine;

namespace THEBADDEST.MonetizationApi
{
    /// <summary>
    /// Attribute to select a key from a specific category in the JsonDataUtility.
    /// </summary>
    public class JsonDataCategoryAttribute : PropertyAttribute
    {
        public string CategoryName { get; private set; }

        public JsonDataCategoryAttribute(string categoryName)
        {
            CategoryName = categoryName;
        }
    }
}
