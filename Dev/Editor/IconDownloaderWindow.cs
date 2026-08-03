using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace THEBADDEST.MonetizationApi.Dev
{
	/// <summary>
	/// Maintainer-only utility. Browse Unity editor icons and export PNGs into Editor/Icons.
	/// Not shipped in bootstrap or MonetizationScripts.unitypackage.
	/// </summary>
	public class IconDownloaderWindow : EditorWindow
	{
		private const string ExportFolder = "Assets/Monetization/Editor/Icons";
		private const float CellSize = 64f;
		private const float CellPadding = 4f;
		private const int NormalizedIconSize = 32;

		/// <summary>
		/// Icons used by MonetizationProfileEditor. Keep in sync with profile inspector rows.
		/// </summary>
		public static readonly string[] ProfileIconKeys =
		{
			"console.infoicon.sml",
			"UnityEditor.ConsoleWindow",
			"Profiler.UI",
			"Refresh",
			"TestStopwatch",
			"BuildSettings.Web.Small",
			"TestPassed",
			"AssemblyLock"
		};

		private static readonly MethodInfo GetEditorAssetBundleMethod =
			typeof(EditorGUIUtility).GetMethod("GetEditorAssetBundle", BindingFlags.NonPublic | BindingFlags.Static);

		private List<IconEntry> _allIcons = new List<IconEntry>();
		private List<IconEntry> _filteredIcons = new List<IconEntry>();
		private readonly HashSet<string> _selected = new HashSet<string>(StringComparer.Ordinal);
		private string _search = string.Empty;
		private Vector2 _scroll;

		private struct IconEntry
		{
			public string Name;
			public Texture2D Texture;
		}

		[MenuItem("Tools/Monetization Dev/Icon Downloader")]
		public static void ShowWindow()
		{
			var window = GetWindow<IconDownloaderWindow>("Icon Downloader");
			window.minSize = new Vector2(480, 420);
			window.Show();
		}

		[MenuItem("Tools/Monetization Dev/Export Profile Icons")]
		public static void ExportProfileIconsMenu()
		{
			int count = ExportProfileIcons();
			EditorUtility.DisplayDialog(
				"Export Profile Icons",
				$"Exported {count} profile icon(s) to {ExportFolder}.",
				"OK");
		}

		/// <summary>
		/// Batch entry: Unity.exe -batchmode -projectPath ... -executeMethod THEBADDEST.MonetizationApi.Dev.IconDownloaderWindow.ExportProfileIconsBatch -quit
		/// </summary>
		public static void ExportProfileIconsBatch()
		{
			int count = ExportProfileIcons();
			Debug.Log($"Icon Downloader batch: exported {count} profile icon(s) to {ExportFolder}.");
		}

		[InitializeOnLoadMethod]
		private static void EnsureProfileIconsOnLoad()
		{
			EditorApplication.delayCall += () =>
			{
				if (EditorApplication.isPlayingOrWillChangePlaymode)
				{
					return;
				}

				if (!AnyProfileIconMissing())
				{
					return;
				}

				ExportProfileIcons();
			};
		}

		private static bool AnyProfileIconMissing()
		{
			string folder = Path.Combine(Application.dataPath, "Monetization/Editor/Icons");
			if (!Directory.Exists(folder))
			{
				return true;
			}

			foreach (string key in ProfileIconKeys)
			{
				string path = Path.Combine(folder, SanitizeFileName(key) + ".png");
				if (!File.Exists(path))
				{
					return true;
				}
			}

			return false;
		}

		public static int ExportProfileIcons()
		{
			EnsureExportFolder();
			int exported = 0;
			var paths = new List<string>();

			foreach (string key in ProfileIconKeys)
			{
				Texture2D source = ResolveIconTexture(key);
				if (source == null)
				{
					Debug.LogWarning($"Icon Downloader: profile icon '{key}' not found.");
					continue;
				}

				Texture2D normalized = CreateNormalizedIcon(source, NormalizedIconSize);
				if (normalized == null)
				{
					Debug.LogWarning($"Icon Downloader: could not normalize '{key}'.");
					continue;
				}

				string fileName = SanitizeFileName(key) + ".png";
				string assetPath = $"{ExportFolder}/{fileName}";
				string absolutePath = Path.Combine(Application.dataPath, "Monetization/Editor/Icons", fileName);
				File.WriteAllBytes(absolutePath, normalized.EncodeToPNG());
				UnityEngine.Object.DestroyImmediate(normalized);
				paths.Add(assetPath);
				exported++;
			}

			AssetDatabase.Refresh();
			foreach (string assetPath in paths)
			{
				ConfigureImporter(assetPath);
			}

			AssetDatabase.Refresh();
			Debug.Log($"Icon Downloader: exported {exported} profile icon(s) to {ExportFolder}.");
			return exported;
		}

		private void OnEnable()
		{
			LoadIcons();
		}

		private void OnGUI()
		{
			DrawToolbar();
			DrawGrid();
			DrawFooter();
		}

		private void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			EditorGUI.BeginChangeCheck();
			_search = GUILayout.TextField(_search, GUI.skin.FindStyle("ToolbarSeachTextField") ?? EditorStyles.toolbarTextField);
			if (EditorGUI.EndChangeCheck())
			{
				ApplyFilter();
			}

			if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
			{
				_search = string.Empty;
				ApplyFilter();
			}

			if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(60)))
			{
				LoadIcons();
			}

			if (GUILayout.Button("Export Profile", EditorStyles.toolbarButton, GUILayout.Width(100)))
			{
				ExportProfileIcons();
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space(4);
			EditorGUILayout.LabelField(
				$"Icons: {_filteredIcons.Count} / {_allIcons.Count}  |  Selected: {_selected.Count}  |  Export → {ExportFolder}",
				EditorStyles.miniLabel);
			EditorGUILayout.Space(2);
		}

		private void DrawGrid()
		{
			_scroll = EditorGUILayout.BeginScrollView(_scroll);
			float width = Mathf.Max(CellSize, position.width - 24f);
			int columns = Mathf.Max(1, Mathf.FloorToInt(width / (CellSize + CellPadding)));
			int rows = Mathf.CeilToInt(_filteredIcons.Count / (float)columns);

			for (int row = 0; row < rows; row++)
			{
				EditorGUILayout.BeginHorizontal();
				for (int col = 0; col < columns; col++)
				{
					int index = row * columns + col;
					if (index >= _filteredIcons.Count)
					{
						break;
					}

					DrawCell(_filteredIcons[index]);
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();
		}

		private void DrawCell(IconEntry entry)
		{
			bool selected = _selected.Contains(entry.Name);
			var prev = GUI.backgroundColor;
			if (selected)
			{
				GUI.backgroundColor = new Color(0.35f, 0.65f, 1f);
			}

			EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(CellSize), GUILayout.Height(CellSize + 18f));
			Rect texRect = GUILayoutUtility.GetRect(CellSize - 8f, CellSize - 20f, GUILayout.ExpandWidth(true));
			if (entry.Texture != null)
			{
				GUI.DrawTexture(texRect, entry.Texture, ScaleMode.ScaleToFit);
			}

			GUILayout.Label(TruncateLabel(entry.Name), EditorStyles.miniLabel, GUILayout.Height(16f));
			EditorGUILayout.EndVertical();

			Rect cellRect = GUILayoutUtility.GetLastRect();
			if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
			{
				if (_selected.Contains(entry.Name))
				{
					_selected.Remove(entry.Name);
				}
				else
				{
					_selected.Add(entry.Name);
				}

				Event.current.Use();
				Repaint();
			}

			GUI.backgroundColor = prev;
		}

		private static string TruncateLabel(string name)
		{
			return name.Length <= 10 ? name : name.Substring(0, 9) + "…";
		}

		private void DrawFooter()
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Select Filtered", GUILayout.Height(28)))
			{
				foreach (var icon in _filteredIcons)
				{
					_selected.Add(icon.Name);
				}
			}

			if (GUILayout.Button("Clear Selection", GUILayout.Height(28)))
			{
				_selected.Clear();
			}

			GUILayout.FlexibleSpace();

			EditorGUI.BeginDisabledGroup(_selected.Count == 0);
			if (GUILayout.Button("Export Selected", GUILayout.Width(140), GUILayout.Height(28)))
			{
				ExportIcons(_allIcons.Where(i => _selected.Contains(i.Name)), normalize: true);
			}

			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(_filteredIcons.Count == 0);
			if (GUILayout.Button("Export Filtered", GUILayout.Width(140), GUILayout.Height(28)))
			{
				ExportIcons(_filteredIcons, normalize: true);
			}

			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space(4);
		}

		private void LoadIcons()
		{
			_allIcons = new List<IconEntry>();
			_selected.Clear();

			var seen = new HashSet<string>(StringComparer.Ordinal);
			AssetBundle bundle = null;
			try
			{
				if (GetEditorAssetBundleMethod != null)
				{
					bundle = GetEditorAssetBundleMethod.Invoke(null, null) as AssetBundle;
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"Icon Downloader: failed to get editor asset bundle. {ex.Message}");
			}

			if (bundle != null)
			{
				foreach (string assetName in bundle.GetAllAssetNames())
				{
					var texture = bundle.LoadAsset<Texture2D>(assetName);
					if (texture == null)
					{
						continue;
					}

					string name = Path.GetFileNameWithoutExtension(assetName);
					if (string.IsNullOrEmpty(name) || !seen.Add(name))
					{
						continue;
					}

					_allIcons.Add(new IconEntry { Name = name, Texture = texture });
				}
			}

			foreach (string probe in ProfileIconKeys)
			{
				TryAddIconContent(probe, seen);
			}

			_allIcons = _allIcons.OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase).ToList();
			ApplyFilter();
		}

		private void TryAddIconContent(string name, HashSet<string> seen)
		{
			if (string.IsNullOrEmpty(name) || !seen.Add(name))
			{
				return;
			}

			Texture2D texture = ResolveIconTexture(name);
			if (texture == null)
			{
				seen.Remove(name);
				return;
			}

			_allIcons.Add(new IconEntry { Name = name, Texture = texture });
		}

		private void ApplyFilter()
		{
			if (string.IsNullOrWhiteSpace(_search))
			{
				_filteredIcons = _allIcons.ToList();
				return;
			}

			string term = _search.Trim();
			_filteredIcons = _allIcons
				.Where(i => i.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
				.ToList();
		}

		private void ExportIcons(IEnumerable<IconEntry> icons, bool normalize)
		{
			EnsureExportFolder();
			int exported = 0;
			var paths = new List<string>();

			foreach (var icon in icons)
			{
				if (icon.Texture == null)
				{
					continue;
				}

				string fileName = SanitizeFileName(icon.Name) + ".png";
				string assetPath = $"{ExportFolder}/{fileName}";
				string absolutePath = Path.Combine(Application.dataPath, "Monetization/Editor/Icons", fileName);

				Texture2D toEncode = normalize
					? CreateNormalizedIcon(icon.Texture, NormalizedIconSize)
					: CreateReadableCopy(icon.Texture);
				if (toEncode == null)
				{
					Debug.LogWarning($"Icon Downloader: could not read texture '{icon.Name}'.");
					continue;
				}

				File.WriteAllBytes(absolutePath, toEncode.EncodeToPNG());
				if (toEncode != icon.Texture)
				{
					UnityEngine.Object.DestroyImmediate(toEncode);
				}

				paths.Add(assetPath);
				exported++;
			}

			AssetDatabase.Refresh();
			foreach (string assetPath in paths)
			{
				ConfigureImporter(assetPath);
			}

			AssetDatabase.Refresh();
			Debug.Log($"Icon Downloader: exported {exported} icon(s) to {ExportFolder}.");
		}

		private static Texture2D ResolveIconTexture(string key)
		{
			try
			{
				GUIContent content = EditorGUIUtility.IconContent(key);
				var texture = content?.image as Texture2D;
				if (texture != null)
				{
					return texture;
				}

				if (!key.StartsWith("d_", StringComparison.Ordinal))
				{
					content = EditorGUIUtility.IconContent("d_" + key);
					return content?.image as Texture2D;
				}
			}
			catch
			{
				// Icon may not exist on this Unity version.
			}

			return null;
		}

		private static Texture2D CreateNormalizedIcon(Texture2D source, int size)
		{
			Texture2D readable = CreateReadableCopy(source);
			if (readable == null)
			{
				return null;
			}

			float scale = Mathf.Min((float)size / readable.width, (float)size / readable.height);
			int drawW = Mathf.Max(1, Mathf.RoundToInt(readable.width * scale));
			int drawH = Mathf.Max(1, Mathf.RoundToInt(readable.height * scale));

			RenderTexture previous = RenderTexture.active;
			var scaledRt = RenderTexture.GetTemporary(drawW, drawH, 0, RenderTextureFormat.ARGB32);
			Graphics.Blit(readable, scaledRt);
			RenderTexture.active = scaledRt;
			var scaled = new Texture2D(drawW, drawH, TextureFormat.RGBA32, false);
			scaled.ReadPixels(new Rect(0, 0, drawW, drawH), 0, 0);
			scaled.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(scaledRt);

			var result = new Texture2D(size, size, TextureFormat.RGBA32, false);
			var clear = new Color[size * size];
			for (int i = 0; i < clear.Length; i++)
			{
				clear[i] = Color.clear;
			}

			result.SetPixels(clear);
			int offsetX = (size - drawW) / 2;
			int offsetY = (size - drawH) / 2;
			result.SetPixels(offsetX, offsetY, drawW, drawH, scaled.GetPixels());
			result.Apply();

			UnityEngine.Object.DestroyImmediate(scaled);
			if (readable != source)
			{
				UnityEngine.Object.DestroyImmediate(readable);
			}

			return result;
		}

		private static void EnsureExportFolder()
		{
			string absolute = Path.Combine(Application.dataPath, "Monetization/Editor/Icons");
			if (!Directory.Exists(absolute))
			{
				Directory.CreateDirectory(absolute);
				AssetDatabase.Refresh();
			}
		}

		private static string SanitizeFileName(string name)
		{
			var builder = new StringBuilder(name.Length);
			foreach (char c in name)
			{
				if (c == '/' || c == '\\' || c == ':' || c == '*' || c == '?' || c == '"' || c == '<' || c == '>' || c == '|')
				{
					builder.Append('_');
				}
				else
				{
					builder.Append(c);
				}
			}

			return builder.ToString();
		}

		private static Texture2D CreateReadableCopy(Texture2D source)
		{
			if (source == null)
			{
				return null;
			}

			try
			{
				if (source.isReadable)
				{
					return source;
				}
			}
			catch
			{
				// Some engine textures throw on isReadable; fall through to blit path.
			}

			RenderTexture previous = RenderTexture.active;
			var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
			Graphics.Blit(source, rt);
			RenderTexture.active = rt;
			var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
			copy.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
			copy.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(rt);
			return copy;
		}

		private static void ConfigureImporter(string assetPath)
		{
			var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
			if (importer == null)
			{
				return;
			}

			importer.textureType = TextureImporterType.GUI;
			importer.mipmapEnabled = false;
			importer.npotScale = TextureImporterNPOTScale.None;
			importer.alphaIsTransparency = true;
			importer.filterMode = FilterMode.Bilinear;
			importer.maxTextureSize = 64;
			importer.SaveAndReimport();
		}
	}
}
