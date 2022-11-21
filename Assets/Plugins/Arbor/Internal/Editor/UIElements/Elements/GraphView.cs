//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Reflection;

namespace ArborEditor.UIElements
{
	using ArborEditor.UnityEditorBridge.UIElements.Extensions;

	public interface IChangeGraphViewEvent
	{
	}

	public sealed class ChangeGraphScrollEvent : EventBase<ChangeGraphScrollEvent>, IChangeGraphViewEvent
	{
		public Vector3 oldPosition
		{
			get; 
			private set;
		}

		public Vector3 newPosition
		{
			get;
			private set;
		}

		public static ChangeGraphScrollEvent GetPooled(Vector3 oldPosition, Vector3 newPosition)
		{
			var e = GetPooled();
			e.oldPosition = oldPosition;
			e.newPosition = newPosition;
			return e;
		}
	}

	public sealed class ChangeGraphExtentsEvent : EventBase<ChangeGraphExtentsEvent>, IChangeGraphViewEvent
	{
		public Rect oldExtents
		{
			get;
			private set;
		}

		public Rect newExtents
		{
			get;
			private set;
		}

		public static ChangeGraphExtentsEvent GetPooled(Rect oldExtents, Rect newExtents)
		{
			var e = GetPooled();
			e.oldExtents = oldExtents;
			e.newExtents = newExtents;
			return e;
		}
	}

	public sealed class GraphView : ImmediateModeElement
	{
		public ArborEditorWindow window
		{
			get; private set;
		}

		public NodeGraphEditor graphEditor
		{
			get
			{
				return window.graphEditor;
			}
		}

		public enum ShowMode
		{
			Normal,
			ForceShow,
			ForceHide,
		}

		public ShowMode horizontalShowMode
		{
			get; set;
		}
		public ShowMode verticalShowMode
		{
			get; set;
		}

		public bool needsHorizontal
		{
			get
			{
				switch (horizontalShowMode)
				{
					case ShowMode.Normal:
						return graphExtents.width - graphViewport.width > 0;
					case ShowMode.ForceShow:
						return true;
					case ShowMode.ForceHide:
						return false;
				}

				return false;
			}
		}

		public bool needsVertical
		{
			get
			{
				switch(verticalShowMode)
				{
					case ShowMode.Normal:
						return graphExtents.height - graphViewport.height > 0;
					case ShowMode.ForceShow:
						return true;
					case ShowMode.ForceHide:
						return false;
				}

				return false;
			}
		}

		public Vector2 scrollOffset
		{
			get
			{
				return new Vector2(horizontalScroller.value, verticalScroller.value) + graphExtents.min;
			}
		}

		public Rect graphViewRect
		{
			get
			{
				return contentViewport.layout;
			}
		}

		public Rect graphViewport
		{
			get
			{
				return ElementToGraph(contentViewport, graphViewRect);
			}
		}

		public ITransform graphTransform
		{
			get
			{
				return m_ContentContainer.transform;
			}
		}

		public Vector3 graphPosition
		{
			get
			{
				return graphTransform.position;
			}
			set
			{
				var position = value;
				position.x = Mathf.Round(position.x);
				position.y = Mathf.Round(position.y);
				position.z = Mathf.Round(position.z);

				if (graphTransform.position != position)
				{
					graphTransform.position = position;
				}
			}
		}

		public Vector3 graphScale
		{
			get
			{
				return graphTransform.scale;
			}
			set
			{
				graphTransform.scale = value;
			}
		}

		public Matrix4x4 graphMatrix
		{
			get
			{
				return graphTransform.matrix;
			}
		}

		private Rect _GraphViewExtents = new Rect(0, 0, 100, 100);
		private Rect _GraphExtents = new Rect(0, 0, 100, 100);

		public event System.Action onChangedGraphExtents;
		public event System.Action onChangedGraphPosition;
		private bool _IsCallingChangedGraphExtents;
		private bool _IsCallingChangedGraphPosition;

