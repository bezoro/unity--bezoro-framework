using System;
using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies Unity object validation helpers.
	/// </summary>
	[TestFixture]
	public sealed class UnityObjectHelpersTests
	{
		private GameObject _gameObject;

		[TearDown]
		public void TearDown()
		{
			if (_gameObject != null)
				Object.DestroyImmediate(_gameObject);
		}

		[Test]
		public void ValidateUnityObject_WhenObjectExists_DoesNotThrow()
		{
			_gameObject = new("UnityObjectHelpersTestsObject");

			Assert.DoesNotThrow(() => UnityObjectHelpers.ValidateUnityObject(_gameObject));
		}

		[Test]
		public void ValidateUnityObject_WhenObjectIsNull_Throws()
		{
			Assert.Throws<Exception>(() => UnityObjectHelpers.ValidateUnityObject(null));
		}
	}
}
