using Bezoro.Unity.Core.Common.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies synchronous Image extension helpers.
	/// </summary>
	[TestFixture]
	public sealed class ImageExtensionsTests
	{
		private GameObject _gameObject;

		[TearDown]
		public void TearDown()
		{
			if (_gameObject != null)
				Object.DestroyImmediate(_gameObject);
		}

		[Test]
		public void SetColor_WhenImageExists_AssignsColor()
		{
			_gameObject = new("ImageExtensionsTestsObject", typeof(Image));
			var image = _gameObject.GetComponent<Image>();

			image.SetColor(Color.magenta);

			Assert.AreEqual(Color.magenta, image.color);
		}

		[Test]
		public void SetColor_WhenImageIsNull_DoesNotThrow()
		{
			Image image = null;

			Assert.DoesNotThrow(() => image.SetColor(Color.cyan));
		}
	}
}
