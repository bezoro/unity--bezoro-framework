using System.Diagnostics;
using System.Linq;
using Bezoro.Framework.Unity.Attributes;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Bezoro.Framework.Unity
{
	public abstract class Scriptable_Singleton<T> : ScriptableObject
	where T : ScriptableObject
	{
		[ReadOnly]
		[SerializeField] private T serialized_instance;

		private static          Scriptable_Singleton<T> _instance;
		private static readonly object                  _lock = new();

		private const string MULTIPLE_INSTANCES_ERROR = "Found multiple instances of {0}. There should only be one.";
		private const string NO_INSTANCE_FOUND_ERROR  = "No instance of {0} found. Please ensure one exists.";
		private const string MULTIPLE_INSTANCES_DETECTED_ERROR =
			"Multiple instances of {0} detected. Destroying duplicate at {1}";

		public static T Instance => Get_Instance();

		[Conditional( "UNITY_EDITOR" )]
		protected virtual void OnValidate()
		{
			Ensure_Single_Instance();
			_instance           = this;
			serialized_instance = this as T;
		}

		private void Ensure_Single_Instance()
		{
			var instances = Resources.FindObjectsOfTypeAll<T>();
			if ( instances.Length <= 1 ) { return; }
			Debug.LogError( MULTIPLE_INSTANCES_ERROR );
			foreach ( var instance in instances )
			{
				if ( instance == this ) { continue; }
				Debug.LogError( instance.name, instance );
				DestroyImmediate( instance, true );
			}
		}

		protected virtual void Awake()
		{
			// _instance = this;
			// serialized_instance = this as T;
			Debug.Log( $"Singleton<{typeof(T).Name}> awake." );
		}

		protected virtual void OnEnable()
		{
			Debug.Log( $"Singleton<{typeof(T).Name}> enabled." );
		}

		private static T Get_Instance()
		{
			Initialize();
			return _instance.serialized_instance;
		}

		[Conditional( "UNITY_EDITOR" )]
		private static void Initialize()
		{
			if ( _instance != null ) { return; }

			lock (_lock)
			{
				_instance                     = Resources.LoadAll<Scriptable_Singleton<T>>( "" ).FirstOrDefault();
				_instance.serialized_instance = _instance as T;
			}

			Debug.Log( $"Singleton<{typeof(T).Name}> initialized." );
		}
	}
}
