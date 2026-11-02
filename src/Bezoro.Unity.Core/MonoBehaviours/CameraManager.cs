using System;
using System.Diagnostics.CodeAnalysis;
using Bezoro.Unity.Core.Common.Extensions;
using Bezoro.Unity.Core.Common.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Runtime registry for active cameras.
	/// </summary>
	[RequireComponent(typeof(SingletonScope), typeof(GlobalReference))]
	[DefaultExecutionOrder(ExecutionOrder.Manager)]
	public sealed class CameraManager : MonoBehaviour
	{
		private readonly CameraManagerModel<Camera> _model = new(4);
		private bool _isSingletonInstance;

		public Camera?[] Cameras => _model.Items;

		public static event Action<Camera>? CameraRegistered;
		public static event Action<Camera>? CameraUnregistered;

		public static bool IsAvailable => TryGetInstance(out _);

		public static CameraManager Instance => GlobalReferenceAccess<CameraManager>.Instance;

		public static bool TryGetInstance([NotNullWhen(true)] out CameraManager? instance) =>
			GlobalReferenceAccess<CameraManager>.TryGetInstance(out instance);

		#region Unity Lifecycle

		private void Awake()
		{
			_isSingletonInstance = GlobalReferenceAccess<CameraManager>.IsCurrent(this);
		}

		private void OnDestroy()
		{
			if (!_isSingletonInstance)
				return;

			_model.Clear();
			CameraRegistered   = null;
			CameraUnregistered = null;
		}

		#endregion

		public static bool TryGetBestCamera(Scene scene, bool allowGlobalLookup, out Camera? camera)
		{
			camera = null;

			if (!TryGetInstance(out var manager))
				return false;

			manager.RemoveDestroyedCameras();

			camera = manager.FindCameraInScene(scene, true) ??
					 manager.FindCameraInScene(scene, false);

			if (camera != null)
				return true;

			if (!allowGlobalLookup)
				return false;

			camera = manager.FindGlobalCamera(true) ??
					 manager.FindGlobalCamera(false);

			return camera != null;
		}

		public static bool TryGetCamera(int index, out Camera? camera)
		{
			camera = null;

			if (!TryGetInstance(out var manager))
				return false;

			manager.RemoveDestroyedCameras();

			if (index < 0 || index >= manager.Cameras.Length)
				return false;

			camera = manager.Cameras[index];
			return !camera.IsUnityNull();
		}

		public static void Register(Camera camera)
		{
			if (camera.IsUnityNull()) return;
			if (!TryGetInstance(out var manager)) return;
			if (!manager._model.ShouldRegister(!camera.IsUnityNull(), manager._model.Contains(camera))) return;

			if (camera.CompareTag("MainCamera"))
				manager._model.RegisterMain(camera);
			else
				manager._model.RegisterSecondary(camera, item => item.IsUnityNull());

			CameraRegistered?.Invoke(camera);
		}

		public static void Unregister(Camera camera)
		{
			if (ReferenceEquals(camera, null)) return;
			if (!TryGetInstance(out var manager)) return;

			if (!manager._model.Unregister(camera)) return;

			CameraUnregistered?.Invoke(camera);
		}

		[ContextMenu("Log Registered Cameras")]
		private void LogRegisteredCameras()
		{
			for (var i = 0; i < Cameras.Length; i++)
				Debug.Log($"Camera slot {i}: {(Cameras[i] != null ? Cameras[i]!.name : "<empty>")}", this);
		}

		private Camera? FindCameraInScene(Scene scene, bool requireMainCamera)
		{
			if (!scene.IsValid() || !scene.isLoaded)
				return null;

			return _model.FindFirst(
				camera =>
					!camera.IsUnityNull() &&
					camera.isActiveAndEnabled &&
					camera.gameObject.scene == scene &&
					(!requireMainCamera || camera.CompareTag("MainCamera"))
			);
		}

		private Camera? FindGlobalCamera(bool requireMainCamera)
		{
			return _model.FindFirst(
				camera =>
					!camera.IsUnityNull() &&
					camera.isActiveAndEnabled &&
					(!requireMainCamera || camera.CompareTag("MainCamera"))
			);
		}

		private void RemoveDestroyedCameras()
		{
			_model.RemoveWhere(camera => camera.IsUnityNull());
		}
	}

	public sealed class CameraManagerModel<TCamera>
		where TCamera : class
	{
		public CameraManagerModel(int capacity)
		{
			Items = new TCamera?[capacity];
		}

		public TCamera?[] Items { get; }

		public bool ShouldRegister(bool hasCamera, bool isAlreadyRegistered)
		{
			return hasCamera && !isAlreadyRegistered;
		}

		public bool Contains(TCamera camera)
		{
			return IndexOf(camera) >= 0;
		}

		public void RegisterMain(TCamera camera)
		{
			Items[0] = camera;
		}

		public bool RegisterSecondary(TCamera camera, Func<TCamera?, bool> isSlotAvailable)
		{
			for (var i = 1; i < Items.Length; i++)
			{
				if (!isSlotAvailable(Items[i]))
					continue;

				Items[i] = camera;
				return true;
			}

			return false;
		}

		public bool Unregister(TCamera camera)
		{
			int cameraIndex = IndexOf(camera);
			if (cameraIndex < 0) return false;

			Items[cameraIndex] = null;
			return true;
		}

		public TCamera? FindFirst(Func<TCamera?, bool> predicate)
		{
			foreach (var camera in Items)
			{
				if (predicate(camera))
					return camera;
			}

			return null;
		}

		public int RemoveWhere(Func<TCamera?, bool> shouldRemove)
		{
			var removed = 0;

			for (int i = Items.Length - 1; i >= 0; i--)
			{
				if (!shouldRemove(Items[i]))
					continue;

				Items[i] = null;
				removed++;
			}

			return removed;
		}

		public void Clear()
		{
			Array.Clear(Items, 0, Items.Length);
		}

		private int IndexOf(TCamera camera)
		{
			for (var i = 0; i < Items.Length; i++)
			{
				if (ReferenceEquals(Items[i], camera))
					return i;
			}

			return -1;
		}
	}
}
