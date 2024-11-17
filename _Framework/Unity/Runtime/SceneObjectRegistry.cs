using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public static class SceneObjectRegistry
	{
		private static readonly Dictionary<string, GameObject> GUIDToObjectMap = new();
		private static readonly object Lock = new();

#if UNITY_EDITOR
		[InitializeOnLoadMethod]
		private static void Initialize()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private static void OnPlayModeStateChanged( PlayModeStateChange state )
		{
			switch ( state )
			{
				case PlayModeStateChange.EnteredPlayMode or PlayModeStateChange.ExitingEditMode:
					ScanForObjectUIDs();
					break;
				case PlayModeStateChange.ExitingPlayMode:
					EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
					break;
			}
		}

		private static void ScanForObjectUIDs()
		{
			lock (Lock)
			{
				GUIDToObjectMap.Clear();
				foreach ( var objectUID in Resources.FindObjectsOfTypeAll<ObjectUID>() )
				{
					RegisterObject( objectUID );
				}
			}
		}
#endif

		public static void RegisterObject( ObjectUID objectUID )
		{
			lock (Lock)
			{
				if ( IsValidUID( objectUID ) && !IsRegistered( objectUID ) )
				{
					GUIDToObjectMap[objectUID.UID] = objectUID.gameObject;
				}
			}
		}

		public static bool IsGUIDRegisteredWithDifferentObject( ObjectUID objectUID )
		{
			lock (Lock)
			{
				if ( !IsValidUID( objectUID ) ) { return false; }

				if ( !GUIDToObjectMap.TryGetValue( objectUID.UID, out var registeredObject ) )
				{
					return false;
				}

				if ( registeredObject == null )
				{
					GUIDToObjectMap.Remove( objectUID.UID );
					return false;
				}

				var isDifferentObject = registeredObject != objectUID.gameObject;
				if ( isDifferentObject )
				{
					Debug.Log(
						$"GUID {objectUID.UID} is already registered with a different object: {registeredObject.name}"
					);
				}

				return isDifferentObject;
			}
		}

		public static bool IsGUIDUnique( string guid )
		{
			lock (Lock)
			{
				return !GUIDToObjectMap.ContainsKey( guid );
			}
		}

		public static bool IsRegistered( ObjectUID objectUID )
		{
			lock (Lock)
			{
				return GUIDToObjectMap.ContainsKey( objectUID.UID );
			}
		}

		public static GameObject GetObjectByGUID( string guid )
		{
			lock (Lock)
			{
				if ( string.IsNullOrEmpty( guid ) )
				{
					return null;
				}

				var gameObject = GUIDToObjectMap.GetValueOrDefault( guid );

				return gameObject;
			}
		}

		public static void UnregisterObject( ObjectUID objectUID )
		{
			lock (Lock)
			{
				if ( IsValidUID( objectUID ) && IsRegistered( objectUID ) )
				{
					GUIDToObjectMap.Remove( objectUID.UID );
				}
			}
		}

		private static bool IsValidUID( ObjectUID objectUID ) =>
			objectUID != null
		 && !string.IsNullOrEmpty( objectUID.UID );
	}
}
