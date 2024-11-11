using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bezoro.Framework.Unity.Utilities.Helpers
{
	public static class SceneHelpers
	{
		public static void GetAllRootGameObjectsNonAlloc(ref GameObject[] results)
		{
			if (SceneManager.sceneCount == 1)
			{
				results = SceneManager.GetActiveScene().GetRootGameObjects();
			}

			for ( var i = 0 ; i < SceneManager.sceneCount ; i++ )
			{
				var scene = SceneManager.GetSceneAt(i);
				ArrayHelpers.Merge(scene.GetRootGameObjects(), ref results);
			}
		}

		public static bool IsSceneAlreadyOpenByPath(string path, out Scene scene, out int index)
		{
			for ( var i = 0 ; i < SceneManager.sceneCount ; i++ )
			{
				var sc = SceneManager.GetSceneAt(i);
				if (sc.path != path)
				{
					continue;
				}

				scene = sc;
				index = i;
				return true;
			}

			scene = default( Scene );
			index = -1;
			return false;
		}

		public static bool IsSceneAlreadyOpenByName(string name, out Scene scene, out int index)
		{
			for ( var i = 0 ; i < SceneManager.sceneCount ; i++ )
			{
				var sc = SceneManager.GetSceneAt(i);
				if (sc.name != name)
				{
					continue;
				}

				scene = sc;
				index = i;
				return true;
			}

			scene = default( Scene );
			index = -1;
			return false;
		}
	}
}
