//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ArborEditor
{
	using Arbor;
	using Arbor.DynamicReflection;
	using Arbor.Playables;
	using ArborEditor.UpdateCheck;
	using ArborEditor.IMGUI.Controls;
	using ArborEditor.UIElements;
	using ArborEditor.UnityEditorBridge;
	using ArborEditor.UnityEditorBridge.Extensions;
	using ArborEditor.UnityEditorBridge.UIElements.Extensions;

	[DefaultExecutionOrder(int.MaxValue)]
	public sealed class ArborEditorWindow : EditorWindow, IHasCustomMenu, IPropertyChanged, IUpdateCallback, IHierarchyChangedCallback
	{
		#region static
		private static GUIContent s_DefaultTitleContent = null;
		private const float k_ShowLogoTime = 3.0f;
		private const float k_FadeLogoTime = 1.0f;
		public const float k_ZoomMin = 10f;
		public const float k_ZoomMax = 100f;

		private static System.Action<NodeGraph> s_ToolbarGUI = null;
		private static event System.Action<bool> onChangedCustomToolbar;

		public static event System.Action<NodeGraph> toolbarGUI
		{
			add
			{
				bool oldToolbarGUI = s_ToolbarGUI != null;
				s_ToolbarGUI += value;
				bool newToolbarGUI = s_ToolbarGUI != null;
				if (oldToolbarGUI != newToolbarGUI)
				{
					if (onChangedCustomToolbar != null)
					{
						onChangedCustomToolbar(newToolbarGUI);
					}
				}
			}
			remove
			{
				bool oldToolbarGUI = s_ToolbarGUI != null;
				s_ToolbarGUI -= value;
				bool newToolbarGUI = s_ToolbarGUI != null;
				if (oldToolbarGUI != newToolbarGUI)
				{
					if (onChangedCustomToolbar != null)
					{
						onChangedCustomToolbar(newToolbarGUI);
					}
				}
			}
		}

		private static System.Action<NodeGraph, Rect> s_UnderlayGUI;
		private static System.Action<bool> onChangedCustomUnderlay;

		public static event System.Action<NodeGraph, Rect> underlayGUI
		{
			add
			{
				bool oldUnderlayGUI = s_UnderlayGUI != null;
				s_UnderlayGUI += value;
				bool newUnderlayGUI = s_UnderlayGUI != null;
				if (oldUnderlayGUI != newUnderlayGUI)
				{
					if (onChangedCustomUnderlay != null)
					{
						onChangedCustomUnderlay(newUnderlayGUI);
					}
				}
			}
			remove
			{
				bool oldToolbarGUI = s_UnderlayGUI != null;
				s_UnderlayGUI -= value;
				bool newToolbarGUI = s_UnderlayGUI != null;
				if (oldToolbarGUI != newToolbarGUI)
				{
					if (onChangedCustomUnderlay != null)
					{
						onChangedCustomUnderlay(newToolbarGUI);
					}
				}
			}
		}

		private static System.Action<NodeGraph, Rect> s_OverlayGUI;
		private static System.Action<bool> onChangedCustomOverlay;

		public static event System.Action<NodeGraph, Rect> overlayGUI
		{
			add
			{
				bool oldOverlayGUI = s_OverlayGUI != null;
				s_OverlayGUI += value;
				bool newOverlayGUI = s_OverlayGUI != null;
				if (oldOverlayGUI != newOverlayGUI)
				{
					if (onChangedCustomOverlay != null)
					{
						onChangedCustomOverlay(newOverlayGUI);
					}
				}
			}
			remove
			{
				bool oldOverlayGUI = s_OverlayGUI != null;
				s_OverlayGUI -= value;
				bool newOverlayGUI = s_OverlayGUI != null;
				if (oldOverlayGUI != newOverlayGUI)
				{
					if (onChangedCustomOverlay != null)
					{
						onChangedCustomOverlay(newOverlayGUI);
					}
				}
			}
		}

		[System.Obsolete]
		public static ISkin skin;

		public static ArborEditorWindow activeWindow
		{
			get;
			private set;
		}

		public static ArborEditorWindow actualWindow
		{
			get
			{
				return EditorWindowBridge.actualWindow as ArborEditorWindow;
			}
		}

		private static GUIContent defaultTitleContent
		{
			get
			{
				if (s_DefaultTitleContent == null)
				{
					s_DefaultTitleContent = new GUIContent("Arbor Editor", EditorGUIUtility.isProSkin ? Icons.logoIcon_DarkSkin : Icons.logoIcon_LightSkin);
				}
				return s_DefaultTitleContent;
			}
		}

		public static bool isBuildDocuments
		{
			get;
			set;
		}

		public static bool isInArborEditor
		{
			get
			{
				return isBuildDocuments || actualWindow != null;
			}
		}

		static ArborEditorWindow()
		{
			NodeGraph.onBreakNode += OnBreakNode;
		}

		static void OnBreakNode(Node node)
		{
			if (!ArborSettings.openBreakNode)
			{
				return;
			}

			EditorApplication.delayCall += () =>
			{
				var nodeGraph = node.nodeGraph;

				ArborEditorWindow window = Open();
				window.OpenInternal(nodeGraph);

				window.graphEditor.BeginFrameSelected(node, false);
			};
		}

		static ArborEditorWindow Open()
		{
			ArborEditorWindow window = ArborSettings.dockingOpen ? EditorWindow.GetWindow<ArborEditorWindow>(typeof(SceneView)) : EditorWindow.GetWindow<ArborEditorWindow>();
			window.titleContent = defaultTitleContent;
			return window;
		}

		[MenuItem("Window/Arbor/Arbor Editor")]
		public static void OpenFromMenu()
		{
			Open();
		}

		public static void Open(NodeGraph nodeGraph)
		{
			ArborEditorWindow window = Open();
			window.OpenInternal(nodeGraph);
		}

		#endregion // static

		#region Serialize fields
		[SerializeField]
		private NodeGraph _NodeGraphRoot = null;

		[SerializeField]
		private int _NodeGraphRootInstanceID = 0;

		[SerializeField]
		private NodeGraph _NodeGraphRootPrev = null;

		[SerializeField]
		private int _NodeGraphRootPrevInstanceID = 0;

		[SerializeField]
		private NodeGraph _NodeGraphCurrent = null;

		[SerializeField]
		private int _NodeGraphCurrentInstanceID = 0;

		[SerializeField]
		private NodeGraphEditor _GraphEditor = null;

		[SerializeField]
		private int _GraphEditorInstanceID = 0;

		[SerializeField]
		private bool _IsLocked = false;

		[SerializeField]
		private TransformCache _TransformCache = new TransformCache();

		[SerializeField]
		private TreeViewState _TreeViewState = new TreeViewState();

		#endregion // Serialize fields

		#region fields

		private double _FadeLogoBeginTime;

		private GraphTreeViewItem _SelectedGraphItem = null;

		private GraphTreeViewItem _NextGraph = null;

		private bool _Initialized = false;

		private bool _IsRepaint = false;

		private bool _IsUpdateLiveTracking = false;

		private bool _IsWindowVisible = false;

		private bool _IsCapture = false;
		private UnityEditorBridge.UIElements.VisualElementCapture _Capture = null;
		private Rect _GraphCaptureExtents = new Rect(0, 0, 100, 100);

		private GraphMainLayout _MainLayout;
		private ResizableElement _MinimapElement;
		private MinimapView _MinimapView;
		private GraphLayout _GraphPanel;
		private GraphView _GraphView;
		private Toolbar _Toolbar;
		private Toggle _SidePanelToggle;
		private ObjectField _ToolbarObjectField;
		private VisualElement _ToolbarGraphEditor;
		private VisualElement _CustomToolbarGUI;
		private ToolbarToggle _ToolbarLiveTracking;
		private Button _ToolbarNotificationButton;
		private VisualElement _NoGraphUI;
		private VisualElement _CustomUnderlayLayer;
		private VisualElement _CustomUnderlayGUI;
		private Label _GraphLabelElement;
		private Label _GraphPlayStateElement;
		private VisualElement _CustomOverlayLayer;
		private VisualElement _CustomOverlayGUI;
		private VisualElement _Breadcrumbs;
		private Image _LogoImage;
		private NotEditableElement _NotEditableElement;
		private VisualElement _GraphTabElement;
		private Button _GraphTabHeaderElement;
		private TreeViewElement _GraphTreeElement;

		internal TabPanel<SidePanelTab> sidePanel
		{
			get;
			private set;
		}

		internal GraphLayout highlightLayer
		{
			get;
			private set;
		}
		
		internal GraphLayout popupLayer
		{
			get;
			private set;
		}

		private GraphLayout _NodeCommentLayer;
		internal GraphLayout nodeCommentLayer
		{
			get
			{
				if (_NodeCommentLayer == null)
				{
					_NodeCommentLayer = new GraphLayout()
					{
						name = "NodeCommentLayer",
						style =
						{
							position = Position.Absolute,
						}
					};
				}
				return _NodeCommentLayer;
			}
		}

		private GraphSettingsWindow _GraphSettingsWindow = null;

		private TreeView _TreeView = new TreeView();

		#endregion // fields

		#region properties

		public NodeGraphEditor graphEditor
		{
			get
			{
				return _GraphEditor;
			}
		}

		internal GraphView graphView
		{
			get
			{
				return _GraphView;
			}
		}

		internal MinimapView minimapView
		{
			get
			{
				return _MinimapView;
			}
		}

		public bool isCapture
		{
			get
			{
				return _IsCapture;
			}
		}

		public TreeView treeView
		{
			get
			{
				return _TreeView;
			}
		}

		#endregion // properties

		#region Unity methods

		void OnEnable()
		{
			titleContent = defaultTitleContent;

			// Conflict between window resizing drag and lock button control ID.
			// Secure a control ID for the lock button in advance to avoid conflicts.
			// Via SearchField to get the controlID with GUIUtility.GetPermanentControlID().
			var searchField = new UnityEditor.IMGUI.Controls.SearchField();
			_LockButtonControlID = searchField.searchFieldControlID;

#if ARBOR_DEBUG
			ArborUpdateCheck updateCheck = ArborUpdateCheck.instance;
			updateCheck.CheckStart(OnUpdateCheckDone,true);
#else
			if (ArborVersion.isUpdateCheck)
			{
				ArborUpdateCheck updateCheck = ArborUpdateCheck.instance;
				updateCheck.CheckStart(OnUpdateCheckDone);
			}
#endif

			if (activeWindow == null)
			{
				activeWindow = this;
			}

			if (_GraphEditor != null)
			{
				_GraphEditor.hostWindow = this;
			}

			_Initialized = false;

			SetupElements();

			if (_NodeGraphRoot != null)
			{
				RegisterRootGraphCallback();
			}

			if (_NodeGraphCurrent != null)
			{
				RegisterCurrentGraphCallback();
			}

			if (_NodeGraphRoot != null)
			{
				BuildTree(_NodeGraphRoot);
			}

			if (_GraphEditor != null)
			{
				OnChangedShowLogo();

				_GraphEditor.OnInitialize();
				_GraphEditor.Update();

				_Initialized = true;
			}

			DoRepaint();

			EditorCallbackUtility.RegisterUpdateCallback(this);
			EditorCallbackUtility.RegisterPropertyChanged(this);
			EditorCallbackUtility.RegisterHierarchyChangedCallback(this);

			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			EditorSceneManager.sceneLoaded += OnSceneLoaded;
			EditorSceneManager.sceneClosed += OnSceneClosed;

			onChangedCustomToolbar += EnableCustomToolbar;
			onChangedCustomUnderlay += EnableCustomUnderlay;
			onChangedCustomOverlay += EnableCustomOverlay;
		}

		void SelectGraphTreeItem(GraphTreeViewItem graphItem)
		{
			if (graphItem == null)
			{
				return;
			}

			if (graphItem != null && !_TreeViewState.IsSelected(graphItem))
			{
				List<int> expandedIDs = _TreeViewState.currentExpandedIDs;
				var parent = graphItem.parent;
				while (parent != null)
				{
					bool expanded = expandedIDs.BinarySearch(parent.id) >= 0;
					if (!expanded)
					{
						expandedIDs.Add(parent.id);
						expandedIDs.Sort();
					}
					parent = parent.parent;
				}

				_GraphTreeElement.UpdateViewTree();
				_GraphTreeElement.SetSelectedItem(graphItem, true);
			}
		}

		void OnSubmitItem(TreeViewItem item)
		{
			var graphItem = item as GraphTreeViewItem;
			if (graphItem == null)
			{
				return;
			}

			ChangeCurrentNodeGraph(graphItem);
		}

		void OnRenameEndedItem(string name, int id)
		{
			GraphTreeViewItem valueItem = _TreeView.FindItem(id) as GraphTreeViewItem;
			NodeGraph nodeGraph = valueItem != null ? valueItem.nodeGraph : null;
			if (nodeGraph != null)
			{
				SetGraphName(nodeGraph, name);
			}

			_GraphTreeElement.ListViewRefresh();
		}

		void SetGraphName(NodeGraph nodeGraph, string graphName)
		{
			Undo.RecordObject(nodeGraph, "Change Graph Name");

			nodeGraph.graphName = graphName;

			EditorUtility.SetDirty(nodeGraph);
		}

		private bool _ChangingPlayMode = false;

		void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (_ChangingPlayMode)
			{
				RepairNodeGraphReferences(true);
			}
		}

		void OnSceneClosed(Scene scene)
		{
			if (_ChangingPlayMode)
			{
				RepairNodeGraphReferences(true);
			}
			else
			{
				ReatachIfNecessary();
			}
		}

		void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			switch (state)
			{
				case PlayModeStateChange.ExitingPlayMode:
					_ChangingPlayMode = true;
					if (_NodeGraphRoot != null)
					{
						UnregisterRootGraphCallback();
					}
					if (_NodeGraphCurrent != null)
					{
						UnregisterCurrentGraphCallback();
					}
					break;
				case PlayModeStateChange.EnteredPlayMode:
					{
						_ChangingPlayMode = false;
						if (_GraphEditor != null)
						{
							_IsUpdateLiveTracking = true;
						}

						SetupToolbarObjectField();
					}
					break;
				case PlayModeStateChange.ExitingEditMode:
					{
						_ChangingPlayMode = true;
					}
					break;
				case PlayModeStateChange.EnteredEditMode:
					_ChangingPlayMode = false;
					RepairNodeGraphReferences();
					if (_NodeGraphRoot != null)
					{
						RegisterRootGraphCallback();
					}
					if (_NodeGraphCurrent != null)
					{
						RegisterCurrentGraphCallback();
					}
					if (_GraphEditor != null)
					{
						_GraphEditor.RaiseOnChangedNodes();
					}

					SetupToolbarObjectField();
					break;
			}
		}

		private void OnDisable()
		{
			if (activeWindow == this)
			{
				activeWindow = null;
			}

			if (_NodeGraphRoot != null)
			{
				UnregisterRootGraphCallback();
			}

			if (_NodeGraphCurrent != null)
			{
				UnregisterCurrentGraphCallback();
			}

			if (!_IsWindowVisible)
			{
				DestroyGraphEditor();
			}

			EditorCallbackUtility.UnregisterUpdateCallback(this);
			EditorCallbackUtility.UnregisterPropertyChanged(this);
			EditorCallbackUtility.UnregisterHierarchyChangedCallback(this);

			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorSceneManager.sceneLoaded -= OnSceneLoaded;
			EditorSceneManager.sceneClosed -= OnSceneClosed;

			onChangedCustomToolbar -= EnableCustomToolbar;
			onChangedCustomUnderlay -= EnableCustomUnderlay;
			onChangedCustomOverlay -= EnableCustomOverlay;

			if (_Capture != null)
			{
				Object.DestroyImmediate(_Capture);
				_Capture = null;
			}
		}

		private void OnSelectionChange()
		{
			if (_IsLocked)
			{
				return;
			}

			GameObject gameObject = Selection.activeGameObject;
			if (gameObject == null)
			{
				return;
			}

			NodeGraph[] nodeGraphs = gameObject.GetComponents<NodeGraph>();
			if (nodeGraphs != null)
			{
				int graphCount = nodeGraphs.Length;
				for (int graphIndex = 0; graphIndex < graphCount; graphIndex++)
				{
					NodeGraph graph = nodeGraphs[graphIndex];
					if ((graph.hideFlags & HideFlags.HideInInspector) == HideFlags.None)
					{
						OpenInternal(graph);
						break;
					}
				}
			}
		}

		void IPropertyChanged.OnPropertyChanged(PropertyChangedType propertyChangedType)
		{
			if (propertyChangedType != PropertyChangedType.UndoRedoPerformed)
			{
				return;
			}

			RepairNodeGraphReferences();

			SetupToolbarObjectField();

			if (_NodeGraphRoot != null)
			{
				if (_SelectedGraphItem != null && _NodeGraphCurrent != _SelectedGraphItem.nodeGraph)
				{
					// Undo/Redo Graph Selection
					// Set null to search again from _NodeGraphCurrent in BuildTree().
					_SelectedGraphItem = null;
				}
				BuildTree(_NodeGraphRoot);
			}

			ReatachIfNecessary();

			if (_GraphEditor != null)
			{
				_GraphEditor.OnUndoRedoPerformed();
			}

			ShowGraphView(_GraphEditor != null);

			DoRepaint();
		}

		void IHierarchyChangedCallback.OnHierarchyChanged()
		{
			ReatachIfNecessary();
		}

		void IUpdateCallback.OnUpdate()
		{
			if (_NextGraph != null)
			{
				SetCurrentNodeGraph(_NextGraph);
				_NextGraph = null;
			}

			if (!_Initialized)
			{
				ReatachIfNecessary();

				if (_GraphEditor != null)
				{
					_GraphEditor.InitializeGraph();
					DoRepaint();
				}

				_Initialized = true;
			}
			else
			{
				if (_GraphEditor != null)
				{
					_GraphEditor.RebuildIfNecessary();
				}
			}

			if (_IsWindowVisible)
			{
				if (_GraphEditor != null && _GraphEditor.nodeGraph != null)
				{
					if (_GraphEditor.editable)
					{
						if (_NotEditableElement != null && _NotEditableElement.parent != null)
						{
							_NotEditableElement.RemoveFromHierarchy();
						}
					}
					else
					{
						if (_NotEditableElement == null)
						{
							_NotEditableElement = new NotEditableElement();
							_NotEditableElement.StretchToParentSize();
						}
						if (_NotEditableElement.parent == null)
						{
							_GraphView.contentOverlay.Add(_NotEditableElement);
						}
					}
					_GraphLabelElement.text = _GraphEditor.GetGraphLabel().text;

					string playStateLabel = null;

					if (Application.isPlaying && _GraphEditor.HasPlayState())
					{
						PlayState playState = _GraphEditor.GetPlayState();

						switch (playState)
						{
							case PlayState.Stopping:
								playStateLabel = Localization.GetTextContent("PlayState.Stopping").text;
								break;
							case PlayState.Playing:
								playStateLabel = Localization.GetTextContent("PlayState.Playing").text;
								break;
							case PlayState.Pausing:
								playStateLabel = Localization.GetTextContent("PlayState.Pausing").text;
								break;
							case PlayState.InactivePausing:
								playStateLabel = Localization.GetTextContent("PlayState.InactivePausing").text;
								break;
						}
					}

					_GraphPlayStateElement.text = playStateLabel;
					
					if (_IsUpdateLiveTracking)
					{
						if (_GraphEditor.LiveTracking())
						{
							// Change GraphEditor
							_IsUpdateLiveTracking = true;
						}
						else
						{
							_IsUpdateLiveTracking = false;
						}
					}

					if (_GraphView.isLayoutSetup)
					{
						UpdateScrollbar();
						_GraphEditor.Update();
						_GraphEditor.UpdateVisibleNodes();
					}

					if (ArborSettings.showLogo == LogoShowMode.FadeOut && _LogoImage.parent != null)
					{
						float t = (float)(EditorApplication.timeSinceStartup - (_FadeLogoBeginTime + k_ShowLogoTime)) / k_FadeLogoTime;
						if (t >= 1.0f)
						{
							_LogoImage.RemoveFromHierarchy();
						}
						else
						{
							float alpha = Mathf.Lerp(0.5f, 0f, Mathf.Clamp01(t));

							_LogoImage.tintColor = new Color(1f, 1f, 1f, alpha);
						}
					}

					if (_IsRepaint)
					{
						DoRepaint();
					}
				}
			}
		}

		void OnBecameVisible()
		{
			_IsWindowVisible = true;
		}

		void OnBecameInvisible()
		{
			_IsWindowVisible = false;
		}

		private int _LockButtonControlID;

		private void ShowButton(Rect r)
		{
			bool flag = GUI.Toggle(r, _LockButtonControlID, _IsLocked, GUIContent.none, BuiltInStyles.lockButton);
			if (flag == _IsLocked)
			{
				return;
			}
			_IsLocked = flag;
		}

		private void OnLostFocus()
		{
			var renameOverlayElement = _CurrentFocusedElement as RenameOverlayElement;
			if (renameOverlayElement != null)
			{
				renameOverlayElement.EndRename(true);
			}
		}

		void OnDestroy()
		{
			SetNodeGraphRoot(null);
			SetNodeGraphCurrent(null);

			if (_Capture != null)
			{
				Object.DestroyImmediate(_Capture);
				_Capture = null;
			}

			DestroyGraphEditor();
		}

		#endregion // Unity methods

		void OnUpdateCheckDone()
		{
			ArborUpdateCheck updateCheck = ArborUpdateCheck.instance;
			UpdateInfo updateInfo = updateCheck.updateInfo;
			if (updateInfo == null)
			{
				return;
			}

			bool isUpdated = updateCheck.isUpdated;
			if (isUpdated)
			{
				DoRepaint();
			}
		}

		void UnderlayGUI(Rect rect)
		{
			if (_GraphEditor == null || _GraphEditor.nodeGraph == null)
			{
				return;
			}

			if (s_UnderlayGUI != null)
			{
				GUI.BeginGroup(rect);

				Rect groupRect = rect;
				groupRect.position = Vector2.zero;

				GUILayout.BeginArea(groupRect);
				s_UnderlayGUI(_GraphEditor.nodeGraph, groupRect);
				GUILayout.EndArea();

				GUI.EndGroup();
			}
		}

		void OverlayGUI(Rect rect)
		{
			if (_GraphEditor == null || _GraphEditor.nodeGraph == null)
			{
				return;
			}

			if (s_OverlayGUI != null)
			{
				GUI.BeginGroup(rect);

				Rect groupRect = rect;
				groupRect.position = Vector2.zero;

				GUILayout.BeginArea(groupRect);
				s_OverlayGUI(_GraphEditor.nodeGraph, groupRect);
				GUILayout.EndArea();

				GUI.EndGroup();
			}
		}

		void RepairNodeGraphReferences(bool repairOnly = false)
		{
			bool repaired = false;
			if (_NodeGraphRootPrev == null && _NodeGraphRootPrevInstanceID != 0)
			{
				_NodeGraphRootPrev = EditorUtility.InstanceIDToObject(_NodeGraphRootPrevInstanceID) as NodeGraph;
				if (!repairOnly)
				{
					if (_NodeGraphRootPrev == null)
					{
						_NodeGraphRootPrevInstanceID = 0;
					}
					ShowGraphTabHeader(_NodeGraphRootPrev != null);
					SetupToolbarObjectField();
				}
				repaired = true;
			}
			if (_NodeGraphRoot == null && _NodeGraphRootInstanceID != 0)
			{
				_NodeGraphRoot = EditorUtility.InstanceIDToObject(_NodeGraphRootInstanceID) as NodeGraph;
				if (_NodeGraphRoot != null)
				{
					RegisterRootGraphCallback();
				}

				if (!repairOnly)
				{
					if (_NodeGraphRoot == null)
					{
						_NodeGraphRootInstanceID = 0;
					}
					ShowGraphTab(_NodeGraphRoot != null);
					if (_NodeGraphRoot != null)
					{
						_GraphTabHeaderElement.text = _NodeGraphRoot.graphName;
					}
					SetupToolbarObjectField();
				}
				repaired = true;
			}
			if (_NodeGraphCurrent == null && _NodeGraphCurrentInstanceID != 0)
			{
				_NodeGraphCurrent = EditorUtility.InstanceIDToObject(_NodeGraphCurrentInstanceID) as NodeGraph;
				if (_NodeGraphCurrent != null)
				{
					RegisterCurrentGraphCallback();
				}

				if (!repairOnly)
				{
					if (_NodeGraphCurrent == null)
					{
						_NodeGraphCurrentInstanceID = 0;
					}
				}
				repaired = true;
			}

			if (repaired)
			{
				BuildTree(_NodeGraphRoot);
			}

			if (_GraphEditor == null && _GraphEditorInstanceID != 0)
			{
				_GraphEditor = EditorUtility.InstanceIDToObject(_GraphEditorInstanceID) as NodeGraphEditor;
				if (!repairOnly)
				{
					if (_GraphEditor == null)
					{
						_GraphEditorInstanceID = 0;
					}
					SetupToolbarGraphEditor();
				}
			}
			if (_GraphEditor != null)
			{
				if (_GraphEditor.RepairReferences())
				{
					if (_GraphEditor.nodeGraph != null)
					{
						_GraphEditor.RebuildIfNecessary();
						_GraphEditor.Update();
					}
				}
			}
		}

		private bool _InPostLayout = false;

		internal void OnPostLayout()
		{
			CenterOnStoredPosition(_SelectedGraphItem);

			if (!_InPostLayout)
			{
				_InPostLayout = true;

				try
				{
					if (_GraphEditor != null && _GraphEditor.nodeGraph != null)
					{
						UpdateScrollbar();
						_GraphEditor.Update();
					}
				}
				finally
				{
					_InPostLayout = false;
				}
			}
		}

		void DestroyGraphEditor()
		{
			if (_GraphEditor == null)
			{
				return;
			}

			Object.DestroyImmediate(_GraphEditor);
			_GraphEditor = null;
			_GraphEditorInstanceID = 0;

			SetupToolbarGraphEditor();

			ShowGraphView(false);
		}

		void ShowGraphView(bool show)
		{
			if (show)
			{
				if (_NoGraphUI.parent != null)
				{
					_NoGraphUI.RemoveFromHierarchy();
				}
				if (_GraphView.parent == null)
				{
					_GraphPanel.Add(_GraphView);
					_MainLayout.leftPanel.Add(_MinimapElement);
				}
				sidePanel.contentContainer.visible = true;
			}
			else
			{
				if (_GraphView.parent != null)
				{
					_GraphView.RemoveFromHierarchy();
					_MinimapElement.RemoveFromHierarchy();
				}
				if (_NoGraphUI.parent == null)
				{
					_GraphPanel.Add(_NoGraphUI);
				}

				sidePanel.contentContainer.visible = false;
			}
		}

		void Initialize()
		{
			DestroyGraphEditor();

			Undo.RecordObject(this, "Select NodeGraph");

			if (_NodeGraphRoot != null)
			{
				SetNodeGraphRoot(_NodeGraphRoot);
				var rootItem = FindTreeViewItem(_NodeGraphRoot) as GraphTreeViewItem;
				SetNodeGraphCurrent(rootItem);
			}
			else
			{
				SetNodeGraphRoot(null);
				SetNodeGraphCurrent(null);
			}

			_Initialized = false;

			EditorUtility.SetDirty(this);

			DoRepaint();
		}

		void InternalSelectRootGraph(NodeGraph rootGraph, bool isExternal)
		{
			int undoGroup = Undo.GetCurrentGroup();

			Undo.RecordObject(this, "Select NodeGraph");

			if (isExternal)
			{
				if (_NodeGraphRootPrev == null)
				{
					_NodeGraphRootPrev = _NodeGraphRoot;
					_NodeGraphRootPrevInstanceID = _NodeGraphRootPrev.GetInstanceID();
				}
			}
			else
			{
				_NodeGraphRootPrev = null;
				_NodeGraphRootPrevInstanceID = 0;
			}

			ShowGraphTabHeader(_NodeGraphRootPrev != null);

			SetupToolbarObjectField();

			SetNodeGraphRoot(rootGraph);

			var rootItem = FindTreeViewItem(rootGraph) as GraphTreeViewItem;
			SetNodeGraphCurrent(rootItem);

			Undo.CollapseUndoOperations(undoGroup);

			EditorUtility.SetDirty(this);

			RebuildGraphEditor();

			OnChangedShowLogo(true);

			DoRepaint();
		}

		void OpenInternal(NodeGraph nodeGraph)
		{
			NodeGraph rootGraph = nodeGraph.rootGraph;
			SelectRootGraph(rootGraph);
			if (rootGraph != nodeGraph)
			{
				var ownerBehaviour = nodeGraph.ownerBehaviourObject;
				int instanceID = ownerBehaviour != null ? ownerBehaviour.GetInstanceID() : nodeGraph.GetInstanceID();
				var graphItem = _TreeView.FindItem(instanceID) as GraphTreeViewItem;
				ChangeCurrentNodeGraph(graphItem);
			}
		}

		public void SelectRootGraph(NodeGraph nodeGraph)
		{
			if (_NodeGraphRootPrev == null && _NodeGraphRoot == nodeGraph && _NodeGraphCurrent == nodeGraph)
			{
				return;
			}

			InternalSelectRootGraph(nodeGraph, false);
		}

		public void SelectExternalGraph(GraphTreeViewItem graphItem)
		{
			NodeGraph rootGraph = graphItem.nodeGraph.rootGraph;

			InternalSelectRootGraph(rootGraph, true);
		}

		internal void OnChangedShowGrid()
		{
			_GraphView.SetShowGridBackground(ArborSettings.showGrid);
		}

		internal void OnChangedShowLogo(bool forceFade = false)
		{
			switch (ArborSettings.showLogo)
			{
				case LogoShowMode.Hidden:
					_LogoImage.RemoveFromHierarchy();
					break;
				case LogoShowMode.FadeOut:
					if (forceFade)
					{
						_LogoImage.tintColor = new Color(1f, 1f, 1f, 0.5f);
						if (!_GraphView.contentOverlay.Contains(_LogoImage))
						{
							_GraphView.contentOverlay.Add(_LogoImage);
						}
						_FadeLogoBeginTime = EditorApplication.timeSinceStartup;
					}
					break;
				case LogoShowMode.AlwaysShow:
					_LogoImage.tintColor = new Color(1f, 1f, 1f, 0.5f);
					if (!_GraphView.contentOverlay.Contains(_LogoImage))
					{
						_GraphView.contentOverlay.Add(_LogoImage);
					}
					break;
			}
		}

		internal void OnChangedNodeCommentAffectsZoom()
		{
			if (ArborSettings.nodeCommentAffectsZoom)
			{
				_GraphView.contentContainer.Insert(_GraphView.contentContainer.IndexOf(_GraphView.nodeLayer) +1, nodeCommentLayer);
			}
			else
			{
				_GraphView.contentOverlay.Insert(_GraphView.contentOverlay.IndexOf(popupLayer)+1, nodeCommentLayer);
			}
		}

		void RebuildGraphEditor()
		{
			NodeGraph nodeGraphCurrent = _SelectedGraphItem != null ? _SelectedGraphItem.nodeGraph : null;

			DestroyGraphEditor();

			bool nextHasGraphEditor = nodeGraphCurrent != null;

			if (!nextHasGraphEditor)
			{
				return;
			}

			_GraphEditor = NodeGraphEditor.CreateEditor(this, nodeGraphCurrent, _SelectedGraphItem.isExternal);
			_GraphEditorInstanceID = _GraphEditor.GetInstanceID();

			_GraphEditor.OnInitialize();

			SetupToolbarGraphEditor();

			ShowGraphView(true);

			_IsRepaint = true;
			
			if (_GraphEditor != null)
			{
				_GraphEditor.InitializeGraph();
				_GraphEditor.Update();

				_Initialized = true;

				_GraphEditor.DirtyGraphExtents();
			}
		}

		public void ChangeCurrentNodeGraph(GraphTreeViewItem graphItem, bool liveTracking = false)
		{
			if (graphItem == null)
			{
				return;
			}

			if (_SelectedGraphItem == graphItem)
			{
				return;
			}

			if (!liveTracking && Application.isPlaying &&
				ArborSettings.liveTracking && ArborSettings.liveTrackingHierarchy &&
				_GraphEditor != null && _GraphEditor.GetPlayState() != PlayState.Stopping)
			{
				ArborSettings.liveTracking = false;
				if (_ToolbarLiveTracking != null)
				{
					_ToolbarLiveTracking.SetValueWithoutNotify(ArborSettings.liveTracking);
				}
			}

			_NextGraph = graphItem;

			DoRepaint();
		}

		void SetCurrentNodeGraph(GraphTreeViewItem graphItem)
		{
			if (_SelectedGraphItem == graphItem)
			{
				return;
			}

			int undoGroup = Undo.GetCurrentGroup();

			Undo.RecordObject(this, "Select NodeGraph");

			SetNodeGraphCurrent(graphItem);

			Undo.CollapseUndoOperations(undoGroup);

			EditorUtility.SetDirty(this);

			RebuildGraphEditor();

			OnChangedShowLogo(true);

			DoRepaint();
		}

		internal void DoRepaint()
		{
			_GraphView.contentContainer.MarkDirtyRepaint();

			Repaint();
			_IsRepaint = false;
		}

		void OpenCreateMenu(Rect buttonRect)
		{
			buttonRect = GUIUtility.GUIToScreenRect(buttonRect);
			GraphMenuWindow.instance.Init(this, buttonRect);
		}

		void SetHelpMenu(GenericMenu menu)
		{
			menu.AddItem(EditorContents.assetStore, false, () =>
			{
				ArborVersion.OpenAssetStore();
			});
			menu.AddSeparator("");
			menu.AddItem(EditorContents.officialSite, false, () =>
			{
				Help.BrowseURL(Localization.GetWord("SiteURL"));
			});
			menu.AddItem(EditorContents.manual, false, () =>
			{
				Help.BrowseURL(Localization.GetWord("ManualURL"));
			});
			menu.AddItem(EditorContents.inspectorReference, false, () =>
			{
				Help.BrowseURL(Localization.GetWord("InspectorReferenceURL"));
			});
			menu.AddItem(EditorContents.scriptReference, false, () =>
			{
				Help.BrowseURL(Localization.GetWord("ScriptReferenceURL"));
			});
			menu.AddSeparator("");
			menu.AddItem(EditorContents.releaseNotes, false, () =>
			{
				Help.BrowseURL(Localization.GetWord("ReleaseNotesURL"));
			});
			menu.AddItem(EditorContents.forum, false, () =>
			{
				Help.BrowseURL(Localization.GetWord("ForumURL"));
			});
		}

		void Capture()
		{
			_GraphEditor.ClearInvsibleNodes();

			_GraphCaptureExtents = new RectOffset(100, 100, 100, 100).Add(_GraphView.graphExtentsRaw);

			if (_GraphCaptureExtents.width < 500)
			{
				float center = _GraphCaptureExtents.center.x;
				_GraphCaptureExtents.xMin = center - 250;
				_GraphCaptureExtents.xMax = center + 250;
			}
			if (_GraphCaptureExtents.height < 500)
			{
				float center = _GraphCaptureExtents.center.y;
				_GraphCaptureExtents.yMin = center - 250;
				_GraphCaptureExtents.yMax = center + 250;
			}

			_GraphCaptureExtents.x = Mathf.Floor(_GraphCaptureExtents.x);
			_GraphCaptureExtents.width = Mathf.Floor(_GraphCaptureExtents.width);
			_GraphCaptureExtents.y = Mathf.Floor(_GraphCaptureExtents.y);
			_GraphCaptureExtents.height = Mathf.Floor(_GraphCaptureExtents.height);

			int maxTextureSize = SystemInfo.maxTextureSize;
			if (_GraphCaptureExtents.width <= maxTextureSize && _GraphCaptureExtents.height <= maxTextureSize)
			{
				_IsCapture = true;

				VisualElement target = _GraphView.contentContainer;

				ITransform transform = target.transform;

				Vector3 oldPosition = transform.position;
				Quaternion oldRotation = transform.rotation;
				Vector3 oldScale = transform.scale;

				transform.rotation = Quaternion.identity;
				transform.scale = Vector3.one;
				transform.position = -_GraphCaptureExtents.position;

				Rect oldGraphExtents = _GraphView.graphExtents;
				_GraphView.graphExtents = _GraphCaptureExtents;

				_GraphCaptureExtents.position = Vector2.zero;

				Color oldColor = _LogoImage.tintColor;
				VisualElement oldLogoParent = _LogoImage.parent;

				_LogoImage.tintColor = new Color(1f, 1f, 1f, 0.5f);
				if (oldLogoParent == null)
				{
					_GraphView.contentOverlay.Add(_LogoImage);
				}

				if (_Capture == null)
				{
					_Capture = CreateInstance<UnityEditorBridge.UIElements.VisualElementCapture>();
				}

				if (_Capture.Capture(_GraphView.contentViewport, _GraphCaptureExtents))
				{
					string path = EditorUtility.SaveFilePanel("Save", ArborEditorCache.captureDirectory, _SelectedGraphItem.displayName, "png");
					if (!string.IsNullOrEmpty(path))
					{
						ArborEditorCache.captureDirectory = System.IO.Path.GetDirectoryName(path);
					}
					_Capture.SaveImage(path, true);
					_Capture.DestroyImage();
				}

				_LogoImage.tintColor = oldColor;
				if (oldLogoParent == null)
				{
					_LogoImage.RemoveFromHierarchy();
				}

				transform.position = oldPosition;
				transform.rotation = oldRotation;
				transform.scale = oldScale;

				_GraphView.graphExtents = oldGraphExtents;

				_IsCapture = false;
			}
			else
			{
				Debug.LogError("Screenshot failed : Graph size is too large.");
			}
		}

		void OnDestroyNodeGraph(NodeGraph nodeGraph)
		{
			if (EditorApplication.isPlaying != EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			if (ReferenceEquals(_NodeGraphRoot, nodeGraph))
			{
				Undo.RecordObject(this, "Destroy NodeGraph");

				SetNodeGraphCurrent(null);
				SetNodeGraphRoot(null);
			}
			else if (ReferenceEquals(_NodeGraphCurrent, nodeGraph))
			{
				Undo.RecordObject(this, "Destroy NodeGraph");

				var rootItem = FindTreeViewItem(_NodeGraphRoot) as GraphTreeViewItem;
				SetNodeGraphCurrent(rootItem);
			}

			RebuildGraphEditor();

			EditorUtility.SetDirty(this);

			DoRepaint();
		}

		public void OnChangedGraphTree()
		{
			BuildTree(_NodeGraphRoot);
		}

		static void AddGraphItem(TreeViewItem parent, NodeGraph nodeGraph)
		{
			if (nodeGraph == null)
			{
				return;
			}

			int nodeCount = nodeGraph.nodeCount;
			for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
			{
				INodeBehaviourContainer behaviours = nodeGraph.GetNodeFromIndex(nodeIndex) as INodeBehaviourContainer;
				if (behaviours == null)
				{
					continue;
				}

				int behaviourCount = behaviours.GetNodeBehaviourCount();
				for (int behaviourIndex = 0; behaviourIndex < behaviourCount; behaviourIndex++)
				{
					NodeBehaviour behaviour = behaviours.GetNodeBehaviour<NodeBehaviour>(behaviourIndex);
					ISubGraphBehaviour subGraphBehaviour = behaviour as ISubGraphBehaviour;
					if (subGraphBehaviour != null)
					{
						var referenceGraph = subGraphBehaviour.GetSubGraph();
						if (referenceGraph != null)
						{
							var newItem = new SubGraphTreeViewItem(behaviour.GetInstanceID(), subGraphBehaviour);
							parent.AddChild(newItem);

							if (!subGraphBehaviour.isExternal)
							{
								AddGraphItem(newItem, subGraphBehaviour.GetSubGraph());
							}
						}
					}
				}
			}
		}

		void BuildTree(NodeGraph rootGraph)
		{
			int currentId = _SelectedGraphItem != null ? _SelectedGraphItem.id : 0;
			_TreeView.ClearTree();

			if (rootGraph != null)
			{
				var item = new GraphTreeViewItem(rootGraph);
				_TreeView.root.AddChild(item);

				AddGraphItem(item, rootGraph);
			}

			_TreeView.SetupDepths();

			_GraphTreeElement.UpdateViewTree();

			GraphTreeViewItem nextSelectedGraphItem = null;

			if (currentId != 0)
			{
				nextSelectedGraphItem = _TreeView.FindItem(currentId) as GraphTreeViewItem;
			}
			if (nextSelectedGraphItem == null && _NodeGraphCurrent != null)
			{
				nextSelectedGraphItem = FindTreeViewItem(_NodeGraphCurrent) as GraphTreeViewItem;
			}

			SetNodeGraphCurrent(nextSelectedGraphItem);

			Repaint();
		}

		TreeViewItem FindTreeViewItem(NodeGraph nodeGraph)
		{
			if (nodeGraph == null)
			{
				return null;
			}

			return _TreeView.FindItem((item) =>
			{
				GraphTreeViewItem graphItem = item as GraphTreeViewItem;
				return graphItem != null && graphItem.nodeGraph == nodeGraph;
			});
		}

		void RegisterRootGraphCallback()
		{
			UnregisterRootGraphCallback();

			_NodeGraphRoot.destroyCallback += OnDestroyNodeGraph;
			_NodeGraphRoot.stateChangedCallback += OnStateChanged;
			_NodeGraphRoot.onChangedGraphTree += OnChangedGraphTree;
			_NodeGraphRoot.onChangedGraphName += OnChangedGraphName;
		}

		void OnChangedGraphName()
		{
			_GraphTabHeaderElement.text = _NodeGraphRoot.graphName;
		}

		void UnregisterRootGraphCallback()
		{
			_NodeGraphRoot.destroyCallback -= OnDestroyNodeGraph;
			_NodeGraphRoot.stateChangedCallback -= OnStateChanged;
			_NodeGraphRoot.onChangedGraphTree -= OnChangedGraphTree;
			_NodeGraphRoot.onChangedGraphName -= OnChangedGraphName;
		}

		void SetNodeGraphRoot(NodeGraph nodeGraph)
		{
			if (!object.ReferenceEquals(_NodeGraphRoot,null))
			{
				UnregisterRootGraphCallback();
				_NodeGraphRootInstanceID = 0;
			}

			if (_NodeGraphRoot != nodeGraph)
			{
				_TreeViewState.Clear();
			}

			_NodeGraphRoot = nodeGraph;

			ShowGraphTab(_NodeGraphRoot != null);
			if (_NodeGraphRoot != null)
			{
				_GraphTabHeaderElement.text = _NodeGraphRoot.graphName;
			}

			SetupToolbarObjectField();

			BuildTree(_NodeGraphRoot);

			if (_NodeGraphRoot != null)
			{
				RegisterRootGraphCallback();
				_NodeGraphRootInstanceID = _NodeGraphRoot.GetInstanceID();
			}
			else
			{
				_NodeGraphRootInstanceID = 0;
			}
		}

		void RegisterCurrentGraphCallback()
		{
			UnregisterCurrentGraphCallback();

			_NodeGraphCurrent.destroyCallback += OnDestroyNodeGraph;
			_NodeGraphCurrent.stateChangedCallback += OnStateChanged;
		}

		void UnregisterCurrentGraphCallback()
		{
			_NodeGraphCurrent.destroyCallback -= OnDestroyNodeGraph;
			_NodeGraphCurrent.stateChangedCallback -= OnStateChanged;
		}

		void BuildBreadcrumbs()
		{
			List<TreeViewItem> items = new List<TreeViewItem>();

			NodeGraph currentGraph = _NodeGraphCurrent;
			if (currentGraph == null && _NodeGraphCurrentInstanceID != 0)
			{
				currentGraph = EditorUtility.InstanceIDToObject(_NodeGraphCurrentInstanceID) as NodeGraph;
			}
			var currentItem = FindTreeViewItem(currentGraph);
			while (currentItem != null && currentItem.id != 0)
			{
				items.Insert(0, currentItem);
				currentItem = currentItem.parent;
			}

			_Breadcrumbs.Clear();
			for (int i = 0; i < items.Count; i++)
			{
				var item = items[i];
				var graphItem = item as GraphTreeViewItem;
				ToolbarButton button = new ToolbarButton(() =>
				{
					ChangeCurrentNodeGraph(graphItem);
				});
				button.text = item.displayName;
				button.RemoveFromClassList(ToolbarButton.ussClassName);
				button.AddToClassList("breadcrumbs-item");
				if (i == 0)
				{
					button.AddToClassList("breadcrumbs-first-item");
				}
				if (graphItem.isExternal)
				{
					button.AddToClassList("external");
				}
				if (i == items.Count - 1)
				{
					button.AddToClassList("on");
				}
				item.onChangedDisplayName += _ =>
				{
					button.text = item.displayName;
				};
				_Breadcrumbs.Add(button);
			}
		}

		void SetNodeGraphCurrent(GraphTreeViewItem graphItem)
		{
			NodeGraph nodeGraph = graphItem != null ? graphItem.nodeGraph : null;

			StoreCurrentTransform();

			if (!object.ReferenceEquals(_NodeGraphCurrent, null))
			{
				UnregisterCurrentGraphCallback();
				_NodeGraphCurrentInstanceID = 0;
			}

			_NodeGraphCurrent = nodeGraph;
			_SelectedGraphItem = graphItem;
			SelectGraphTreeItem(graphItem);

			if (_NodeGraphCurrent != null)
			{
				RegisterCurrentGraphCallback();
				_NodeGraphCurrentInstanceID = _NodeGraphCurrent.GetInstanceID();
			}
			else
			{
				_NodeGraphCurrentInstanceID = 0;
			}

			BuildBreadcrumbs();
		}

		private void ReatachIfNecessary()
		{
			bool reatached = false;

			bool setRoot = false;

			NodeGraph currentGraph = _NodeGraphCurrent;

			if (currentGraph != null && (_SelectedGraphItem == null || _SelectedGraphItem.nodeGraph != currentGraph))
			{
				if (!setRoot)
				{
					BuildTree(_NodeGraphRoot);
				}
				else
				{
					var currentItem = FindTreeViewItem(currentGraph) as GraphTreeViewItem;
					SetNodeGraphCurrent(currentItem);
				}
				RebuildGraphEditor();
				reatached = true;
			}

			if (!reatached)
			{
				if (_NodeGraphRoot != null)
				{
					RegisterRootGraphCallback();
				}

				if (_NodeGraphCurrent != null)
				{
					RegisterCurrentGraphCallback();
				}
			}

			if (reatached || _NodeGraphRoot == null || _NodeGraphCurrent == null)
			{
				Initialize();
			}
			else
			{
				if ((_GraphEditor == null && _NodeGraphCurrent != null) || (_GraphEditor != null && _GraphEditor.nodeGraph != _NodeGraphCurrent))
				{
					RebuildGraphEditor();
				}
				else if (_GraphEditor != null && _GraphEditor.nodeGraph == null)
				{
					Initialize();
				}
			}
		}

		public void FrameSelected(Vector2 frameSelectTarget)
		{
			_GraphView.FrameSelected(frameSelectTarget);

			DoRepaint();
		}

		public bool OverlapsVewArea(Rect position)
		{
			return _GraphView.graphViewport.Overlaps(position);
		}

		void UpdateScrollbar()
		{
			if (_GraphView.UpdateFrameSelected())
			{
				DoRepaint();
			}
		}

		void OnStateChanged(NodeGraph nodeGraph)
		{
			_IsRepaint = true;

			if (_GraphEditor != null && _GraphEditor.nodeGraph == nodeGraph)
			{
				_IsUpdateLiveTracking = true;
			}
		}

		private void FlipLocked()
		{
			_IsLocked = !_IsLocked;
		}

		public void AddItemsToMenu(GenericMenu menu)
		{
			menu.AddItem(EditorGUITools.GetTextContent("Lock"), _IsLocked, FlipLocked);
		}

		internal void OnChangedScroll()
		{
			StoreCurrentTransform();
		}

		private void CenterOnStoredPosition(GraphTreeViewItem graphItem)
		{
			if (!_GraphView.isLayoutSetup)
			{
				return;
			}

			if (graphItem != null && _TransformCache.HasTransform(graphItem.id))
			{
				int id = graphItem.id;
				Vector2 scrollPos = _TransformCache.GetPosition(id);
				Vector3 scale = _TransformCache.GetScale(id);

				_GraphView.SetZoom(Vector2.zero, scale, false, false);
				_GraphView.SetScroll(scrollPos, true, false);
			}
			else
			{
				_GraphView.SetZoom(Vector2.zero, Vector3.one, false, false);

				Vector2 center = _GraphView.graphExtents.center - _GraphView.graphViewRect.size * 0.5f;
				center.x = Mathf.Floor(center.x);
				center.y = Mathf.Floor(center.y);
				_GraphView.SetScroll(center, true, false);
			}
		}

		private void StoreCurrentTransform(Vector3 scrollPos, Vector3 graphScale)
		{
			if (_SelectedGraphItem == null)
			{
				return;
			}

			if (!_GraphView.isLayoutSetup)
			{
				return;
			}

			int id = _SelectedGraphItem.id;
			_TransformCache.SetPosition(id, scrollPos);
			_TransformCache.SetScale(id, graphScale);
		}

		private void StoreCurrentTransform()
		{
			StoreCurrentTransform(_GraphView.scrollPos, _GraphView.graphScale);
		}

#if ARBOR_TRIAL
		private class TrialButton : Button
		{
			public TrialButton(System.Action clickEvent) : base(clickEvent)
			{
			}
		}
#endif

		internal void ChangedSidePanel()
		{
			_MainLayout.ShowLeftPanel(ArborSettings.openSidePanel);
			if (ArborSettings.openSidePanel)
			{
				sidePanel.toolbar.Add(_SidePanelToggle);
			}
			else
			{
				_Toolbar.Insert(0, _SidePanelToggle);
			}
		}

		private static readonly string s_ItemExternalButtonName = "arbor-tree-view__item-external_button";

		VisualElement MakeTreeItem()
		{
			var itemContainer = new VisualElement()
			{
				style =
					{
						flexDirection = FlexDirection.Row,
					}
			};

			Button externalButton = null;
			System.Action clicked = () =>
			{
				var graphTreeViewItem = externalButton.userData as GraphTreeViewItem;
				SelectExternalGraph(graphTreeViewItem);
			};
			externalButton = new Button(clicked)
			{
				name = s_ItemExternalButtonName
			};
			externalButton.RemoveFromClassList(Button.ussClassName);
			externalButton.AddToClassList("arrow-navigation-right");

			itemContainer.hierarchy.Add(externalButton);

			return itemContainer;
		}

		void BindTreeItem(VisualElement element, TreeViewItem item)
		{
			var graphTreeItem = item as GraphTreeViewItem;
			var itemContainer = UIElementsUtility.GetFirstAncestorWithClass(element, TreeViewElement.itemUssClassName);
			itemContainer.EnableInClassList("external", graphTreeItem != null && graphTreeItem.isExternal);

			var externalButton = element.Q<Button>(s_ItemExternalButtonName);
			if (graphTreeItem != null && graphTreeItem.isExternal)
			{
				externalButton.userData = graphTreeItem;
				externalButton.style.display = DisplayStyle.Flex;
			}
			else
			{
				externalButton.style.display = DisplayStyle.None;
			}
		}

		void SetupElements()
		{
			_MainLayout = new GraphMainLayout() {
				name = "MainLayout"
			};

			sidePanel = new TabPanel<SidePanelTab>()
			{
				style =
				{
					minHeight = 100f,
				}
			};

			sidePanel.CreateTab("Graph", SidePanelTab.Graph);
			sidePanel.CreateTab("NodeList", SidePanelTab.NodeList);			
			sidePanel.CreateTab("Parameters", SidePanelTab.Parameters);			

			sidePanel.SetValueWithoutNotify(ArborSettings.sidePanelTab);

			sidePanel.RegisterValueChangedCallback(e =>
			{
				ArborSettings.sidePanelTab = e.newValue;
			});

			_MainLayout.leftPanel.Add(sidePanel);

			_GraphTabElement = new VisualElement()
			{
				style =
				{
					flexGrow = 1f,
				}
			};

			_GraphTabHeaderElement = new Button(() =>
			{
				EditorGUIUtility.PingObject(_NodeGraphRoot.gameObject);
			})
			{
				style =
				{
					height = 25f,
				}
			};
			_GraphTabHeaderElement.RemoveFromClassList(Button.ussClassName);
			_GraphTabHeaderElement.AddToClassList("header-bar");
			_GraphTabHeaderElement.AddToClassList("graph-tree-header");
			UIElementsUtility.SetBoldFont(_GraphTabHeaderElement);
			_GraphTabElement.Add(_GraphTabHeaderElement);

			Button externalPrevButton = new Button(() =>
			{
				SelectRootGraph(_NodeGraphRootPrev);
			});
			externalPrevButton.RemoveFromClassList(Button.ussClassName);
			externalPrevButton.AddToClassList("arrow-navigation-left");
			_GraphTabHeaderElement.Add(externalPrevButton);

			_GraphTreeElement = new TreeViewElement(_TreeView, _TreeViewState, MakeTreeItem, BindTreeItem)
			{
				selectSubmit = true,
				renamable = true,
			};
			_GraphTreeElement.AddToClassList("graph-tree");
			_GraphTreeElement.onSubmit += OnSubmitItem;
			_GraphTreeElement.onRenameEnded += OnRenameEndedItem;
			_GraphTabElement.Add(_GraphTreeElement);

			ShowGraphTab(_NodeGraphRoot != null);
			ShowGraphTabHeader(_NodeGraphRootPrev != null);
			if (_NodeGraphRoot != null)
			{
				_GraphTabHeaderElement.text = _NodeGraphRoot.graphName;
			}

			_MinimapElement = new ResizableElement()
			{
				minSize = 100f,
			};

			_MinimapElement.SetValueWithoutNotify(ArborSettings.minimapSize);

			var resizerLabel = new Label()
			{
				pickingMode = PickingMode.Ignore,
			};
			UIElementsUtility.SetBoldFont(resizerLabel);
			resizerLabel.AddManipulator(new LocalizationManipulator("Minimap", LocalizationManipulator.TargetText.Text));
			_MinimapElement.header.Add(resizerLabel);

			_MinimapElement.RegisterValueChangedCallback(e =>
			{
				ArborSettings.minimapSize = e.newValue;
			});

			_MinimapElement.contentContainer.AddToClassList("graphview-background");

			_MinimapView = new MinimapView(this)
			{
				style =
				{
					flexGrow = 1f,
				}
			};
			_MinimapElement.Add(_MinimapView);

			var sliderGUI = new IMGUIContainer(OnZoomSliderGUI)
			{
				style =
				{
					marginLeft = 2f,
					marginRight = 2f,
					marginBottom = 4f,
				}
			};
			_MinimapElement.Add(sliderGUI);

			_Toolbar = new Toolbar()
			{
				style =
				{
					paddingLeft = 6f,
					paddingRight = 6f,
					flexShrink = 0f,
				}
			};
			_MainLayout.rightPanel.Add(_Toolbar);
			
			_SidePanelToggle = new Toggle();
			_SidePanelToggle.AddToClassList("visibility");
			_SidePanelToggle.SetValueWithoutNotify(ArborSettings.openSidePanel);
			_SidePanelToggle.AddManipulator(new LocalizationManipulator("Side Panel", LocalizationManipulator.TargetText.Tooltip));

			_SidePanelToggle.RegisterValueChangedCallback(e =>
			{
				ArborSettings.openSidePanel = e.newValue;
				ChangedSidePanel();
			});

			ChangedSidePanel();

			ToolbarDropdown toolbarCreateDropdown = null;
			toolbarCreateDropdown = new ToolbarDropdown(() =>
			{
				OpenCreateMenu(toolbarCreateDropdown.worldBound);
			})
			{
				style =
				{
					flexShrink = 0f,
				}
			};
			toolbarCreateDropdown.AddManipulator(new LocalizationManipulator("Create", LocalizationManipulator.TargetText.Text));
			_Toolbar.Add(toolbarCreateDropdown);

			_ToolbarObjectField = new ObjectField()
			{
				objectType = typeof(NodeGraph),
				allowSceneObjects = true,
				style =
				{
					width = 200f,
					flexShrink = 1f,
				}
			};
			SetupToolbarObjectField();
			_ToolbarObjectField.RegisterValueChangedCallback(e =>
			{
				SelectRootGraph(e.newValue as NodeGraph);
			});

			_Toolbar.Add(_ToolbarObjectField);

			_Toolbar.Add(new ToolbarSpacer()
			{
				flex = true,
			});

			_ToolbarGraphEditor = new VisualElement()
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					flexShrink = 0f,
				}
			};
			_Toolbar.Add(_ToolbarGraphEditor);

			SetupToolbarGraphEditor();

			EnableCustomToolbar(s_ToolbarGUI != null);

			_ToolbarLiveTracking = new ToolbarToggle();
			_ToolbarLiveTracking.AddManipulator(new LocalizationManipulator("Live Tracking", LocalizationManipulator.TargetText.Text));
			_ToolbarLiveTracking.SetValueWithoutNotify(ArborSettings.liveTracking);

			_ToolbarLiveTracking.RegisterValueChangedCallback(e =>
			{
				ArborSettings.liveTracking = e.newValue;
				if (EditorApplication.isPlaying)
				{
					_IsUpdateLiveTracking = true;
				}
			});

			_ToolbarGraphEditor.Add(_ToolbarLiveTracking);

			ToolbarDropdown toolbarViewDropdown = null;
			toolbarViewDropdown = new ToolbarDropdown(() =>
			{
				GenericMenu menu = new GenericMenu();

				_GraphEditor.SetViewMenu(menu);

				menu.DropDown(toolbarViewDropdown.worldBound);
			});
			toolbarViewDropdown.AddManipulator(new LocalizationManipulator("View", LocalizationManipulator.TargetText.Text));
			_ToolbarGraphEditor.Add(toolbarViewDropdown);

			ToolbarDropdown toolbarDebugDropdown = null;
			toolbarDebugDropdown = new ToolbarDropdown(() =>
			{
				GenericMenu menu = new GenericMenu();

				_GraphEditor.SetDenugMenu(menu);

				menu.DropDown(toolbarDebugDropdown.worldBound);
			});
			toolbarDebugDropdown.AddManipulator(new LocalizationManipulator("Debug", LocalizationManipulator.TargetText.Text));
			_ToolbarGraphEditor.Add(toolbarDebugDropdown);

			Button captureButton = new Button(Capture);
			captureButton.RemoveFromClassList(Button.ussClassName);
			captureButton.AddToClassList("toolbar-icon-button");
			captureButton.Add(new Image()
			{
				image = Icons.captureIcon,
				tintColor = EditorGUITools.GetIconColor(),
			});
			captureButton.AddManipulator(new LocalizationManipulator("Screen Shot", LocalizationManipulator.TargetText.Tooltip));
			_ToolbarGraphEditor.Add(captureButton);

			_ToolbarNotificationButton = new Button(UpdateNotificationWindow.Open)
			{
				style =
				{
					flexShrink = 0f,
				}
			};
			_ToolbarNotificationButton.RemoveFromClassList(Button.ussClassName);
			_ToolbarNotificationButton.AddToClassList("toolbar-icon-button");
			_ToolbarNotificationButton.Add(new Image()
			{
				image = Icons.notificationIcon,
				tintColor = EditorGUITools.GetIconColor(),
			});
			_ToolbarNotificationButton.AddManipulator(new LocalizationManipulator("Notification", LocalizationManipulator.TargetText.Tooltip));
			_ToolbarNotificationButton.RegisterCallback<AttachToPanelEvent>(e =>
			{
				ArborVersion.instance.onLoaded += SetupNotificationButton;
			});
			_ToolbarNotificationButton.RegisterCallback<DetachFromPanelEvent>(e =>
			{
				ArborVersion.instance.onLoaded -= SetupNotificationButton;
			});
			_Toolbar.Add(_ToolbarNotificationButton);

			if (ArborUpdateCheck.instance.isDone)
			{
				SetupNotificationButton();
			}
			else
			{
				ArborUpdateCheck.instance.onDone += SetupNotificationButton;
			}

			Button helpButton = null;
			helpButton = new Button(() =>
			{
				GenericMenu menu = new GenericMenu();

				SetHelpMenu(menu);

				menu.DropDown(helpButton.worldBound);
			})
			{
				style =
				{
					flexShrink = 0f,
				}
			};
			helpButton.RemoveFromClassList(Button.ussClassName);
			helpButton.AddToClassList("toolbar-icon-button");
			helpButton.Add(new Image()
			{
				image = Icons.helpIcon,
			});
			helpButton.AddManipulator(new LocalizationManipulator("Help", LocalizationManipulator.TargetText.Tooltip));
			_Toolbar.Add(helpButton);

			Button settingsButton = null;
			settingsButton = new Button(() =>
			{
				if (_GraphSettingsWindow == null)
				{
					_GraphSettingsWindow = new GraphSettingsWindow(this);
				}
				PopupWindowUtility.Show(settingsButton.worldBound, _GraphSettingsWindow, true);
			})
			{
				style =
				{
					flexShrink = 0f,
				}
			};
			settingsButton.RemoveFromClassList(Button.ussClassName);
			settingsButton.AddToClassList("toolbar-icon-button");
			settingsButton.AddManipulator(new LocalizationManipulator("Settings", LocalizationManipulator.TargetText.Tooltip));
			settingsButton.Add(new Image()
			{
				image = Icons.popupIcon,
			});
			_Toolbar.Add(settingsButton);

			_GraphPanel = new GraphLayout() {
				name = "GraphPanel",
				style =
				{
					flexGrow = 1f,
				}
			};

			_GraphView = new GraphView(this) {
				name = "GraphView",
			};

			_CustomUnderlayLayer = new VisualElement()
			{
				pickingMode = PickingMode.Ignore,
				focusable = false,
			};
			_CustomUnderlayLayer.StretchToParentSize();
			_GraphView.contentUnderlay.Add(_CustomUnderlayLayer);

			EnableCustomUnderlay(s_ToolbarGUI != null);

			_GraphLabelElement = new Label();
			_GraphLabelElement.AddToClassList("graph-label");
			UIElementsUtility.SetBoldFont(_GraphLabelElement);

			_GraphView.contentUnderlay.Add(_GraphLabelElement);

			_GraphPlayStateElement = new Label();
			_GraphPlayStateElement.AddToClassList("graph-playstate-label");

			_GraphView.contentUnderlay.Add(_GraphPlayStateElement);

			OnChangedShowGrid();

			highlightLayer = new GraphLayout()
			{
				name = "HighlightLayer",
			};

			_GraphView.contentOverlay.Add(highlightLayer);

			popupLayer = new GraphLayout()
			{
				name = "PopupLayer",
			};

			_GraphView.contentOverlay.Add(popupLayer);

			OnChangedNodeCommentAffectsZoom();

			_CustomOverlayLayer = new VisualElement()
			{
				pickingMode = PickingMode.Ignore,
				focusable = false,
			};
			_CustomOverlayLayer.StretchToParentSize();
			_GraphView.contentOverlay.Add(_CustomOverlayLayer);

			EnableCustomOverlay(s_OverlayGUI != null);

			Texture2D logoTex = Icons.logo;
			float logoWidth = 256f;
			float logoScale = logoWidth / logoTex.width;
			float logoHeight = logoTex.height * logoScale;

			_LogoImage = new Image()
			{
				pickingMode = PickingMode.Ignore,
				image = logoTex,
				scaleMode = ScaleMode.ScaleToFit,
				style =
				{
					position = Position.Absolute,
					top = 0,
					left = 0,
					width = logoWidth,
					height = logoHeight,
				}
			};