		public Rect graphExtents
		{
			get
			{
				return _GraphViewExtents;
			}
			internal set
			{
				if (_GraphViewExtents != value)
				{
					isSettedGraphExtents = true;

					using (var e = ChangeGraphExtentsEvent.GetPooled(_GraphViewExtents, value))
					{
						_GraphViewExtents = value;

						if (onChangedGraphExtents != null)
						{
							if (!_IsCallingChangedGraphExtents)
							{
								_IsCallingChangedGraphExtents = true;
								try
								{
									onChangedGraphExtents();
								}
								finally
								{
									_IsCallingChangedGraphExtents = false;
								}
							}
						}

						if (!_IsCallingChangedGraphExtents)
						{
							_IsCallingChangedGraphExtents = true;
							try
							{
								e.target = this;
								SendEvent(e);
							}
							finally
							{
								_IsCallingChangedGraphExtents = false;
							}
						}
					}
				}
			}
		}

		public Rect graphExtentsRaw
		{
			get
			{
				return _GraphExtents;
			}
		}

		public Vector2 scrollPos
		{
			get
			{
				return CalcTransformToGraph(graphPosition);
			}
			private set
			{
				graphPosition = CalcGraphToTransform(value);
			}
		}

		private bool _EnableChangeScroll = true;
		private int _DisableDelayChangeScrollCount = 0;

		private bool _IsLayoutSetup = false;
		public bool isLayoutSetup
		{
			get
			{
				return _IsLayoutSetup;
			}
		}

		void SetScrollOffset(Vector2 value, bool updateTransform, bool endFrameSelected)
		{
			Vector2 tmpValue = value;
			
			bool changed = false;

			value -= graphExtents.min;
			value.x = Mathf.Clamp(value.x, horizontalScroller.lowValue, horizontalScroller.highValue);
			value.y = Mathf.Clamp(value.y, verticalScroller.lowValue, verticalScroller.highValue);

			bool EnableChangeScroll = _EnableChangeScroll;
			_EnableChangeScroll = false;

			if (horizontalScroller.value != value.x)
			{
				Slider slider = horizontalScroller.slider;
				var newValue = Mathf.Clamp(value.x, slider.lowValue, slider.highValue);

				if (slider.value != newValue)
				{
					_DisableDelayChangeScrollCount++;

					horizontalScroller.value = value.x;
					changed = true;
				}
			}

			if (verticalScroller.value != value.y)
			{
				Slider slider = verticalScroller.slider;
				var newValue = Mathf.Clamp(value.y, slider.lowValue, slider.highValue);

				if (slider.value != newValue)
				{
					_DisableDelayChangeScrollCount++;

					verticalScroller.value = value.y;
					changed = true;
				}
			}

			if (tmpValue != scrollOffset)
			{
				changed = true;
			}

			_EnableChangeScroll = EnableChangeScroll;

			if (changed && updateTransform)
			{
				UpdateContentViewTransform(endFrameSelected);
			}
		}

		private GraphLayout m_ContentContainer;

		void UpdateContentViewTransform(Vector2 scrollOffset, bool endFrameSelected)
		{
			// Adjust contentContainer's position
			SetScroll(scrollOffset, false, endFrameSelected);

			MarkDirtyRepaint();
		}

		void UpdateContentViewTransform(bool endFrameSelected)
		{
			UpdateContentViewTransform(scrollOffset, endFrameSelected);
		}

		private GraphLayout _ScrollViewport;

		// Represents the visible part of contentContainer
		public VisualElement contentViewport
		{
			get; private set;
		}

		public VisualElement contentUnderlay
		{
			get; private set;
		}

		internal GraphLayout groupNodeLayer
		{
			get;
			private set;
		}

		internal GraphLayout dataBranchUnderlayLayer
		{
			get;
			private set;
		}

		internal GraphLayout branchUnderlayLayer
		{
			get;
			private set;
		}

		internal GraphLayout nodeLayer
		{
			get;
			private set;
		}

