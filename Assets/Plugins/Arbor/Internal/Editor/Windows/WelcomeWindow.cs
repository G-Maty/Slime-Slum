//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace ArborEditor
{
	using ArborEditor.UpdateCheck;
	using ArborEditor.UIElements;

	internal sealed class WelcomeWindow : EditorWindow
	{
		[MenuItem("Window/Arbor/Welcome")]
		static void Open()
		{
			ArborEditorTemp.instance.openedWelcomeWindow = true;
			ArborEditorCache.welcomeWindowOpenedVersion = ArborVersion.fullVersion;

			var windows = Resources.FindObjectsOfTypeAll<WelcomeWindow>();
			if (windows != null && windows.Length > 0)
			{
				windows[0].Focus();
				return;
			}
			else
			{
				var window = CreateInstance<WelcomeWindow>();
				window.ShowUtility();
			}
		}

		[InitializeOnLoadMethod]
		static void OnLoadMethod()
		{
			EditorApplication.delayCall += OnAutoOpen;
		}

		static void OnAutoOpen()
		{
			if (Application.isBatchMode ||
				(ArborEditorCache.welcomeWindowOpenedVersion == ArborVersion.fullVersion) &&
				(ArborEditorTemp.instance.openedWelcomeWindow || ArborEditorCache.welcomeWindowDontDisplayNextVersion))
			{
				return;
			}

			ArborEditorCache.welcomeWindowDontDisplayNextVersion = false;
			Open();
		}

		private abstract class ElementGUI
		{
			public abstract VisualElement CreateElement();
		}

		private class ButtonGUI : ElementGUI
		{
			private GUIContent _IconContent;
			private string _Title;
			private string _Description;

			public ButtonGUI(Texture2D icon, string title, string description)
			{
				_IconContent = new GUIContent(icon);
				_Title = title;
				_Description = description;
			}

			protected virtual void OnButtonDown()
			{
			}

			protected virtual VisualElement CreateCustomElement()
			{
				return null;
			}

			public override VisualElement CreateElement()
			{
				VisualElement element = new VisualElement()
				{
					style =
					{
						flexDirection = FlexDirection.Row,
						marginBottom = 10f,
						marginTop = 10f,
					}
				};

				CircleButton circleButton = new CircleButton(OnButtonDown);
				element.Add(circleButton);

				Image icon = new Image()
				{
					pickingMode = PickingMode.Ignore,
					image = _IconContent.image,
					scaleMode = ScaleMode.ScaleToFit,
				};
				circleButton.Add(icon);

				VisualElement info = new VisualElement()
				{
					style =
					{
						marginLeft = 5f,
					}
				};
				element.Add(info);

				Label title = new Label();
				title.AddToClassList("large-label");
				title.AddManipulator(new LocalizationManipulator(_Title, LocalizationManipulator.TargetText.Text));
				UIElementsUtility.SetBoldFont(title);
				info.Add(title);

				Label description = new Label();
				description.AddToClassList("wordwrapped-label");
				description.AddManipulator(new LocalizationManipulator(_Description, LocalizationManipulator.TargetText.Text));
				info.Add(description);

				var customElement = CreateCustomElement();
				if (customElement != null)
				{
					info.Add(customElement);
				}

				return element;
			}			
		}

		private class BrowseButtonGUI : ButtonGUI
		{
			private string _URL = null;

			public BrowseButtonGUI(Texture2D icon, string title, string description, string url) : base(icon, title, description)
			{
				_URL = url;
			}

			protected override void OnButtonDown()
			{
				Help.BrowseURL(Localization.GetWord(_URL));
			}
		}

		private sealed class DownloadDocumentGUI : BrowseButtonGUI
		{
			private bool _IsDownloading = false;

			public DownloadDocumentGUI(Texture2D icon, string title, string description, string url) : base(icon, title, description, url)
			{
			}

			private static readonly string[] s_Unit = new[] { "B", "KB", "MB", "GB" };

			public static string ToReadableSize(double size, int scale = 0, int standard = 1024)
			{
				if (scale == s_Unit.Length - 1 || size <= standard)
				{
					return $"{size.ToString(".##")} {s_Unit[scale]}";
				}
				return ToReadableSize(size / standard, scale + 1, standard);
			}

			private Button _DownloadButton;

			protected override VisualElement CreateCustomElement()
			{
				_DownloadButton = new Button(OnClickDownload)
				{
					text = "Download Zip",
				};
				_DownloadButton.SetEnabled(!_IsDownloading);
				return _DownloadButton;
			}

			void OnClickDownload()
			{
				string fileName = "ArborDocumentation_";
				if (ArborSettings.currentLanguage == SystemLanguage.Japanese)
				{
					fileName += "ja_";
				}
				else
				{
					fileName += "en_";
				}
				fileName += ArborVersion.documentVersion + ".zip";

				string url = "https://caitsithware.com/assets/arbor/download/docs/" + fileName;

				string directory = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);

				string path = EditorUtility.SaveFilePanel("Download ArborDocumentation", directory, fileName, "zip");
				if (!string.IsNullOrEmpty(path))
				{
					string tempPath = FileUtil.GetUniqueTempPathInProject();
					try
					{
						System.Net.WebClient client = new System.Net.WebClient();
						client.DownloadProgressChanged += (sender, e) => {
							bool isCancel = EditorUtility.DisplayCancelableProgressBar("Download ArborDocumentation", string.Format("{1} of {2} bytes. {3} % complete...", fileName, ToReadableSize(e.BytesReceived), ToReadableSize(e.TotalBytesToReceive), e.ProgressPercentage), (float)e.BytesReceived / e.TotalBytesToReceive);
							if (isCancel)
							{
								client.CancelAsync();
							}
						};
						client.DownloadFileCompleted += (sender, e) =>
						{
							_IsDownloading = false;
							_DownloadButton.SetEnabled(!_IsDownloading);
							EditorUtility.ClearProgressBar();
							if (e.Cancelled)
							{
								if (System.IO.File.Exists(tempPath))
								{
									System.IO.File.Delete(tempPath);
								}
								return;
							}
							if (e.Error != null)
							{
								Debug.LogException(e.Error);
							}
							else
							{
								if (System.IO.File.Exists(path))
								{
									System.IO.File.Delete(path);
								}
								FileUtil.MoveFileOrDirectory(tempPath, path);
								EditorUtility.RevealInFinder(path);
							}

							client.Dispose();
							client = null;
						};
						_IsDownloading = true;
						_DownloadButton.SetEnabled(!_IsDownloading);
						client.DownloadFileAsync(new System.Uri(url), tempPath);
					}
					catch
					{
						Debug.LogError("Download failed : " + url);
					}
				}
			}
		}

		private sealed class OpenAssetButtonGUI : ButtonGUI
		{
			private string _PathInArbor = "";

			public OpenAssetButtonGUI(Texture2D icon, string title, string description, string path) : base(icon, title, description)
			{
				_PathInArbor = path;
			}

			protected override void OnButtonDown()
			{
				string path = PathUtility.Combine(EditorResources.arborRootDirectory, Localization.GetWord(_PathInArbor));

				Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
				if (asset == null)
				{
					return;
				}

				if (AssetDatabase.OpenAsset(asset))
				{
					GUIUtility.ExitGUI();
				}
			}
		}

		private List<ElementGUI> _ElementGUIs = new List<ElementGUI>();

		private Label _VersionLabel;
		private ToolbarDropdown _LanguagePopup;

		void OnEnable()
		{
			Vector2 windowSize = new Vector2(400, 600f);

			this.minSize = windowSize;
			this.maxSize = windowSize;

			titleContent = new GUIContent("Welcome to Arbor " + ArborVersion.version);

			_ElementGUIs.Add(new BrowseButtonGUI(Icons.homeIcon, "WelcomeWindow.OfficialSite.Title", "WelcomeWindow.OfficialSite.Description", "SiteURL"));
			_ElementGUIs.Add(new DownloadDocumentGUI(Icons.documentationIcon, "WelcomeWindow.Documentation.Title", "WelcomeWindow.Documentation.Description", "DocumentURL"));
			_ElementGUIs.Add(new BrowseButtonGUI(Icons.forumIcon, "WelcomeWindow.SupportForum.Title", "WelcomeWindow.SupportForum.Description", "ForumURL"));
			_ElementGUIs.Add(new BrowseButtonGUI(Icons.reviewIcon, "WelcomeWindow.Review.Title", "WelcomeWindow.Review.Description", "ReviewURL"));
			_ElementGUIs.Add(new BrowseButtonGUI(Icons.tutorialIcon, "WelcomeWindow.Tutorial.Title", "WelcomeWindow.Tutorial.Description", "TutorialURL"));
			_ElementGUIs.Add(new OpenAssetButtonGUI(Icons.documentationIcon, "WelcomeWindow.ReadMe.Title", "WelcomeWindow.ReadMe.Description", "readme-en.txt"));
			_ElementGUIs.Add(new OpenAssetButtonGUI(Icons.documentationIcon, "WelcomeWindow.CHANGELOG.Title", "WelcomeWindow.CHANGELOG.Description", "CHANGELOG-en.md"));

			ArborStyleSheets.Setup(rootVisualElement);

			rootVisualElement.AddToClassList("window-background");

			VisualElement logoElement = new VisualElement();
			logoElement.AddToClassList("graphview-background");
			rootVisualElement.Add(logoElement);

			GridElement grid = new GridElement();
			grid.StretchToParentSize();
			logoElement.Add(grid);

			Image logo = new Image()
			{
				image = Icons.logo,
				scaleMode = ScaleMode.ScaleToFit,
				style =
				{
					height = 100f,
				}
			};
			logoElement.Add(logo);

			_VersionLabel = new Label()
			{
				style =
				{
					unityTextAlign = TextAnchor.MiddleRight,
				}
			};
			_VersionLabel.AddToClassList("white-mini-label");
			UpdateVersionLabel();
			logoElement.Add(_VersionLabel);

			Toolbar toolbar = new Toolbar()
			{
				style =
				{
					paddingLeft = 6f,
					paddingRight = 6f,
				}
			};
			rootVisualElement.Add(toolbar);

			_LanguagePopup = new ToolbarDropdown(() =>
			{
				Localization.DisplayLanguagePopup(_LanguagePopup.worldBound);
			})
			{
				style =
				{
					flexShrink = 0f,
				}
			};
			UpdateLanguagePopup();
			toolbar.Add(_LanguagePopup);

			toolbar.Add(new ToolbarSpacer() { flex = true });

			var toggleNextVersion = new ToolbarToggle();
			toggleNextVersion.AddManipulator(new LocalizationManipulator("WelcomeWindow.DontDisplayNextVersion", LocalizationManipulator.TargetText.Text));
			toggleNextVersion.SetValueWithoutNotify(ArborEditorCache.welcomeWindowDontDisplayNextVersion);
			toggleNextVersion.RegisterValueChangedCallback(e =>
			{
				ArborEditorCache.welcomeWindowDontDisplayNextVersion = e.newValue;
			});
			toolbar.Add(toggleNextVersion);

			var scrollView = new ScrollView()
			{
				style =
				{
					flexGrow = 1f,
				}
			};
			scrollView.AddToClassList("entry-box");
			rootVisualElement.Add(scrollView);

			for (int index = 0; index < _ElementGUIs.Count; index++)
			{
				var elementGUI = _ElementGUIs[index];

				VisualElement entryElement = new VisualElement();
				var entryClassName = (index + 1) % 2 == 0 ? "entry-back-even" : "entry-back-odd";
				entryElement.AddToClassList(entryClassName);

				scrollView.Add(entryElement);

				entryElement.Add(elementGUI.CreateElement());
			}

			var openEditorButton = new Button(() =>
			{
				ArborEditorWindow.OpenFromMenu();
			});
			openEditorButton.AddManipulator(new LocalizationManipulator("OpenArborEditor", LocalizationManipulator.TargetText.Text));
			rootVisualElement.Add(openEditorButton);

			var copyright = new Label()
			{
				style =
				{
					unityTextAlign = TextAnchor.MiddleRight,
				}
			};
			copyright.AddToClassList("mini-label");
			copyright.AddManipulator(new LocalizationManipulator("WelcomeWindow.Copyright", LocalizationManipulator.TargetText.Text ));

			rootVisualElement.Add(copyright);

			Repaint();

			EditorContents.onChanged += OnChangedContents;
			ArborVersion.instance.onLoaded += OnLoadedVersion;
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
			ArborSettings.onChangedLanguage += OnChangedLanguage;
			ArborSettings.onChangedLanguageMode += OnChangedLanguageMode;
		}

		private void OnDisable()
		{
			EditorContents.onChanged -= OnChangedContents;
			ArborVersion.instance.onLoaded -= OnLoadedVersion;
			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
			ArborSettings.onChangedLanguage -= OnChangedLanguage;
			ArborSettings.onChangedLanguageMode -= OnChangedLanguageMode;
		}

		void OnChangedContents()
		{
			UpdateVersionLabel();
		}

		void OnLoadedVersion()
		{
			UpdateVersionLabel();
		}

		void OnUndoRedoPerformed()
		{
			UpdateVersionLabel();
		}

		void OnChangedLanguage()
		{
			UpdateLanguagePopup();
		}

		void OnChangedLanguageMode()
		{
			UpdateLanguagePopup();
		}

		void UpdateLanguagePopup()
		{
			var label = Localization.GetCurrentLanguageLabel();
			_LanguagePopup.text = label.text;
		}

		void UpdateVersionLabel()
		{
			titleContent = new GUIContent("Welcome to Arbor " + ArborVersion.version);
			_VersionLabel.text = EditorContents.version.text + " : " + ArborVersion.fullVersion;
		}

		sealed class GridElement : ImmediateGUIElement
		{
			protected override void OnImmediateGUI()
			{
				EditorGUITools.DrawGrid(contentRect, 1f, ArborSettings.kDefaultGridSize, ArborSettings.kDefaultGridSplitNum);
			}
		}
	}
}