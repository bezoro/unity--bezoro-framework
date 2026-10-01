using System.Reflection;
using Bezoro.Unity.Core.MonoBehaviours;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using WWBC.CCS.Core.Editor.Extensions;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies alpha raycast setup on real UI components.
	/// </summary>
	[TestFixture]
	public sealed class AlphaRaycastEnablerTests
	{
		private GameObject _buttonObject;
		private Sprite     _sprite;
		private Texture2D  _texture;

		[TearDown]
		public void TearDown()
		{
			if (_buttonObject != null)
				Object.DestroyImmediate(_buttonObject);

			if (_sprite != null)
				Object.DestroyImmediate(_sprite);

			if (_texture != null)
				Object.DestroyImmediate(_texture);
		}

		[Test]
		public void Awake_WhenButtonHasNoImage_DoesNotThrow()
		{
			_buttonObject = new("AlphaRaycastEnablerTestsButtonWithoutImage");
			var button  = _buttonObject.AddComponent<Button>();
			var enabler = _buttonObject.AddComponent<AlphaRaycastEnabler>();

			enabler.SetPrivateField("button", button);

			Assert.DoesNotThrow(() => InvokeAwake(enabler));
		}

		[Test]
		public void Awake_WhenButtonHasReadableSprite_AppliesAlphaHitTestThreshold()
		{
			_buttonObject = new("AlphaRaycastEnablerTestsButton");
			var image   = _buttonObject.AddComponent<Image>();
			var button  = _buttonObject.AddComponent<Button>();
			var enabler = _buttonObject.AddComponent<AlphaRaycastEnabler>();
			_texture     = CreateReadableTexture();
			_sprite      = Sprite.Create(_texture, new(0f, 0f, 2f, 2f), Vector2.one * 0.5f);
			image.sprite = _sprite;

			enabler.SetPrivateField("button", button);
			InvokeAwake(enabler);

			Assert.AreEqual(0.1f, image.alphaHitTestMinimumThreshold);
		}

		private static Texture2D CreateReadableTexture()
		{
			var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
			texture.SetPixels(new[] { Color.clear, Color.white, Color.white, Color.clear });
			texture.Apply();
			return texture;
		}

		private static void InvokeAwake(AlphaRaycastEnabler enabler)
		{
			var method = typeof(AlphaRaycastEnabler).GetMethod(
				"Awake",
				BindingFlags.Instance | BindingFlags.NonPublic
			);

			Assert.IsNotNull(method);
			method.Invoke(enabler, null);
		}
	}
}
