using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Scene_Helpers
	{
		public static void GetAllRootGameObjectsNonAlloc(ref GameObject[] results)
		{
			if (UnityEngine.SceneManagement.SceneManager.sceneCount == 1)
				results = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

			for (var i = 0 ; i < UnityEngine.SceneManagement.SceneManager.sceneCount ; i++)
			{
				var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
				Core.ArrayHelpers.Merge(scene.GetRootGameObjects(), ref results);
			}
		}

		public static bool IsSceneAlreadyOpenByPath(string path, out Scene scene, out int index)
		{
			for (var i = 0 ; i < UnityEngine.SceneManagement.SceneManager.sceneCount ; i++)
			{
				var sc = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

				if (sc.path != path) continue;

				scene = sc;
				index = i;

				return true;
			}

			scene = default;
			index = -1;

			return false;
		}

		public static bool IsSceneAlreadyOpenByName(string name, out Scene scene, out int index)
		{
			for (var i = 0 ; i < UnityEngine.SceneManagement.SceneManager.sceneCount ; i++)
			{
				var sc = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

				if (sc.name != name) continue;

				scene = sc;
				index = i;

				return true;
			}

			scene = default;
			index = -1;

			return false;
		}
	}
}
