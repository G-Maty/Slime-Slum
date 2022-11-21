//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace ArborEditor.UIElements
{
	using Arbor;
	using ArborEditor.UnityEditorBridge.UIElements.Extensions;

	internal sealed class StateLinkElement : StateLinkElementBase
	{
		public readonly SerializedPropertyKey propertyKey;

		private readonly string _DefaultLabel;
		
		private Image _Icon;
		private Label _Label;

		private VisualElement _SettingsButtonElement;
		private ConnectManipulator _ConnectManipulator = null;

		private VisualElement _Pin;
		private bool _IsPinRight = false;

		private PolyLineElement _ConnectLines;
		
		protected override void OnConnectionChanged(bool on)
		{
			EnableInClassList("node-link-slot-on", on);
			_ConnectLines.visible = on;
			if (!on)
			{
				SetPinRight(true);
			}

			UpdateLineColor();
		}

		protected override void OnSetup(bool changedStateLink, bool changedFieldInfo)
		{
			if (changedStateLink || changedFieldInfo)
			{
				UpdateIcon();
			}

			if (changedStateLink)
			{
				UpdateLabel();
				UpdateLineColor();
			}
		}

		void SetPinRight(bool value)
		{
			if (_IsPinRight != value)
			{
				_IsPinRight = value;

				_Pin.EnableInClassList("right", _IsPinRight);
				_Pin.EnableInClassList("left", !_IsPinRight);
			}
		}

		public StateLinkElement(NodeEditor nodeEditor, StateBehaviour behaviour, SerializedPropertyKey propertyKey, string defaultLabel) : base(nodeEditor, behaviour)
		{
			this.propertyKey = propertyKey;
			_DefaultLabel = defaultLabel;

			AddToClassList("node-link-slot");
			AddToClassList("state-link-slot");

			VisualElement content = new VisualElement();
			content.AddToClassList("content");
			Add(content);

			_Icon = new Image();
			_Icon.AddToClassList("content-icon");
			content.Add(_Icon);

			_Label = new Label();
			_Label.AddToClassList("content-label");
			content.Add(_Label);

			_SettingsButtonElement = new MouseDownButton(() =>
			{
				OpenSettingsWindow();
			});
			_SettingsButtonElement.RemoveFromClassList("unity-button");
			_SettingsButtonElement.AddToClassList("settings-button");
			VisualElement popupButtonImage = new Image()
			{
				image = Icons.stateLinkPopupIcon,
			};
			popupButtonImage.AddManipulator(new LocalizationManipulator("Settings", LocalizationManipulator.TargetText.Tooltip));
			_SettingsButtonElement.Add(popupButtonImage);
			Add(_SettingsButtonElement);

			_ConnectLines = new PolyLineElement()
			{
				edgeWidth = 8f,
			};
			_ConnectLines.visible = false;
			Add(_ConnectLines);

			_Pin = new VisualElement();
			_Pin.AddToClassList("pin");
			SetPinRight(true);
			_Pin.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedPin);
			Add(_Pin);

			RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

			_ConnectManipulator = new ConnectManipulator();
			_ConnectManipulator.onChangedActive += isActive =>
			{
				EnableInClassList("node-link-slot-active", isActive);
				UpdateLineColor();
			};
			this.AddManipulator(_ConnectManipulator);

			this.AddManipulator(new ContextClickManipulator(OnContextClick));
		}

		void OnCustomStyleResolved(CustomStyleResolvedEvent e)
		{
			UpdateLineColor();
		}

		void OnGeometryChangedPin(GeometryChangedEvent e)
		{
			NodeEditor nodeEditor = this.nodeEditor;

			Vector2 pinPos = e.newRect.center;
			Vector2 endPos = _IsPinRight ? new Vector2(nodeEditor.node.position.width, 0f) : new Vector2(0f, 0f);
			endPos = nodeEditor.nodeElement.ChangeCoordinatesTo(this, endPos);
			endPos.y = pinPos.y;

			_ConnectLines.SetPoints(pinPos, endPos);
		}

		List<VisualElement> _RegisterElements = new List<VisualElement>();

		protected override void OnAttachToPanel(AttachToPanelEvent e)
		{
			base.OnAttachToPanel(e);

			var nodeEditor = this.nodeEditor;
			var nodeElement = nodeEditor.nodeElement;

			foreach (var element in _RegisterElements)
			{
				element.UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedParent);
			}
			_RegisterElements.Clear();

			var current = this.hierarchy.parent;
			while (current != null && current != nodeElement)
			{
				current.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedParent);

				_RegisterElements.Add(current);

				current = current.hierarchy.parent;
			}
		}

		protected override void OnDetachFromPanel(DetachFromPanelEvent e)
		{
			base.OnDetachFromPanel(e);

			foreach (var element in _RegisterElements)
			{
				element.UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedParent);
			}
			_RegisterElements.Clear();
		}

		void OnGeometryChangedParent(GeometryChangedEvent e)
		{
			DoChangedPosition();
		}

		protected override void OnUndoRedoPerformed()
		{
			UpdateLabel();
			UpdateIcon();
			UpdateLineColor();
		}

		protected override Bezier2D OnUpdateBezier(Rect rect, Node targetNode)
		{
			NodeEditor nodeEditor = this.nodeEditor;

			Node node = nodeEditor.node;
			
			Vector2 leftPos = new Vector2(0f, rect.center.y);
			Vector2 rightPos = new Vector2(node.position.width, rect.center.y);

			bool isPinRight = true;
			Bezier2D bezier = GetTargetBezier(node, targetNode, leftPos, rightPos, ref isPinRight);

			SetPinRight(isPinRight);

			return bezier;
		}

		void OpenSettingsWindow()
		{
			OpenSettingsWindow(_SettingsButtonElement.worldBound, false);
		}

		void OnContextClick(ContextClickEvent e)
		{
			OpenSettingsWindow();
			e.StopPropagation();
		}

		protected override void OnSettingsChanged()
		{
			UpdateLabel();
			UpdateIcon();
			UpdateLineColor();
		}

		static readonly CustomStyleProperty<Color> s_ColorsBackgroundProperty = new CustomStyleProperty<Color>("--local-colors-node_link_slot-background");

		void UpdateLabel()
		{
			string label = stateLink.name;
			if (string.IsNullOrEmpty(label))
			{
				label = _DefaultLabel;
			}
			_Label.text = label;
		}

		void UpdateIcon()
		{
			TransitionTiming transitionTiming = GetTransitionTiming(stateLink, fieldInfo);
			_Icon.image = GetTransitionTimingIcon(transitionTiming);
		}

		void UpdateLineColor()
		{
			Color lineColor = stateLink.lineColor;

			_ConnectLines.lineColor = lineColor;

			Color slotColor = lineColor;
			slotColor.a = 1f;

			_Pin.style.unityBackgroundImageTintColor = slotColor;

			if (_ConnectManipulator.isActive)
			{
				style.unityBackgroundImageTintColor = StyleKeyword.Null;
			}
			else
			{
				if (!on)
				{
					if (customStyle.TryGetValue(s_ColorsBackgroundProperty, out var value))
					{
						slotColor = slotColor * value;
					}
				}
				style.unityBackgroundImageTintColor = slotColor;
			}
		}

		static TransitionTiming GetTransitionTiming(StateLink stateLink, System.Reflection.FieldInfo stateLinkFieldInfo)
		{
			FixedTransitionTiming fixedTransitionTiming = AttributeHelper.GetAttribute<FixedTransitionTiming>(stateLinkFieldInfo);
			FixedImmediateTransition fixedImmediateTransition = AttributeHelper.GetAttribute<FixedImmediateTransition>(stateLinkFieldInfo);

			TransitionTiming transitionTiming = TransitionTiming.LateUpdateDontOverwrite;

			if (fixedTransitionTiming != null)
			{
				transitionTiming = fixedTransitionTiming.transitionTiming;
			}
			else if (fixedImmediateTransition != null)
			{
				transitionTiming = fixedImmediateTransition.immediate ? TransitionTiming.Immediate : TransitionTiming.LateUpdateOverwrite;
			}
			else
			{
				transitionTiming = stateLink.transitionTiming;
			}

			return transitionTiming;
		}

		static Texture GetTransitionTimingIcon(TransitionTiming transitionTiming)
		{
			switch (transitionTiming)
			{
				case TransitionTiming.LateUpdateOverwrite:
					return Icons.transitionTimingLateUpdateOverwrite;
				case TransitionTiming.Immediate:
					return Icons.transitionTimingImmediate;
				case TransitionTiming.LateUpdateDontOverwrite:
					return Icons.transitionTimingLateUpdateDontOverwrite;
				case TransitionTiming.NextUpdateOverwrite:
					return Icons.transitionTimingNextUpdateOverwrite;
				case TransitionTiming.NextUpdateDontOverwrite:
					return Icons.transitionTimingNextUpdateDontOverwrite;
			}

			return null;
		}

		sealed class ConnectManipulator : DragManipulator
		{
			private Node _DragTargetNode = null;

			private PolyLineElement _ConnectLines;
			private VisualElement _ActivePin;
			private bool _OldOn;

			private bool _IsPinRight;

			public ConnectManipulator()
			{
				activators.Add(new ManipulatorActivationFilter() { button = MouseButton.LeftMouse });

				_ConnectLines = new PolyLineElement()
				{
					edgeWidth = 8f,
				};

				_IsPinRight = true;

				_ActivePin = new VisualElement();
				_ActivePin.AddToClassList("pin-active");

				_ActivePin.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedActivePin);
			}

			private void OnGeometryChangedActivePin(GeometryChangedEvent e)
			{
				StateLinkElement linkElement = target as StateLinkElement;
				if (linkElement == null)
				{
					return;
				}

				if (isActive)
				{
					NodeEditor nodeEditor = linkElement.nodeEditor;

					Vector2 pinPos = e.newRect.center;
					Vector2 endPos = _IsPinRight? new Vector2(nodeEditor.node.position.width, 0f) : new Vector2(0f, 0f);
					endPos = nodeEditor.nodeElement.ChangeCoordinatesTo(linkElement, endPos);
					endPos.y = pinPos.y;

					_ConnectLines.SetPoints(pinPos, endPos);
				}
			}

			protected override void RegisterCallbacksOnTarget()
			{
				base.RegisterCallbacksOnTarget();

				target.RegisterCallback<KeyDownEvent>(OnKeyDown);
			}

			protected override void UnregisterCallbacksFromTarget()
			{
				base.UnregisterCallbacksFromTarget();

				target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
			}

			protected override void RegisterCallbacksOnGraphView(GraphView graphView)
			{
				base.RegisterCallbacksOnGraphView(graphView);

				graphView.RegisterCallback<ChangeGraphScrollEvent>(OnChangeGraphView);
				graphView.RegisterCallback<ChangeGraphExtentsEvent>(OnChangeGraphView);
			}

			protected override void UnregisterCallbacksFromGraphView(GraphView graphView)
			{
				base.UnregisterCallbacksFromGraphView(graphView);

				graphView.UnregisterCallback<ChangeGraphScrollEvent>(OnChangeGraphView);
				graphView.UnregisterCallback<ChangeGraphExtentsEvent>(OnChangeGraphView);
			}

			void OnChangeGraphView(IChangeGraphViewEvent e)
			{
				EventBase evtBase = e as EventBase;
				GraphView graphView = evtBase.target as GraphView;

				Vector2 mousePosition = graphView.GraphToElement(target, graphView.mousePosition);
				UpdateMousePosition(mousePosition);
			}

			static readonly CustomStyleProperty<Color> s_ColorsPinBackgroundPressedProperty = new CustomStyleProperty<Color>("--colors-pin-background-pressed");

			protected override void OnMouseDown(MouseDownEvent e)
			{
				StateLinkElement linkElement = target as StateLinkElement;
				if (linkElement == null)
				{
					return;
				}

				_DragTargetNode = null;
				var graphEditor = linkElement._GraphEditor;
				if (graphEditor != null)
				{
					StateBehaviour behaviour = linkElement._StateBehaviour;

					ArborFSMInternal stateMachine = behaviour.stateMachine;
					State state = stateMachine.GetStateFromID(behaviour.nodeID);
					NodeEditor nodeEditor = linkElement.nodeEditor;

					Vector2 mousePosition = (e.target as VisualElement).ChangeCoordinatesTo(nodeEditor.nodeElement, e.localMousePosition);

					Rect rect = linkElement.parent.ChangeCoordinatesTo(nodeEditor.nodeElement, linkElement.layout);

					Vector2 leftPos = new Vector2(0f, rect.center.y);
					Vector2 rightPos = new Vector2(nodeEditor.node.position.width, rect.center.y);

					Bezier2D bezier = GetTargetBezier(mousePosition, leftPos, rightPos, ref _IsPinRight);

					Vector2 bezierStartPosition = nodeEditor.nodeElement.ChangeCoordinatesTo(linkElement, bezier.startPosition);

					Vector2 statePosition = new Vector2(state.position.x, state.position.y);

					if (nodeEditor != null)
					{
						bezier.startPosition = nodeEditor.NodeToGraphPoint(bezier.startPosition);
						bezier.startControl = nodeEditor.NodeToGraphPoint(bezier.startControl);
					}
					else
					{
						bezier.startPosition += statePosition;
						bezier.startControl += statePosition;
					}
					bezier.endPosition += statePosition;
					bezier.endControl += statePosition;

					graphEditor.BeginDragStateBranch(state.nodeID);
					graphEditor.DragStateBranchBezie(bezier);

					linkElement.Add(_ConnectLines);
					if (linkElement.customStyle.TryGetValue(s_ColorsPinBackgroundPressedProperty, out var value))
					{
						_ConnectLines.lineColor = value;
					}

					linkElement.Add(_ActivePin);
					_ActivePin.EnableInClassList("right", _IsPinRight);
					_ActivePin.EnableInClassList("left", !_IsPinRight);

					_OldOn = linkElement.on;
					if (!_OldOn)
					{
						linkElement._Pin.visible = false;
					}
				}

				e.StopPropagation();
			}

			void UpdateMousePosition(Vector2 localMousePosition)
			{
				StateLinkElement linkElement = target as StateLinkElement;
				if (linkElement == null)
				{
					return;
				}

				var nodeEditor = linkElement.nodeEditor;
				var graphEditor = linkElement._GraphEditor;

				StateBehaviour behaviour = linkElement._StateBehaviour;

				ArborFSMInternal stateMachine = behaviour.stateMachine;
				State state = stateMachine.GetStateFromID(behaviour.nodeID);

				Vector2 mousePosition = linkElement.ChangeCoordinatesTo(nodeEditor.nodeElement, localMousePosition);
				Rect rect = linkElement.parent.ChangeCoordinatesTo(nodeEditor.nodeElement, linkElement.layout);

				Node hoverNode = !rect.Contains(mousePosition) ? graphEditor.GetTargetNodeFromPosition(graphEditor.graphView.ElementToGraph(nodeEditor.nodeElement, mousePosition), state) : null;
				if (hoverNode != null)
				{
					if (graphEditor != null)
					{
						graphEditor.DragStateBranchHoverStateID(hoverNode.nodeID);
					}

					_DragTargetNode = hoverNode;
				}
				else
				{
					if (graphEditor != null)
					{
						graphEditor.DragStateBranchHoverStateID(0);
					}

					_DragTargetNode = null;
				}

				Vector2 leftPos = new Vector2(0f, rect.center.y);
				Vector2 rightPos = new Vector2(nodeEditor.node.position.width, rect.center.y);

				Bezier2D bezier = null;
				if (_DragTargetNode != null)
				{
					bezier = StateLinkElementBase.GetTargetBezier(state, _DragTargetNode, leftPos, rightPos, ref _IsPinRight);
				}
				else
				{
					bezier = GetTargetBezier(mousePosition, leftPos, rightPos, ref _IsPinRight);
				}

				Vector2 bezierStartPosition = nodeEditor.nodeElement.ChangeCoordinatesTo(linkElement, bezier.startPosition);

				Vector2 statePosition = new Vector2(state.position.x, state.position.y);

				if (nodeEditor != null)
				{
					bezier.startPosition = nodeEditor.NodeToGraphPoint(bezier.startPosition);
					bezier.startControl = nodeEditor.NodeToGraphPoint(bezier.startControl);
				}
				else
				{
					bezier.startPosition += statePosition;
					bezier.startControl += statePosition;
				}
				bezier.endPosition += statePosition;
				bezier.endControl += statePosition;

				if (graphEditor != null)
				{
					graphEditor.DragStateBranchBezie(bezier);
				}

				_ActivePin.EnableInClassList("right", _IsPinRight);
				_ActivePin.EnableInClassList("left", !_IsPinRight);
			}

			protected override void OnMouseMove(MouseMoveEvent e)
			{
				StateLinkElement linkElement = target as StateLinkElement;
				if (linkElement == null)
				{
					return;
				}

				DragAndDrop.PrepareStartDrag();

				UpdateMousePosition(e.localMousePosition);

				e.StopPropagation();
			}

			protected override void OnMouseUp(MouseUpEvent e)
			{
				StateLinkElement linkElement = target as StateLinkElement;
				if (linkElement == null)
				{
					return;
				}

				var stateLink = linkElement.stateLink;
				var nodeEditor = linkElement.nodeEditor;
				var graphEditor = linkElement._GraphEditor;

				var behaviour = linkElement._StateBehaviour;
				ArborFSMInternal stateMachine = behaviour.stateMachine;
				State state = stateMachine.GetStateFromID(behaviour.nodeID);

				Node targetNode = stateMachine.GetNodeFromID(stateLink.stateID);

				Rect rect = linkElement.parent.ChangeCoordinatesTo(nodeEditor.nodeElement, linkElement.layout);

				Vector2 leftPos = new Vector2(0f, rect.center.y);
				Vector2 rightPos = new Vector2(nodeEditor.node.position.width, rect.center.y);

				Vector2 localMousePosition = e.localMousePosition;
				VisualElement eventTarget = e.target as VisualElement;

				Vector2 mousePosition = eventTarget.ChangeCoordinatesTo(nodeEditor.nodeElement, localMousePosition);

				Bezier2D draggingBezier = new Bezier2D();
				bool isDraggingRight = true;
				if (_DragTargetNode != null)
				{
					draggingBezier = StateLinkElementBase.GetTargetBezier(state, _DragTargetNode, leftPos, rightPos, ref isDraggingRight);
				}
				else
				{
					draggingBezier = GetTargetBezier(mousePosition, leftPos, rightPos, ref isDraggingRight);
				}

				Vector2 statePosition = new Vector2(state.position.x, state.position.y);

				if (nodeEditor != null)
				{
					draggingBezier.startPosition = nodeEditor.NodeToGraphPoint(draggingBezier.startPosition);
					draggingBezier.startControl = nodeEditor.NodeToGraphPoint(draggingBezier.startControl);
				}
				else
				{
					draggingBezier.startPosition += statePosition;
					draggingBezier.startControl += statePosition;
				}
				draggingBezier.endPosition += statePosition;
				draggingBezier.endControl += statePosition;

				if (_DragTargetNode == null)
				{
					GenericMenu menu = new GenericMenu();

					Vector2 graphMousePosition = graphEditor.graphView.ElementToGraph(eventTarget, localMousePosition);
					Vector2 screenMousePosition = eventTarget.LocalToScreen(localMousePosition);

					menu.AddItem(EditorContents.createState, false, () =>
					{
						graphMousePosition -= new Vector2(8f, 12f);

						State newState = graphEditor.CreateState(graphMousePosition, false);

						Undo.RecordObject(behaviour, "Link State");

						stateLink.stateID = newState.nodeID;
						linkElement.bezier = draggingBezier;
						linkElement.UpdateBezier();

						linkElement.DoChangedPosition();

						EditorUtility.SetDirty(behaviour);
					});

					menu.AddItem(EditorContents.reroute, false, () =>
					{
						Undo.IncrementCurrentGroup();
						int undoGroup = Undo.GetCurrentGroup();

						graphMousePosition -= new Vector2(16f, 16f);

						Color lineColor = stateLink.lineColor;

						StateLinkRerouteNode newStateLinkNode = graphEditor.CreateStateLinkRerouteNode(graphMousePosition, lineColor);

						Undo.RecordObject(behaviour, "Link State");

						stateLink.stateID = newStateLinkNode.nodeID;
						linkElement.bezier = draggingBezier;

						linkElement.UpdateBezier();

						linkElement.DoChangedPosition();

						Undo.CollapseUndoOperations(undoGroup);

						EditorUtility.SetDirty(behaviour);
					});

					menu.AddSeparator("");

					menu.AddItem(EditorContents.nodeListSelection, false, () =>
					{
						StateLinkElement currentLinkElement = linkElement;
						StateBehaviour currentBehaviour = behaviour;

						StateLinkSelectorWindow.instance.Open(graphEditor, new Rect(screenMousePosition, Vector2.zero), currentLinkElement.stateLink.stateID,
							(targetNodeEditor) =>
							{
								Undo.RecordObject(currentBehaviour, "Link State");

								currentLinkElement.stateLink.stateID = targetNodeEditor.nodeID;
								currentLinkElement.bezier = draggingBezier;

								currentLinkElement.UpdateBezier();

								linkElement.DoChangedPosition();

								EditorUtility.SetDirty(currentBehaviour);

								//graphEditor.BeginFrameSelected(targetNodeEditor.node);
							}
						);
					});

					if (stateLink.stateID != 0)
					{
						menu.AddSeparator("");
						menu.AddItem(EditorContents.disconnect, false, () =>
						{
							Undo.RecordObject(behaviour, "Disconect StateLink");

							stateLink.stateID = 0;

							linkElement.DoChangedPosition();

							EditorUtility.SetDirty(behaviour);
						});
					}
					menu.ShowAsContext();
				}
				else if (_DragTargetNode != targetNode)
				{
					Undo.RecordObject(behaviour, "Link State");

					stateLink.stateID = _DragTargetNode.nodeID;
					linkElement.bezier = draggingBezier;

					linkElement.UpdateBezier();

					linkElement.DoChangedPosition();

					EditorUtility.SetDirty(behaviour);
				}
			}

			protected override void OnEndDrag()
			{
				base.OnEndDrag();

				StateLinkElement linkElement = target as StateLinkElement;
				if (linkElement == null)
				{
					return;
				}

				var graphEditor = linkElement._GraphEditor;

				if (graphEditor != null)
				{
					graphEditor.EndDragStateBranch();
				}

				_ConnectLines.RemoveFromHierarchy();
				_ActivePin.RemoveFromHierarchy();

				if (!_OldOn)
				{
					linkElement._Pin.visible = true;
				}

				_DragTargetNode = null;
			}

			void OnKeyDown(KeyDownEvent e)
			{
				StateLinkElement linkElement = target as StateLinkElement;
				if (linkElement == null)
				{
					return;
				}

				if (!isActive || e.keyCode != KeyCode.Escape)
				{
					return;
				}

				EndDrag();
				e.StopPropagation();
			}

			public static Bezier2D GetTargetBezier(Vector2 targetPos, Vector2 leftPos, Vector2 rightPos, ref bool isRight)
			{
				bool right = (targetPos - leftPos).magnitude > (targetPos - rightPos).magnitude;

				Vector2 startPos;
				Vector2 startTangent;

				if (right)
				{
					isRight = true;
					startPos = rightPos;
					startTangent = rightPos + EditorGUITools.kBezierTangentOffset;
				}
				else
				{
					isRight = false;
					startPos = leftPos;
					startTangent = leftPos - EditorGUITools.kBezierTangentOffset;
				}

				return new Bezier2D(startPos, startTangent, targetPos, startTangent);
			}
		}
	}
}