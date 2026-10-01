using System.Threading.Tasks;
using Bezoro.Unity.Core.Common.Helpers;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class InstantiationAndSingletonHelpersTests
	{
		private GameObject _createdObject;
		private GameObject _prefab;

		[TearDown]
		public void TearDown()
		{
			if (_createdObject != null)
				Object.DestroyImmediate(_createdObject);

			if (_prefab != null)
				Object.DestroyImmediate(_prefab);
		}

		[Test]
		public async Task InstantiateAsync_CreatesInstanceAtPositionAndRunsInitializer()
		{
			_prefab = new("InstantiationHelpersTestsPrefab");

			GameObject initializedInstance = null;

			await InstantiationHelpers.InstantiateAsync(
				_prefab,
				instance =>
				{
					initializedInstance = instance;
					instance.name       = "InitializedInstance";
					return UniTask.CompletedTask;
				},
				position: new(1f, 2f, 3f)
			);

			_createdObject = initializedInstance;

			Assert.IsNotNull(initializedInstance);
			Assert.AreEqual("InitializedInstance", initializedInstance.name);
			AssertVector3(new(1f, 2f, 3f), initializedInstance.transform.position);
		}

		[Test]
		public async Task InstantiateAsync_WithParent_AttachesCreatedInstanceToParent()
		{
			_prefab        = new("InstantiationHelpersTestsPrefab");
			_createdObject = new("InstantiationHelpersTestsParent");
			var parent = _createdObject.transform;

			GameObject initializedInstance = null;

			await InstantiationHelpers.InstantiateAsync(
				_prefab,
				instance =>
				{
					initializedInstance = instance;
					return UniTask.CompletedTask;
				},
				parent
			);

			Assert.IsNotNull(initializedInstance);
			Assert.AreSame(parent, initializedInstance.transform.parent);

			Object.DestroyImmediate(initializedInstance);
		}

		private static void AssertVector3(Vector3 expected, Vector3 actual)
		{
			Assert.AreEqual(expected.x, actual.x, 0.0001f);
			Assert.AreEqual(expected.y, actual.y, 0.0001f);
			Assert.AreEqual(expected.z, actual.z, 0.0001f);
		}
	}
}
