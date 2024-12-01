using System.Diagnostics;
using Bezoro.Framework.Unity.Attributes;
using Bezoro.Framework.Unity.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

		public static T Instance
		{
			get
			{
				Initialize();
				return _instance.serialized_instance;
			}
		}

		[Conditional( "UNITY_EDITOR" )]
		protected virtual void OnValidate()
		{
			Ensure_Single_Instance();
			_instance           = this;
			serialized_instance = this as T;
		}

		protected virtual void Awake()
		{
			_instance = this;
		}

		[Conditional( "UNITY_EDITOR" )]
		private void Ensure_Single_Instance()
		{
			var instances = Resources.FindObjectsOfTypeAll<T>();
			if ( instances.Length <= 1 ) { return; }
			BDebug.Log_Error( MULTIPLE_INSTANCES_ERROR );
			foreach ( var instance in instances )
			{
				if ( instance == this ) { continue; }
				BDebug.Log_Error( instance.name, instance );
				DestroyImmediate( instance, true );
			}
		}

		[Conditional( "UNITY_EDITOR" )]
		private static void Initialize()
		{
			if ( _instance != null ) { return; }

			lock (_lock)
			{
				var async_operation = Addressables.LoadAssetAsync<Scriptable_Singleton<T>>( nameof(T) );
				if ( async_operation.IsValid() )
				{
					_instance = async_operation.WaitForCompletion();
				}
				if ( _instance == null )
				{
					_instance = Resources.LoadAll<Scriptable_Singleton<T>>( "" )[0];
				}
				_instance.serialized_instance = _instance as T;
			}

			BDebug.Log( $"Singleton<{typeof(T).Name}> initialized." );
		}
	}
}
