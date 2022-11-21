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
	internal class GraphLayout : VisualElement
	{
		public event HandleEventDelegate handleEventDelegate;

		public GraphLayout() : base()
		{
			pickingMode = PickingMode.Ignore;
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