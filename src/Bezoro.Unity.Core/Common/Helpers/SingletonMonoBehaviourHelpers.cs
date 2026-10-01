using UnityEngine;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Finds or creates singleton MonoBehaviour instances in the scene.
/// </summary>
public static class SingletonMonoBehaviourHelpers
{
	public static T CreateSingleton<T>()
		where T : Component
	{
		var gameObject = new GameObject(
			$"[{typeof(T).Name}]"
		);

		var instance = gameObject.AddComponent<T>();
		instance.gameObject.hideFlags = HideFlags.NotEditable;
		Object.DontDestroyOnLoad(
			gameObject
		);

		return instance;
	}
}
