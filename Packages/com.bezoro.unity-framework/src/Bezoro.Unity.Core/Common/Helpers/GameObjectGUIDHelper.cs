using System;
using System.Collections.Generic;
using Bezoro.Logging.Types;
using Bezoro.Unity.Core.MonoBehaviours;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Reads or assigns persistent GUIDs for GameObjects.
/// </summary>
public static class GameObjectGUIDHelper
{
	public static string[] GetGUIDsFromGameObjects(ICollection<GameObject> gameObjects, string[]? outputArray = null)
	{
		if (gameObjects == null || gameObjects.Count == 0)
		{
			Logger.LogError(
				"GameObjects list is null or empty",
				contextObject: nameof(GameObjectGUIDHelper),
				category: LogCategory.Utilities
			);

			return Array.Empty<string>();
		}

		int      count      = gameObjects.Count;
		string[] uids       = outputArray != null && outputArray.Length >= count ? outputArray : new string[count];
		var      validCount = 0;

		foreach (var gameObject in gameObjects)
		{
			if (!gameObject) continue;

			if (gameObject.TryGetComponent(
					out SceneObjectIdentifier identifier
				))
			{
				string? guid = identifier.GUID;
				if (string.IsNullOrEmpty(guid)) continue;

				uids[validCount++] = guid;
			}
		}

		if (validCount < count)
			Array.Resize(
				ref uids,
				validCount
			);

		Logger.LogSuccess(
			$"Got {validCount} GUIDs from {count} GameObjects.",
			contextObject: nameof(GameObjectGUIDHelper),
			category:
			LogCategory.Utilities
		);

		return uids;
	}
}
