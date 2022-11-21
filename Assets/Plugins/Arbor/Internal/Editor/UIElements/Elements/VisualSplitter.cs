//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace ArborEditor.UIElements
{
	internal class VisualSplitter : ImmediateModeElement
	{
		private const int kDefalutSplitSize = 6;
		public int splitSize = kDefalutSplitSize;

		public VisualSplitter() : base()
		{
			this.AddManipulator(new SplitManipulator());
		}

		public VisualElement[] GetAffectedVisualElements()
		{
			List<VisualElement> visualElementList = new List<VisualElement>();
			for (int index = 0; index < hierarchy.childCount; ++index)
			{
				VisualElement visualElement = hierarchy[index];
				if (visualElement.resolvedStyle.position == Position.Relative)
				{
					visualElementList.Add(visualElement);
				}
			}
			return visualElementList.ToArray();
		}

		public static bool IsHorizontal(FlexDirection flexDirection)
		{
			return flexDirection == FlexDirection.Row || flexDirection == FlexDirection.RowReverse;
		}

		public static float CalcRelativePosition(Vector2 localPosition, VisualElement visualElement, VisualElement nextVisualElement, FlexDirection flexDirection)
		{
			bool isHorizontal = IsHorizontal(flexDirection);

			float relativeMousePosition;
			if (isHorizontal)
			{
				float minWidth = visualElement.resolvedStyle.minWidth == StyleKeyword.Auto ? 0 : visualElement.resolvedStyle.minWidth.value;
				float nextMinWidth = nextVisualElement.resolvedStyle.minWidth == StyleKeyword.Auto ? 0 : nextVisualElement.resolvedStyle.minWidth.value;
				float availableWidth = visualElement.layout.width + nextVisualElement.layout.width - minWidth - nextMinWidth;
				float maxWidth = visualElement.resolvedStyle.maxWidth.value <= 0 ? availableWidth : visualElement.resolvedStyle.maxWidth.value;

				relativeMousePosition = (Math.Min(localPosition.x, visualElement.layout.xMin + maxWidth) - visualElement.layout.xMin - minWidth) / availableWidth;
			}
			else
			{
				float minHeight = visualElement.resolvedStyle.minHeight == StyleKeyword.Auto ? 0 : visualElement.resolvedStyle.minHeight.value;
				float nextMinHeight = nextVisualElement.resolvedStyle.minHeight == StyleKeyword.Auto ? 0 : nextVisualElement.resolvedStyle.minHeight.value;
				float availableHeight = visualElement.layout.height + nextVisualElement.layout.height - minHeight - nextMinHeight;
				float maxHeight = visualElement.resolvedStyle.maxHeight.value <= 0 ? availableHeight : visualElement.resolvedStyle.maxHeight.value;

				relativeMousePosition = (Math.Min(localPosition.y, visualElement.layout.yMin + maxHeight) - visualElement.layout.yMin - minHeight) / availableHeight;
			}

			return Math.Max(0.0f, Math.Min(0.999f, relativeMousePosition));
		}

		protected override void ImmediateRepaint()
		{
			for (int index = 0; index < this.hierarchy.childCount - 1; ++index)
			{
				EditorGUIUtility.AddCursorRect(GetSplitterRect(hierarchy[index]), IsHorizontal(resolvedStyle.flexDirection) ? MouseCursor.SplitResizeLeftRight : MouseCursor.ResizeVertical);
			}
		}

		public Rect GetSplitterRect(VisualElement visualElement)
		{
			Rect layout = visualElement.layout;
			FlexDirection flexDirection = resolvedStyle.flexDirection;
			switch (flexDirection)
			{
				case FlexDirection.Row:
					layout.xMin = visualElement.layout.xMax - splitSize * 0.5f;
					layout.xMax = visualElement.layout.xMax + splitSize * 0.5f;
					break;
				case FlexDirection.RowReverse:
					layout.xMin = visualElement.layout.xMin - splitSize * 0.5f;
					layout.xMax = visualElement.layout.xMin + splitSize * 0.5f;
					break;
				case FlexDirection.Column:
					layout.yMin = visualElement.layout.yMax - splitSize * 0.5f;
					layout.yMax = visualElement.layout.yMax + splitSize * 0.5f;
					break;
				case FlexDirection.ColumnReverse:
					layout.yMin = visualElement.layout.yMin - splitSize * 0.5f;
					layout.yMax = visualElement.layout.yMin + splitSize * 0.5f;
					break;
			}
			return layout;
		}

		private sealed class SplitManipulator : DragManipulator
		{
			private int m_ActiveVisualElementIndex;
			private int m_NextVisualElementIndex;
			private VisualElement[] m_AffectedElements;

			public SplitManipulator() : base(TrickleDownMode.TrickleDown)
			{
				m_ActiveVisualElementIndex = -1;
				m_NextVisualElementIndex = -1;

				ManipulatorActivationFilter activationFilter = new ManipulatorActivationFilter();
				activationFilter.button = MouseButton.LeftMouse;
				activators.Add(activationFilter);
			}

			protected override void OnMouseDown(MouseDownEvent e)
			{
				VisualSplitter target = this.target as VisualSplitter;
				FlexDirection flexDirection = target.resolvedStyle.flexDirection;
				m_AffectedElements = target.GetAffectedVisualElements();
				for (int index = 0; index < m_AffectedElements.Length - 1; ++index)
				{
					VisualElement affectedElement = m_AffectedElements[index];
					if (target.GetSplitterRect(affectedElement).Contains(e.localMousePosition))
					{
						if (flexDirection == FlexDirection.RowReverse || flexDirection == FlexDirection.ColumnReverse)
						{
							m_ActiveVisualElementIndex = index + 1;
							m_NextVisualElementIndex = index;
						}
						else
						{
							m_ActiveVisualElementIndex = index;
							m_NextVisualElementIndex = index + 1;
						}
						e.StopPropagation();
					}
				}
			}

			protected override void OnMouseMove(MouseMoveEvent e)
			{
				VisualSplitter visualSplitter = this.target as VisualSplitter;
				VisualElement visualElement = m_AffectedElements[m_ActiveVisualElementIndex];
				VisualElement nextVisualElement = m_AffectedElements[m_NextVisualElementIndex];

				FlexDirection flexDirection = visualSplitter.resolvedStyle.flexDirection;
				float relativeMousePosition = VisualSplitter.CalcRelativePosition(e.localMousePosition, visualElement, nextVisualElement, flexDirection);

				float totalFlex = visualElement.resolvedStyle.flexGrow + nextVisualElement.resolvedStyle.flexGrow;
				visualElement.style.flexGrow = relativeMousePosition * totalFlex;
				nextVisualElement.style.flexGrow = (1.0f - relativeMousePosition) * totalFlex;

				e.StopPropagation();
			}

			protected override void OnEndDrag()
			{
				base.OnEndDrag();

				m_ActiveVisualElementIndex = -1;
				m_NextVisualElementIndex = -1;
			}
		}
	}
}