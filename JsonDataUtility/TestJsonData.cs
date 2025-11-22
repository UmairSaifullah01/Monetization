using UnityEngine;
using THEBADDEST.MonetizationApi;

public class TestJsonData : MonoBehaviour
{
    [Header("Test Keys System")]
    [JsonDataCategory("AdKeys")]
    public string bannerKey;

    [JsonDataCategory("AdKeys")]
    public string interstitialKey;

    [JsonDataCategory("GameSettings")]
    public string gameVersion;

    void Start()
    {
        Debug.Log($"Banner Key: {bannerKey} -> Value: {JsonDataUtility.GetData("AdKeys", bannerKey)}");
        Debug.Log($"Interstitial Key: {interstitialKey} -> Value: {JsonDataUtility.GetData("AdKeys", interstitialKey)}");
        Debug.Log($"Game Version: {gameVersion} -> Value: {JsonDataUtility.GetData("GameSettings", gameVersion)}");
    }
}
