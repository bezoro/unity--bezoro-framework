using System;
using System.Collections.Generic;
using Bezoro.Unity.Core.Common.Helpers;
using Bezoro.Unity.Core.MonoBehaviours;
using Bezoro.Unity.Core.Scenes;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies GUID-backed scene object lookup behaviour.
	/// </summary>
	[TestFixture]
	public sealed class SceneObjectReferenceTests
	{
		private GameObject _gameObject = null!;
		private string     _guid       = null!;

		[SetUp]
		public void SetUp()
		{
			_guid       = Guid.NewGuid().ToString();
			_gameObject = new("Referenced Scene Object");

			SceneObjectRegistry.Register(
				_gameObject,
				_guid
			);
		}

		[TearDown]
		public void TearDown()
		{
			SceneObjectRegistry.Unregister(
				_guid
			);

			if (_gameObject != null)
				Object.DestroyImmediate(
					_gameObject
				);
		}

		[Test]
		public void Reference_WhenGuidIsRegistered_ReturnsRegisteredGameObject()
		{
			var reference = new SceneObjectReference
			{
				GUID = _guid
			};

			Assert.AreSame(
				_gameObject,
				reference.Reference
			);
		}

		[Test]
		public void GetGUIDsFromGameObjects_WhenObjectHasSceneObjectIdentifier_ReturnsIdentifierGuid()
		{
			_gameObject.SetActive(false);
			var identifier = _gameObject.AddComponent<SceneObjectIdentifier>();
			identifier.ObjectReference.GUID = _guid;

			var guids = GameObjectGUIDHelper.GetGUIDsFromGameObjects(
				new List<GameObject>
				{
					_gameObject
				}
			);

			Assert.That(
				guids,
				Is.EqualTo(
					new[]
					{
						_guid
					}
				)
			);
		}
	}
}
