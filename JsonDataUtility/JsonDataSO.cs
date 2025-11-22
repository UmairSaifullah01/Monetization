using UnityEngine;

namespace THEBADDEST.MonetizationApi
{
    /// <summary>
    /// ScriptableObject to view all loaded Monetization Keys in the Inspector.
    /// </summary>
    [CreateAssetMenu(menuName = "THEBADDEST/MonetizationApi/JsonDataViewer", fileName = "JsonDataViewer")]
    public class JsonDataSO : ScriptableObject
    {
        [TextArea]
        public string description = "This object displays all keys loaded from MonetizationKeys.json";
    }
}
