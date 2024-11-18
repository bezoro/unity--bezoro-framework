using System;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	[Serializable]
	public class SceneObjectReference
	{
		[SerializeField] private string guid;

		public string GUID
		{
			get => guid;
			set
			{
				guid         = value;
				_sceneObject = string.IsNullOrEmpty( guid ) ? null : SceneObjectRegistry.GetObjectByGUID( guid );
			}
		}

		private GameObject _sceneObject;

		public GameObject Reference => Resolve();

		private GameObject Resolve()
		{
			if ( _sceneObject != null )
			{
				return _sceneObject;
			}

			_sceneObject = string.IsNullOrEmpty( guid ) ? null : SceneObjectRegistry.GetObjectByGUID( guid );

			return _sceneObject;
		}
	}
}
