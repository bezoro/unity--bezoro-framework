using System;
using System.Collections.Generic;
using Bezoro.Unity.Core.Scenes;
using NUnit.Framework;
using UnityEngine.SceneManagement;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies scene reference path, conversion, and collection helpers.
	/// </summary>
	[TestFixture]
	public sealed class SceneReferenceTests
	{
		[Test]
		public void Constructor_WithPath_StoresPathAndHasValue()
		{
			var sceneReference = new SceneReference("Assets/Scenes/Test.unity");

			Assert.IsTrue(sceneReference.HasValue);
			Assert.AreEqual("Assets/Scenes/Test.unity", sceneReference.Path);
			Assert.AreEqual("Assets/Scenes/Test.unity", sceneReference.ToString());
			Assert.AreEqual("Assets/Scenes/Test.unity", (string)sceneReference);
		}

		[Test]
		public void Paths_ReturnsOnlyReferencesWithValues()
		{
			var paths = new List<SceneReference>
				{
					new("Assets/Scenes/A.unity"),
					new(string.Empty),
					null
				}
				.Paths();

			CollectionAssert.AreEqual(new[] { "Assets/Scenes/A.unity" }, paths);
		}

		[Test]
		public void Paths_WhenCollectionIsNull_Throws()
		{
			ICollection<SceneReference> references = null;

			Assert.Throws<ArgumentNullException>(() => references.Paths());
		}

		[Test]
		public void SetLoadSceneMode_DoesNotChangePath()
		{
			var sceneReference = new SceneReference("Assets/Scenes/Test.unity");

			sceneReference.SetLoadSceneMode(LoadSceneMode.Additive);

			Assert.AreEqual("Assets/Scenes/Test.unity", sceneReference.Path);
		}
	}
}
