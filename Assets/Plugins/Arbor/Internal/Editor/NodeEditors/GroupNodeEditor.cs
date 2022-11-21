//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using Arbor;
using UnityEngine.UIElements;

namespace ArborEditor
{
	using ArborEditor.UIElements;

	[CustomNodeEditor(typeof(GroupNode))]
	internal sealed class GroupNodeEditor : NodeEditor
	{
		public override bool IsWindow()
		{
			return false;
		}

		public override void OnBeginDrag(bool altKey)
		{
			base.OnBeginDrag(altKey);

			if (!altKey)
			{
				List<Node> nodesInRect = graphEditor.GetNodesInRect(node.position);
				for (int nodeIndex = 0; nodeIndex < nodesInRect.Count; nodeIndex++)
				{
					Node targetNode = nodesInRect[nodeIndex];
					if (!(targetNode is GroupNode))
					{
						graphEditor.RegisterDragNode(targetNode);
					}
				}
			}
		}

		public override Styles.BaseColor GetStyleBaseColor()
		{
			return Styles.BaseColor.White;
		}

		public override Styles.Color GetStyleColor()
		{
			return Styles.Color.White;
		}

		public override Rect GetSelectableRect()
		{
			return GetHeaderRect();
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			isRenamable = true;
			isShowableComment = true;
			showContextMenuInWindow = ShowContextMenu.None;
		}

		public override Texture2D GetIcon()
		{
			GroupNode group = node as GroupNode;
			switch (group.autoAlignment)
			{
				case GroupNode.AutoAlignment.Vertical:
					return Icons.groupNodeIconVertical;
				case GroupNode.AutoAlignment.Horizonal:
					return Icons.groupNodeIconHorizontal;
			}
			return Icons.groupNodeIcon;
		}

		private PopupWindowContent _SettingsWindow = null;

		protected override void SetContextMenu(GenericMenu menu, Rect headerPosition, bool editable)
		{
			Rect mousePosition = new Rect(0, 0, 0, 0);
			mousePosition.position = Event.current.mousePosition;
			Rect position = GUIUtility.GUIToScreenRect(mousePosition);

			if (editable)
			{
				menu.AddItem(EditorContents.settings, false, () =>
				{
					if (_SettingsWindow == null)
					{
						_SettingsWindow = new GroupNodeSettingsWindow(graphEditor.hostWindow, node as GroupNode);
					}
					position = GUIUtility.ScreenToGUIRect(position);
					PopupWindowUtility.Show(position, _SettingsWindow, false);
				});
			}
			else
			{
				menu.AddDisabledItem(EditorContents.settings);
			}
		}

		private Dictionary<Node, Rect> _LastNodePositions = new Dictionary<Node, Rect>();

		struct AlignmentRect
		{
			private Rect _Rect;
			private GroupNode.AutoAlignment _Alignment;

			private float _Position;

			public Rect rect
			{
				get
				{
					return _Rect;
				}
			}

			public float position
			{
				get
				{
					return _Position;
				}
				set
				{
					switch (_Alignment)
					{
						case GroupNode.AutoAlignment.Vertical:
							_Rect.y = value;
							_Position = _Rect.y;
							maxPosition = _Rect.yMax;
							break;
						case GroupNode.AutoAlignment.Horizonal:
							_Rect.x = value;
							_Position = _Rect.x;
							maxPosition = _Rect.xMax;
							break;
					}
				}
			}

			public float maxPosition
			{
				get;
				private set;
			}

			static float GetPosition(Rect rect, GroupNode.AutoAlignment alignment)
			{
				switch (alignment)
				{
					case GroupNode.AutoAlignment.Vertical:
						return rect.y;
					case GroupNode.AutoAlignment.Horizonal:
						return rect.x;
				}

				return rect.x;
			}