#if ARBOR_TRIAL
			TrialButton assetStoreButton = new TrialButton(() =>
			{
				ArborVersion.OpenAssetStore();
			})
			{
				text = "Open Asset Store",
				style =
				{
					position = Position.Absolute,
					bottom = 16,
					left = 16,
				}
			};

			_GraphView.contentOverlay.Add(assetStoreButton);
#endif

			_NoGraphUI = CreateNoGraphElement();

			ShowGraphView(_GraphEditor != null);

			_MainLayout.rightPanel.Add(_GraphPanel);

			_Breadcrumbs = new Toolbar();
			_Breadcrumbs.AddToClassList("breadcrumbs");
			_MainLayout.rightPanel.Add(_Breadcrumbs);

			rootVisualElement.Add(_MainLayout);

			ArborStyleSheets.Setup(rootVisualElement);

			if (_GraphEditor != null)
			{
				_GraphEditor.DirtyGraphExtents();
			}

			rootVisualElement.RegisterCallback<FocusEvent>(OnFocusElement, TrickleDown.TrickleDown);
			rootVisualElement.RegisterCallback<BlurEvent>(OnBlurElement, TrickleDown.TrickleDown);
		}

		private VisualElement _CurrentFocusedElement;

		void OnFocusElement(FocusEvent e)
		{
			_CurrentFocusedElement = e.target as VisualElement;
		}

		void OnBlurElement(BlurEvent e)
		{
			if (_CurrentFocusedElement == e.target)
			{
				_CurrentFocusedElement = null;
			}
		}

		VisualElement CreateNoGraphElement()
		{
			VisualElement noGraphElement = new VisualElement()
			{
				style =
				{
					alignItems = Align.Center,
					justifyContent = Justify.Center,
					overflow = Overflow.Hidden,
				}
			};
			noGraphElement.StretchToParentSize();

			var label = new Label()
			{
				style =
				{
					marginBottom = 8f,
				}
			};
			label.AddManipulator(new LocalizationManipulator("NoGraphSelected.Message", LocalizationManipulator.TargetText.Text));
			noGraphElement.Add(label);

			DropdownButton createButton = null;
			createButton = new DropdownButton(() =>
			{
				OpenCreateMenu(createButton.worldBound);
			})
			{
				style =
				{
					paddingTop = 3f,
					paddingBottom = 3f,
					marginBottom = 8f,
				}
			};
			createButton.AddManipulator(new LocalizationManipulator("Create", LocalizationManipulator.TargetText.Text));
			noGraphElement.Add(createButton);

			float buttonWidth = 130;

			VisualElement buttons = new VisualElement()
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					flexWrap = Wrap.Wrap,
					width = buttonWidth * 3,
				}
			};

			noGraphElement.Add(buttons);

			Button assetStore = new Button(() =>
			{
				ArborVersion.OpenAssetStore();
			})
			{
				style =
				{
					width = buttonWidth,
				}
			};
			assetStore.AddToClassList("button-left");
			assetStore.AddManipulator(new LocalizationManipulator("Asset Store", LocalizationManipulator.TargetText.Text));
			buttons.Add(assetStore);

			Button officialSite = new Button(() =>
			{
				Help.BrowseURL(Localization.GetWord("SiteURL"));
			})
			{
				style =
				{
					width = buttonWidth,
				}
			};
			officialSite.AddToClassList("button-middle");
			officialSite.AddManipulator(new LocalizationManipulator("Official Site", LocalizationManipulator.TargetText.Text));
			buttons.Add(officialSite);

			Button releaseNodes = new Button(() =>
			{
				Help.BrowseURL(Localization.GetWord("ReleaseNotesURL"));
			})
			{
				style =
				{
					width = buttonWidth,
				}
			};
			releaseNodes.AddToClassList("button-right");
			releaseNodes.AddManipulator(new LocalizationManipulator("Release Notes", LocalizationManipulator.TargetText.Text));
			buttons.Add(releaseNodes);

			var manual = new Button(() =>
			{
				Help.BrowseURL(Localization.GetWord("ManualURL"));
			})
			{
				style =
				{
					width = buttonWidth,
				}
			};
			manual.AddToClassList("button-left");
			manual.AddManipulator(new LocalizationManipulator("Manual", LocalizationManipulator.TargetText.Text));
			buttons.Add(manual);

			var inspectorReference = new Button(() =>
			{
				Help.BrowseURL(Localization.GetWord("InspectorReferenceURL"));
			})
			{
				style =
				{
					width = buttonWidth,
				}
			};
			inspectorReference.AddToClassList("button-middle");
			inspectorReference.AddManipulator(new LocalizationManipulator("Inspector Reference", LocalizationManipulator.TargetText.Text));
			buttons.Add(inspectorReference);

			var scriptReference = new Button(() =>
			{
				Help.BrowseURL(Localization.GetWord("ScriptReferenceURL"));
			})
			{
				style =
				{
					width = buttonWidth,
				}
			};
			scriptReference.AddToClassList("button-right");
			scriptReference.AddManipulator(new LocalizationManipulator("Script Reference", LocalizationManipulator.TargetText.Text));
			buttons.Add(scriptReference);

			return noGraphElement;
		}

		void ShowGraphTab(bool show)
		{
			if (show)
			{
				if (_GraphTabElement.parent == null)
				{
					sidePanel.GetTab(SidePanelTab.Graph).Add(_GraphTabElement);
				}
			}
			else if (_GraphTabElement.parent != null)
			{
				_GraphTabElement.RemoveFromHierarchy();
			}
		}

		void ShowGraphTabHeader(bool show)
		{
			if (show)
			{
				_GraphTabHeaderElement.style.display = DisplayStyle.Flex;
			}
			else
			{
				_GraphTabHeaderElement.style.display = DisplayStyle.None;
			}
		}

		void OnZoomSliderGUI()
		{
			if (_GraphEditor == null || _GraphEditor.nodeGraph == null)
			{
				return;
			}

			float zoomLevel = _GraphView.graphScale.x * 100;
			EditorGUI.BeginChangeCheck();
			zoomLevel = EditorGUILayout.Slider(GUIContent.none, zoomLevel, k_ZoomMin, k_ZoomMax) / 100f;
			if (EditorGUI.EndChangeCheck())
			{
				_GraphView.SetZoom(_GraphView.graphViewport.center, new Vector3(zoomLevel, zoomLevel, 1f), true);
			}
		}

		void EnableCustomToolbar(bool enable)
		{
			if (enable)
			{
				if (_CustomToolbarGUI == null)
				{
					System.Action onGUIHandler = () =>
					{
						if (_GraphEditor != null && s_ToolbarGUI != null)
						{
							EditorGUILayout.BeginHorizontal();

							s_ToolbarGUI(_GraphEditor.nodeGraph);

							EditorGUILayout.EndHorizontal();
						}
					};
					var imguiContainer = new IMGUIContainer(onGUIHandler);
					_CustomToolbarGUI = imguiContainer;
				}
				if (_CustomToolbarGUI.parent == null)
				{
					_ToolbarGraphEditor.Insert(0, _CustomToolbarGUI);
				}
			}
			else
			{
				if (_CustomToolbarGUI != null && _CustomToolbarGUI.parent != null)
				{
					_CustomToolbarGUI.RemoveFromHierarchy();
				}
			}
		}

		void EnableCustomUnderlay(bool enable)
		{
			if (enable)
			{
				if (_CustomUnderlayGUI == null)
				{
					System.Action onGUIHandler = () =>
					{
						Rect rect = _CustomUnderlayGUI.contentRect;
						if (RectUtility.IsNaN(rect))
						{
							return;
						}
						UnderlayGUI(rect);
					};
					var imguiContainer = new IMGUIContainer(onGUIHandler)
					{
						name = "CustomUnderlayGUI",
						pickingMode = PickingMode.Ignore,
						focusable = false,
					};
					_CustomUnderlayGUI = imguiContainer;
				}
				_CustomUnderlayGUI.StretchToParentSize();
				if (_CustomUnderlayGUI.parent == null)
				{
					_CustomUnderlayLayer.Insert(0, _CustomUnderlayGUI);
				}
			}
			else
			{
				if (_CustomUnderlayGUI != null && _CustomUnderlayGUI.parent != null)
				{
					_CustomUnderlayGUI.RemoveFromHierarchy();
				}
			}
		}

		void EnableCustomOverlay(bool enable)
		{
			if (enable)
			{
				if (_CustomOverlayGUI == null)
				{
					System.Action onGUIHandler = () =>
					{
						Rect rect = _CustomOverlayGUI.contentRect;
						if (RectUtility.IsNaN(rect))
						{
							return;
						}

						OverlayGUI(rect);
					};
					var imguiContainer = new IMGUIContainer(onGUIHandler)
					{
						name = "CustomOverlayGUI",
						pickingMode = PickingMode.Ignore,
						focusable = false,
					};
					imguiContainer.StretchToParentSize();
					_CustomOverlayGUI = imguiContainer;
				}
				if (_CustomOverlayGUI.parent == null)
				{
					_CustomOverlayLayer.Insert(0, _CustomOverlayGUI);
				}
			}
			else
			{
				if (_CustomOverlayGUI != null && _CustomOverlayGUI.parent != null)
				{
					_CustomOverlayGUI.RemoveFromHierarchy();
				}
			}
		}

		void SetupToolbarObjectField()
		{
			if (_ToolbarObjectField != null)
			{
				_ToolbarObjectField.SetValueWithoutNotify(_NodeGraphRootPrev ?? _NodeGraphRoot);
			}
		}

		void SetupToolbarGraphEditor()
		{
			if (_GraphEditor != null)
			{
				_ToolbarGraphEditor.style.display = StyleKeyword.Null;
			}
			else
			{
				_ToolbarGraphEditor.style.display = DisplayStyle.None;
			}
		}

		void SetupNotificationButton()
		{
			ArborUpdateCheck updateCheck = ArborUpdateCheck.instance;
			if (updateCheck.isUpdated || updateCheck.isUpgrade)
			{
				_ToolbarNotificationButton.style.display = StyleKeyword.Null;
			}
			else
			{
				_ToolbarNotificationButton.style.display = DisplayStyle.None;
			}
		}

