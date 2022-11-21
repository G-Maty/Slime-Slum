//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace ArborEditor
{
	using Arbor;
	using ArborEditor.IMGUI.Controls;

	using UnityObject = UnityEngine.Object;

	public sealed class TypePopupWindow : TreePopupWindow<Type>, ITreeFilter
	{
		private static readonly int s_TypePopupHash = "ArborEditor.TypePopupWindow.s_TypePopupHash".GetHashCode();

		public const string kNoneText = "None";

		private UniqueIDGenerator _UniqueIDGenerator = new UniqueIDGenerator();

		protected override string searchWord
		{
			get
			{
				return ArborEditorCache.typeSearch;
			}
			set
			{
				ArborEditorCache.typeSearch = value;
			}
		}

		protected override bool isReady
		{
			get
			{
				return ClassList.isReady;
			}
		}

		static void Open(Rect buttonRect, int controlID, Type selected, IDefinableType definableType, bool hasFilter, TypeFilterFlags filterFlags)
		{
			TypePopupWindow window = s_Instance as TypePopupWindow;
			if (window == null)
			{
				window = ScriptableObject.CreateInstance<TypePopupWindow>();
				s_Instance = window;
			}
			window.Init(buttonRect, controlID, selected, definableType, hasFilter, filterFlags);
		}

		static int BitCount(int flags)
		{
			var x = flags - ((flags >> 1) & 0x55555555);
			x = ((x >> 2) & 0x33333333) + (x & 0x33333333);
			x = (x >> 4) + x & 0x0f0f0f0f;
			x += x >> 8;
			return (x >> 16) + x & 0xff;
		}

		void Init(Rect buttonRect, int controlID, Type selected, IDefinableType definableType, bool hasFilter, TypeFilterFlags filterFlags)
		{
			_DefinableType = definableType;
			_HasFilter = hasFilter;
			_UseFilter = hasFilter && BitCount((int)filterFlags) > 1;
			if (ArborEditorCache.typeFilterMask != filterFlags)
			{
				ArborEditorCache.typeFilterMask = filterFlags;
				ArborEditorCache.typeFilterFlags = filterFlags;
			}

			Init(buttonRect, controlID, selected);
		}

		public static Type PopupField(Rect position, Type selected)
		{
			int controlID = GUIUtility.GetControlID(s_TypePopupHash, FocusType.Keyboard, position);

			return PopupField(position, controlID, selected, null);
		}

		public static Type PopupField(Rect position, Type selected, GUIContent label, IDefinableType definableType)
		{
			return PopupField(position, selected, label, definableType, false, (TypeFilterFlags)0);
		}

		public static Type PopupField(Rect position, Type selected, GUIContent label, IDefinableType definableType, bool hasFilter, TypeFilterFlags filterFlags)
		{
			int controlID = GUIUtility.GetControlID(s_TypePopupHash, FocusType.Keyboard, position);

			return PopupField(position, controlID, selected, label, definableType, hasFilter, filterFlags);
		}

		public static Type PopupField(Rect position, Type selected, GUIContent label, IDefinableType definableType, bool hasFilter, TypeFilterFlags filterFlags, GUIContent selectedName)
		{
			int controlID = GUIUtility.GetControlID(s_TypePopupHash, FocusType.Keyboard, position);

			return PopupField(position, controlID, selected, label, definableType, hasFilter, filterFlags, selectedName);
		}

		public static Type PopupField(Rect position, int controlID, Type selected, GUIContent label, IDefinableType definableType)
		{
			return PopupField(position, controlID, selected, label, definableType, false, (TypeFilterFlags)0);
		}

		public static Type PopupField(Rect position, int controlID, Type selected, GUIContent label, IDefinableType definableType, bool hasFilter, TypeFilterFlags filterFlags)
		{
			position = EditorGUI.PrefixLabel(position, controlID, label);

			return PopupField(position, controlID, selected, definableType, hasFilter, filterFlags);
		}

		public static Type PopupField(Rect position, int controlID, Type selected, GUIContent label, IDefinableType definableType, bool hasFilter, TypeFilterFlags filterFlags, GUIContent selectedName)
		{
			position = EditorGUI.PrefixLabel(position, controlID, label);

			return PopupField(position, controlID, selected, definableType, hasFilter, filterFlags, selectedName);
		}

		public static Type PopupField(Rect position, int controlID, Type selected, IDefinableType definableType)
		{
			return PopupField(position, controlID, selected, definableType, false, (TypeFilterFlags)0);
		}

		public static Type PopupField(Rect position, int controlID, Type selected, IDefinableType definableType, bool hasFilter, TypeFilterFlags filterFlags)
		{
			return PopupField(position, controlID, selected, definableType, hasFilter, filterFlags, null);
		}

		public static Type PopupField(Rect position, int controlID, Type selected, IDefinableType definableType, bool hasFilter, TypeFilterFlags filterFlags, GUIContent selectedName)
		{
			using (new EditorGUI.DisabledGroupScope(EditorApplication.isCompiling))
			{
				selected = GetSelectedValueForControl(controlID, selected);

				Event current = Event.current;

				EventType eventType = current.GetTypeForControl(controlID);

				selectedName = selectedName ?? new GUIContent(selected != null ? TypeUtility.GetTypeName(selected) : kNoneText);
				GUIStyle style = EditorStyles.popup;

				switch (eventType)
				{
					case EventType.MouseDown:
						if (current.button == 0 && position.Contains(current.mousePosition))
						{
							Rect buttonRect = GUIUtility.GUIToScreenRect(position);
							Open(buttonRect, controlID, selected, definableType, hasFilter, filterFlags);
							current.Use();
						}
						break;
					case EventType.KeyDown:
						if (current.MainActionKeyForControl(controlID))
						{
							Rect buttonRect = GUIUtility.GUIToScreenRect(position);
							Open(buttonRect, controlID, selected, definableType, hasFilter, filterFlags);
							current.Use();
						}
						break;
					case EventType.Repaint:
						style.Draw(position, selectedName, controlID);
						break;
				}

				return selected;
			}
		}

		IDefinableType _DefinableType;

		static Dictionary<System.Type, bool> s_HideTypes = new Dictionary<System.Type, bool>();

		static bool IsHideType(System.Type type)
		{
			bool isHideType = false;
			if (s_HideTypes.TryGetValue(type, out isHideType))
			{
				return isHideType;
			}

			for (System.Type current = type; current != null; current = current.BaseType)
			{
				HideTypeAttribute hideType = AttributeHelper.GetAttribute<HideTypeAttribute>(current);
				if (hideType != null)
				{
					if (type == current || hideType.forChildren)
					{
						isHideType = true;
						break;
					}
				}
			}

			s_HideTypes.Add(type, isHideType);

			return isHideType;
		}

		TreeViewValueItem<Type> CreateTypeTree(ClassList.TypeItem typeItem, string name)
		{
			using (new ProfilerScope("CreateTypeTree"))
			{
				Type type = (typeItem != null) ? typeItem.type : null;
				bool disable = false;
				bool expand = false;
				List<TreeViewItem> children = null;

				if (type != null)
				{
					if (_DefinableType != null)
					{
						disable |= !_DefinableType.IsDefinableType(type);
					}

					if (_HasFilter)
					{
						disable |= !IsValidType(type, ArborEditorCache.typeFilterMask);
					}
				}

				if (typeItem != null)
				{
					List<int> nestedTypes = typeItem.nestedTypeIndices;
					int nestedTypeCount = nestedTypes.Count;
					for (int nestedTypeIndex = 0; nestedTypeIndex < nestedTypeCount; nestedTypeIndex++)
					{
						ClassList.TypeItem nestedType = ClassList.GetType(nestedTypes[nestedTypeIndex]);

						if (IsHideType(nestedType.type))
						{
							continue;
						}

						TreeViewValueItem<Type> nestedTypeElement = CreateTypeTree(nestedType, nestedType.name);
						if (nestedTypeElement != null)
						{
							if (IsExpanded(nestedTypeElement))
							{
								expand = true;
							}
							if (children == null)
							{
								children = new List<TreeViewItem>();
							}
							children.Add(nestedTypeElement);
						}
					}
				}

				if (disable && (children == null || children.Count == 0))
				{
					return null;
				}

				int id = _UniqueIDGenerator.CreateID();
				TreeViewValueItem<Type> typeElement = new TreeViewValueItem<Type>(id, name, type, Icons.GetTypeIcon(type) as Texture2D);
				typeElement.disable = disable;
				SetExpanded(typeElement, expand);

				typeElement.AddChildren(children);

				if (selectedValue == type)
				{
					SetSelectedItem(typeElement, true);
					SetExpanded(typeElement, true);
				}

				return typeElement;
			}
		}

		protected override void OnCreateTree(TreeViewItem root)
		{
			using (new ProfilerScope("OnCreateTree"))
			{
				_TreeView.noneValueItem = CreateTypeTree(null, kNoneText);
				root.AddChild(_TreeView.noneValueItem);

				int namespaceCount = ClassList.namespaceCount;
				for (int namespaceIndex = 0; namespaceIndex < namespaceCount; namespaceIndex++)
				{
					ClassList.NamespaceItem namespaceItem = ClassList.GetNamespaceItem(namespaceIndex);

					List<TreeViewItem> children = null;
					bool expand = false;

					int typeCount = namespaceItem.typeIndices.Count;
					for (int typeIndex = 0; typeIndex < typeCount; typeIndex++)
					{
						ClassList.TypeItem typeItem = ClassList.GetType(namespaceItem.typeIndices[typeIndex]);

						if (IsHideType(typeItem.type))
						{
							continue;
						}

						TreeViewValueItem<Type> typeElemnt = CreateTypeTree(typeItem, typeItem.name);
						if (typeElemnt != null)
						{
							if (IsExpanded(typeElemnt))
							{
								expand = true;
							}
							if (children == null)
							{
								children = new List<TreeViewItem>();
							}
							children.Add(typeElemnt);
						}
					}

					if (children != null && children.Count > 0)
					{
						int id = _UniqueIDGenerator.CreateID();
						TreeViewItem namespaceGroup = new TreeViewItem(id, namespaceItem.name, Icons.namespaceIcon);
						SetExpanded(namespaceGroup, expand);

						namespaceGroup.AddChildren(children);

						root.AddChild(namespaceGroup);
					}
				}
			}
		}

		private bool _HasFilter;
		private bool _UseFilter;

		bool ITreeFilter.useFilter
		{
			get
			{
				return _UseFilter;
			}
		}

		bool ITreeFilter.openFilter
		{
			get
			{
				return ArborEditorCache.typeFilterEnable;
			}
			set
			{
				ArborEditorCache.typeFilterEnable = value;
			}
		}

		private sealed class FilterMenu
		{
			public TypeFilterFlags flags;
			public GUIContent content;
		};

		private static readonly FilterMenu[] s_FilterMenu =
		{
			new FilterMenu()
			{
				flags = TypeFilterFlags.SceneObject,
				content = new GUIContent("Scene"),
			},
			new FilterMenu()
			{
				flags = TypeFilterFlags.AssetObject,
				content = new GUIContent("Asset"),
			},
			new FilterMenu()
			{
				flags = TypeFilterFlags.Class,
				content = new GUIContent("Class"),
			},
			new FilterMenu()
			{
				flags = TypeFilterFlags.Struct,
				content = new GUIContent("Struct"),
			},
			new FilterMenu()
			{
				flags = TypeFilterFlags.Interface,
				content = new GUIContent("Interface"),
			},
			new FilterMenu()
			{
				flags = TypeFilterFlags.Enum,
				content = new GUIContent("Enum"),
			},
			new FilterMenu()
			{
				flags = TypeFilterFlags.Primitive,
				content = new GUIContent("Primitive"),
			},
			new FilterMenu()
			{
				flags = TypeFilterFlags.Delegate,
				content = new GUIContent("Delegate"),
			},
			new FilterMenu()
			{
				flags = TypeFilterFlags.Static,
				content = new GUIContent("Static"),
			},
		};

		VisualElement ITreeFilter.CreateFilterSettingsElement()
		{
			VisualElement element = new VisualElement()
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					flexWrap = Wrap.Wrap,
				},
			};

			for (int filterMenuIndex = 0; filterMenuIndex < s_FilterMenu.Length; filterMenuIndex++)
			{
				FilterMenu filterMenu = s_FilterMenu[filterMenuIndex];
				TypeFilterFlags flags = filterMenu.flags;
				if ((ArborEditorCache.typeFilterMask & flags) == 0)
				{
					continue;
				}

				var toggle = new Toggle()
				{
					text = filterMenu.content.text,
				};
				
				bool isFlag = (ArborEditorCache.typeFilterFlags & flags) != 0;
				toggle.SetValueWithoutNotify(isFlag);
				toggle.RegisterValueChangedCallback(e =>
				{
					if (e.newValue)
					{
						ArborEditorCache.typeFilterFlags |= flags;
					}
					else
					{
						ArborEditorCache.typeFilterFlags &= ~flags;
					}
					RebuildSearch();
				});

				element.Add(toggle);
			}

			return element;
		}

		static bool IsValidType(Type value, TypeFilterFlags typeFilterFlags)
		{
			if (value.IsClass && value.IsAbstract && value.IsSealed) // static
			{
				return (typeFilterFlags & TypeFilterFlags.Static) != 0;
			}
			else if (typeof(Component).IsAssignableFrom(value) || typeof(GameObject).IsAssignableFrom(value))
			{
				return (typeFilterFlags & TypeFilterFlags.SceneObject) != 0;
			}
			else if (typeof(UnityObject).IsAssignableFrom(value)) // only Asset Object : IsUnityObject && !IsComponent && !IsGameObject
			{
				return (typeFilterFlags & TypeFilterFlags.AssetObject) != 0;
			}
			else if (value.IsClass) // only normal class : IsClass && !IsUnityObject
			{
				if (typeof(System.Delegate).IsAssignableFrom(value))
				{
					return (typeFilterFlags & TypeFilterFlags.Delegate) != 0;
				}
				return (typeFilterFlags & TypeFilterFlags.Class) != 0;
			}
			else if (value.IsInterface)
			{
				return (typeFilterFlags & TypeFilterFlags.Interface) != 0;
			}
			else if (value.IsEnum)
			{
				return (typeFilterFlags & TypeFilterFlags.Enum) != 0;
			}
			else if (value.IsPrimitive)
			{
				return (typeFilterFlags & TypeFilterFlags.Primitive) != 0;
			}
			else if (value.IsValueType) // only struct : IsValueType && !IsEnum && !IsPrimitite
			{
				return (typeFilterFlags & TypeFilterFlags.Struct) != 0;
			}

			return false;
		}

		bool ITreeFilter.IsValid(TreeViewItem item)
		{
			var valueItem = item as TreeViewValueItem<Type>;
			if (valueItem == null)
			{
				return false;
			}
			return IsValidType(valueItem.value, ArborEditorCache.typeFilterFlags);
		}
	}
}
