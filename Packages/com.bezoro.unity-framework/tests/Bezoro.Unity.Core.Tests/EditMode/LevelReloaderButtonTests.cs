using System.Reflection;
using System.Threading;
using Bezoro.Unity.Core.MonoBehaviours;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class LevelReloaderButtonTests
	{
		private GameObject _buttonObject;

		[TearDown]
		public void TearDown()
		{
			if (_buttonObject != null)
				Object.DestroyImmediate(_buttonObject);
		}

		[Test]
		public void Awake_ResolvesButtonReference()
		{
			var reloaderButton = CreateInactiveButton();

			InvokeLifecycle(reloaderButton, "Awake");

			var resolvedButton = typeof(LevelReloaderButton)
				.GetField("button", BindingFlags.Instance | BindingFlags.NonPublic)!
				.GetValue(reloaderButton);
			Assert.AreSame(_buttonObject.GetComponent<Button>(), resolvedButton);
		}

		[Test]
		public void OnDestroy_CancelsReloadToken()
		{
			var reloaderButton = CreateInactiveButton();
			var cts = (CancellationTokenSource) typeof(LevelReloaderButton)
				.GetField("_cts", BindingFlags.Instance | BindingFlags.NonPublic)!
				.GetValue(reloaderButton)!;

			InvokeLifecycle(reloaderButton, "OnDestroy");

			Assert.IsTrue(cts.IsCancellationRequested);
		}

		[TestCase(false, true)]
		[TestCase(true,  false)]
		public void ShouldReload_ReflectsCancellationState(bool isCancellationRequested, bool expected)
		{
			var model = new LevelReloaderButtonModel();

			Assert.AreEqual(expected, model.ShouldReload(isCancellationRequested));
		}

		private static void InvokeLifecycle(LevelReloaderButton reloaderButton, string methodName)
		{
			typeof(LevelReloaderButton)
				.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
				.Invoke(reloaderButton, null);
		}

		private LevelReloaderButton CreateInactiveButton()
		{
			_buttonObject = new("LevelReloaderButtonTestsButton");
			_buttonObject.SetActive(false);
			_buttonObject.AddComponent<Button>();
			return _buttonObject.AddComponent<LevelReloaderButton>();
		}
	}
}
