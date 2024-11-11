using Bezoro.Framework.Unity.Utilities.Extensions;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public abstract class ScriptableSingleton<T> : ScriptableObject
	where T : ScriptableObject
	{
		private static T _instance;

		public static T Instance
		{
			get
			{
				if ( !_instance.EqualsNull() ) { return _instance; }

				var instances = Resources.LoadAll<T>( "" );
				if ( instances.Length > 1 )
				{
					Debug.LogError( $"Found multiple instances of {typeof(T)}. There should only be one." );
					_instance = instances[0];
				}
				else if ( instances.Length == 1 )
				{
					_instance = instances[0];
				}

				return _instance;
			}
		}

		protected virtual void OnEnable()
		{
			if ( _instance != null && _instance != this )
			{
				Debug.LogError( $"Multiple instances of {typeof(T)} detected. Destroying duplicate at {name}" );
				DestroyImmediate( this );

				return;
			}

			_instance = this as T;
		}
	}
}
