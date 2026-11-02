#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using Bezoro.Unity.Core.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class TextBubbleServiceTests
	{
		private readonly List<GameObject> _createdObjects = new();

		private GameObject? _canvasObject;
		private GameObject? _serviceObject;
		private GameObject? _prefabObject;

		[TearDown]
		public void TearDown()
		{
			for (var i = _createdObjects.Count - 1; i >= 0; i--)
			{
				if (_createdObjects[i] != null)
					Object.DestroyImmediate(_createdObjects[i]);
			}

			_createdObjects.Clear();
			_canvasObject  = null;
			_serviceObject = null;
			_prefabObject  = null;

			ClearTextBubbleServiceInstance();
		}

		[Test]
		public void Show_WhenRequestIsValid_CreatesValidHandleAndActiveBubble()
		{
			var service = CreateService();

			var handle = service.Show(
				new(
					"Show the prompt",
					TextBubbleAnchor.ForScreenPosition(new(100f, 120f))
				)
			);

			Assert.IsTrue(handle.IsValid);
			Assert.AreEqual(1, service.ActiveCount);
			Assert.AreEqual(0, service.PooledCount);
		}

		[Test]
		public void Show_WhenRequestHasEmptyText_ReturnsInvalidHandle()
		{
			var service = CreateService();

			var handle = service.Show(
				new(
					string.Empty,
					TextBubbleAnchor.ForScreenPosition(new(100f, 120f))
				)
			);

			Assert.IsFalse(handle.IsValid);
			Assert.AreEqual(0, service.ActiveCount);
			Assert.AreEqual(0, service.PooledCount);
		}

		[Test]
		public void Hide_WhenHandleIsValid_KeepsHandleValid()
		{
			var service = CreateService();
			var handle = service.Show(
				new(
					"Use this hint",
					TextBubbleAnchor.ForScreenPosition(new(100f, 120f))
				)
			);

			bool hidden = handle.Hide();

			Assert.IsTrue(hidden);
			Assert.IsTrue(handle.IsValid);
			Assert.AreEqual(1, service.ActiveCount);
		}

		[Test]
		public void Release_WhenHandleIsValid_InvalidatesHandleAndPoolsView()
		{
			var service = CreateService();
			var handle = service.Show(
				new(
					"Display this message",
					TextBubbleAnchor.ForScreenPosition(new(100f, 120f))
				)
			);

			bool released = handle.Release();

			Assert.IsTrue(released);
			Assert.IsFalse(handle.IsValid);
			Assert.AreEqual(0, service.ActiveCount);
			Assert.AreEqual(1, service.PooledCount);
		}

		[Test]
		public void ReleasedHandle_CannotMutateReusedBubble()
		{
			var service = CreateService();
			var first = service.Show(
				new(
					"First bubble",
					TextBubbleAnchor.ForScreenPosition(new(100f, 120f))
				)
			);
			Assert.IsTrue(first.Release());

			var second = service.Show(
				new(
					"Second bubble",
					TextBubbleAnchor.ForScreenPosition(new(140f, 160f))
				)
			);

			var reusedView = FindInstantiatedBubbleView();

			Assert.AreEqual("Second bubble", reusedView.CurrentText);
			Assert.IsFalse(first.SetText("Stale mutation"));
			Assert.AreEqual("Second bubble", reusedView.CurrentText);
			Assert.IsTrue(second.SetText("Fresh mutation"));
			Assert.AreEqual("Fresh mutation", reusedView.CurrentText);
			Assert.IsTrue(second.IsValid);
			Assert.AreEqual(1, service.ActiveCount);
			Assert.AreEqual(0, service.PooledCount);
		}

		[Test]
		public void SetText_WhenUpdatedTextIsEmptyAndEmptyTextIsNotAllowed_ReturnsFalseAndKeepsCurrentText()
		{
			var service = CreateService();
			var handle = service.Show(
				new(
					"Original text",
					TextBubbleAnchor.ForScreenPosition(new(100f, 120f))
				)
			);
			var view = FindInstantiatedBubbleViewWithText("Original text");

			bool changed = handle.SetText(" ");

			Assert.IsFalse(changed);
			Assert.AreEqual("Original text", view.CurrentText);
			Assert.IsTrue(handle.IsValid);
		}

		[Test]
		public void TrySetText_WhenHandleBelongsToAnotherService_ReturnsFalseAndDoesNotMutate()
		{
			var firstService = CreateService("FirstTextBubbleServiceTests");
			var firstHandle = firstService.Show(
				new(
					"First bubble",
					TextBubbleAnchor.ForScreenPosition(new(100f, 120f))
				)
			);

			ClearTextBubbleServiceInstance();

			var secondService = CreateService("SecondTextBubbleServiceTests");
			var secondHandle = secondService.Show(
				new(
					"Second bubble",
					TextBubbleAnchor.ForScreenPosition(new(140f, 160f))
				)
			);
			var secondView = FindInstantiatedBubbleViewWithText("Second bubble");

			Assert.IsTrue(firstHandle.IsValid);
			Assert.IsTrue(secondHandle.IsValid);

			bool changed = secondService.TrySetText(firstHandle, "Cross-service mutation");

			Assert.IsFalse(changed);
			Assert.AreEqual("Second bubble", secondView.CurrentText);
			Assert.AreEqual(1, firstService.ActiveCount);
			Assert.AreEqual(1, secondService.ActiveCount);
		}

		[Test]
		public void Show_WhenWorldAnchorHasScreenSpaceCameraCanvas_UsesCanvasWorldCameraFallback()
		{
			var worldPosition = new Vector3(1.25f, 0.5f, 0f);
			var canvasCameraObject = CreateTrackedObject("TextBubbleCanvasCamera", typeof(Camera));
			var canvasCamera = canvasCameraObject.GetComponent<Camera>();
			canvasCamera.transform.position = new(0f, 0f, -10f);
			canvasCamera.transform.rotation = Quaternion.identity;
			canvasCamera.fieldOfView        = 30f;

			var mainCameraObject = CreateTrackedObject("TextBubbleMainCamera", typeof(Camera));
			var mainCamera = mainCameraObject.GetComponent<Camera>();
			mainCamera.tag                = "MainCamera";
			mainCamera.transform.position = new(0f, 0f, -10f);
			mainCamera.transform.rotation = Quaternion.identity;
			mainCamera.fieldOfView        = 90f;

			var service = CreateService(
				"ScreenSpaceCameraTextBubbleServiceTests",
				RenderMode.ScreenSpaceCamera,
				canvasCamera
			);

			var handle = service.Show(
				new(
					"Camera fallback",
					TextBubbleAnchor.ForWorldPosition(worldPosition)
				)
			);
			var view = FindInstantiatedBubbleViewWithText("Camera fallback");
			var expectedScreenPoint = canvasCamera.WorldToScreenPoint(worldPosition);
			var resolvedExpectedPosition = RectTransformUtility.ScreenPointToLocalPointInRectangle(
				_canvasObject!.GetComponent<RectTransform>(),
				new(expectedScreenPoint.x, expectedScreenPoint.y),
				canvasCamera,
				out var expectedAnchoredPosition
			);

			Assert.IsTrue(handle.IsValid);
			Assert.Greater(expectedScreenPoint.z, 0f);
			Assert.IsTrue(resolvedExpectedPosition);
			Assert.IsTrue(view.IsVisible);
			Assert.AreEqual(expectedAnchoredPosition.x, view.RectTransform.anchoredPosition.x, 0.001f);
			Assert.AreEqual(expectedAnchoredPosition.y, view.RectTransform.anchoredPosition.y, 0.001f);
			Assert.AreEqual(1, service.ActiveCount);
		}

		[Test]
		public void Show_WhenProjectionCameraDiffersFromCanvasCamera_UsesCanvasWorldCameraForUiConversion()
		{
			var worldPosition = new Vector3(1.25f, 0.5f, 0f);
			var projectionCameraObject = CreateTrackedObject("TextBubbleProjectionCamera", typeof(Camera));
			var projectionCamera = projectionCameraObject.GetComponent<Camera>();
			projectionCamera.transform.position = new(0f, 0f, -10f);
			projectionCamera.transform.rotation = Quaternion.identity;
			projectionCamera.fieldOfView        = 30f;

			var canvasCameraObject = CreateTrackedObject("TextBubbleCanvasCamera", typeof(Camera));
			var canvasCamera = canvasCameraObject.GetComponent<Camera>();
			canvasCamera.transform.position = new(0f, 0f, -10f);
			canvasCamera.transform.rotation = Quaternion.identity;
			canvasCamera.fieldOfView        = 90f;

			var service = CreateService(
				"SplitCameraTextBubbleServiceTests",
				RenderMode.ScreenSpaceCamera,
				canvasCamera,
				projectionCamera
			);

			var handle = service.Show(
				new(
					"Split cameras",
					TextBubbleAnchor.ForWorldPosition(worldPosition)
				)
			);
			var view = FindInstantiatedBubbleViewWithText("Split cameras");
			var expectedScreenPoint = projectionCamera.WorldToScreenPoint(worldPosition);
			var resolvedExpectedPosition = RectTransformUtility.ScreenPointToLocalPointInRectangle(
				_canvasObject!.GetComponent<RectTransform>(),
				new(expectedScreenPoint.x, expectedScreenPoint.y),
				canvasCamera,
				out var expectedAnchoredPosition
			);

			Assert.IsTrue(handle.IsValid);
			Assert.Greater(expectedScreenPoint.z, 0f);
			Assert.IsTrue(resolvedExpectedPosition);
			Assert.IsTrue(view.IsVisible);
			Assert.AreEqual(expectedAnchoredPosition.x, view.RectTransform.anchoredPosition.x, 0.001f);
			Assert.AreEqual(expectedAnchoredPosition.y, view.RectTransform.anchoredPosition.y, 0.001f);
		}

		[Test]
		public void SetAnchor_WhenVisibleWorldBubbleProjectionRecovers_RestoresVisibility()
		{
			var cameraObject = CreateTrackedObject("TextBubbleProjectionRecoveryCamera", typeof(Camera));
			var camera = cameraObject.GetComponent<Camera>();
			camera.transform.position = new(0f, 0f, -10f);
			camera.transform.rotation = Quaternion.identity;

			var service = CreateService(
				"ProjectionRecoveryTextBubbleServiceTests",
				targetDefaultCamera: camera
			);

			var handle = service.Show(
				new(
					"Recoverable bubble",
					TextBubbleAnchor.ForWorldPosition(new(0f, 0f, -20f))
				)
			);
			var view = FindInstantiatedBubbleViewWithText("Recoverable bubble");

			Assert.IsTrue(handle.IsValid);
			Assert.IsFalse(view.IsVisible);

			bool changed = handle.SetAnchor(TextBubbleAnchor.ForWorldPosition(Vector3.zero));

			Assert.IsTrue(changed);
			Assert.IsTrue(view.IsVisible);
		}

		[Test]
		public void TryGetInstance_WhenActiveServiceIsDisabledAndReEnabled_TracksEnabledLifecycle()
		{
			var service = CreateService();

			Assert.IsTrue(TextBubbleService.TryGetInstance(out var registeredService));
			Assert.AreSame(service, registeredService);

			DisableServiceForTest(service);

			Assert.IsFalse(TextBubbleService.TryGetInstance(out var disabledService));
			Assert.IsNull(disabledService);

			EnableServiceForTest(service);

			Assert.IsTrue(TextBubbleService.TryGetInstance(out var reenabledService));
			Assert.AreSame(service, reenabledService);
		}

		[Test]
		public void DuplicateService_WhenCreatedOutsidePlayMode_RemainsAliveAndDoesNotReplaceActiveInstance()
		{
			var firstService = CreateService("FirstTextBubbleServiceTests");

			var duplicateObject = CreateTrackedObject("DuplicateTextBubbleServiceTestsService");
			var duplicateService = duplicateObject.AddComponent<TextBubbleService>();
			EnableServiceForTest(duplicateService);

			Assert.IsNotNull(duplicateService);
			Assert.IsTrue(TextBubbleService.TryGetInstance(out var registeredService));
			Assert.AreSame(firstService, registeredService);
			Assert.AreNotSame(firstService, duplicateService);
		}

		[Test]
		public void OnDisable_ReleasesAllActiveBubblesAndInvalidatesHandles()
		{
			var service = CreateService();
			var first = service.Show(
				new(
					"First bubble",
					TextBubbleAnchor.ForScreenPosition(new(100f, 120f))
				)
			);
			var second = service.Show(
				new(
					"Second bubble",
					TextBubbleAnchor.ForScreenPosition(new(140f, 160f))
				)
			);

			DisableServiceForTest(service);

			Assert.IsFalse(first.IsValid);
			Assert.IsFalse(second.IsValid);
			Assert.AreEqual(0, service.ActiveCount);
		}

		private TextBubbleView FindInstantiatedBubbleView()
		{
			var prefabView = _prefabObject != null ? _prefabObject.GetComponent<TextBubbleView>() : null;
			var views = Object.FindObjectsByType<TextBubbleView>(FindObjectsInactive.Include);

			for (var i = 0; i < views.Length; i++)
			{
				if (views[i] != prefabView)
					return views[i];
			}

			Assert.Fail("Expected an instantiated text bubble view.");
			return null!;
		}

		private TextBubbleView FindInstantiatedBubbleViewWithText(string currentText)
		{
			var views = Object.FindObjectsByType<TextBubbleView>(FindObjectsInactive.Include);

			for (var i = 0; i < views.Length; i++)
			{
				if (views[i].CurrentText == currentText)
					return views[i];
			}

			Assert.Fail($"Expected an instantiated text bubble view with text '{currentText}'.");
			return null!;
		}

		private TextBubbleService CreateService(
			string     namePrefix = "TextBubbleServiceTests",
			RenderMode renderMode = RenderMode.ScreenSpaceOverlay,
			Camera?    canvasWorldCamera = null,
			Camera?    targetDefaultCamera = null)
		{
			_canvasObject = CreateTrackedObject($"{namePrefix}Canvas", typeof(RectTransform), typeof(Canvas));
			var canvas = _canvasObject.GetComponent<Canvas>();
			canvas.renderMode  = renderMode;
			canvas.worldCamera = canvasWorldCamera;

			_prefabObject = CreateTrackedObject($"{namePrefix}Prefab", typeof(RectTransform), typeof(CanvasGroup));
			var textObject = new GameObject("Text", typeof(RectTransform));
			textObject.transform.SetParent(_prefabObject.transform, false);
			textObject.AddComponent<TextMeshProUGUI>();
			_prefabObject.AddComponent<Image>();
			var prefabView = _prefabObject.AddComponent<TextBubbleView>();
			prefabView.ResetView();
			_prefabObject.SetActive(false);

			_serviceObject = CreateTrackedObject($"{namePrefix}Service");
			var service = _serviceObject.AddComponent<TextBubbleService>();
			service.ConfigureForRuntime(prefabView, canvas.GetComponent<RectTransform>(), canvas, targetDefaultCamera);
			EnableServiceForTest(service);
			return service;
		}

		private GameObject CreateTrackedObject(string name, params Type[] components)
		{
			var gameObject = new GameObject(name, components);
			_createdObjects.Add(gameObject);
			return gameObject;
		}

		private static void EnableServiceForTest(TextBubbleService service)
		{
			service.enabled = true;
			InvokeServiceLifecycle(service, "OnEnable");
		}

		private static void DisableServiceForTest(TextBubbleService service)
		{
			service.enabled = false;
			InvokeServiceLifecycle(service, "OnDisable");
		}

		private static void InvokeServiceLifecycle(TextBubbleService service, string methodName)
		{
			var method = typeof(TextBubbleService).GetMethod(
				methodName,
				BindingFlags.Instance | BindingFlags.NonPublic
			);

			Assert.IsNotNull(method);
			method!.Invoke(service, null);
		}

		private static void ClearTextBubbleServiceInstance()
		{
			var instanceField = typeof(TextBubbleService).GetField(
				"_instance",
				BindingFlags.NonPublic | BindingFlags.Static
			);

			Assert.IsNotNull(instanceField);
			instanceField!.SetValue(null, null);
		}
	}
}
