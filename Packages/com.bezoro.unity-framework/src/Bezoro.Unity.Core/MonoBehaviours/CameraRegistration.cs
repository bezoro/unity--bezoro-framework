using Bezoro.Unity.Core.Attributes;
using Bezoro.Unity.Core.Common.Extensions;
using UnityEngine;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Registers a camera with <see cref="CameraManager" /> for runtime lookup.
	/// </summary>
	[RequireComponent(typeof(Camera))]
	public sealed class CameraRegistration : MonoBehaviour
	{
		private readonly CameraRegistrationModel _model = new();

		[EditorGetComponent(typeof(Camera))]
		[SerializeField] private Camera targetCamera = null!;

		#region Unity Lifecycle

		private void OnEnable() => Register();

		private void OnDisable() => Unregister();

		#endregion

		private void Register()
		{
			if (targetCamera.IsUnityNull())
				targetCamera = GetComponent<Camera>();

			if (!_model.ShouldRegister(!targetCamera.IsUnityNull())) return;

			CameraManager.Register(targetCamera);
		}

		private void Unregister()
		{
			CameraManager.Unregister(targetCamera);
		}
	}

	public sealed class CameraRegistrationModel
	{
		public bool ShouldRegister(bool hasTargetCamera) =>
			hasTargetCamera;
	}
}
