//-----------------------------------------------------
//            Arbor 3: FSM & BT Graph Editor
//		  Copyright(c) 2014-2021 caitsithware
//-----------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

namespace ArborEditor
{
	[System.Serializable]
	internal sealed class TransformInfoCache
	{
		public int id = 0;
		public Vector3 position = Vector3.zero;
		public Vector3 scale = Vector3.one;

		public override string ToString()
		{
			return string.Format("{0} : {1} , {2}", id, position, scale);
		}
	}

	[System.Serializable]
	internal sealed class TransformCache : ISerializationCallbackReceiver
	{
		[SerializeField]
		private List<TransformInfoCache> _Transforms = new List<TransformInfoCache>();

		private Dictionary<int, TransformInfoCache> _DicTransforms = new Dictionary<int, TransformInfoCache>();

		public bool HasTransform(int id)
		{
			return _DicTransforms.ContainsKey(id);
		}

		private TransformInfoCache GetTransform(int id)
		{
			TransformInfoCache transform = null;
			if (_DicTransforms.TryGetValue(id, out transform))
			{
				return transform;
			}
			return null;
		}

		public Vector3 GetPosition(int id)
		{
			TransformInfoCache transform = GetTransform(id);
			if (transform != null)
			{
				return transform.position;
			}
			return Vector3.zero;
		}

		public Vector3 GetScale(int id)
		{
			TransformInfoCache transform = GetTransform(id);
			if (transform != null)
			{
				return transform.scale;
			}
			return Vector3.one;
		}

		private TransformInfoCache GetOrCreateTransform(int id)
		{
			TransformInfoCache transform = GetTransform(id);
			if (transform == null)
			{
				transform = new TransformInfoCache();
				transform.id = id;
				_Transforms.Add(transform);
				_DicTransforms.Add(id, transform);
			}

			return transform;
		}

		public void SetPosition(int id, Vector3 position)
		{
			TransformInfoCache transform = GetOrCreateTransform(id);
			if (transform != null)
			{
				transform.position = position;
			}
		}

		public void SetScale(int id, Vector3 scale)
		{
			TransformInfoCache transform = GetOrCreateTransform(id);
			if (transform != null)
			{
				transform.scale = scale;
			}
		}

		public void Clear()
		{
			_Transforms.Clear();
			_DicTransforms.Clear();
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (_DicTransforms != null)
			{
				_DicTransforms.Clear();
			}
			else
			{
				_DicTransforms = new Dictionary<int, TransformInfoCache>();
			}

			for (int transformIndex = 0; transformIndex < _Transforms.Count; transformIndex++)
			{
				TransformInfoCache transform = _Transforms[transformIndex];
				if (transform.id == 0)
				{
					continue;
				}
				_DicTransforms.Add(transform.id, transform);
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			_Transforms.Clear();
			foreach (var pair in _DicTransforms)
			{
				_Transforms.Add(pair.Value);
			}
		}
	}
}