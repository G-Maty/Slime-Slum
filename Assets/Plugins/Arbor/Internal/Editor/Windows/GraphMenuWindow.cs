//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ArborEditor
{
	using Arbor;

	internal class GraphMenuWindow : SelectScriptWindow
	{
		private static GraphMenuWindow _Instance;

		public static GraphMenuWindow instance
		{
			get
			{
				if (_Instance == null)
				{
					GraphMenuWindow[] objects = Resources.FindObjectsOfTypeAll<GraphMenuWindow>();
					if (objects.Length > 0)
					{
						_Instance = objects[0];
					}
				}
				if (_Instance == null)
				{
					_Instance = ScriptableObject.CreateInstance<GraphMenuWindow>();
				}
				return _Instance;
			}
		}

		protected override string searchWord
		{
			get
			{
				return ArborEditorCache.graphSearch;
			}
			set
			{
				ArborEditorCache.graphSearch = value;
			}
		}

		public ArborEditorWindow hostWindow
		{
			get;
			private set;
		}

		protected override Type GetClassType()
		{
			return typeof(NodeGraph);
		}

		protected override string GetRootElementName()
		{
			return "Graphs";
		}

		protected override void OnSelect(Type classType)
		{
			int undoGroup = Undo.GetCurrentGroup();

			NodeGraph nodeGraph = NodeGraphUtility.CreateGraphObject(classType, classType.Name, null);

			hostWindow.SelectRootGraph(nodeGraph);

			Undo.CollapseUndoOperations(undoGroup);
		}

		public void Init(ArborEditorWindow hostWindow, Rect buttonRect)
		{
			this.hostWindow = hostWindow;
			Open(buttonRect);
		}
	}
}