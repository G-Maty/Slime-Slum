//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

namespace Arbor
{
#if ARBOR_DOC_JA
	/// <summary>
	/// Stateの挙動を定義するクラス。継承して利用する。
	/// </summary>
	/// <remarks>
	/// 使用可能な属性 : <br/>
	/// <list type="bullet">
	/// <item><description><see cref="AddBehaviourMenu" /></description></item>
	/// <item><description><see cref="HideBehaviour" /></description></item>
	/// <item><description><see cref="BehaviourTitle" /></description></item>
	/// <item><description><see cref="BehaviourHelp" /></description></item>
	/// </list>
	/// </remarks>
#else
	/// <summary>
	/// Class that defines the behavior of the State. Inherited and to use.
	/// </summary>
	/// <remarks>
	/// Available Attributes : <br/>
	/// <list type="bullet">
	/// <item><description><see cref="AddBehaviourMenu" /></description></item>
	/// <item><description><see cref="HideBehaviour" /></description></item>
	/// <item><description><see cref="BehaviourTitle" /></description></item>
	/// <item><description><see cref="BehaviourHelp" /></description></item>
	/// </list>
	/// </remarks>
#endif
	[AddComponentMenu("")]
	[Internal.DocumentManual("/manual/scripting/calculator.md")]
	public class Calculator : NodeBehaviour
	{
		#region Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// 再演算モード
		/// </summary>
#else
		/// <summary>
		/// Recalculate mode
		/// </summary>
#endif
		[HideInInspector]
		public RecalculateMode recalculateMode = RecalculateMode.Dirty;

