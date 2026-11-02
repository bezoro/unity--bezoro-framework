using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies common object existence checks.
	/// </summary>
	[TestFixture]
	public sealed class UtilsTests
	{
		private GameObject _gameObject;

		[TearDown]
		public void TearDown()
		{
			if (_gameObject != null)
				Object.DestroyImmediate(_gameObject);
		}

		[Test]
		public void VerifyObjectExists_ForManagedObject_ReturnsTrueOnlyWhenNotNull()
		{
			Assert.IsTrue(Utils.VerifyObjectExists(new object(),  "exists"));
			Assert.IsFalse(Utils.VerifyObjectExists((object)null, "missing"));
		}

		[Test]
		public void VerifyObjectExists_ForUnityObject_ReturnsTrueOnlyWhenAlive()
		{
			_gameObject = new("UtilsTestsObject");

			Assert.IsTrue(Utils.VerifyObjectExists(_gameObject, "exists"));
			Assert.IsFalse(Utils.VerifyObjectExists(null,       "missing"));
		}
	}
}
