using System.Collections.Generic;
using Bezoro.Unity.Core.AsyncLoading;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Editor.Build
{
	/// <summary>
	///     Build-time scene processor that prepares every <see cref="AsyncGameObject" />
	///     before Unity serializes a scene into a player build.
	/// </summary>
	public sealed class AsyncGameObjectBuildProcessor : IProcessSceneWithReport
	{
		private const int CALLBACK_ORDER = int.MinValue;

		private readonly List<AsyncGameObject> _asyncGameObjects = new();

		/// <summary>
		///     Runs before other scene processors so async-object build state is ready
		///     for later processors.
		/// </summary>
		public int callbackOrder => CALLBACK_ORDER;

		/// <summary>
		///     Finds async game objects in a valid build scene and invokes their
		///     editor build preparation hook. Play mode processing is ignored to avoid
		///     mutating objects while the editor is simulating.
		/// </summary>
		public void OnProcessScene(Scene scene, BuildReport report)
		{
			if (UnityEngine.Application.isPlaying || !scene.IsValid())
				return;

			var preparedCount = 0;

			foreach (var root in scene.GetRootGameObjects())
			{
				_asyncGameObjects.Clear();
				root.GetComponentsInChildren(
					true,
					_asyncGameObjects
				);

				foreach (var asyncGameObject in _asyncGameObjects)
				{
					if (asyncGameObject == null)
						continue;

					asyncGameObject.EditorPrepareForBuild();
					preparedCount++;
				}
			}

			_asyncGameObjects.Clear();

			if (preparedCount <= 0)
				return;

			Logger.Log(
				$"Prepared {preparedCount} AsyncGameObject(s) for build in scene '{scene.name}'."
			);
		}
	}
}
