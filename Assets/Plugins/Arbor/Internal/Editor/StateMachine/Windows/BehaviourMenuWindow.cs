//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

using Arbor;

namespace ArborEditor
{
	internal sealed class BehaviourMenuWindow : SelectScriptWindow
	{
		private static BehaviourMenuWindow _Instance;

		public static BehaviourMenuWindow instance
		{
			get
			{
				if (_Instance == null)
				{
					BehaviourMenuWindow[] objects = Resources.FindObjectsOfTypeAll<BehaviourMenuWindow>();
					if (objects.Length > 0)
					{
						_Instance = objects[0];
					}
				}
				if (_Instance == null)
				{
					_Instance = ScriptableObject.CreateInstance<BehaviourMenuWindow>();
				}
				return _Instance;
			}
		}

		private StateEditor _StateEditor;
		private int _InsertIndex = -1;

		protected override string searchWord
		{
			get
			{
				return ArborEditorCache.behaviourSearch;
			}

			set
			{
				ArborEditorCache.behaviourSearch = value;
			}
		}

		protected override System.Type GetClassType()
		{
			return typeof(StateBehaviour);
		}

		protected override string GetRootElementName()
		{
			return "Behaviours";
		}

		protected override void OnSelect(System.Type classType)
		{
			if (_InsertIndex != -1)
			{
				_StateEditor.InsertBehaviour(_InsertIndex, classType);
			}
			else
			{
				_StateEditor.AddBehaviour(classType);
			}
		}

		public void Init(StateEditor stateEditor, Rect buttonRect, int insertIndex = -1)
		{
			_StateEditor = stateEditor;
			_InsertIndex = insertIndex;

			Vector2 center = buttonRect.center;
			buttonRect.width = 300f;
			buttonRect.center = center;

			Open(buttonRect);
		}
	}
}
