using System.Collections.Generic;
using Bezoro.Core.Extensions;
using Bezoro.Unity.Core.Common.Serialization;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Scenes;

/// <summary>
///     Maps persistent scene object GUIDs to their live GameObject instances.
/// </summary>
public static class SceneObjectRegistry
{
	private static readonly Dictionary<string, GameObject> _GUID_TO_GAME_OBJECT_MAP = new();

	public static bool IsGuidRegistered(string guid)
	{
		if (!GUIDUtils.Check_GUID_Validity(
				guid
			)) return false;

		if (_GUID_TO_GAME_OBJECT_MAP.TryGetValue(
				guid,
				out var go
			))
		{
			// Check if the registered object is still valid
			if (go == null)
				// Treat as not registered if the object is destroyed
				// Optionally, clean up here as well: _GuidToGameObjectMap.Remove(guid);
				return false;

			return true; // Valid object found for GUID
		}

		return false; // GUID not in dictionary
	}

	public static GameObject? Get_Object_By_GUID(string guid) =>
		GetObjectByGuid(guid);

	public static GameObject? GetObjectByGuid(string guid)
	{
		if (!GUIDUtils.Check_GUID_Validity(
				guid
			)) return null;

		if (_GUID_TO_GAME_OBJECT_MAP.TryGetValue(
				guid,
				out var go
			))
		{
			// IMPORTANT: Check if the GameObject was destroyed
			if (go == null)
			{
				Logger.LogWarning(
					$"GUID {guid.Bold()} found, but the associated GameObject was destroyed. Cleaning up entry."
				);

				_GUID_TO_GAME_OBJECT_MAP.Remove(
					guid
				);

				return null;
			}

			return go; // Found valid GameObject
		}

		return null; // Not found
	}

	public static void Register(GameObject gameObject, string guid)
	{
		if (gameObject == null)
		{
			Logger.LogWarning(
				"Cannot register null GameObject."
			);

			return;
		}

		if (!GUIDUtils.Check_GUID_Validity(
				guid
			))
		{
			Logger.LogWarning(
				$"Cannot register GameObject {gameObject.name.Bold()} with invalid GUID: {guid.Bold()}"
			);

			return;
		}

		if (_GUID_TO_GAME_OBJECT_MAP.TryGetValue(
				guid,
				out var existingObject
			))
		{
			// Handle cases where the GUID is already registered
			if (existingObject == gameObject)
			{
				Logger.Log(
					$"Object {gameObject.name.Bold()} with GUID {guid.Bold()} is already registered."
				);

				return;
			}

			if (existingObject == null)
			{
				Logger.LogWarning(
					$"GUID {guid.Bold()} was registered to a destroyed object. Re-registering with {gameObject.name.Bold()}."
				);

				_GUID_TO_GAME_OBJECT_MAP[guid] = gameObject;
			}
			else
			{
				Logger.LogError(
					$"Duplicate GUID detected! GUID {guid.Bold()} is already registered to GameObject {existingObject.name.Bold()}." +
					$" Cannot register {gameObject.name.Bold()}."
				);
			}
		}
		else
		{
			_GUID_TO_GAME_OBJECT_MAP.Add(
				guid,
				gameObject
			);

			Logger.Log(
				$"Registered Object: {gameObject.name.Bold()} with GUID: {guid.Bold()}"
			);
		}
	}

	public static void Unregister(string guid)
	{
		if (!GUIDUtils.Check_GUID_Validity(
				guid
			))
		{
			Logger.LogWarning(
				$"Cannot unregister invalid GUID: {guid.Bold()}"
			);

			return;
		}

		if (_GUID_TO_GAME_OBJECT_MAP.Remove(
				guid
			))
			Logger.Log(
				$"Unregistered GUID: {guid.Bold()}"
			);
		else
			Logger.Log(
				$"Attempted to unregister GUID {guid.Bold()} which was not registered."
			);
	}
}
