//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor
{
	using Arbor;

	internal sealed class OutputSlotGUI : DataSlotGUI
	{
		public override void RebuildConnectGUI()
		{
			DataSlot slot = this.slot;
			OutputSlotBase outputSlot = slot as OutputSlotBase;

			for (int i = outputSlot.branchIDs.Count - 1; i >= 0; i--)
			{
				int branchID = outputSlot.branchIDs[i];
				DataBranch branch = outputSlot.nodeGraph.GetDataBranchFromID(branchID);
				if (branch == null || (branch != null && branch.outputSlot != outputSlot))
				{
					outputSlot.RemoveBranchAt(i);
					EditorUtility.SetDirty(targetObject);
					branch = null;
				}

				if (branch != null)
				{
					if (!branch.isValidInputSlot || !DataSlot.IsConnectable(branch.inputSlot, slot))
					{
						var window = ArborEditorWindow.actualWindow;
						var graphEditor = window?.graphEditor;
						if (graphEditor != null && graphEditor.nodeGraph == outputSlot.nodeGraph)
						{
							graphEditor.DeleteDataBranch(branch);
						}
						else
						{
							outputSlot.nodeGraph.DeleteDataBranch(branch);
						}
					}
				}
			}
		}

		protected override void OnGUI(Rect position, GUIContent label, bool isElement)
		{
			DataSlot slot = this.slot;
			OutputSlotBase outputSlot = slot as OutputSlotBase;

			ArborEditorWindow window = ArborEditorWindow.actualWindow;

			int controlID = EditorGUIUtility.GetControlID(s_DataSlotHash, FocusType.Passive, position);

			Event current = Event.current;

			Vector2 pinPos = new Vector2(node.position.width, position.center.y);
			Vector2 targetPos = current.mousePosition;

			var graphEditor = window?.graphEditor;
			var graphView = window?.graphView;
			if (graphView != null)
			{
				pinPos = graphView.GUIToGraph(pinPos);
				pinPos.x = node.position.xMax;
				targetPos = graphView.GUIToGraph(targetPos);
			}

			Vector2 pinControlPos = pinPos + EditorGUITools.kBezierTangentOffset;
			Vector2 targetControlPos = targetPos - EditorGUITools.kBezierTangentOffset;

			if (_HoverSlot != null)
			{
				DataBranchRerouteNode hoverRerouteNode = _HoverNode as DataBranchRerouteNode;
				if (hoverRerouteNode != null)
				{
					targetPos = _HoverSlot.position.center;
					targetControlPos = targetPos - hoverRerouteNode.direction * EditorGUITools.kBezierTangent;
				}
				else
				{
					if (ArborSettings.dataSlotShowMode == DataSlotShowMode.Inside)
					{
						targetPos = new Vector2(_HoverNode.position.xMin, _HoverSlot.position.center.y);
					}
					else
					{
						targetPos = new Vector2(_HoverSlot.position.xMin, _HoverSlot.position.center.y);
					}
					targetControlPos = targetPos - EditorGUITools.kBezierTangentOffset;
				}
			}

			switch (current.GetTypeForControl(controlID))
			{
				case EventType.ContextClick:
					if (position.Contains(current.mousePosition))
					{
						GenericMenu menu = new GenericMenu();

						int branchCount = outputSlot.branchCount;
						if (branchCount != 0)
						{
							menu.AddItem(EditorContents.disconnectAll, false, () =>
							{
								for (int i = branchCount - 1; i >= 0; i--)
								{
									graphEditor.DeleteDataBranch(outputSlot.GetBranch(i));
								}
							});
						}
						else
						{
							menu.AddDisabledItem(EditorContents.disconnectAll);
						}

						menu.ShowAsContext();

						current.Use();
					}
					break;
				case EventType.MouseDown:
					if (position.Contains(current.mousePosition) && current.button == 0)
					{
						GUIUtility.hotControl = GUIUtility.keyboardControl = controlID;

						if (graphEditor != null)
						{
							BeginDragSlot(node, slot, targetObject);

							graphEditor.BeginDragDataBranch(node.nodeID);
							graphEditor.DragDataBranchBezier(pinPos, pinControlPos, targetPos, targetControlPos);
						}

						current.Use();
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == controlID && current.button == 0)
					{
						DragAndDrop.PrepareStartDrag();

						UpdateHoverSlot(nodeGraph, node, targetObject, outputSlot, graphView.GUIToGraph(current.mousePosition));

						current.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == controlID)
					{
						if (current.button == 0)
						{
							GUIUtility.hotControl = 0;

							if (graphEditor != null)
							{
								EndDragSlot();

								graphEditor.EndDragDataBranch();

								DataBranch branch = null;

								if (_HoverSlot == null)
								{
									GenericMenu menu = new GenericMenu();

									Vector2 mousePosition = graphView.GUIToGraph(current.mousePosition);

									DataBranch currentBranch = branch;
									Bezier2D lineBezier = new Bezier2D(pinPos, pinControlPos, targetPos, targetControlPos);

									menu.AddItem(EditorContents.reroute, false, () =>
									{
										if (currentBranch != null)
										{
											graphEditor.DeleteDataBranch(currentBranch);
											currentBranch = null;
										}

										Undo.IncrementCurrentGroup();
										int undoGroup = Undo.GetCurrentGroup();

										mousePosition -= new Vector2(16f, 16f);

										DataBranchRerouteNode newRerouteNode = graphEditor.CreateDataBranchRerouteNode(EditorGUITools.SnapToGrid(mousePosition), slot.connectableType);

										Undo.RecordObject(nodeGraph, "Reroute");

										RerouteSlot rerouteSlot = newRerouteNode.link;

										currentBranch = graphEditor.ConnectDataBranch(newRerouteNode.nodeID, null, rerouteSlot, node.nodeID, targetObject, outputSlot);
										if (currentBranch != null)
										{
											currentBranch.enabled = true;
											currentBranch.lineBezier = lineBezier;
										}

										Undo.CollapseUndoOperations(undoGroup);

										EditorUtility.SetDirty(nodeGraph);
									});

									menu.ShowAsContext();
								}
								else if (_HoverSlot != null)
								{
									InputSlotBase inputSlot = _HoverSlot as InputSlotBase;

									if (inputSlot != null)
									{
										branch = nodeGraph.GetDataBranchFromID(inputSlot.branchID);
									}
									else
									{
										RerouteSlot rerouteSlot = _HoverSlot as RerouteSlot;
										if (rerouteSlot != null)
										{
											branch = nodeGraph.GetDataBranchFromID(rerouteSlot.inputBranchID);
										}
									}

									if (branch != null)
									{
										graphEditor.DeleteDataBranch(branch);
										branch = null;
									}

									branch = graphEditor.ConnectDataBranch(_HoverNode.nodeID, _HoverObj, _HoverSlot, node.nodeID, targetObject, outputSlot);
									if (branch != null)
									{
										branch.lineBezier = new Bezier2D(pinPos, pinControlPos, targetPos, targetControlPos);
									}
								}

								if (branch != null)
								{
									branch.enabled = true;
								}

								ClearHoverSlot();
							}
						}

						current.Use();
					}
					break;
				case EventType.KeyDown:
					if (GUIUtility.hotControl == controlID && current.keyCode == KeyCode.Escape)
					{
						GUIUtility.hotControl = 0;

						if (graphEditor != null)
						{
							EndDragSlot();

							graphEditor.EndDragDataBranch();

							ClearHoverSlot();
						}

						current.Use();
					}
					break;
				case EventType.Repaint:
					{
						if (GUIUtility.hotControl == controlID && current.button == 0)
						{
							graphEditor.DragDataBranchBezier(pinPos, pinControlPos, targetPos, targetControlPos);
						}

						bool on = false;
						int idCount = outputSlot.branchIDs.Count;
						for (int idIndex = 0; idIndex < idCount; idIndex++)
						{
							int branchID = outputSlot.branchIDs[idIndex];
							if (branchID != 0)
							{
								on = true;
								DataBranch branch = nodeGraph.GetDataBranchFromID(branchID);
								if (branch != null)
								{
									branch.lineBezier.startPosition = pinPos;
									branch.lineBezier.startControl = pinControlPos;
								}
							}
						}

						if (!isElement)
						{
							DrawSlot(position, label, controlID, on, false);
						}
					}
					break;
			}
		}

		public override void UpdateBezier(Rect position)
		{
			DataSlot slot = this.slot;
			OutputSlotBase outputSlot = slot as OutputSlotBase;
			int idCount = outputSlot.branchIDs.Count;
			if (idCount == 0)
			{
				return;
			}

			ArborEditorWindow window = ArborEditorWindow.actualWindow;

			Vector2 pinPos = new Vector2(node.position.width, position.center.y);
			Vector2 pinControlPos = pinPos + EditorGUITools.kBezierTangentOffset;
			var graphView = (window != null) ? window.graphView : null;
			if (graphView != null)
			{
				pinPos = graphView.GUIToGraph(pinPos);
				pinPos.x = node.position.xMax;
				pinControlPos = graphView.GUIToGraph(pinControlPos);
			}

			for (int idIndex = 0; idIndex < idCount; idIndex++)
			{
				int branchID = outputSlot.branchIDs[idIndex];
				if (branchID == 0)
				{
					continue;
				}
				
				DataBranch branch = nodeGraph.GetDataBranchFromID(branchID);
				if (branch != null)
				{
					branch.lineBezier.startPosition = pinPos;
					branch.lineBezier.startControl = pinControlPos;
				}
			}
		}
	}
}