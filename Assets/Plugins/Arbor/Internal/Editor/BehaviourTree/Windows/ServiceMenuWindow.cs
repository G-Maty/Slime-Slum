//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor.BehaviourTree
{
	using Arbor.BehaviourTree;

	internal sealed class ServiceMenuWindow : SelectScriptWindow
	{
		private static ServiceMenuWindow _Instance;

		public static ServiceMenuWindow instance
		{
			get
			{
				if (_Instance == null)
				{
					ServiceMenuWindow[] objects = Resources.FindObjectsOfTypeAll<ServiceMenuWindow>();
					if (objects.Length > 0)
					{
						_Instance = objects[0];
					}
				}
				if (_Instance == null)
				{
					_Instance = ScriptableObject.CreateInstance<ServiceMenuWindow>();
				}
				return _Instance;
			}
		}

		private TreeBehaviourNodeEditor _TreeBehaviourNodeEditor;
		private int _Index;

		protected override string searchWord
		{
			get
			{
				return ArborEditorCache.serviceSearch;
			}

			set
			{
				ArborEditorCache.serviceSearch = value;
			}
		}

		protected override System.Type GetClassType()
		{
			return typeof(Service);
		}

		protected override string GetRootElementName()
		{
			return "Services";
		}

		protected override void OnSelect(System.Type classType)
		{
			if (_Index == -1)
			{
				_TreeBehaviourNodeEditor.AddService(classType);
			}
			else
			{
				_TreeBehaviourNodeEditor.InsertService(_Index, classType);
			}
		}

		public void Init(TreeBehaviourNodeEditor nodeEditor, Rect buttonRect, int index)
		{
			_TreeBehaviourNodeEditor = nodeEditor;
			_Index = index;

			Vector2 center = buttonRect.center;
			buttonRect.width = 300f;
			buttonRect.center = center;

			Open(buttonRect);
		}
	}
}