		internal GraphLayout branchOverlayLayer
		{
			get;
			private set;
		}

		internal GraphLayout dataBranchOverlayLayer
		{
			get;
			private set;
		}

		public VisualElement contentOverlay
		{
			get; private set;
		}

		public Scroller horizontalScroller
		{
			get; private set;
		}
		public Scroller verticalScroller
		{
			get; private set;
		}

		public override VisualElement contentContainer // Contains full content, potentially partially visible
		{
			get
			{
				return m_ContentContainer;
			}
		}

		private ZoomManipulator _ZoomManipulator;
		private PanManipulator _PanManipulator;

		void OnChangedScrollValue()
		{
			if (_DisableDelayChangeScrollCount == 0)
			{
				if (_EnableChangeScroll)
				{

					UpdateContentViewTransform(true);
				}
			}
			else
			{
				_DisableDelayChangeScrollCount--;
			}
		}

		private Vector2 _LastScrollerValue = Vector2.zero;

		public Vector2 mousePosition
		{
			get;
			private set;
		}

		private AutoScrollElement _AutoScrollElement;

		private bool _IsAutoScroll = false;
		public bool autoScroll
		{
			get
			{
				return _IsAutoScroll;
			}
			set
			{
				if (_IsAutoScroll != value)
				{
					_IsAutoScroll = value;

					if (_IsAutoScroll)
					{
						if (_AutoScrollElement == null)
						{
							_AutoScrollElement = new AutoScrollElement(this);
							_AutoScrollElement.StretchToParentSize();
						}

						if (_AutoScrollElement.parent == null)
						{
							contentViewport.Add(_AutoScrollElement);
						}
					}
					else
					{
						if (_AutoScrollElement != null && _AutoScrollElement.parent != null)
						{
							_AutoScrollElement.RemoveFromHierarchy();
						}
					}
				}
			}
		}

