using THEBADDEST.MonetizationApi;
using UnityEditor;
using UnityEngine;


namespace THEBADDEST.MonetizationEditor
{


	[CustomEditor(typeof(MonetizationProfile))]
	public class MonetizationProfileEditor : CustomProfileEditor<MonetizationProfile, MonetizationModule>
	{

		protected override string collectionTitle => "Modules";
		protected override string collectionPropertyName => "modules";

		DrawCollection<MonetizationModule> drawComponentCollection;
		bool titleFoldout;
		bool settingsFoldout;
		private Texture2D logoTexture;
		private void OnEnable()
		{
			logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Monetization/Logo/logo.png");
		}

		protected override void DrawTitle()
		{
			EditorGUILayout.Space();
			GUILayout.BeginVertical(EditorTools.Window);
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (logoTexture != null)
			{
				GUILayout.Label(logoTexture, GUILayout.Width(70), GUILayout.Height(70));
				GUILayout.Space(10);
			}
			var titleStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = 28,
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.MiddleLeft,
				normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
			};
			GUILayout.Label("Monetization Profile", titleStyle, GUILayout.Height(50));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			GUILayout.Label("Version 4.0b - Developed by Umair Saifullah", new GUIStyle() { alignment = TextAnchor.LowerRight, fontStyle = FontStyle.Italic, normal = { textColor = Color.gray } });
			EditorGUILayout.Space();
			GUILayout.EndVertical();
			EditorGUILayout.Space(10);
		}


		protected override void OnGUIUpdate()
		{
			DrawTitle();
			ProviderProfileValidator.DrawWarnings(target as MonetizationProfile);
			DrawCollections();
			EditorGUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			if (GUILayout.Button("Sync Project", GUILayout.Width(200), GUILayout.Height(40)))
			{
				serializedObject.ApplyModifiedProperties();
				ProjectSettingsSync.SyncFromJson();
			}

			if (GUILayout.Button("Sync IAP Catalog", GUILayout.Width(200), GUILayout.Height(40)))
			{
				serializedObject.ApplyModifiedProperties();
				SyncIapCatalogFromJson();
			}

			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			EditorUtility.SetDirty(target as MonetizationProfile);
		}

		private void SyncIapCatalogFromJson()
		{
			var profile = target as MonetizationProfile;
			if (profile == null)
			{
				return;
			}

			int synced = 0;
			foreach (var module in profile.modules)
			{
				if (module is IAPModule iapModule)
				{
					iapModule.ApplyCatalogFromJson();
					synced++;
					EditorUtility.SetDirty(module);
				}
			}

			if (synced > 0)
			{
				Debug.Log($"Synced IAP catalog from MonetizationKeys.json for {synced} module(s).");
			}
			else
			{
				Debug.LogWarning("No IAPModule found in profile to sync.");
			}
		}

	}


}