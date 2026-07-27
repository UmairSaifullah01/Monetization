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
				ExportIcons(_allIcons.Where(i => _selected.Contains(i.Name)));
			}

			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(_filteredIcons.Count == 0);
			if (GUILayout.Button("Export Filtered", GUILayout.Width(140), GUILayout.Height(28)))
			{
				ExportIcons(_filteredIcons);
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

			// Also probe IconContent for common names that may not appear as raw bundle paths.
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

			GUIContent content = EditorGUIUtility.IconContent(name);
			var texture = content?.image as Texture2D;
			if (texture == null)
			{
				seen.Remove(name);
				return;
			}

			_allIcons.Add(new IconEntry { Name = name, Texture = texture });
		}

		private static readonly string[] ProfileIconKeys =
		{
			"console.infoicon.sml",
			"UnityEditor.ConsoleWindow",
			"Profiler.UI",
			"d_Profiler.UI",
			"Refresh",
			"TestStopwatch",
			"BuildSettings.Web.Small",
			"TestPassed",
			"AssemblyLock"
		};

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

		private void ExportIcons(IEnumerable<IconEntry> icons)
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

				Texture2D readable = CreateReadableCopy(icon.Texture);
				if (readable == null)
				{
					Debug.LogWarning($"Icon Downloader: could not read texture '{icon.Name}'.");
					continue;
				}

				byte[] png = readable.EncodeToPNG();
				if (readable != icon.Texture)
				{
					DestroyImmediate(readable);
				}

				File.WriteAllBytes(absolutePath, png);
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
			importer.SaveAndReimport();
		}
	}
}
