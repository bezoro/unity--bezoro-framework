using Bezoro.Framework.Unity.Utilities;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
	{
		private static T _instance;
		private static readonly object _lock = new();
		private static bool _initialized;

		private const string MULTIPLE_INSTANCES_ERROR = "Found multiple instances of {0}. There should only be one.";
		private const string NO_INSTANCE_FOUND_ERROR = "No instance of {0} found. Please ensure one exists.";
		private const string MULTIPLE_INSTANCES_DETECTED_ERROR =
			"Multiple instances of {0} detected. Destroying duplicate at {1}";

		public static T Instance
		{
			get
			{
				if ( _instance == null ) { Initialize(); }
				return _instance;
			}
		}

		private static void Initialize()
		{
			lock (_lock)
			{
				if ( _initialized ) { return; }
				var instances = Resources.LoadAll<T>( "" );
				HandleInstances( instances );
				_initialized = true;
			}
		}

		private static void HandleInstances( T[] instances )
		{
			if ( instances.Length > 1 )
			{
				BDebug.LogError( string.Format( MULTIPLE_INSTANCES_ERROR, typeof(T) ) );
				_instance = instances[0];
			}
			else if ( instances.Length == 1 ) { _instance = instances[0]; }
			else { BDebug.LogError( string.Format( NO_INSTANCE_FOUND_ERROR, typeof(T) ) ); }
		}

		protected virtual void OnEnable()
		{
			lock (_lock)
			{
				if ( _instance != null && _instance != this )
				{
					BDebug.LogError( string.Format( MULTIPLE_INSTANCES_DETECTED_ERROR, typeof(T), name ) );
					DestroyImmediate( this, true );
					return;
				}

				if ( _initialized ) { return; }
				_instance    = this as T;
				_initialized = true;
			}
		}
	}
}
