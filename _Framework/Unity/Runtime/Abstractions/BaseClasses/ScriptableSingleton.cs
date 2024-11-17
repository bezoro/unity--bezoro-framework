using Bezoro.Framework.Unity.Utilities;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
	{
		private static T _instance;
		private static readonly object _lock = new();
		private static bool _initialized;

		public static T Instance
		{
			get
			{
				if ( _instance == null )
				{
					Initialize();
				}

				return _instance;
			}
		}

		private static void Initialize()
		{
			lock (_lock)
			{
				if ( _initialized )
				{
					return;
				}

				var instances = Resources.LoadAll<T>( "" );
				if ( instances.Length > 1 )
				{
					BDebug.LogError( $"Found multiple instances of {typeof(T)}. There should only be one." );
					_instance = instances[0];
				}
				else if ( instances.Length == 1 )
				{
					_instance = instances[0];
				}
				else
				{
					BDebug.LogError( $"No instance of {typeof(T)} found. Please ensure one exists." );
				}

				_initialized = true;
			}
		}

		protected virtual void OnEnable()
		{
			if ( _instance != null && _instance != this )
			{
				BDebug.LogError( $"Multiple instances of {typeof(T)} detected. Destroying duplicate at {name}" );
				DestroyImmediate( this );

				return;
			}

			_instance    = this as T;
			_initialized = true;
		}
	}
}