		internal GraphView(ArborEditorWindow window)
		{
			this.window = window;

			pickingMode = PickingMode.Ignore;
			style.flexGrow = 1;
			style.overflow = Overflow.Hidden;

			_ScrollViewport = new GraphLayout()
			{
				name = "ScrollViewport",
				style =
				{
					flexDirection = FlexDirection.Row,
					overflow = Overflow.Hidden,
				}
			};
			_ScrollViewport.StretchToParentSize();
			hierarchy.Add(_ScrollViewport);

			contentViewport = new GraphLayout() {
				name = "ContentViewport",
				pickingMode = PickingMode.Position,
				focusable = true,
				style =
				{
					flexDirection = FlexDirection.Row,
					overflow = Overflow.Hidden,
				}
			};
			contentViewport.AddToClassList("graphview-background");
			contentViewport.StretchToParentSize();
			_ScrollViewport.Add(contentViewport);

			contentUnderlay = new GraphLayout()
			{
				name = "ContentUnderlay",
				style =
				{
					overflow = Overflow.Hidden,
				}
			};
			contentUnderlay.StretchToParentSize();
			contentViewport.Add(contentUnderlay);

			// Basic content container; its constraints should be defined in the USS file
			m_ContentContainer = new GraphLayout() {
				name = "ContentView",
				usageHints = UsageHints.GroupTransform,
				style =
				{
					position = Position.Absolute,
				}
			};

			UIElementsUtility.SetTransformOrigin(m_ContentContainer, 0f, 0f, 0f);

			contentViewport.Add(m_ContentContainer);

			groupNodeLayer = new GraphLayout()
			{
				name = "GroupNodeLayer",
				style =
				{
					position = Position.Absolute,
				}
			};

			m_ContentContainer.Add(groupNodeLayer);

			dataBranchUnderlayLayer = new GraphLayout()
			{
				name = "DataBranchUnderlayLayer",
				style =
				{
					position = Position.Absolute,
				}
			};

			m_ContentContainer.Add(dataBranchUnderlayLayer);

			branchUnderlayLayer = new GraphLayout()
			{
				name = "BranchUnderlayLayer",
				style =
				{
					position = Position.Absolute,
				}
			};

			m_ContentContainer.Add(branchUnderlayLayer);

			nodeLayer = new GraphLayout()
			{
				name = "NodeLayer",
				style =
				{
					position = Position.Absolute
				}
			};

			m_ContentContainer.Add(nodeLayer);

			branchOverlayLayer = new GraphLayout()
			{
				name = "BranchOverlayLayer",
				style =
				{
					position = Position.Absolute,
				}
			};

			m_ContentContainer.Add(branchOverlayLayer);

			dataBranchOverlayLayer = new GraphLayout()
			{
				name = "DataBranchOverlayLayer",
				style =
				{
					position = Position.Absolute,
				}
			};

			m_ContentContainer.Add(dataBranchOverlayLayer);

			contentOverlay = new GraphLayout()
			{
				name = "ContentOverlay",
				style =
				{
					overflow = Overflow.Hidden,
				}
			};
			contentOverlay.StretchToParentSize();
			contentViewport.Add(contentOverlay);

			IMGUIContainer commandContainer = new IMGUIContainer(OnCommandGUI)
			{
				pickingMode = PickingMode.Ignore,
				style =
				{
					width = 0f,
					height = 0f,
				}
			};
			contentViewport.Add(commandContainer);

			horizontalScroller = new Scroller(0f, 100f,
				(value) =>
				{
					if (_LastScrollerValue.x != value)
					{
						OnChangedScrollValue();
					}
					_LastScrollerValue.x = value;
				},
				SliderDirection.Horizontal
				)
			{
				name = "HorizontalScroller",
				viewDataKey = "HorizontalScroller",
				style =
				{
					position = Position.Absolute,
					left = 0f,
					bottom = 0f,
					right = 17f,
				}
			};
			hierarchy.Add(horizontalScroller);

			verticalScroller = new Scroller(0f, 100f,
					(value) =>
					{
						if (_LastScrollerValue.y != value)
						{
							OnChangedScrollValue();
						}
						_LastScrollerValue.y = value;
					},
					SliderDirection.Vertical
					)
			{
				name = "VerticalScroller",
				viewDataKey = "VerticalScroller",
				style =
				{
					position = Position.Absolute,
					top = 0f,
					bottom = 17f,
					right = 0f,
				}
			};

			hierarchy.Add(verticalScroller);

			_ZoomManipulator = new ZoomManipulator(this);
			contentViewport.AddManipulator(_ZoomManipulator);

			_PanManipulator = new PanManipulator(this);
			contentViewport.AddManipulator(_PanManipulator);

			RectangleSelector rectangleSelector = new RectangleSelector(this);
			contentViewport.AddManipulator(rectangleSelector);

			ContextClickManipulator contextClick = new ContextClickManipulator(OnContextClick);
			contentViewport.AddManipulator(contextClick);

			contentViewport.RegisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.TrickleDown);
			contentViewport.RegisterCallback<DragUpdatedEvent>(OnMouseMove, TrickleDown.TrickleDown);

			contentViewport.RegisterCallback<DragEnterEvent>(OnDragEnter);
			contentViewport.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
			contentViewport.RegisterCallback<DragPerformEvent>(OnDragPerform);
			contentViewport.RegisterCallback<DragLeaveEvent>(OnDragLeave);
			contentViewport.RegisterCallback<DragExitedEvent>(OnDragExited, TrickleDown.TrickleDown);

			contentViewport.RegisterCallback<ValidateCommandEvent>(OnValidateCommand);
			contentViewport.RegisterCallback<ExecuteCommandEvent>(OnExecuteCommand);

			RegisterCallback<GeometryChangedEvent>(e =>
			{
				if (layout.width == 0 || layout.height == 0)
				{
					// Do not scroll and update until the size is confirmed.
					return;
				}

				OnPostLayout(true);
			});
		}

