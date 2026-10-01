using System;
using Bezoro.Unity.Core.Scenes;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies SceneObjectRegistry GUID registration and lookup behavior.
	/// </summary>
	[TestFixture]
	public sealed class SceneObjectRegistryTests
	{
		private readonly string     _guid = Guid.NewGuid().ToString();
		private          GameObject _gameObject;

		[SetUp]
		public void SetUp()
		{
			_gameObject = new("SceneObjectRegistryTestsObject");
		}

		[TearDown]
		public void TearDown()
		{
			SceneObjectRegistry.Unregister(_guid);

			if (_gameObject != null)
				Object.DestroyImmediate(_gameObject);
		}

		[Test]
		public void Register_WithInvalidGuid_DoesNotRegisterObject()
		{
			SceneObjectRegistry.Register(_gameObject, "invalid");

			Assert.IsNull(SceneObjectRegistry.GetObjectByGuid("invalid"));
		}

		[Test]
		public void Register_WithValidGuid_AllowsLookupAndRegisteredCheck()
		{
			SceneObjectRegistry.Register(_gameObject, _guid);

			Assert.IsTrue(SceneObjectRegistry.IsGuidRegistered(_guid));
			Assert.AreSame(_gameObject, SceneObjectRegistry.GetObjectByGuid(_guid));
			Assert.AreSame(_gameObject, SceneObjectRegistry.Get_Object_By_GUID(_guid));
		}

		[Test]
		public void Unregister_RemovesGuid()
		{
			SceneObjectRegistry.Register(_gameObject, _guid);

			SceneObjectRegistry.Unregister(_guid);

			Assert.IsFalse(SceneObjectRegistry.IsGuidRegistered(_guid));
			Assert.IsNull(SceneObjectRegistry.GetObjectByGuid(_guid));
		}
	}
}
