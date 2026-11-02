using System.Reflection;
using Bezoro.Unity.Core.MonoBehaviours;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class FreeCameraTests
	{
		private GameObject _cameraObject;
		private FreeCamera _freeCamera;

		[TearDown]
		public void TearDown()
		{
			if (_cameraObject != null)
				Object.DestroyImmediate(_cameraObject);
		}

		[Test]
		public void Awake_SetsEnabledFromInputCaptureConfiguration()
		{
			_freeCamera = CreateFreeCamera();
			SetField("enableInputCapture", false);

			InvokeLifecycle("Awake");

			Assert.IsFalse(_freeCamera.enabled);
		}

		[Test]
		public void RefreshCameraRoll_UpdatesZRotationAndPreservesExistingAxes()
		{
			_freeCamera                         = CreateFreeCamera();
			_cameraObject.transform.eulerAngles = new(10f, 25f, 0f);

			_freeCamera.RefreshCameraRoll(45f);

			var eulerAngles = _cameraObject.transform.eulerAngles;
			Assert.That(eulerAngles.x, Is.EqualTo(10f).Within(0.001f));
			Assert.That(eulerAngles.y, Is.EqualTo(25f).Within(0.001f));
			Assert.That(eulerAngles.z, Is.EqualTo(45f).Within(0.001f));
		}

		[Test]
		public void RefreshCameraRoll_WhenAwakeCachedTransform_UpdatesCachedTransform()
		{
			_freeCamera = CreateFreeCamera();
			InvokeLifecycle("Awake");

			_freeCamera.RefreshCameraRoll(30f);

			Assert.That(_cameraObject.transform.eulerAngles.z, Is.EqualTo(30f).Within(0.001f));
		}

		private FreeCamera CreateFreeCamera()
		{
			_cameraObject = new("Free Camera");
			return _cameraObject.AddComponent<FreeCamera>();
		}

		private void InvokeLifecycle(string methodName)
		{
			typeof(FreeCamera)
				.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
				.Invoke(_freeCamera, null);
		}

		private void SetField(string fieldName, object value)
		{
			typeof(FreeCamera)
				.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
				.SetValue(_freeCamera, value);
		}
	}
}
