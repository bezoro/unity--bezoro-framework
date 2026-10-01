using System;
using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies Unity reference validation helpers.
	/// </summary>
	[TestFixture]
	public sealed class ValidationHelpersTests
	{
		[Test]
		public void RequiredComponent_WhenComponentExists_DoesNotThrow()
		{
			var gameObject = new GameObject("ValidationHelpersTestsObject");
			var transform  = gameObject.transform;

			Assert.DoesNotThrow(() => ValidationHelpers.RequiredComponent(transform));

			Object.DestroyImmediate(gameObject);
		}

		[Test]
		public void RequiredComponent_WhenComponentIsMissing_ThrowsArgumentNullException()
		{
			Transform transform = null;

			Assert.Throws<ArgumentNullException>(() => ValidationHelpers.RequiredComponent(transform));
		}
	}
}
