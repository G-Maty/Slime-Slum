//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArborEditor.UIElements
{
	using ArborEditor.UnityEditorBridge.UIElements.Extensions;

	internal sealed class MinimapNodeElement : VisualElement
	{
		private NodeEditor _NodeEditor;

		public MinimapNodeElement(NodeEditor nodeEditor)
		{
			_NodeEditor = nodeEditor;

			AddToClassList("node-element");

			this.AddManipulator(new MinimapTransformManipulator(_NodeEditor.graphEditor.minimapView, UpdateLayout));

			RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
			RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
		}

		void OnAttachToPanel(AttachToPanelEvent e)
		{
			_NodeEditor.nodeElement.RegisterCallback<ChangeNodePositionEvent>(OnChangeNodePosition);
			_NodeEditor.nodeElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedNode);

			UpdateLayout();
		}

		void OnDetachFromPanel(DetachFromPanelEvent e)
		{
			_NodeEditor.nodeElement.UnregisterCallback<ChangeNodePositionEvent>(OnChangeNodePosition);
			_NodeEditor.nodeElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedNode);
		}

		void OnChangeNodePosition(ChangeNodePositionEvent e)
		{
			UpdateLayout();
		}

		void OnGeometryChangedNode(GeometryChangedEvent e)
		{
			UpdateLayout();
		}

		void UpdateLayout()
		{
			var minimapView = _NodeEditor.graphEditor.minimapView;

			Rect nodeRect = minimapView.GraphToMinimap(_NodeEditor.node.position);
			this.SetLayout(nodeRect);
		}
	}
}