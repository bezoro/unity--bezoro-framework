using System.Collections.Generic;
using Bezoro.Framework.Unity.Utilities;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	/// <summary>
	///    Static class for registering and managing scene objects via their GUIDs.
	/// </summary>
	public static class Scene_Object_Registry
	{
		private static readonly Dictionary<string, GameObject> GUID_To_Object_Map = new();
		private static readonly object                         Lock_Obj           = new();

#if UNITY_EDITOR
		/// <summary>
		///    Initializes the registry when Unity Editor enters or exits play mode.
		/// </summary>
		[InitializeOnLoadMethod]
		private static void Initialize()
		{
			EditorApplication.playModeStateChanged -= On_Play_Mode_State_Changed;
			EditorApplication.playModeStateChanged += On_Play_Mode_State_Changed;
			Refresh_Registry();
		}

		/// <summary>
		///    Handles play mode state changes to scan for object UIDs.
		/// </summary>
		/// <param name="state">The state of the play mode.</param>
		private static void On_Play_Mode_State_Changed( PlayModeStateChange state )
		{
			switch ( state )
			{
				case PlayModeStateChange.EnteredPlayMode or PlayModeStateChange.ExitingEditMode:
					Refresh_Registry();
					break;
				case PlayModeStateChange.ExitingPlayMode:
					EditorApplication.playModeStateChanged -= On_Play_Mode_State_Changed;
					break;
			}
		}

		/// <summary>
		///    Scans the scene for objects with UIDs and registers them.
		/// </summary>
		private static void Refresh_Registry()
		{
			lock (Lock_Obj)
			{
				GUID_To_Object_Map.Clear();
				foreach ( var object_uid in Resources.FindObjectsOfTypeAll<Object_UID>() )
				{
					if ( EditorUtility.IsPersistent( object_uid ) ) { continue; }
					Register_Object_By_UID( object_uid );
				}
			}
		}
#endif

		/// <summary>
		///    Registers an object with a valid UID.
		/// </summary>
		/// <param name="object_uid">The object UID component.</param>
		public static void Register_Object_By_UID( Object_UID object_uid )
		{
			lock (Lock_Obj)
			{
				if ( !Is_Object_UID_Valid( object_uid ) ) { return; }
				if ( Is_Object_Registered( object_uid ) ) { return; }
				GUID_To_Object_Map[object_uid.UID] = object_uid.gameObject;
				Debug.Log( $"Registered Object: {object_uid.gameObject.name} with GUID: {object_uid.UID}" );
			}
		}

		/// <summary>
		///    Checks if the given Object_UID's GUID is registered with a different object.
		/// </summary>
		/// <param name="object_uid">The object UID component.</param>
		/// <returns>True if registered with a different object; otherwise, False.</returns>
		public static bool Is_GUID_Registered_With_Different_Object( Object_UID object_uid )
		{
			lock (Lock_Obj)
			{
				if ( !Is_Object_UID_Valid( object_uid ) ) { return false; }
				if ( !GUID_To_Object_Map.TryGetValue( object_uid.UID, out var registered_object ) ) { return false; }

				if ( registered_object == null )
				{
					GUID_To_Object_Map.Remove( object_uid.UID );
					return false;
				}

				var isDifferentObject = registered_object != object_uid.gameObject;
				if ( isDifferentObject )
				{
					BDebug.Log_Warning(
						$"GUID {object_uid.UID} is already registered with a different object: {registered_object.name}" );
				}

				return isDifferentObject;
			}
		}

		/// <summary>
		///    Checks if the object with the given UID is registered.
		/// </summary>
		/// <param name="object_uid">The object UID component.</param>
		/// <returns>True if registered; otherwise, False.</returns>
		public static bool Is_Object_Registered( Object_UID object_uid )
		{
			lock (Lock_Obj)
			{
				if ( !Is_Object_UID_Valid( object_uid ) ) { return false; }
				if ( GUID_To_Object_Map.ContainsKey( object_uid.UID ) ) { return true; }
				BDebug.Log( $"Object_UID {object_uid.UID} is not registered." );
				return false;
			}
		}

		/// <summary>
		///    Retrieves the object associated with the given GUID.
		/// </summary>
		/// <param name="guid">The GUID string.</param>
		/// <returns>The associated GameObject if found; otherwise, Null.</returns>
		public static GameObject Get_Object_By_GUID( string guid )
		{
			if ( string.IsNullOrEmpty( guid ) )
			{
				BDebug.Log_Error( "GUID is empty" );
				return null;
			}

			lock (Lock_Obj) { return GUID_To_Object_Map.GetValueOrDefault( guid ); }
		}

		/// <summary>
		///    Unregisters the object with the given UID component.
		/// </summary>
		/// <param name="object_uid">The object UID component.</param>
		public static void Unregister_Object_By_UID( Object_UID object_uid )
		{
			lock (Lock_Obj)
			{
				if ( !Is_Object_UID_Valid( object_uid ) ) { return; }
				if ( !Is_Object_Registered( object_uid ) ) { return; }
				GUID_To_Object_Map.Remove( object_uid.UID );
				BDebug.Log( $"Unregistered Object: {object_uid.gameObject.name} with GUID: {object_uid.UID}" );
			}
		}

		/// <summary>
		///    Validates the UID of the given object.
		/// </summary>
		/// <param name="object_uid">The object UID component.</param>
		/// <returns>True if valid; otherwise, False.</returns>
		private static bool Is_Object_UID_Valid( Object_UID object_uid )
		{
			if ( object_uid != null && !string.IsNullOrEmpty( object_uid.UID ) ) { return true; }
			BDebug.Log_Error( "Invalid Object_UID" );
			return false;
		}
	}
}
