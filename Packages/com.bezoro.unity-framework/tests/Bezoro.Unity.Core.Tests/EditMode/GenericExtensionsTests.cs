using System;
using Bezoro.Unity.Core.Common.Extensions;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies Unity object null and type helper extensions.
	/// </summary>
	[TestFixture]
	public sealed class GenericExtensionsTests
	{
		private GameObject _gameObject;

		[SetUp]
		public void SetUp()
		{
			_gameObject = new("GenericExtensionsTestsObject");
		}

		[TearDown]
		public void TearDown()
		{
			if (_gameObject != null)
				Object.DestroyImmediate(_gameObject);
		}

		[Test]
		public void IsUnityNull_ReturnsTrueForNullReference()
		{
			GameObject value = null;

			Assert.IsTrue(value.IsUnityNull());
		}

		[Test]
		public void IsUnityObject_WhenObjectIsUnityObject_ReturnsTrueAndOutputsObject()
		{
			bool isUnityObject = _gameObject.IsUnityObject(out var unityObject);

			Assert.IsTrue(isUnityObject);
			Assert.AreSame(_gameObject, unityObject);
		}

		[Test]
		public void ThrowIfUnityNull_WhenObjectExists_ReturnsSameObject()
		{
			var result = _gameObject.ThrowIfUnityNull();

			Assert.AreSame(_gameObject, result);
		}

		[Test]
		public void ThrowIfUnityNull_WhenObjectIsNull_ThrowsArgumentNullException()
		{
			GameObject value = null;

			Assert.Throws<ArgumentNullException>(() => value.ThrowIfUnityNull("value"));
		}
	}
}
