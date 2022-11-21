//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace ArborEditor.BehaviourTree
{
	using Arbor.BehaviourTree;

	internal sealed class ActionBehaviourMenuWindow : SelectScriptWindow
	{
		private static ActionBehaviourMenuWindow _Instance;

		public static ActionBehaviourMenuWindow instance
		{
			get
			{
				if (_Instance == null)
				{
					ActionBehaviourMenuWindow[] objects = Resources.FindObjectsOfTypeAll<ActionBehaviourMenuWindow>();
					if (objects.Length > 0)
					{
						_Instance = objects[0];
					}
				}
				if (_Instance == null)
				{
					_Instance = ScriptableObject.CreateInstance<ActionBehaviourMenuWindow>();
				}
				return _Instance;
			}
		}

		private Vector2 _Position;

		protected override string searchWord
		{
			get
			{
				return ArborEditorCache.actionBehaviourSearch;
			}

			set
			{
				ArborEditorCache.actionBehaviourSearch = value;
			}
		}

		protected override System.Type GetClassType()
		{
			return typeof(ActionBehaviour);
		}

		protected override string GetRootElementName()
		{
			return "Actions";
		}

		public delegate void OnSelectCallback(Vector2 position, System.Type classType);
		public delegate void OnCancelCallback();
		public delegate void OnCloseCallback();

		private OnSelectCallback _OnSelect;
		private OnCancelCallback _OnCancel;
		private OnCloseCallback _OnClose;

		protected override void OnSelect(System.Type classType)
		{
			if (_OnSelect != null)
			{
				_OnSelect(_Position, classType);
			}
		}

		protected override void OnCancel()
		{
			if (_OnCancel != null)
			{
				_OnCancel();
			}
		}

		protected override void OnClose()
		{
			if (_OnClose != null)
			{
				_OnClose();
			}
		}

		public void Init(NodeGraphEditor graphEditor, Vector2 position, Rect buttonRect, OnSelectCallback onSelect, OnCancelCallback onCancel, OnCloseCallback onClose)
		{
			_OnSelect = onSelect;
			_OnCancel = onCancel;
			_OnClose = onClose;
			_Position = position;

			Open(buttonRect);
		}
	}
}