		internal void OnMouseMove(IMouseEvent e)
		{
			EventBase eventBase = e as EventBase;
			VisualElement target = eventBase.currentTarget as VisualElement;
			mousePosition = ElementToGraph(target, e.localMousePosition);
		}

		void OnDragEnter(DragEnterEvent e)
		{
			NodeGraphEditor graphEditor = this.graphEditor;
			if (graphEditor != null)
			{
				graphEditor.OnDragEnter();
			}
		}

		void OnDragUpdated(DragUpdatedEvent e)
		{
			NodeGraphEditor graphEditor = this.graphEditor;
			if (graphEditor != null)
			{
				graphEditor.OnDragUpdated();
			}
		}

		void OnDragPerform(DragPerformEvent e)
		{
			NodeGraphEditor graphEditor = this.graphEditor;
			if (graphEditor != null)
			{
				graphEditor.OnDragPerform(ElementToGraph(e.currentTarget as VisualElement, e.localMousePosition));
			}
		}

		void OnDragLeave(DragLeaveEvent e)
		{
			NodeGraphEditor graphEditor = this.graphEditor;
			if (graphEditor != null)
			{
				graphEditor.OnDragLeave();
			}

			window.DoRepaint();
		}

		void OnDragExited(DragExitedEvent e)
		{
			NodeGraphEditor graphEditor = this.graphEditor;
			if (graphEditor != null)
			{
				graphEditor.OnDragExited();
			}
		}

		void OnContextClick(ContextClickEvent e)
		{
			NodeGraphEditor graphEditor = this.graphEditor;
			if (graphEditor != null)
			{
				VisualElement currentTarget = e.currentTarget as VisualElement;
				Vector2 graphPosition = ElementToGraph(currentTarget, e.localMousePosition);
				Vector2 screenPosition = currentTarget.LocalToScreen(e.localMousePosition);
				if (graphEditor.OnContextMenu(new MousePosition(graphPosition, screenPosition)))
				{
					e.StopPropagation();
				}
			}
		}

		void OnCommandGUI()
		{
			NodeGraphEditor graphEditor = this.graphEditor;
			if (graphEditor == null)
			{
				return;
			}

			Event current = Event.current;
			if (current.type == EventType.Repaint)
			{
				graphEditor.OnRepainted();
			}

			switch (current.type)
			{
				case EventType.ValidateCommand:
					{
						if (graphEditor.OnValidateCommand(current.commandName))
						{
							current.Use();
						}
					}
					break;
				case EventType.ExecuteCommand:
					{
						if (graphEditor.OnExecuteCommand(current.commandName, mousePosition))
						{
							current.Use();
						}
					}
					break;
			}
		}

		void OnValidateCommand(ValidateCommandEvent e)
		{
			NodeGraphEditor graphEditor = this.graphEditor;
			if (graphEditor != null)
			{
				if (graphEditor.OnValidateCommand(e.commandName))
				{
					e.StopPropagation();
					if (e.imguiEvent != null)
					{
						e.imguiEvent.Use();
					}
				}
			}
		}

		void OnExecuteCommand(ExecuteCommandEvent e)
		{
			NodeGraphEditor graphEditor = this.graphEditor;
			if (graphEditor != null)
			{
				if (graphEditor.OnExecuteCommand(e.commandName, mousePosition))
				{
					e.StopPropagation();
					if (e.imguiEvent != null)
					{
						e.imguiEvent.Use();
					}
				}
			}
		}

		public Vector2 GraphToElement(VisualElement dest,Vector2 point)
		{
			if (m_ContentContainer != dest)
			{
				point = m_ContentContainer.ChangeCoordinatesTo(dest, point);
			}
			return point;
		}

		public Rect GraphToElement(VisualElement dest, Rect rect)
		{
			if (m_ContentContainer != dest)
			{
				rect = m_ContentContainer.ChangeCoordinatesTo(dest, rect);
			}
			return rect;
		}

