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
		[SerializeField] private T serialized_Instance;

		private const string _MULTIPLE_INSTANCES_ERROR =
			"Found multiple instances of {0}. There should only be one.";
		private const string _NO_INSTANCE_FOUND_ERROR =
			"No instance of {0} found. Please ensure one exists.";
		private const string _MULTIPLE_INSTANCES_DETECTED_ERROR =
			"Multiple instances of {0} detected. Destroying duplicate at {1}";

		private static readonly object _lock = new();

		private static Scriptable_Singleton<T> _Instance;

		public static T Instance
		{
			get
			{
				Initialize();
				return _Instance.serialized_Instance;
			}
		}

		protected virtual void Awake()
		{
			_Instance = this;
		}

		[Conditional( "UNITY_EDITOR" )]
		protected virtual void OnValidate()
		{
			Ensure_Single_Instance();
			_Instance           = this;
			serialized_Instance = this as T;
		}

		[Conditional( "UNITY_EDITOR" )]
		private void Ensure_Single_Instance()
		{
			var instances = Resources.FindObjectsOfTypeAll<T>();
			if ( instances.Length <= 1 ) return;

			BDebug.Log_Error( _MULTIPLE_INSTANCES_ERROR );
			foreach ( var instance in instances )
			{
				if ( instance == this ) continue;
				BDebug.Log_Error( instance.name, instance );
				DestroyImmediate( instance, true );
			}
		}

		[Conditional( "UNITY_EDITOR" )]
		private static void Initialize()
		{
			if ( _Instance != null ) return;

			lock ( _lock )
			{
				var async_operation = Addressables.LoadAssetAsync<Scriptable_Singleton<T>>( nameof( T ) );

				if ( async_operation.IsValid() )
					_Instance = async_operation.WaitForCompletion();

				if ( _Instance == null )
					_Instance = Resources.LoadAll<Scriptable_Singleton<T>>( "" )[ 0 ];

				_Instance.serialized_Instance = _Instance as T;
			}

			BDebug.Log( $"Singleton<{typeof( T ).Name}> initialized." );
		}
	}
}
