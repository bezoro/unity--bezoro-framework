using System.Reflection;
using Bezoro.Unity.Core.MonoBehaviours;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class CanvasCameraProviderTests
	{
		[SetUp]
		public void SetUp()
		{
			ResetCameraManager();
		}

		[TearDown]
		public void TearDown()
		{
			ResetCameraManager();
		}

		[Test]
		public void CameraRegistered_WhenProviderIsDisabled_DoesNotAssignCanvasWorldCamera()
		{
			CreateCameraManager();
			var canvasObject = new GameObject("World Canvas");
			var camera       = new GameObject("Registered Camera").AddComponent<Camera>();
			camera.gameObject.tag = "MainCamera";
			try
			{
				var canvas   = canvasObject.AddComponent<Canvas>();
				var provider = canvasObject.AddComponent<CanvasCameraProvider>();
				SetField(provider, "canvas", canvas);

				InvokeLifecycle(provider, "OnEnable");
				InvokeLifecycle(provider, "OnDisable");
				CameraManager.Register(camera);

				Assert.IsNull(canvas.worldCamera);
			}
			finally
			{
				Object.DestroyImmediate(canvasObject);
				Object.DestroyImmediate(camera.gameObject);
			}
		}

		[Test]
		public void CameraRegistered_WhenProviderIsEnabled_AssignsCanvasWorldCamera()
		{
			CreateCameraManager();
			var canvasObject = new GameObject("World Canvas");
			var camera       = new GameObject("Registered Camera").AddComponent<Camera>();
			camera.gameObject.tag = "MainCamera";
			try
			{
				var canvas   = canvasObject.AddComponent<Canvas>();
				var provider = canvasObject.AddComponent<CanvasCameraProvider>();
				SetField(provider, "canvas", canvas);

				InvokeLifecycle(provider, "OnEnable");
				CameraManager.Register(camera);

				Assert.AreSame(camera, canvas.worldCamera);
			}
			finally
			{
				Object.DestroyImmediate(canvasObject);
				Object.DestroyImmediate(camera.gameObject);
			}
		}

		private static CameraManager CreateCameraManager()
		{
			var manager = new GameObject("Camera Manager").AddComponent<CameraManager>();
			manager.GetComponent<SingletonScope>().Configure(manager, SingletonLifetime.Scene);
			manager.GetComponent<GlobalReference>().ConfigureTarget(manager);
			InvokeAwake(manager);
			return manager;
		}

		private static void InvokeAwake(CameraManager manager)
		{
			typeof(CameraManager)
				.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)!
				.Invoke(manager, null);
		}

		private static void InvokeLifecycle(CanvasCameraProvider provider, string methodName)
		{
			typeof(CanvasCameraProvider)
				.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
				.Invoke(provider, null);
		}

		private static void ResetCameraManager()
		{
			foreach (var manager in Object.FindObjectsByType<CameraManager>(FindObjectsInactive.Include))
			{
				if (manager != null)
					Object.DestroyImmediate(manager.gameObject);
			}

			foreach (var provider in Object.FindObjectsByType<CanvasCameraProvider>(FindObjectsInactive.Include))
			{
				if (provider != null)
					Object.DestroyImmediate(provider.gameObject);
			}

			SingletonScopeRegistry.ResetForTests();
			GlobalReferenceRegistry.ResetForTests();
		}

		private static void SetField(CanvasCameraProvider provider, string fieldName, object value)
		{
			typeof(CanvasCameraProvider)
				.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!
				.SetValue(provider, value);
		}
	}
}
