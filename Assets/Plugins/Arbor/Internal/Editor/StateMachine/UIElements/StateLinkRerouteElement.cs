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

	internal sealed class StateLinkRerouteElement : StateLinkElementBase
	{
		public StateLinkRerouteNodeEditor _NodeEditor;

		private VisualElement _PinCenter;
		private VisualElement _PinElement;
		private bool _IsDragHover;
		private Color _PinColor = Color.white;

		private ConnectManipulator _ConnectManipulator;

		public bool isDragHover
		{
			get
			{
				return _IsDragHover;
			}
			private set
			{
				if (_IsDragHover != value)
				{
					_IsDragHover = value;
					_PinElement.EnableInClassList("pin-active", _IsDragHover);
					UpdatePinElementColor();
				}
			}
		}

		void UpdatePinElementColor()
		{
			if (_IsDragHover || _ConnectManipulator.isActive)
			{
				_PinElement.style.unityBackgroundImageTintColor = StyleKeyword.Null;
			}
			else
			{
				_PinElement.style.unityBackgroundImageTintColor = _PinColor;
			}
		}

		public Color pinColor
		{
			get
			{
				return _PinColor;
			}
		}

		public StateLinkRerouteElement(StateLinkRerouteNodeEditor nodeEditor) : base(nodeEditor, null)
		{
			_NodeEditor = nodeEditor;

			AddToClassList("reroute-link-slot");
			AddToClassList("state-link-reroute-slot");

			_PinCenter = new VisualElement();
			_PinCenter.AddToClassList("pin-center");
			Add(_PinCenter);

			_PinElement = new VisualElement();
			_PinElement.AddToClassList("pin");
			_PinElement.EnableInClassList("pin-normal", true);
			_PinCenter.Add(_PinElement);

			UpdateDirection();

			_ConnectManipulator = new ConnectManipulator();
			_ConnectManipulator.onChangedActive += isActive =>
			{
				EnableInClassList("reroute-link-slot-active", isActive);
				if (isActive)
				{
					_PinElement.EnableInClassList("pin-normal", false);
				}
				else
				{
					_PinElement.EnableInClassList("pin-normal", !on);
				}
				UpdatePinElementColor();
				UpdateDirection();
			};
			this.AddManipulator(_ConnectManipulator);

			var graphEditor = nodeEditor.graphEditor as StateMachineGraphEditor;
			if (graphEditor != null)
			{
				StateLinkRerouteNode stateLinkRerouteNode = _NodeEditor.stateLinkRerouteNode;
				isDragHover = graphEditor.IsDragBranchHover(stateLinkRerouteNode);
			}

			Setup(_NodeEditor.stateLinkRerouteNode.link, null);
		}

		protected override void OnSettingsChanged()
		{
			UpdateLineColor();
		}

		protected override void OnAttachToPanel(AttachToPanelEvent e)
		{
			base.OnAttachToPanel(e);

			var graphEditor = _NodeEditor.graphEditor as StateMachineGraphEditor;
			if (graphEditor != null)
			{
				graphEditor.onChangedDragBranchHover -= OnChanedDragBranchHover;
				graphEditor.onChangedDragBranchHover += OnChanedDragBranchHover;
			}
		}

		protected override void OnDetachFromPanel(DetachFromPanelEvent e)
		{
			base.OnDetachFromPanel(e);

			var graphEditor = _NodeEditor.graphEditor as StateMachineGraphEditor;
			if (graphEditor != null)
			{
				graphEditor.onChangedDragBranchHover -= OnChanedDragBranchHover;
			}
		}

		protected override void OnRebuildElement(RebuildElementEvent e)
		{
			Setup(_NodeEditor.stateLinkRerouteNode.link, null);

			base.OnRebuildElement(e);
		}

		protected override void OnConnectionChanged(bool on)
		{
			EnableInClassList("reroute-link-slot-on", on);

			_PinElement.EnableInClassList("pin-normal", !on);
			UpdateDirection();
		}

		void OnChanedDragBranchHover(int nodeID)
		{
			isDragHover = _NodeEditor.nodeID == nodeID;
		}

		protected override void OnSetup(bool changedStateLink, bool changedFieldInfo)
		{
			if (changedStateLink)
			{
				UpdateLineColor();
			}
		}

		protected override void OnUndoRedoPerformed()
		{
			UpdateLineColor();
		}

		protected override Bezier2D OnUpdateBezier(Rect pinPos, Node targetNode)
		{
			NodeEditor nodeEditor = this.nodeEditor;
			
			Node node = nodeEditor.node;

			bool isRight = false;
			Bezier2D bezier = GetTargetBezier(node, targetNode, pinPos.center, pinPos.center, ref isRight);

			return bezier;
		}

		public event System.Action<Color> onChangedPinColor;

		void UpdateLineColor()
		{
			StateLink stateLink = this.stateLink;

			Color lineColor = stateLink.lineColor;
			lineColor.a = 1f;

			if (_PinColor != lineColor)
			{
				_PinColor = lineColor;

				onChangedPinColor?.Invoke(_PinColor);

				UpdatePinElementColor();
			}
		}

		public void UpdateDirection()
		{
			if (!on)
			{
				_PinCenter.transform.rotation = Quaternion.FromToRotation(Vector2.right, _NodeEditor.stateLinkRerouteNode.direction);
			}
			else
			{
				_PinCenter.transform.rotation = Quaternion.identity;
			}

			DoChangedPosition();
		}

		sealed class ConnectManipulator : DragManipulator
		{
			private Node _DragTargetNode = null;

			public ConnectManipulator()
			{
				activators.Add(new ManipulatorActivationFilter() { button = MouseButton.LeftMouse });
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

			protected override void OnMouseDown(MouseDownEvent e)
			{
				StateLinkRerouteElement linkElement = target as StateLinkRerouteElement;
				if (linkElement == null)
				{
					return;
				}

				_DragTargetNode = null;

				var nodeEditor = linkElement._NodeEditor;
				var stateLinkRerouteNode = nodeEditor.stateLinkRerouteNode;
				var graphEditor = linkElement._GraphEditor;
				
				if (graphEditor != null)
				{

					graphEditor.BeginDragStateBranch(nodeEditor.nodeID);

					Vector2 mousePosition = (e.target as VisualElement).ChangeCoordinatesTo(nodeEditor.nodeElement, e.localMousePosition);
					Rect pinPos = linkElement.parent.ChangeCoordinatesTo(nodeEditor.nodeElement, linkElement.layout);

					Bezier2D bezier = new Bezier2D();
					bezier.startPosition = pinPos.center;
					bezier.startControl = bezier.startPosition + stateLinkRerouteNode.direction * EditorGUITools.kBezierTangent;
					bezier.endPosition = mousePosition;
					bezier.endControl = bezier.startControl;

					Vector2 statePosition = new Vector2(stateLinkRerouteNode.position.x, stateLinkRerouteNode.position.y);
					bezier.startPosition += statePosition;
					bezier.startControl += statePosition;
					bezier.endPosition += statePosition;
					bezier.endControl += statePosition;

					graphEditor.DragStateBranchBezie(bezier);
				}

				e.StopPropagation();
			}

			void UpdateMousePosition(Vector2 localMousePosition)
			{
				StateLinkRerouteElement linkElement = target as StateLinkRerouteElement;
				if (linkElement == null)
				{
					return;
				}

				var nodeEditor = linkElement._NodeEditor;
				var stateLinkRerouteNode = nodeEditor.stateLinkRerouteNode;
				var graphEditor = linkElement._GraphEditor;

				Vector2 mousePosition = target.ChangeCoordinatesTo(nodeEditor.nodeElement, localMousePosition);

				Node hoverNode = graphEditor.GetTargetNodeFromPosition(graphEditor.graphView.ElementToGraph(nodeEditor.nodeElement, mousePosition), stateLinkRerouteNode);
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

				Rect pinPos = linkElement.parent.ChangeCoordinatesTo(nodeEditor.nodeElement, linkElement.layout);

				Bezier2D bezier = new Bezier2D();
				if (_DragTargetNode != null)
				{
					bool isRight = false;
					bezier = StateLinkElementBase.GetTargetBezier(stateLinkRerouteNode, _DragTargetNode, pinPos.center, pinPos.center, ref isRight);
				}
				else
				{
					bezier.startPosition = pinPos.center;
					bezier.startControl = bezier.startPosition + stateLinkRerouteNode.direction * EditorGUITools.kBezierTangent;
					bezier.endPosition = mousePosition;
					bezier.endControl = bezier.startControl;
				}

				Vector2 statePosition = new Vector2(stateLinkRerouteNode.position.x, stateLinkRerouteNode.position.y);
				bezier.startPosition += statePosition;
				bezier.startControl += statePosition;
				bezier.endPosition += statePosition;
				bezier.endControl += statePosition;

				graphEditor.DragStateBranchBezie(bezier);
			}

			protected override void OnMouseMove(MouseMoveEvent e)
			{
				StateLinkRerouteElement linkElement = target as StateLinkRerouteElement;
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
				StateLinkRerouteElement linkElement = target as StateLinkRerouteElement;
				if (linkElement == null)
				{
					return;
				}

				var nodeEditor = linkElement._NodeEditor;
				var stateLink = linkElement.stateLink;
				var stateLinkRerouteNode = nodeEditor.stateLinkRerouteNode;
				var graphEditor = linkElement._GraphEditor;

				Rect pinPos = linkElement.parent.ChangeCoordinatesTo(nodeEditor.nodeElement, linkElement.layout);

				Vector2 localMousePosition = e.localMousePosition;
				VisualElement eventTarget = e.target as VisualElement;

				Vector2 mousePosition = eventTarget.ChangeCoordinatesTo(nodeEditor.nodeElement, localMousePosition);

				Bezier2D bezier = new Bezier2D();
				if (_DragTargetNode != null)
				{
					bool isRight = false;
					bezier = StateLinkElementBase.GetTargetBezier(stateLinkRerouteNode, _DragTargetNode, pinPos.center, pinPos.center, ref isRight);
				}
				else
				{
					bezier.startPosition = pinPos.center;
					bezier.startControl = bezier.startPosition + stateLinkRerouteNode.direction * EditorGUITools.kBezierTangent;
					bezier.endPosition = mousePosition;
					bezier.endControl = bezier.startControl;
				}

				Vector2 statePosition = new Vector2(stateLinkRerouteNode.position.x, stateLinkRerouteNode.position.y);
				bezier.startPosition += statePosition;
				bezier.startControl += statePosition;
				bezier.endPosition += statePosition;
				bezier.endControl += statePosition;

				if (_DragTargetNode == null)
				{
					GenericMenu menu = new GenericMenu();

					Vector2 graphMousePosition = graphEditor.graphView.ElementToGraph(eventTarget, localMousePosition);
					Vector2 screenMousePosition = eventTarget.LocalToScreen(localMousePosition);

					menu.AddItem(EditorContents.createState, false, () =>
					{
						graphMousePosition -= new Vector2(8f, 12f);

						State newState = graphEditor.CreateState(graphMousePosition, false);

						Undo.RecordObject(graphEditor.nodeGraph, "Link State");

						stateLink.stateID = newState.nodeID;
						linkElement.bezier = bezier;

						linkElement.UpdateBezier();

						EditorUtility.SetDirty(graphEditor.nodeGraph);
					});

					menu.AddItem(EditorContents.reroute, false, () =>
					{
						Undo.IncrementCurrentGroup();
						int undoGroup = Undo.GetCurrentGroup();

						graphMousePosition -= new Vector2(16f, 16f);

						Color lineColor = stateLink.lineColor;

						StateLinkRerouteNode newStateLinkNode = graphEditor.CreateStateLinkRerouteNode(graphMousePosition, lineColor);

						Undo.RecordObject(graphEditor.nodeGraph, "Link State");

						stateLink.stateID = newStateLinkNode.nodeID;
						linkElement.bezier = bezier;

						linkElement.UpdateBezier();

						Undo.CollapseUndoOperations(undoGroup);

						EditorUtility.SetDirty(graphEditor.nodeGraph);
					});

					menu.AddSeparator("");

					menu.AddItem(EditorContents.nodeListSelection, false, () =>
					{
						StateLink currentStateLink = stateLink;
						NodeGraph currentGraph = graphEditor.nodeGraph;

						StateLinkSelectorWindow.instance.Open(graphEditor, new Rect(screenMousePosition, Vector2.zero), currentStateLink.stateID,
							(targetNodeEditor) =>
							{
								Undo.RecordObject(currentGraph, "Link State");

								currentStateLink.stateID = targetNodeEditor.nodeID;
								linkElement.bezier = bezier;

								linkElement.UpdateBezier();

								EditorUtility.SetDirty(currentGraph);

								//graphEditor.BeginFrameSelected(targetNodeEditor.node);
							}
						);
					});

					if (stateLink.stateID != 0)
					{
						menu.AddSeparator("");
						menu.AddItem(EditorContents.disconnect, false, () =>
						{
							Undo.RecordObject(graphEditor.nodeGraph, "Disconect StateLink");

							stateLink.stateID = 0;

							EditorUtility.SetDirty(graphEditor.nodeGraph);
						});
					}
					menu.ShowAsContext();
				}
				else 
				{
					Node targetNode = graphEditor.nodeGraph.GetNodeFromID(stateLink.stateID);
					if (_DragTargetNode != targetNode)
					{
						Undo.RecordObject(graphEditor.nodeGraph, "Link State");

						stateLink.stateID = _DragTargetNode.nodeID;
						linkElement.bezier = bezier;

						linkElement.UpdateBezier();

						EditorUtility.SetDirty(graphEditor.nodeGraph);
					}
				}
			}

			protected override void OnEndDrag()
			{
				base.OnEndDrag();

				StateLinkRerouteElement linkElement = target as StateLinkRerouteElement;
				if (linkElement == null)
				{
					return;
				}

				var graphEditor = linkElement._GraphEditor;

				if (graphEditor != null)
				{
					graphEditor.EndDragStateBranch();
				}

				_DragTargetNode = null;

			}

			void OnKeyDown(KeyDownEvent e)
			{
				StateLinkRerouteElement linkElement = target as StateLinkRerouteElement;
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
		}
	}
}