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
	internal sealed class PanManipulator : DragManipulator
	{
		private GraphView m_GraphView;
		
		public PanManipulator(GraphView graphView) : base(TrickleDownMode.Both)
		{
			m_GraphView = graphView;

			ManipulatorActivationFilter filter1 = new ManipulatorActivationFilter();
			filter1.button = MouseButton.LeftMouse;
			filter1.modifiers = EventModifiers.Alt;
			activators.Add(filter1);

			ManipulatorActivationFilter filter2 = new ManipulatorActivationFilter();
			filter2.button = MouseButton.MiddleMouse;
			activators.Add(filter2);
		}

		protected override void RegisterCallbacksOnTarget()
		{
			base.RegisterCallbacksOnTarget();
			target.RegisterCallback<WheelEvent>(OnScroll, TrickleDown.TrickleDown);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			base.UnregisterCallbacksFromTarget();
			target.UnregisterCallback<WheelEvent>(OnScroll, TrickleDown.TrickleDown);
		}

		private void OnScroll(WheelEvent e)
		{
			if (ArborSettings.mouseWheelMode == MouseWheelMode.Scroll)
			{
				m_GraphView.OnScroll(e.delta * 20.0f);
				e.StopPropagation();
			}
		}

		protected override void OnMouseDown(MouseDownEvent e)
		{
			if (e.propagationPhase == PropagationPhase.TrickleDown && e.button != 2)
			{
				return;
			}

			EditorGUIUtility.SetWantsMouseJumping(1);
			e.StopPropagation();
		}

		protected override void OnMouseMove(MouseMoveEvent e)
		{
			Vector2 delta = e.mouseDelta;
			Vector3 scale = m_GraphView.graphScale;
			delta = Vector2.Scale(delta, new Vector3(1f/scale.x, 1f/scale.y, 1f/scale.z));

			m_GraphView.OnScroll(-delta);

			e.StopPropagation();
		}

		protected override void OnEndDrag()
		{
			base.OnEndDrag();

			EditorGUIUtility.SetWantsMouseJumping(0);
		}
	}
}