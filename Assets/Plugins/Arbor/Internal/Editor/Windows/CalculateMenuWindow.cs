//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

using Arbor;

namespace ArborEditor
{
	internal sealed class CalculateMenuWindow : SelectScriptWindow
	{
		private static CalculateMenuWindow _Instance;

		public static CalculateMenuWindow instance
		{
			get
			{
				if (_Instance == null)
				{
					CalculateMenuWindow[] objects = Resources.FindObjectsOfTypeAll<CalculateMenuWindow>();
					if (objects.Length > 0)
					{
						_Instance = objects[0];
					}
				}
				if (_Instance == null)
				{
					_Instance = ScriptableObject.CreateInstance<CalculateMenuWindow>();
				}
				return _Instance;
			}
		}

		private Vector2 _Position;
		private NodeGraphEditor _GraphEditor;

		protected override string searchWord
		{
			get
			{
				return ArborEditorCache.calculatorSearch;
			}

			set
			{
				ArborEditorCache.calculatorSearch = value;
			}
		}

		protected override System.Type GetClassType()
		{
			return typeof(Calculator);
		}

		protected override string GetRootElementName()
		{
			return "Calculators";
		}

		protected override void OnSelect(System.Type classType)
		{
			_GraphEditor.CreateCalculator(_Position, classType);
		}

		public void Init(NodeGraphEditor graphEditor, Vector2 position, Rect buttonRect)
		{
			_GraphEditor = graphEditor;
			_Position = position;

			Open(buttonRect);
		}
	}
}
