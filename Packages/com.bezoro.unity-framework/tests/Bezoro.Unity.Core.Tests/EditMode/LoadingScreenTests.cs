using System.Reflection;
using Bezoro.Unity.Core.MonoBehaviours;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class LoadingScreenTests
	{
		private GameObject    _host;
		private Image         _image;
		private LoadingScreen _loadingScreen;
		private TextMeshPro   _text;

		[SetUp]
		public void SetUp()
		{
			_host          = new("Loading Screen Test");
			_loadingScreen = _host.AddComponent<LoadingScreen>();

			var imageObject = new GameObject("Progress Image");
			imageObject.transform.SetParent(_host.transform);
			_image            = imageObject.AddComponent<Image>();
			_image.fillAmount = 0.35f;

			var textObject = new GameObject("Progress Text");
			textObject.transform.SetParent(_host.transform);
			_text      = textObject.AddComponent<TextMeshPro>();
			_text.text = "stale";
		}

		[TearDown]
		public void TearDown()
		{
			if (_host != null)
				Object.DestroyImmediate(_host);
		}

		[Test]
		public void Awake_WithImageAndText_AppliesInitialPresentation()
		{
			InvokeLifecycle("Awake");

			Assert.AreEqual(0f,    _image.fillAmount);
			Assert.AreEqual("0 %", _text.text);
			Assert.IsTrue(_loadingScreen.enabled);
		}

		[Test]
		public void OnOverallProgressChanged_UpdatesFillAndPercentText()
		{
			InvokeLifecycle("Awake");

			InvokeProgressChanged(0.65f);

			Assert.AreEqual(0.65f,  _image.fillAmount);
			Assert.AreEqual("65 %", _text.text);
		}

		[Test]
		public void OnOverallProgressChanged_WhenProgressRegresses_KeepsCurrentFillAmount()
		{
			InvokeLifecycle("Awake");
			InvokeProgressChanged(0.65f);

			InvokeProgressChanged(0.25f);

			Assert.AreEqual(0.65f,  _image.fillAmount);
			Assert.AreEqual("65 %", _text.text);
		}

		private void InvokeLifecycle(string methodName)
		{
			typeof(LoadingScreen)
				.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
				.Invoke(_loadingScreen, null);
		}

		private void InvokeProgressChanged(float value)
		{
			typeof(LoadingScreen)
				.GetMethod("OnOverallProgressChanged", BindingFlags.Instance | BindingFlags.NonPublic)!
				.Invoke(_loadingScreen, new object[] { value });
		}
	}
}
