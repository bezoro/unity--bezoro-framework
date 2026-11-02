using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies renderer material property block color helpers.
	/// </summary>
	[TestFixture]
	public sealed class RendererColorHelperTests
	{
		private GameObject            _gameObject;
		private MaterialPropertyBlock _propertyBlock;
		private MeshRenderer          _renderer;

		[SetUp]
		public void SetUp()
		{
			_gameObject    = GameObject.CreatePrimitive(PrimitiveType.Cube);
			_renderer      = _gameObject.GetComponent<MeshRenderer>();
			_propertyBlock = new();
		}

		[TearDown]
		public void TearDown()
		{
			if (_gameObject != null)
				Object.DestroyImmediate(_gameObject);
		}

		[Test]
		public void Clear_RemovesPreviouslySetPropertyValues()
		{
			RendererColorHelper.SetTintColor(_renderer, _propertyBlock, Color.blue);

			RendererColorHelper.Clear(_renderer, _propertyBlock);
			_renderer.GetPropertyBlock(_propertyBlock);

			Assert.AreEqual(default(Color), _propertyBlock.GetColor(RendererColorHelper.TintColorProperty));
		}

		[Test]
		public void SetBaseColor_SetsBaseAndColorProperties()
		{
			RendererColorHelper.SetBaseColor(_renderer, _propertyBlock, Color.red);

			_renderer.GetPropertyBlock(_propertyBlock);

			Assert.AreEqual(Color.red, _propertyBlock.GetColor(RendererColorHelper.BaseColorProperty));
			Assert.AreEqual(Color.red, _propertyBlock.GetColor(RendererColorHelper.ColorProperty));
		}

		[Test]
		public void SetEmissionColor_SetsEmissionProperty()
		{
			RendererColorHelper.SetEmissionColor(_renderer, _propertyBlock, Color.green);

			_renderer.GetPropertyBlock(_propertyBlock);

			Assert.AreEqual(Color.green, _propertyBlock.GetColor(RendererColorHelper.EmissionColorProperty));
		}
	}
}
