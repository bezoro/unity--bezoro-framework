using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bezoro.Core.Helpers
{
	public static class SceneHelpers
	{
		public static void GetAllRootGameObjectsNonAlloc(ref GameObject[] results)
		{
			if (SceneManager.sceneCount == 1)
				results = SceneManager.GetActiveScene().GetRootGameObjects();

			for (var i = 0; i < SceneManager.sceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				ArrayHelpers.Merge(scene.GetRootGameObjects(), ref results);
			}
		}
	}
}