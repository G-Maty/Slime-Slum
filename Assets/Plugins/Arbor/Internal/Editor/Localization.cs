//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Arbor;

namespace ArborEditor
{
	internal sealed class LanguageImporter : AssetPostprocessor
	{
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			bool rebuild = false;

			for (int importIndex = 0; importIndex < importedAssets.Length; importIndex++)
			{
				string importAssetPath = importedAssets[importIndex];
				if (!importAssetPath.StartsWith("Assets/", System.StringComparison.Ordinal))
				{
					continue;
				}

				string directory = PathUtility.GetDirectoryName(importAssetPath);
				string languageName = Path.GetFileNameWithoutExtension(importAssetPath);
				string ext = Path.GetExtension(importAssetPath);

				if (ext == ".txt" && Localization.Contains(directory))
				{
					SystemLanguage language;
					rebuild = EnumUtility.TryParse<SystemLanguage>(languageName, out language);
				}
				else
				{
					LanguagePathInternal languagePath = AssetDatabase.LoadAssetAtPath<LanguagePathInternal>(importAssetPath);
					if (languagePath != null)
					{
						languagePath.Setup();
						Localization.AddLanguagePath(languagePath);
						rebuild = true;
					}
				}
			}

			if (deletedAssets != null && deletedAssets.Length > 0)
			{
				rebuild = true;
			}

