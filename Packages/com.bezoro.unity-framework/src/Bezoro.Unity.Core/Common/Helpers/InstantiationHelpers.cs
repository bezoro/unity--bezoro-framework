using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Wraps common GameObject instantiation patterns.
/// </summary>
public static class InstantiationHelpers
{
	public static async UniTask InstantiateAsync<T>(
		T                prefab,
		Func<T, UniTask> initializeFunc,
		Transform?       parent   = null,
		Vector3          position = default
	)
		where T : Object
	{
		var instance = Object.Instantiate(
			prefab,
			position,
			Quaternion.identity,
			parent
		);

		await initializeFunc(
			instance
		);
	}
}
