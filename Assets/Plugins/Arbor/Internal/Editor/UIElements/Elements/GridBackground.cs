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
	internal sealed class GridBackground : ImmediateGUIElement
	{
		private GraphView _GraphView;

		public GridBackground(GraphView graphView)
		{
			_GraphView = graphView;
			style.position = Position.Absolute;
		}

		protected override void OnImmediateGUI()
		{
			EditorGUITools.DrawGrid(_GraphView.graphViewport, _GraphView.graphScale.x);
		}
	}
}