			if (rebuild)
			{
				Localization.Rebuild();
			}
		}
	}

	[InitializeOnLoad]
	public static class Localization
	{
		static Localization()
		{
			Initialize();
		}

		private sealed class WordDictionary : Dictionary<string, GUIContent>
		{
		}

		private sealed class SystemLanguageCompare : IComparer<SystemLanguage>
		{
			public int Compare(SystemLanguage x, SystemLanguage y)
			{
				return ((int)x).CompareTo((int)y);
			}
		}

		private static List<LanguagePathInternal> _LanguagePaths = new List<LanguagePathInternal>();
		private static SortedDictionary<SystemLanguage, WordDictionary> _LanguageDics = new SortedDictionary<SystemLanguage, WordDictionary>(new SystemLanguageCompare());
		private static List<SystemLanguage> _Languages = new List<SystemLanguage>();
		private static GUIContent[] _LanguageLabels;
		private static SystemLanguage _LastLanguage;

		public static event System.Action onRebuild;

		const string k_DirectoryName = "Languages";

		public static string languageDirectory
		{
			get
			{
				return PathUtility.Combine(EditorResources.directory, k_DirectoryName);
			}
		}

		public static bool Contains(string path)
		{
			if (path == languageDirectory)
			{
				return true;
			}

			for (int pathIndex = 0; pathIndex < _LanguagePaths.Count; pathIndex++)
			{
				LanguagePathInternal languagePath = _LanguagePaths[pathIndex];
				if (path == languagePath.path)
				{
					return true;
				}
			}

			return false;
		}

		internal static void Rebuild()
		{
			for (int i = _LanguagePaths.Count - 1; i >= 0; i--)
			{
				LanguagePathInternal languagePath = _LanguagePaths[i];
				if (languagePath == null)
				{
					_LanguagePaths.RemoveAt(i);
				}
			}

			_Languages.Clear();
			_LanguageDics.Clear();

			var languages = EnumUtility.GetValues<SystemLanguage>();
			for (int languageIndex = 0; languageIndex < languages.Length; languageIndex++)
			{
				SystemLanguage language = languages[languageIndex];
				Load(language);
			}

			_LanguageLabels = null;

			if (onRebuild != null)
			{
				onRebuild();
			}
		}

		internal static void AddLanguagePath(LanguagePathInternal languagePath)
		{
			if (!_LanguagePaths.Contains(languagePath))
			{
				_LanguagePaths.Add(languagePath);
			}
		}

		static void Initialize()
		{
			var assets = AssetDatabase.FindAssets("t:ArborEditor.LanguagePath");
			for (int i = 0; i < assets.Length; i++)
			{
				string guid = assets[i];
				string path = AssetDatabase.GUIDToAssetPath(guid);
				LanguagePathInternal languagePath = AssetDatabase.LoadAssetAtPath<LanguagePathInternal>(path);
				if (languagePath != null)
				{
					languagePath.Setup();
					AddLanguagePath(languagePath);
				}
			}

			Rebuild();
		}

		static T LoadAssetAtPath<T>(string path, string ext) where T : Object
		{
			T obj = AssetDatabase.LoadAssetAtPath<T>(path);
			if (obj != null)
			{
				return obj;
			}
			return AssetDatabase.LoadAssetAtPath<T>(Path.ChangeExtension(path, ext));
		}

		static void Load(SystemLanguage language)
		{
			if (_LanguageDics.ContainsKey(language))
			{
				return;
			}

			WordDictionary wordDic = new WordDictionary();

			string languageName = language.ToString();

			string path = PathUtility.Combine(k_DirectoryName, languageName);

			// load default words
			TextAsset languageAsset = EditorResources.Load<TextAsset>(path, ".txt");
			if (languageAsset != null)
			{
				Load(wordDic, languageAsset.text);
			}

			// sort language path order
			_LanguagePaths.Sort((a, b) =>
			{
				return a.order.CompareTo(b.order);
			});

			// load additional words
			for (int pathIndex = 0; pathIndex < _LanguagePaths.Count; pathIndex++)
			{
				LanguagePathInternal languagePath = _LanguagePaths[pathIndex];
				path = languagePath.path;
				if (string.IsNullOrEmpty(path))
				{
					continue;
				}
				path = PathUtility.Combine(path, languageName);

				languageAsset = LoadAssetAtPath<TextAsset>(path, ".txt");
				if (languageAsset != null)
				{
					Load(wordDic, languageAsset.text);
				}
			}

			if (wordDic.Count > 0)
			{
				_LanguageDics.Add(language, wordDic);
				_Languages.Add(language);
			}
		}

		static void Load(WordDictionary wordDic, string text)
		{
			var lines = text.Split('\n');
			for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
			{
				string line = lines[lineIndex];
				if (line.StartsWith("//", System.StringComparison.Ordinal))
				{
					continue;
				}

				int firstColonIndex = line.IndexOf(':');
				if (firstColonIndex < 0)
				{
					continue;
				}

				string key = line.Substring(0, firstColonIndex);
				string word = line.Substring(firstColonIndex + 1).Trim().Replace("\\n", "\n");

				wordDic[key] = new GUIContent(word);
			}
		}

		public static GUIContent GetTextContent(SystemLanguage language, string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return GUIContent.none;
			}

			WordDictionary wordDic = null;
			if (_LanguageDics.TryGetValue(language, out wordDic))
			{
				GUIContent content;
				if (wordDic.TryGetValue(key, out content))
				{
					return content;
				}
			}

			if (language != SystemLanguage.English)
			{
				return GetTextContent(SystemLanguage.English, key);
			}

			return EditorGUITools.GetTextContent(key);
		}

		public static GUIContent GetTextContent(string key)
		{
			return GetTextContent(ArborSettings.currentLanguage, key);
		}

		public static string GetWord(SystemLanguage language, string key)
		{
			return GetTextContent(language, key).text;
		}

		public static string GetWord(string key)
		{
			return GetWord(ArborSettings.currentLanguage, key);
		}

		static readonly int s_LanguageModeCount = 3;

		static GUIContent[] GetLanguageLabels()
		{
			bool changed = _LastLanguage != ArborSettings.currentLanguage;
			_LastLanguage = ArborSettings.currentLanguage;

			int languageCount = _Languages.Count;
			
			if (_LanguageLabels == null || _LanguageLabels.Length != languageCount + s_LanguageModeCount)
			{
				_LanguageLabels = new GUIContent[languageCount + s_LanguageModeCount];

				changed = true;
			}

			if (changed)
			{
				_LanguageLabels[0] = EditorGUITools.GetTextContent(GetWord("System") + "(" + GetWord(GetSystemLanguage().ToString()) + ")");
				_LanguageLabels[1] = EditorGUITools.GetTextContent(GetWord("UnityEditor") + "(" + GetWord(GetEditorLanguage().ToString()) + ")");

				// separator
				_LanguageLabels[s_LanguageModeCount - 1] = EditorGUITools.GetTextContent("");

				for (int i = 0; i < languageCount; i++)
				{
					SystemLanguage language = _Languages[i];
					_LanguageLabels[i + s_LanguageModeCount] = GetTextContent(language.ToString());
				}
			}

			return _LanguageLabels;
		}

		static int GetSelectedIndex()
		{
			int selectIndex = 0;

			switch (ArborSettings.languageMode)
			{
				case LanguageMode.Custom:
					{
						int languageCount = _Languages.Count;
						for (int i = 0; i < languageCount; i++)
						{
							SystemLanguage language = _Languages[i];
							if (language == ArborSettings.language)
							{
								selectIndex = i + s_LanguageModeCount;
								break;
							}
						}
					}
					break;
				case LanguageMode.System:
					selectIndex = 0;
					break;
				case LanguageMode.UnityEditor:
					selectIndex = 1;
					break;
			}

			return selectIndex;
		}

		public static GUIContent GetCurrentLanguageLabel()
		{
			var contents = GetLanguageLabels();
			int selectedIndex = GetSelectedIndex();
			return contents[selectedIndex];
		}

		static void SetSelectIndex(int selectIndex)
		{
			if (selectIndex == 0)
			{
				ArborSettings.languageMode = LanguageMode.System;
			}
			else if (selectIndex == 1)
			{
				ArborSettings.languageMode = LanguageMode.UnityEditor;
			}
			else if (selectIndex == s_LanguageModeCount - 1)
			{
				// separator
			}
			else
			{
				ArborSettings.languageMode = LanguageMode.Custom;
				ArborSettings.language = _Languages[selectIndex - s_LanguageModeCount];
			}
		}

		static void OnSelectPopupMenu(object userData, string[] options, int selected)
		{
			SetSelectIndex(selected);
		}

		public static void DisplayLanguagePopup(Rect position)
		{
			EditorUtility.DisplayCustomMenu(position, GetLanguageLabels(), GetSelectedIndex(), OnSelectPopupMenu, null);
		}

		public static void LanguagePopup(Rect position, GUIContent label, GUIStyle style)
		{
			using (new ProfilerScope("LanguagePopup"))
			{
				var languageLabels = GetLanguageLabels();
				int languageCount = _Languages.Count;

				int selectIndex = GetSelectedIndex();

				EditorGUI.BeginChangeCheck();
				selectIndex = EditorGUI.Popup(position, label, selectIndex, languageLabels, style);
				if (EditorGUI.EndChangeCheck())
				{
					SetSelectIndex(selectIndex);
				}
			}
		}

		public static void LanguagePopupLayout(GUIContent label, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, style, options);

			LanguagePopup(position, label, style);
		}

		public static bool ContainsLanguage(SystemLanguage language)
		{
			return _LanguageDics.ContainsKey(language);
		}

		public static SystemLanguage GetSystemLanguage()
		{
			SystemLanguage language = Application.systemLanguage;
			if (!ContainsLanguage(language))
			{
				language = SystemLanguage.English;
			}

			return language;
		}

		public static SystemLanguage GetEditorLanguage()
		{
			SystemLanguage language = UnityEditorBridge.LocalizationDatabaseBridge.currentEditorLanguage;
			if (!ContainsLanguage(language))
			{
				language = SystemLanguage.English;
			}

			return language;
		}
	}
}
