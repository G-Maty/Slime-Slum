﻿//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;

namespace Arbor.Calculators
{
#if ARBOR_DOC_JA
	/// <summary>
	/// Vector3Intをフォーマットした文字列を返す。
	/// </summary>
#else
	/// <summary>
	/// Returns a formatted string of Vector3Int.
	/// </summary>
#endif
	[AddComponentMenu("")]
	[AddBehaviourMenu("Vector3Int/Vector3Int.ToString")]
	[BehaviourTitle("Vector3Int.ToString")]
	[BuiltInBehaviour]
	public sealed class Vector3IntToStringCalculator : Calculator
	{
		#region Serialize fields

		/// <summary>
		/// Vector3Int
		/// </summary>
		[SerializeField] private FlexibleVector3Int _Vector3Int = new FlexibleVector3Int();

#if ARBOR_DOC_JA
		/// <summary>
		/// フォーマット<br/>
		/// フォーマットの詳細については、次を参照してください。<a href="https://msdn.microsoft.com/ja-jp/library/dwhawy9k(v=vs.110).aspx" target="_blank">標準の数値書式指定文字列</a>、<a href="https://msdn.microsoft.com/ja-jp/library/0c899ak8(v=vs.110).aspx" target="_blank">カスタム数値書式指定文字列</a>
		/// </summary>
#else
		/// <summary>
		/// Format<br/>
		/// For more information about numeric format specifiers, see <a href="https://msdn.microsoft.com/en-us/library/dwhawy9k(v=vs.110).aspx" target="_blank">Standard Numeric Format Strings</a> and <a href="https://msdn.microsoft.com/en-us/library/0c899ak8(v=vs.110).aspx" target="_blank">Custom Numeric Format Strings</a>.
		/// </summary>
#endif
		[SerializeField] private FlexibleString _Format = new FlexibleString();

#if ARBOR_DOC_JA
		/// <summary>
		/// 結果出力
		/// </summary>
#else
		/// <summary>
		/// Output result
		/// </summary>
#endif
		[SerializeField] private OutputSlotString _Result = new OutputSlotString();

#endregion // Serialize fields

		public override void OnCalculate()
		{
			Vector3Int value = _Vector3Int.value;
			string format = _Format.value;
			string str = !string.IsNullOrEmpty(format) ? value.ToString(format) : value.ToString();
			_Result.SetValue(str);
		}
	}
}