#if !UNITY_2021_1_OR_NEWER
		// Worked around a Unity issue that wasn't reflected when renamed.
		sealed class ObjectField : UnityEditor.UIElements.ObjectField
		{
			private readonly System.Action _AsyncOnProjectOrHierarchyChangedCallback;
			private readonly System.Action _OnProjectOrHierarchyChangedCallback;

			public ObjectField() : base()
			{
				_AsyncOnProjectOrHierarchyChangedCallback = () => schedule.Execute(_OnProjectOrHierarchyChangedCallback);
				_OnProjectOrHierarchyChangedCallback = UpdateContent;
				RegisterCallback<AttachToPanelEvent>((evt) =>
				{
					EditorApplication.projectChanged += _AsyncOnProjectOrHierarchyChangedCallback;
					EditorApplication.hierarchyChanged += _OnProjectOrHierarchyChangedCallback;
				});
				RegisterCallback<DetachFromPanelEvent>((evt) =>
				{
					EditorApplication.projectChanged -= _AsyncOnProjectOrHierarchyChangedCallback;
					EditorApplication.hierarchyChanged -= _OnProjectOrHierarchyChangedCallback;
				});
			}

			void UpdateContent()
			{
				// Call ObjectFieldDisplay.Update ().
				// Since it cannot be called internally, change the objectType and call it indirectly.
				var tmpType = objectType;
				objectType = typeof(Object);
				objectType = tmpType;
			}
		}
#endif
	}
}