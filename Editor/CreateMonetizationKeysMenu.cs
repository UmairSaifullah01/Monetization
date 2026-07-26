using System.IO;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Editor
{
	public static class CreateMonetizationKeysMenu
	{
		private const string FileName = "MonetizationKeys.json";
		private const string RelativePath = "Assets/Resources/" + FileName;

		private const string TemplateJson =
@"{
  ""AdKeys"": {
    ""AppId"": ""ca-app-pub-3940256099942544~3347511713"",
    ""MaxSdkKey"": ""your-applovin-max-sdk-key"",
    ""BannerTop"": ""ca-app-pub-1234567890/banner_top"",
    ""BannerBottom"": ""ca-app-pub-1234567890/banner_bottom"",
    ""Rewarded"": ""ca-app-pub-1234567890/rewarded_video"",
    ""Interstitial"": ""ca-app-pub-1234567890/interstitial"",
    ""InterstitialVideo"": ""ca-app-pub-1234567890/interstitial_video"",
    ""AppOpen"": ""ca-app-pub-1234567890/app_open""
  },
  ""IAPKeys"": {
    ""NoAds"": ""remove_ads"",
    ""GemPackSmall"": ""gems_small_pack"",
    ""GemPackMedium"": ""gems_medium_pack"",
    ""GemPackLarge"": ""gems_large_pack"",
    ""CoinPackSmall"": ""coins_small_pack"",
    ""CoinPackLarge"": ""coins_large_pack"",
    ""PremiumSubscription"": ""premium_subscription_monthly"",
    ""ProSubscription"": ""pro_subscription_yearly""
  },
  ""GameAnalyticsKeys"": {
    ""GameKey"": ""your-ga-game-key"",
    ""SecretKey"": ""your-ga-secret-key""
  },
  ""FacebookKeys"": {
    ""AppId"": ""your-facebook-app-id"",
    ""ClientToken"": ""your-facebook-client-token""
  },
  ""TenjinKeys"": {
    ""AndroidApiKey"": ""your-tenjin-android-api-key"",
    ""IosApiKey"": ""your-tenjin-ios-api-key""
  },
  ""PrivacyKeys"": {
    ""PrivacyPolicyUrl"": ""https://example.com/privacy"",
    ""TermsOfServiceUrl"": ""https://example.com/terms""
  },
  ""ProjectKeys"": {
    ""PackageName"": ""com.umgames.test"",
    ""Version"": ""1.0"",
    ""BundleVersionCode"": ""1"",
    ""MinApiLevel"": ""26"",
    ""TargetApiLevel"": ""36"",
    ""KeyStorePath"": ""Assets/Keystore/user.keystore"",
    ""KeyAliasName"": ""user"",
    ""KeyStorePassword"": """",
    ""KeyAliasPassword"": """"
  }
}
";

		[MenuItem("Tools/Monetization/Create Monetization Keys", priority = 10)]
		public static void CreateMonetizationKeys()
		{
			string absolutePath = Path.Combine(Application.dataPath, "Resources", FileName);
			string resourcesDir = Path.GetDirectoryName(absolutePath);

			if (File.Exists(absolutePath))
			{
				bool overwrite = EditorUtility.DisplayDialog(
					"Monetization Keys",
					$"{FileName} already exists at {RelativePath}. Overwrite with the template?",
					"Overwrite",
					"Cancel");
				if (!overwrite)
					return;
			}

			if (!Directory.Exists(resourcesDir))
				Directory.CreateDirectory(resourcesDir);

			File.WriteAllText(absolutePath, TemplateJson.TrimStart() + "\n");
			AssetDatabase.Refresh();

			var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(RelativePath);
			if (asset != null)
				Selection.activeObject = asset;

			Debug.Log($"[Monetization] Created template keys at {RelativePath}");
			EditorUtility.DisplayDialog("Monetization Keys", $"Created {RelativePath}", "OK");
		}
	}
}
