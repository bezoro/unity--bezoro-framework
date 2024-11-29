using System;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	[Serializable]
	public class Scene_Object_Reference
	{
		[SerializeField] private string guid;

		public string GUID
		{
			get => guid;
			set
			{
				guid          = value;
				_scene_object = string.IsNullOrEmpty( guid ) ? null : Scene_Object_Registry.Get_Object_By_GUID( guid );
			}
		}

		private GameObject _scene_object;

		public GameObject Reference => Resolve();

		private GameObject Resolve()
		{
			if ( _scene_object != null ) { return _scene_object; }
			_scene_object = string.IsNullOrEmpty( guid ) ? null : Scene_Object_Registry.Get_Object_By_GUID( guid );
			return _scene_object;
		}
	}
}
