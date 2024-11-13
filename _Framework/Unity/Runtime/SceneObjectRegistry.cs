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
			GUIDToObjectMap.Clear();
			foreach ( var objectUID in Resources.FindObjectsOfTypeAll<ObjectUID>() )
			{
				RegisterObject( objectUID );
			}
		}
#endif

		public static void RegisterObject( ObjectUID objectUID )
		{
			lock (Lock)
			{
				if ( IsValidObject( objectUID ) && !GUIDToObjectMap.ContainsKey( objectUID.UID ) )
				{
					GUIDToObjectMap[objectUID.UID] = objectUID.gameObject;
				}
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
				if ( IsValidObject( objectUID ) )
				{
					GUIDToObjectMap.Remove( objectUID.UID );
				}
			}
		}

		private static bool IsValidObject( ObjectUID objectUID ) =>
			objectUID != null
		 && !string.IsNullOrEmpty( objectUID.UID )
		 && !GUIDToObjectMap.ContainsKey( objectUID.UID );
	}
}
