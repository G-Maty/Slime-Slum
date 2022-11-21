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
	internal sealed class GraphMainLayout : VisualSplitter
	{
		public event HandleEventDelegate handleEventDelegate;

		public GraphLayout leftPanel
		{
			get; private set;
		}

		public GraphLayout rightPanel
		{
			get; private set;
		}

		private bool _IsInitialized = false;

		public GraphMainLayout() : base()
		{
			pickingMode = PickingMode.Ignore;
			style.flexDirection = FlexDirection.Row;
			style.flexGrow = 1f;
			RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

			leftPanel = new GraphLayout()
			{
				name = "LeftPanel",
				style =
				{
					minWidth = 230.0f,
					flexGrow = 1f,
					flexBasis = 0f,
					borderRightColor = EditorGUITools.GetSplitColor(),
					borderRightWidth = 1f,
				}
			};

			leftPanel.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedLeftPanel);

			rightPanel = new GraphLayout()
			{
				name = "RightPanel",
				style =
				{
					minWidth = 150.0f,
					flexGrow = 3f,
					flexBasis = 0f,
				}
			};

			Add(rightPanel);

			ShowLeftPanel(ArborSettings.openSidePanel);
		}

		public void ShowLeftPanel(bool show)
		{
			if (show)
			{
				if (leftPanel.parent == null)
				{
					Insert(0, leftPanel);
				}
			}
			else if(leftPanel.parent != null)
			{
				leftPanel.RemoveFromHierarchy();
			}
		}

		void OnGeometryChanged(GeometryChangedEvent e)
		{
			if (!_IsInitialized && leftPanel.parent != null)
			{
				InitializePanelFlex();
			}
		}

		void OnGeometryChangedLeftPanel(GeometryChangedEvent e)
		{
			if (_IsInitialized)
			{
				ArborSettings.sidePanelWidth = leftPanel.layout.width;
			}
			else
			{
				InitializePanelFlex();
			}
		}

		void InitializePanelFlex()
		{
			FlexDirection flexDirection = resolvedStyle.flexDirection;
			bool isHorizontal = IsHorizontal(flexDirection);

			float sidePanelWidth = ArborSettings.sidePanelWidth;
			Vector2 localPosition = isHorizontal ? new Vector2(sidePanelWidth, 0f) : new Vector2(0f, sidePanelWidth);

			float relativeMousePosition = CalcRelativePosition(localPosition, leftPanel, rightPanel, flexDirection);

			float leftPanelFlex = leftPanel.resolvedStyle.flexGrow;
			float rightPanelFlex = rightPanel.resolvedStyle.flexGrow;

			float totalFlex = leftPanelFlex + rightPanelFlex;

			leftPanel.style.flexGrow = relativeMousePosition * totalFlex;
			rightPanel.style.flexGrow = (1.0f - relativeMousePosition) * totalFlex;

			_IsInitialized = true;
		}

		protected override void ExecuteDefaultAction(EventBase evtBase)
		{
			if (handleEventDelegate != null)
			{
				handleEventDelegate(evtBase);
			}
			base.ExecuteDefaultAction(evtBase);
		}

		public delegate void HandleEventDelegate(EventBase evtBase);
	}
}