		public Vector2 ElementToGraph(VisualElement dest, Vector2 point)
		{
			if (m_ContentContainer != dest)
			{
				point = dest.ChangeCoordinatesTo(m_ContentContainer, point);
			}
			return point;
		}

		public Rect ElementToGraph(VisualElement dest, Rect rect)
		{
			if (m_ContentContainer != dest)
			{
				rect = dest.ChangeCoordinatesTo(m_ContentContainer, rect);
			}
			return rect;
		}

		public Vector2 GUIToGraph(Vector2 position)
		{
			return m_ContentContainer.GUIToLocal(position);
		}

		public Rect GUIToGraph(Rect rect)
		{
			return m_ContentContainer.GUIToLocal(rect);
		}

		public Vector2 GraphToGUI(Vector2 position)
		{
			return m_ContentContainer.LocalToGUI(position);
		}

		public Rect GraphToGUI(Rect rect)
		{
			return m_ContentContainer.LocalToGUI(rect);
		}

		public Vector2 ScreenToGraph(Vector2 position)
		{
			return m_ContentContainer.ScreenToLocal(position);
		}

		public Rect ScreenToGraph(Rect rect)
		{
			return m_ContentContainer.ScreenToLocal(rect);
		}

		public Vector2 GraphToScreen(Vector2 position)
		{
			return m_ContentContainer.LocalToScreen(position);
		}

		public Rect GraphToScreen(Rect rect)
		{
			return m_ContentContainer.LocalToScreen(rect);
		}