		#endregion // Serialize fields

#if ARBOR_DOC_JA
		/// <summary>
		/// CalculatorNodeを取得。
		/// </summary>
#else
		/// <summary>
		/// Get the CalculatorNode.
		/// </summary>
#endif
		public CalculatorNode calculatorNode
		{
			get
			{
				return node as CalculatorNode;
			}
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// CalculatorIDを取得。
		/// </summary>
#else
		/// <summary>
		/// Gets the Calculator identifier.
		/// </summary>
#endif
		[System.Obsolete("use nodeID")]
		public int calculatorID
		{
			get
			{
				return nodeID;
			}
		}

		bool _IsDirty = true;

		private bool _HasRandomFlexiblePrimitive = false;

		private int _FrameCount = -1;

#if ARBOR_DOC_JA
		/// <summary>
		/// 変更されているかどうか。
		/// </summary>
#else
		/// <summary>
		/// Whether it has been changed.
		/// </summary>
#endif
		public bool isDirty
		{
			get
			{
				switch (recalculateMode)
				{
					case RecalculateMode.Dirty:
						break;
					case RecalculateMode.Frame:
						if (Time.frameCount != _FrameCount)
						{
							return true;
						}
						break;
					case RecalculateMode.Scope:
						if (!CalculateScope.IsCalculated(this))
						{
							return true;
						}
						break;
					case RecalculateMode.Always:
						return true;
				}

				if (CallCheckDirty() || _IsDirty || _HasRandomFlexiblePrimitive)
				{
					return true;
				}

				int slotCount = dataSlotCount;
				for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
				{
					InputSlotBase s = GetDataSlot(slotIndex) as InputSlotBase;
					if (s == null)
					{
						continue;
					}

					DataBranch branch = s.branch;
					if (branch == null)
					{
						continue;
					}

					if (branch.IsDirty())
					{
						return true;
					}
				}

				return false;
			}
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 生成時に呼ばれるメソッド.
		/// </summary>
#else
		/// <summary>
		/// Raises the created event.
		/// </summary>
#endif
		protected override void OnCreated()
		{
			base.OnCreated();

			recalculateMode = RecalculateMode.Frame;
		}

		bool CallCheckDirty()
		{
			try
			{
#if ARBOR_PROFILER && (DEVELOPMENT_BUILD || UNITY_EDITOR)
				using (new ProfilerScope(GetProfilerName("OnCheckDirty()")))
#endif
				{
					return OnCheckDirty();
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogException(ex);
				return false;
			}
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 変更されているか判定する際に呼ばれる。
		/// </summary>
		/// <returns>変更されている場合はtrue、そうでなければfalseを返す。</returns>
#else
		/// <summary>
		/// It is called when judging whether it has been changed.
		/// </summary>
		/// <returns>Returns true if it has been changed, false otherwise.</returns>
#endif
		public virtual bool OnCheckDirty()
		{
			return false;
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// OnCalculateを呼んでほしい場合に呼び出す。
		/// </summary>
#else
		/// <summary>
		/// Call if you want call OnCalculate.
		/// </summary>
#endif
		public void SetDirty()
		{
			_IsDirty = true;
		}

		private static List<ParameterReference> s_FindParameterReferences = null;

		private List<Parameter> _CachedParameters = null;
		private List<Parameter> cachedParameters
		{
			get
			{
				if (_CachedParameters == null)
				{
					if (s_FindParameterReferences == null)
					{
						s_FindParameterReferences = new List<ParameterReference>();
					}
					else
					{
						s_FindParameterReferences.Clear();
					}
					EachField<ParameterReference>.Find(this, GetType(), s_FindParameterReferences);

					_CachedParameters = new List<Parameter>();
					for (int i = 0; i < s_FindParameterReferences.Count; i++)
					{
						ParameterReference r = s_FindParameterReferences[i];
						Parameter parameter = r.parameter;
						if (parameter != null)
						{
							_CachedParameters.Add(parameter);
						}
					}
				}
				return _CachedParameters;
			}
		}

		private bool _IsSettedOnChanged;

		private static List<IFlexibleField> s_FindFlexibleFields = null;

		void SetOnChanged()
		{
			if (_IsSettedOnChanged)
			{
				ReleaseOnChanged();
				_CachedParameters.Clear();
				_CachedParameters = null;
				_IsSettedOnChanged = false;
			}

			List<Parameter> parameters = cachedParameters;
			for (int i = 0; i < parameters.Count; i++)
			{
				Parameter parameter = parameters[i];
				parameter.onChanged += ParameterOnChanged;
			}

			if (s_FindFlexibleFields == null)
			{
				s_FindFlexibleFields = new List<IFlexibleField>();
			}
			else
			{
				s_FindFlexibleFields.Clear();
			}

			EachField<IFlexibleField>.Find(this, GetType(), s_FindFlexibleFields);

			_HasRandomFlexiblePrimitive = false;
			for (int i = 0; i < s_FindFlexibleFields.Count; i++)
			{
				var f = s_FindFlexibleFields[i];
				FlexiblePrimitiveBase flexiblePrimitive = f as FlexiblePrimitiveBase;
				if (flexiblePrimitive != null && flexiblePrimitive.type == FlexiblePrimitiveType.Random)
				{
					_HasRandomFlexiblePrimitive = true;
					return;
				}
			}

			_IsSettedOnChanged = true;
		}

		void ReleaseOnChanged()
		{
			if (_IsSettedOnChanged)
			{
				List<Parameter> parameters = cachedParameters;
				for (int i = 0; i < parameters.Count; i++)
				{
					Parameter parameter = parameters[i];
					parameter.onChanged -= ParameterOnChanged;
				}
				_IsSettedOnChanged = false;
			}
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// この関数はスクリプトのインスタンスがロードされたときに呼び出される。
		/// </summary>
#else
		/// <summary>
		/// This function is called when the script instance is being loaded.
		/// </summary>
#endif
		protected virtual void Awake()
		{
			SetOnChanged();
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// この関数はMonoBehaviourが破棄されるときに呼び出される。
		/// </summary>
#else
		/// <summary>
		/// This function is called when MonoBehaivour will be destroyed.
		/// </summary>
#endif
		protected virtual void OnDestroy()
		{
			ReleaseOnChanged();
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// この関数はスクリプトがロードされた時やインスペクターの値が変更されたときに呼び出されます（この呼出はエディター上のみ）
		/// </summary>
#else
		/// <summary>
		/// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only).
		/// </summary>
#endif
		protected override void OnValidate()
		{
			base.OnValidate();

			if (Application.isPlaying)
			{
				if (isActiveAndEnabled && _IsSettedOnChanged)
				{
					SetOnChanged();
				}

				SetDirty();
			}
		}

		void ParameterOnChanged(Parameter parameter)
		{
			SetDirty();
		}

		internal static Calculator CreateCalculator(Node node, System.Type type)
		{
			System.Type classType = typeof(Calculator);
			if (type != classType && !TypeUtility.IsSubclassOf(type, classType))
			{
				throw new System.ArgumentException("The type `" + type.Name + "' must be convertible to `Calculator' in order to use it as parameter `type'", "type");
			}

			return CreateNodeBehaviour(node, type) as Calculator;
		}

		void CallCalculate()
		{
			UpdateDataLink(DataLinkUpdateTiming.Execute);

			try
			{
#if ARBOR_PROFILER && (DEVELOPMENT_BUILD || UNITY_EDITOR)
				using (new ProfilerScope(GetProfilerName("OnCalculate()")))
#endif
				{
					OnCalculate();
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogException(ex);
			}

			_FrameCount = Time.frameCount;
			if (recalculateMode == RecalculateMode.Scope)
			{
				CalculateScope.SetCalculated(this);
			}
			
			_IsDirty = false;
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 必要であれば演算する。
		/// </summary>
#else
		/// <summary>
		/// It is calculated, if necessary.
		/// </summary>
#endif
		public void Calculate()
		{
			if (isDirty)
			{
				CallCalculate();
			}
		}

#if ARBOR_DOC_JA
		/// <summary>
		/// 演算される際に呼ばれる。
		/// </summary>
#else
		/// <summary>
		/// It called when it is calculated .
		/// </summary>
#endif
		public virtual void OnCalculate()
		{
		}
	}
}
