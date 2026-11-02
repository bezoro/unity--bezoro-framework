using System.Linq;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using CoreSceneManager = Bezoro.Unity.Core.Scenes.SceneManager;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies scene query helpers without loading or unloading scenes.
	/// </summary>
	[TestFixture]
	public sealed class SceneManagerTests
	{
		[Test]
		public void FindSceneByName_WhenSceneDoesNotExist_ReturnsDefaultScene()
		{
			var scene = CoreSceneManager.FindSceneByName("MissingSceneForEditModeTest");

			Assert.AreEqual(default(Scene), scene);
		}

		[Test]
		public void GetAllLoadedScenes_ReturnsSameCountAsUnitySceneManager()
		{
			var scenes = CoreSceneManager.GetAllLoadedScenes().ToArray();

			Assert.AreEqual(SceneManager.sceneCount, scenes.Length);
		}

		[Test]
		public void GetLoadedSceneAt_WhenIndexIsInvalid_ReturnsDefaultScene()
		{
			var scene = CoreSceneManager.GetLoadedSceneAt(-1);

			Assert.AreEqual(default(Scene), scene);
		}

		[Test]
		public void GetLoadedSceneCount_MatchesUnitySceneManager()
		{
			Assert.AreEqual(
				SceneManager.sceneCount,
				CoreSceneManager.GetLoadedSceneCount()
			);
		}
	}
}
