using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public abstract class Scriptable_Singleton<T> : ScriptableObject
	where T : ScriptableObject
	{
		[SerializeField] private T serialized_instance;

		private static          Scriptable_Singleton<T> _instance;
		private static readonly object                  _lock = new();

		private const string MULTIPLE_INSTANCES_ERROR = "Found multiple instances of {0}. There should only be one.";
		private const string NO_INSTANCE_FOUND_ERROR  = "No instance of {0} found. Please ensure one exists.";
		private const string MULTIPLE_INSTANCES_DETECTED_ERROR =
			"Multiple instances of {0} detected. Destroying duplicate at {1}";

		public static T Instance => Get_Instance();

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			_instance           = this;
			serialized_instance = this as T;
		}
#endif

		protected virtual void OnEnable()
		{
			if ( _instance != null && _instance != this )
			{
				Log_Error( MULTIPLE_INSTANCES_DETECTED_ERROR, typeof(T), name );
			}
		}

		private static T Get_Instance()
		{
			Initialize();
			return _instance.serialized_instance;
		}

		private static void Initialize()
		{
			if ( _instance != null ) { return; }

			lock (_lock)
			{
				_instance = CreateInstance<T>() as Scriptable_Singleton<T>;
			}
			Debug.Log( $"Singleton<{typeof(T).Name}> initialized." );
		}

		private static void Log_Error( string message, params object[] args )
		{
			Debug.LogErrorFormat( message, args );
		}
	}
}
