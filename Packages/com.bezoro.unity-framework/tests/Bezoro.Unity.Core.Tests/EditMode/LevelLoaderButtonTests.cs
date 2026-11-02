using System.Reflection;
using Bezoro.Unity.Core.MonoBehaviours;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies LevelLoaderButton Unity UI listener wiring.
	/// </summary>
	[TestFixture]
	public sealed class LevelLoaderButtonTests
	{
		private GameObject _buttonObject;

		[TearDown]
		public void TearDown()
		{
			if (_buttonObject != null)
				Object.DestroyImmediate(_buttonObject);
		}

		[Test]
		public void OnDisable_RemovesClickListener()
		{
			var button = CreateInactiveButton(out var loaderButton);

			InvokeLifecycle(loaderButton, "OnEnable");
			InvokeLifecycle(loaderButton, "OnDisable");
			button.onClick.Invoke();

			LogAssert.NoUnexpectedReceived();
		}

		[Test]
		public void OnEnable_WhenClickedWithMissingSceneCollection_LogsWarning()
		{
			var button = CreateInactiveButton(out var loaderButton);

			InvokeLifecycle(loaderButton, "OnEnable");

			LogAssert.Expect(
				LogType.Warning,
				"LevelLoaderButton cannot load because sceneCollection is not assigned."
			);

			button.onClick.Invoke();
		}

		private static void InvokeLifecycle(LevelLoaderButton loaderButton, string methodName)
		{
			typeof(LevelLoaderButton)
				.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
				.Invoke(loaderButton, null);
		}

		private Button CreateInactiveButton(out LevelLoaderButton loaderButton)
		{
			_buttonObject = new("LevelLoaderButtonTestsButton");
			_buttonObject.SetActive(false);

			var button = _buttonObject.AddComponent<Button>();
			loaderButton = _buttonObject.AddComponent<LevelLoaderButton>();
			typeof(LevelLoaderButton).GetField("button", BindingFlags.Instance | BindingFlags.NonPublic)!
				.SetValue(loaderButton, button);
			typeof(LevelLoaderButton).GetField("useSceneCollection", BindingFlags.Instance | BindingFlags.NonPublic)!
				.SetValue(loaderButton, true);

			return button;
		}
	}
}
