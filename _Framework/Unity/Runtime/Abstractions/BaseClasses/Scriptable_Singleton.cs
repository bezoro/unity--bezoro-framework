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
		private const string _MULTIPLE_INSTANCES_DETECTED_ERROR =
			"Multiple instances of {0} detected. Destroying duplicate at {1}";

		private const string _MULTIPLE_INSTANCES_ERROR =
			"Found multiple instances of {0}. There should only be one.";
		private const string _NO_INSTANCE_FOUND_ERROR =
			"No instance of {0} found. Please ensure one exists.";

		private static Scriptable_Singleton<T> _Instance;

		private static readonly object _Lock = new();

		public static T Instance
		{
			get
			{
				if (_Instance == null)
					Initialize();

				return _Instance.serialized_Instance;
			}
		}

		protected virtual void Awake()
		{
			_Instance = this;
		}

		[Conditional("UNITY_EDITOR")]
		protected virtual void OnValidate()
		{
			Ensure_Single_Instance();
			_Instance           = this;
			serialized_Instance = this as T;
		}

		private static void Assign_Serialized_Instance()
		{
			_Instance.serialized_Instance = _Instance as T;
			BDebug.Log_Success($"Singleton<{typeof(T).Name}> initialized successfully.");
		}

		[Conditional("UNITY_EDITOR")]
		private void Ensure_Single_Instance()
		{
			var instances = Resources.FindObjectsOfTypeAll<T>();
			if (instances.Length <= 1) return;

			BDebug.Log_Error(_MULTIPLE_INSTANCES_ERROR);

			foreach ( var instance in instances )
			{
				if (instance == this) continue;
				BDebug.Log_Error(instance.name, instance);
				DestroyImmediate(instance, true);
			}
		}

		private static Scriptable_Singleton<T> Get_Instance()
		{
			// Try loading from Addressables
			var instance = Load_Instance_From_Addressables();

			// If loading from Addressables fails, fallback to Resources
			return instance ?? Load_Instance_From_Resources();
		}

		[Conditional("UNITY_EDITOR")]
		private static void Initialize()
		{
			BDebug.Log($"Initializing Singleton<{typeof(T).Name}>...");

			lock ( _Lock )
			{
				_Instance = Get_Instance();

				if (_Instance == null)
				{
					BDebug.Log_Error($"Failed to initialize Singleton<{typeof(T).Name}>. No valid instance found.");
					return;
				}

				Assign_Serialized_Instance();
			}
		}

		private static Scriptable_Singleton<T> Load_Instance_From_Addressables()
		{
			var async_operation = Addressables.LoadAssetAsync<Scriptable_Singleton<T>>(typeof(T).Name);

			if (async_operation.IsValid())
				return async_operation.WaitForCompletion();

			BDebug.Log_Warning(
				$"Addressables failed to load Singleton<{typeof(T).Name}>. Falling back to Resources.");

			return null;
		}

		private static Scriptable_Singleton<T> Load_Instance_From_Resources()
		{
			var resources = Resources.LoadAll<Scriptable_Singleton<T>>("");

			if (resources.Length == 0)
				return null;

			BDebug.Log_Warning($"Loaded Singleton<{typeof(T).Name}> from Resources as fallback.");
			return resources[0];
		}
	}
}