		Vector2 CalcTransformToGraph(Vector2 value)
		{
			Vector3 scale = graphScale;

			return -Vector3.Scale(value, new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z));
		}

		Vector2 CalcGraphToTransform(Vector2 value)
		{
			Vector3 scale = graphScale;
			return -Vector3.Scale(value, scale);
		}

		internal void OnZoom(Vector2 zoomCenter, float zoomScale)
		{
			Vector3 scale = graphScale;
			SetZoom(zoomCenter, Vector3.Scale(scale, new Vector3(zoomScale, zoomScale, 1f)), true);
		}

		internal void OnScroll(Vector2 delta)
		{
			SetScroll(scrollPos + delta, true, true);
		}

		protected override void ImmediateRepaint()
		{
			if (_PanManipulator.isActive)
			{
				EditorGUIUtility.AddCursorRect(contentViewport.layout, MouseCursor.Pan);
			}
			else if (_ZoomManipulator.isActive)
			{
				EditorGUIUtility.AddCursorRect(contentViewport.layout, MouseCursor.Zoom);
			}
		}

		private Vector2 _LastViewportSize = Vector2.zero;
		private bool _IsInitialize = false;

		public bool isSettedGraphExtents
		{
			get;
			private set;
		}

		internal void UpdateGraphExtents()
		{
			NodeGraphEditor graphEditor = this.graphEditor;
			if (graphEditor == null)
			{
				return;
			}

			Rect extents = graphEditor.UpdateGraphExtents();
			_GraphExtents = extents;

			Rect graphPosition = graphViewport;

			extents.xMin -= graphPosition.width * 0.6f;
			extents.xMax += graphPosition.width * 0.6f;
			extents.yMin -= graphPosition.height * 0.6f;
			extents.yMax += graphPosition.height * 0.6f;

			extents.xMin = (int)extents.xMin;
			extents.xMax = (int)extents.xMax;
			extents.yMin = (int)extents.yMin;
			extents.yMax = (int)extents.yMax;

			if (graphEditor.isDragNodes)
			{
				if (graphPosition.xMin < extents.xMin)
				{
					extents.xMin = graphPosition.xMin;
				}
				if (extents.xMax < graphPosition.xMax)
				{
					extents.xMax = graphPosition.xMax;
				}

				if (graphPosition.yMin < extents.yMin)
				{
					extents.yMin = graphPosition.yMin;
				}
				if (extents.yMax < graphPosition.yMax)
				{
					extents.yMax = graphPosition.yMax;
				}
			}

			if (graphExtents != extents)
			{
				this.graphExtents = extents;
			}
		}

		void UpdateView(bool updateScroll = true)
		{
			bool enableChangeScroll = _EnableChangeScroll;
			_EnableChangeScroll = false;

			Vector2 oldScrollOffset = scrollOffset;
			
			Rect lastGraphExtents = this.graphExtents;

			UpdateGraphExtents();

			Rect graphExtents = this.graphExtents;
			Rect viewportLayout = graphViewport;

			if (graphExtents.width > Mathf.Epsilon)
			{
				horizontalScroller.Adjust(viewportLayout.width / graphExtents.width);
			}
			if (graphExtents.height > Mathf.Epsilon)
			{
				verticalScroller.Adjust(viewportLayout.height / graphExtents.height);
			}

			// Set availability
			horizontalScroller.SetEnabled(graphExtents.width - viewportLayout.width > 0);
			verticalScroller.SetEnabled(graphExtents.height - viewportLayout.height > 0);

			// Expand content if scrollbars are hidden
			_ScrollViewport.style.right = needsVertical ? verticalScroller.layout.width : 0;
			horizontalScroller.style.right = needsVertical ? verticalScroller.layout.width : 0;
			_ScrollViewport.style.bottom = needsHorizontal ? horizontalScroller.layout.height : 0;
			verticalScroller.style.bottom = needsHorizontal ? horizontalScroller.layout.height : 0;

			Vector2 scrollValue = scrollOffset;

			if (needsHorizontal)
			{
				horizontalScroller.lowValue = 0;
				horizontalScroller.highValue = Mathf.Max(graphExtents.width - viewportLayout.width, 1);
			}
			else
			{
				horizontalScroller.value = 0.0f;
			}

			if (needsVertical)
			{
				verticalScroller.lowValue = 0;
				verticalScroller.highValue = Mathf.Max(graphExtents.height - viewportLayout.height, 1);
			}
			else
			{
				verticalScroller.value = 0.0f;
			}

			// Set visibility and remove/add content viewport margin as necessary
			if (horizontalScroller.visible != needsHorizontal)
			{
				horizontalScroller.visible = needsHorizontal;
				if (needsHorizontal)
				{
					_ScrollViewport.style.bottom = 17;
				}
				else
				{
					_ScrollViewport.style.bottom = 0;
				}
			}

			if (verticalScroller.visible != needsVertical)
			{
				verticalScroller.visible = needsVertical;
				if (needsVertical)
				{
					_ScrollViewport.style.right = 17;
				}
				else
				{
					_ScrollViewport.style.right = 0;
				}
			}

			_EnableChangeScroll = enableChangeScroll;

			bool changeExtents = graphExtents != lastGraphExtents;
			bool changeViewport = _LastViewportSize != viewportLayout.size;
			if (updateScroll && (!_IsInitialize || changeExtents || changeViewport))
			{
				SetScrollOffset(oldScrollOffset, true, false);
			}

			_IsInitialize = true;
			_LastViewportSize = viewportLayout.size;

			if (!_IsLayoutSetup)
			{
				_IsLayoutSetup = true;
				window.OnPostLayout();
			}
		}

		internal void DirtyGraphExtents()
		{
			_IsLayoutSetup = false;
			UpdateLayout();
		}

		private void OnPostLayout(bool hasNewLayout)
		{
			if (!hasNewLayout)
				return;

			UpdateView();
		}

		void UpdateLayout(bool updateScroll = true)
		{
			if (!_IsInitialize)
			{
				return;
			}

			UpdateView(updateScroll);
		}

		internal void SetScroll(Vector2 position, bool updateView, bool endFrameSelected)
		{
			if (!_IsLayoutSetup)
			{
				return;
			}

			if (endFrameSelected)
			{
				EndFrameSelected();
			}

			Rect extents = graphExtents;
			Rect viewport = graphViewport;
			position.x = Mathf.Clamp(position.x, extents.xMin, extents.xMax - viewport.width);
			position.y = Mathf.Clamp(position.y, extents.yMin, extents.yMax - viewport.height);

			Vector2 oldGraphPosaition = scrollPos;

			Vector2 offset = position - oldGraphPosaition;
			mousePosition += offset;

			scrollPos = position;

			if (updateView)
			{
				SetScrollOffset(position, false, false);
			}

			window.OnChangedScroll();

			Vector2 newGraphPosition = position;
			if (oldGraphPosaition != newGraphPosition)
			{
				using (var e = ChangeGraphScrollEvent.GetPooled(oldGraphPosaition, newGraphPosition))
				{
					if (onChangedGraphPosition != null)
					{
						if (!_IsCallingChangedGraphPosition)
						{
							_IsCallingChangedGraphPosition = true;
							try
							{
								onChangedGraphPosition();
							}
							finally
							{
								_IsCallingChangedGraphPosition = false;
							}
						}
					}

					if (!_IsCallingChangedGraphPosition)
					{
						_IsCallingChangedGraphPosition = true;
						try
						{
							e.target = this;
							SendEvent(e);
						}
						finally
						{
							_IsCallingChangedGraphPosition = false;
						}
					}
				}
			}
		}

		internal void SetZoom(Vector2 zoomCenter, Vector3 zoomScale, bool endFrameSelected, bool updateScroll = true)
		{
			Vector3 position = graphPosition;
			Vector3 scale = graphScale;

			zoomCenter = GraphToElement(m_ContentContainer, zoomCenter);

			position += Vector3.Scale(zoomCenter, scale);
			float zoomMin = ArborEditorWindow.k_ZoomMin / 100f;
			float zoomMax = ArborEditorWindow.k_ZoomMax / 100f;
			zoomScale.x = Mathf.Clamp(zoomScale.x, zoomMin, zoomMax);
			zoomScale.y = Mathf.Clamp(zoomScale.y, zoomMin, zoomMax);

			graphPosition = position - Vector3.Scale(zoomCenter, zoomScale);
			graphScale = zoomScale;

			Vector2 scrollPos = this.scrollPos;

			UpdateLayout(updateScroll);

			if (updateScroll)
			{
				SetScroll(scrollPos, true, endFrameSelected);
			}
		}

		private GridBackground _GridBackground;

		internal void SetShowGridBackground(bool showGrid)
		{
			if (showGrid)
			{
				if (_GridBackground == null)
				{
					_GridBackground = new GridBackground(this)
					{
						name = "GridBackground"
					};
				}

				if (_GridBackground.parent == null)
				{
					m_ContentContainer.Insert(0, _GridBackground);
				}
			}
			else
			{
				if (_GridBackground != null)
				{
					_GridBackground.RemoveFromHierarchy();
				}
			}
		}

		private FrameSelected _FrameSelected = new FrameSelected();
		private FrameSelected _FrameSelectedZoom = new FrameSelected()
		{
			stoppingDistance = 0.001f,
		};
		
		internal void FrameSelected(Vector2 frameSelectTarget)
		{
			_FrameSelected.Begin(frameSelectTarget);
			_FrameSelectedZoom.Begin(Vector2.one);
		}

		void EndFrameSelected()
		{
			_FrameSelected.End();
			_FrameSelectedZoom.End();
		}

		internal bool UpdateFrameSelected()
		{
			bool repaint = false;

			if (_FrameSelectedZoom.isPlaying)
			{
				Vector2 zoomScale = _FrameSelectedZoom.Update(graphScale, Vector2.zero);

				SetZoom(graphViewport.center, new Vector3(zoomScale.x, zoomScale.y, 1), false, !_FrameSelected.isPlaying);

				repaint = true;
			}

			if (_FrameSelected.isPlaying)
			{
				Vector2 scrollPos = _FrameSelected.Update(this.scrollPos, -graphViewport.size * 0.5f);

				SetScroll(scrollPos, true, false);

				repaint = true;
			}

			return repaint;
		}
	}
}