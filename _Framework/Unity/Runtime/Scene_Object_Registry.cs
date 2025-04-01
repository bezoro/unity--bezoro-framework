using System.Collections.Generic;
using Bezoro.Framework.Unity.Utilities;
using Bezoro.Framework.Unity.Utilities.String;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public static class Scene_Object_Registry
	{
		private static readonly Dictionary<string, GameObject> _GUID_To_GameObject_Map = new();
		private static readonly object                         _Lock                   = new();

		public static void Register(GameObject gameObject)
		{
			// Assert.Is_Not_Null( gameObject );
			//
			// lock ( _Lock )
			// {
			// 	var guid = gameObject.Serializable_GUID.Value;
			// 	if ( !GUID_Utils.Check_GUID_Validity( guid ) ) return;
			// 	if ( Is_GUID_Registered( guid ) ) return;
			//
			// 	_GUID_To_GameObject_Map.Add( guid, gameObject.GameObject );
			// 	BDebug.Log( $"Registered Object: {gameObject.GameObject.name} with GUID: {guid}" );
			// }
		}

		public static void Unregister(string guid)
		{
			lock ( _Lock )
			{
				if (!GUID_Utils.Check_GUID_Validity(guid)) return;
				if (!Is_GUID_Registered(guid)) return;

				_GUID_To_GameObject_Map.Remove(guid);
				BDebug.Log($"Unregistered GUID: {guid.Bold()}");
			}
		}

		/// <summary>
		///     Retrieves the object associated with the given GUID.
		/// </summary>
		/// <param name="guid">The GUID string.</param>
		/// <returns>The associated GameObject if found; otherwise, Null.</returns>
		public static GameObject Get_Object_By_GUID(string guid)
		{
			lock ( _Lock )
			{
				if (!GUID_Utils.Check_GUID_Validity(guid))
				{
					BDebug.Log_Warning(
						$"Couldn't find the GameObject by its GUID because the GUID ({guid}) is not valid."
					);

					return null;
				}

				var go = _GUID_To_GameObject_Map.GetValueOrDefault(guid);
				BDebug.Log_Success($"Found the GameObject {go.name} by its GUID: {guid}");
				return go;
			}
		}

		public static bool Is_GUID_Registered(string guid)
		{
			lock ( _Lock )
			{
				if (!GUID_Utils.Check_GUID_Validity(guid)) return false;

				if (!_GUID_To_GameObject_Map.ContainsKey(guid))
				{
					BDebug.Log($"Serializable_GUID {guid} is not registered.");
					return false;
				}

				BDebug.Log($"Serializable_GUID {guid} is registered.");
				return true;
			}
		}

		// public static bool Is_GUID_Registered_With_Different_Object( GUID_GameObject_Pair guidGameObjectPair )
		// {
		// 	lock ( _Lock )
		// 	{
		// 		var guid        = guidGameObjectPair.Serializable_GUID.Value;
		// 		var game_object = guidGameObjectPair.GameObject;
		//
		// 		if ( !GUID_Utils.Check_GUID_Validity( guid ) ) return false;
		// 		if ( !_GUID_To_GameObject_Map.TryGetValue( guid, out var registered_object ) ) return false;
		// 		if ( registered_object == null )
		// 		{
		// 			_GUID_To_GameObject_Map.Remove( guid );
		// 			BDebug.Log_Warning( $"GUID {guid} is registered but the object is Null." );
		// 			return false;
		// 		}
		//
		// 		var is_different_object = registered_object != game_object;
		// 		if ( !is_different_object )
		// 		{
		// 			BDebug.Log(
		// 				$"GUID {guid} is already registered with the same object: {registered_object.name}"
		// 			);
		//
		// 			return false;
		// 		}
		//
		// 		BDebug.Log( $"GUID {guid} is registered with a different object." );
		// 		return true;
		// 	}
		// }

#if UNITY_EDITOR
		/// <summary>
		///     Initializes the registry when Unity Editor enters or exits play mode.
		/// </summary>
		[InitializeOnLoadMethod]
		private static void Initialize()
		{
			EditorApplication.playModeStateChanged -= On_Play_Mode_State_Changed;
			EditorApplication.playModeStateChanged += On_Play_Mode_State_Changed;

			// Refresh_Registry();
		}

		/// <summary>
		///     Handles play mode state changes to scan for object UIDs.
		/// </summary>
		/// <param name="state">The state of the play mode.</param>
		private static void On_Play_Mode_State_Changed(PlayModeStateChange state)
		{
			switch ( state )
			{
				case PlayModeStateChange.EnteredPlayMode or PlayModeStateChange.ExitingEditMode:
					// Refresh_Registry();
					break;

				case PlayModeStateChange.ExitingPlayMode:
					EditorApplication.playModeStateChanged -= On_Play_Mode_State_Changed;
					break;
			}
		}

		// /// <summary>
		// ///     Scans the scene for objects with UIDs and registers them.
		// /// </summary>
		// private static void Refresh_Registry()
		// {
		// 	lock ( _Lock )
		// 	{
		// 		_GUID_To_GameObject_Map.Clear();
		//
		// 		foreach ( var object_uid in Resources.FindObjectsOfTypeAll<GameObject>()
		// 											 .SelectMany( go => go.GetComponents<I_Behaviour>() ) )
		// 		{
		// 			if ( EditorUtility.IsPersistent( object_uid.GameObject ) ) continue;
		//
		// 			Register( object_uid );
		// 		}
		// 	}
		// }
#endif
	}
}
