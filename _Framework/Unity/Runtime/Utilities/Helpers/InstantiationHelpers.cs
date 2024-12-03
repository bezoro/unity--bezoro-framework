using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class InstantiationHelpers
	{
		public static async UniTask InstantiateAsync<T>(
			T prefab,
			Func<T, UniTask> initializeFunc,
			Transform parent = null,
			Vector3 position = default
		)
		where T : Object
		{
			var instance = Object.Instantiate(
				prefab,
				position,
				Quaternion.identity,
				parent );

			await initializeFunc( instance );
		}
	}
}
