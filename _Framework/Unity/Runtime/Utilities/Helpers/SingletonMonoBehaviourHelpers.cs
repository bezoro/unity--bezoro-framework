using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class SingletonMonoBehaviourHelpers
	{
		public static T CreateSingleton<T>()
		where T : Component
		{
			var gameObject = new GameObject( $"[{typeof(T).Name}]" );
			var instance = gameObject.AddComponent<T>();
			instance.gameObject.hideFlags = HideFlags.NotEditable;
			Object.DontDestroyOnLoad( gameObject );

			return instance;
		}
	}
}
