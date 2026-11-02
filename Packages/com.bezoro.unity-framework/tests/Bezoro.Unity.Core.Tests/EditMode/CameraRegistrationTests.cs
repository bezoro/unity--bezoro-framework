using System.Reflection;
using Bezoro.Unity.Core.MonoBehaviours;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class CameraRegistrationTests
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
		public void OnEnableAndOnDisable_RegisterAndUnregisterTargetCamera()
		{
			CreateCameraManager();

			var cameraObject = new GameObject("Registered Camera");
			cameraObject.SetActive(false);
			cameraObject.tag = "MainCamera";

			var camera       = cameraObject.AddComponent<Camera>();
			var registration = cameraObject.AddComponent<CameraRegistration>();

			Assert.IsFalse(CameraManager.TryGetCamera(0, out _));

			InvokeLifecycle(registration, "OnEnable");

			Assert.IsTrue(CameraManager.TryGetCamera(0, out var registeredCamera));
			Assert.AreSame(camera, registeredCamera);

			InvokeLifecycle(registration, "OnDisable");

			Assert.IsFalse(CameraManager.TryGetCamera(0, out var unregisteredCamera));
			Assert.IsNull(unregisteredCamera);
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

		private static void InvokeLifecycle(CameraRegistration registration, string methodName)
		{
			typeof(CameraRegistration)
				.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
				.Invoke(registration, null);
		}

		private static void ResetCameraManager()
		{
			foreach (var manager in Object.FindObjectsByType<CameraManager>(FindObjectsInactive.Include))
			{
				if (manager != null)
					Object.DestroyImmediate(manager.gameObject);
			}

			foreach (var registration in Object.FindObjectsByType<CameraRegistration>(FindObjectsInactive.Include))
			{
				if (registration != null)
					Object.DestroyImmediate(registration.gameObject);
			}

			SingletonScopeRegistry.ResetForTests();
			GlobalReferenceRegistry.ResetForTests();
		}
	}
}
