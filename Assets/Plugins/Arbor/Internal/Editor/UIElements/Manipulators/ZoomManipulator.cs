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
	internal sealed class ZoomManipulator : DragManipulator
	{
		private GraphView m_GraphView;
		private Vector2 m_Start;
		private Vector2 m_Last;
		private Vector2 m_ZoomCenter;

		public float zoomStep
		{
			get;
			set;
		}

		public ZoomManipulator(GraphView graphView) : base(TrickleDownMode.TrickleDown)
		{
			m_GraphView = graphView;
			zoomStep = 0.01f;

			ManipulatorActivationFilter filter = new ManipulatorActivationFilter();
			filter.button = MouseButton.RightMouse;
			filter.modifiers = EventModifiers.Alt;
			activators.Add(filter);
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
			if (ArborSettings.mouseWheelMode == MouseWheelMode.Zoom)
			{
				Vector2 zoomCenter = m_GraphView.ElementToGraph(e.currentTarget as VisualElement, e.localMousePosition);
				m_GraphView.OnZoom(zoomCenter, 1.0f - e.delta.y * zoomStep);
				e.StopPropagation();
			}
		}

		protected override void OnMouseDown(MouseDownEvent e)
		{
			m_Start = m_Last = e.localMousePosition;
			m_ZoomCenter = m_GraphView.ElementToGraph(target, m_Start);
			e.StopPropagation();
		}

		protected override void OnMouseMove(MouseMoveEvent e)
		{
			Vector2 vector2 = e.localMousePosition - m_Last;
			m_GraphView.OnZoom(m_ZoomCenter, 1.0f + (vector2.x + vector2.y) * zoomStep);
			e.StopPropagation();
			m_Last = e.localMousePosition;
		}
	}
}