			static float GetMaxPosition(Rect rect, GroupNode.AutoAlignment alignment)
			{
				switch (alignment)
				{
					case GroupNode.AutoAlignment.Vertical:
						return rect.yMax;
					case GroupNode.AutoAlignment.Horizonal:
						return rect.xMax;
				}

				return rect.xMax;
			}

			public AlignmentRect(Rect rect, GroupNode.AutoAlignment alignment)
			{
				_Rect = rect;
				_Alignment = alignment;

				_Position = GetPosition(rect, alignment);
				maxPosition = GetMaxPosition(rect, alignment);
			}
		}

		public void AutoLayout()
		{
			GroupNode groupNode = node as GroupNode;
			GroupNode.AutoAlignment autoAlignment = groupNode.autoAlignment;

			List<Node> nodes = new List<Node>();

			var nodesInRect = graphEditor.GetNodesInRect(node.position);
			for (int nodeIndex = 0; nodeIndex < nodesInRect.Count; nodeIndex++)
			{
				Node targetNode = nodesInRect[nodeIndex];
				if (!(targetNode is GroupNode))
				{
					nodes.Add(targetNode);
				}
			}

			int nodeCount = nodes.Count;

			if (autoAlignment != GroupNode.AutoAlignment.None)
			{
				nodes.Sort((a, b) =>
				{
					AlignmentRect alignA = new AlignmentRect(a.position, autoAlignment);
					AlignmentRect alignB = new AlignmentRect(b.position, autoAlignment);
					return alignA.position.CompareTo(alignB.position);
				});

				for (int i = 0; i < nodeCount - 1; i++)
				{
					Node node1 = nodes[i];

					Rect lastRect1;
					bool hasLastRect1 = _LastNodePositions.TryGetValue(node1, out lastRect1);

					AlignmentRect align1 = new AlignmentRect(node1.position, autoAlignment);
					AlignmentRect lastAlign1 = new AlignmentRect(lastRect1, autoAlignment);

					for (int j = i + 1; j < nodeCount; j++)
					{
						Node node2 = nodes[j];

						Rect lastRect2 = node2.position;
						bool hasLastRect2 = _LastNodePositions.TryGetValue(node2, out lastRect2);

						AlignmentRect align2 = new AlignmentRect(node2.position, autoAlignment);
						AlignmentRect lastAlign2 = new AlignmentRect(lastRect2, autoAlignment);

						if (node1.position.Overlaps(node2.position))
						{
							float space = EditorGUITools.GetSnapSpace();
							if (hasLastRect1 && hasLastRect2)
							{
								if (lastAlign1.maxPosition < lastAlign2.position)
								{
									space = lastAlign2.position - lastAlign1.maxPosition;
								}
							}

							align2.position = align1.maxPosition + space;
							Rect position = EditorGUITools.SnapPositionToGrid(align2.rect);

							graphEditor.OnAutoLayoutNode(node2, position);

							Undo.RecordObject(graphEditor.nodeGraph, "AutoLayout");

							node2.position = position;

							EditorUtility.SetDirty(graphEditor.nodeGraph);
						}
					}
				}
			}

			_LastNodePositions.Clear();
			for (int i = 0; i < nodeCount; i++)
			{
				Node n = nodes[i];
				_LastNodePositions[n] = n.position;
			}
		}

		public override MinimapLayer minimapLayer
		{
			get
			{
				return MinimapLayer.Underlay;
			}
		}

		protected override ResizeDirection GetResizeDirection()
		{
			return ResizeDirection.Left | ResizeDirection.Right |
				ResizeDirection.Top | ResizeDirection.Bottom;
		}

		protected override Color GetBackgroundColor()
		{
			GroupNode groupNode = node as GroupNode;
			return groupNode.color;
		}

		protected override void OnCreatedMinimapNodeElement()
		{
			base.OnCreatedMinimapNodeElement();

			minimapNodeElement.AddToClassList("group");
		}

		public override Color GetListColor()
		{
			return GetBackgroundColor() * Color.gray;
		}
	}
}
