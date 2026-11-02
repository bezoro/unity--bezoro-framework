using Bezoro.Unity.Core.Attributes;
using Bezoro.Unity.Core.Common.Extensions;
using UnityEngine;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Assigns a registered camera to a Canvas.
	/// </summary>
	public class CanvasCameraProvider : MonoBehaviour
	{
		private readonly CanvasCameraProviderModel _model = new();

		[EditorGetComponent(typeof(Canvas), true, true)]
		[SerializeField] private Canvas? canvas;

		[SerializeField] private int cameraIndex;

		#region Unity Lifecycle

		private void Start()
		{
			CameraManager.TryGetCamera(cameraIndex, out var camera);
			SetCanvasWorldCamera(camera);
		}

		private void OnEnable()
		{
			CameraManager.CameraRegistered += OnCameraRegistered;
		}

		private void OnDisable()
		{
			CameraManager.CameraRegistered -= OnCameraRegistered;
		}

		#endregion

		private void OnCameraRegistered(Camera registeredCamera)
		{
			SetCanvasWorldCamera(registeredCamera);
		}

		private void SetCanvasWorldCamera(Camera? registeredCamera)
		{
			var decision = _model.DecideAssignment(
				!registeredCamera.IsUnityNull(),
				canvas != null,
				ReferenceEquals(registeredCamera, Camera.main)
			);

			if (!decision.ShouldAssign) return;

			Camera? camera = registeredCamera;
			if (!decision.UseRegisteredCamera)
				CameraManager.TryGetCamera(cameraIndex, out camera);

			if (!_model.ShouldApplyResolvedCamera(!camera.IsUnityNull())) return;

			canvas!.worldCamera = camera;
		}
	}

	public readonly struct CanvasCameraAssignmentDecision
	{
		public CanvasCameraAssignmentDecision(bool shouldAssign, bool useRegisteredCamera)
		{
			ShouldAssign        = shouldAssign;
			UseRegisteredCamera = useRegisteredCamera;
		}

		public bool ShouldAssign { get; }
		public bool UseRegisteredCamera { get; }
	}

	public sealed class CanvasCameraProviderModel
	{
		public CanvasCameraAssignmentDecision DecideAssignment(
			bool hasRegisteredCamera,
			bool hasCanvas,
			bool registeredCameraIsMainCamera)
		{
			if (!hasRegisteredCamera || !hasCanvas)
				return new(false, false);

			return new(true, registeredCameraIsMainCamera);
		}

		public bool ShouldApplyResolvedCamera(bool hasResolvedCamera) =>
			hasResolvedCamera;
	}
}
