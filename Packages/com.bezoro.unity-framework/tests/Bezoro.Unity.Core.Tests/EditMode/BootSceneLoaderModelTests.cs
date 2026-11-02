using System;
using Bezoro.Unity.Core.MonoBehaviours;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class BootSceneLoaderModelTests
	{
		[Test]
		public void DecidePathsToLoad_WithEmptySceneCollection_ReportsEmptyCollection()
		{
			var model = new BootSceneLoaderModel();

			var decision = model.DecidePathsToLoad(true, Array.Empty<string>(), 0, 2);

			Assert.AreEqual(BootSceneLoaderPathStatus.EmptySceneCollection, decision.Status);
			Assert.IsNull(decision.Paths);
			Assert.IsNull(decision.NextBuildIndex);
		}

		[Test]
		public void DecidePathsToLoad_WithoutSceneCollectionAndNextBuildScene_UsesNextBuildIndex()
		{
			var model = new BootSceneLoaderModel();

			var decision = model.DecidePathsToLoad(false, null, 1, 3);

			Assert.AreEqual(BootSceneLoaderPathStatus.UseNextBuildScene, decision.Status);
			Assert.IsNull(decision.Paths);
			Assert.AreEqual(2, decision.NextBuildIndex);
		}

		[Test]
		public void DecidePathsToLoad_WithoutSceneCollectionAndNoNextBuildScene_ReportsNoNextBuildScene()
		{
			var model = new BootSceneLoaderModel();

			var decision = model.DecidePathsToLoad(false, null, 2, 3);

			Assert.AreEqual(BootSceneLoaderPathStatus.NoNextBuildScene, decision.Status);
			Assert.IsNull(decision.Paths);
			Assert.IsNull(decision.NextBuildIndex);
		}

		[Test]
		public void DecidePathsToLoad_WithSceneCollectionPaths_UsesSceneCollection()
		{
			var model = new BootSceneLoaderModel();
			string[] paths =
			{
				"Assets/Scenes/Main.unity",
				"Assets/Scenes/UI.unity"
			};

			var decision = model.DecidePathsToLoad(true, paths, 0, 1);

			Assert.AreEqual(BootSceneLoaderPathStatus.UseSceneCollection, decision.Status);
			CollectionAssert.AreEqual(paths, decision.Paths);
			Assert.IsNull(decision.NextBuildIndex);
		}
	}
}
