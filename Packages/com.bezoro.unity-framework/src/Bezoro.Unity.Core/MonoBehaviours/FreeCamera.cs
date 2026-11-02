using Bezoro.Unity.Core.Common.Extensions;
using UnityEngine;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Provides mouse-look and keyboard navigation for an editor-style free camera.
	/// </summary>
	public class FreeCamera : MonoBehaviour
	{
		private const string HORIZONTAL_KEY   = "Horizontal";
		private const string MOUSE_X_KEY      = "Mouse X";
		private const string MOUSE_Y_KEY      = "Mouse Y";
		private const string SCROLL_WHEEL_KEY = "Mouse ScrollWheel";
		private const string VERTICAL_KEY     = "Vertical";

		private bool  _inputCaptured;
		private float _pitch;
		private float _yaw;

		private Transform? _cachedTransform;

		#region Unity Lifecycle

		private void Awake()
		{
			enabled          = enableInputCapture;
			_cachedTransform = transform;
		}

		private void Update()
		{
			if (!enableInputCapture) return;

			if (_cachedTransform.IsUnityNull())
				_cachedTransform = transform;

			if (!_inputCaptured)
			{
				if (!holdRightMouseCapture &&
					UnityEngine.Input.GetMouseButtonDown(
						0
					))
					CaptureInput();
				else if (holdRightMouseCapture &&
						 UnityEngine.Input.GetMouseButtonDown(
							 1
						 )) CaptureInput();
			}

			if (!_inputCaptured) return;

			if (_inputCaptured)
			{
				if (useScrollSpeedIncrease)
				{
					sprintSpeed += UnityEngine.Input.GetAxis(
									   SCROLL_WHEEL_KEY
								   ) *
								   speedIncreaseValue;

					if (sprintSpeed < moveSpeed) sprintSpeed = moveSpeed;
				}

				if (!holdRightMouseCapture &&
					UnityEngine.Input.GetKeyDown(
						KeyCode.Escape
					))
					ReleaseInput();
				else if (holdRightMouseCapture &&
						 UnityEngine.Input.GetMouseButtonUp(
							 1
						 )) ReleaseInput();
			}

			float rotStrafe = UnityEngine.Input.GetAxis(
				MOUSE_X_KEY
			);

			float rotFwd = UnityEngine.Input.GetAxis(
				MOUSE_Y_KEY
			);

			_yaw   = (_yaw + lookSpeed * rotStrafe) % 360f;
			_pitch = (_pitch - lookSpeed * rotFwd) % 360f;

			transform.rotation =
				Quaternion.AngleAxis(
					_yaw,
					Vector3.up
				) *
				Quaternion.AngleAxis(
					_pitch,
					Vector3.right
				);

			var eulerAngles = _cachedTransform.eulerAngles;
			_cachedTransform.eulerAngles = new(
				eulerAngles.x,
				eulerAngles.y,
				cameraRoll
			);

			float speed = Time.deltaTime *
						  (UnityEngine.Input.GetKey(
							   KeyCode.LeftShift
						   )
							   ? sprintSpeed
							   : moveSpeed);

			float forward = speed *
							UnityEngine.Input.GetAxis(
								VERTICAL_KEY
							);

			float right = speed *
						  UnityEngine.Input.GetAxis(
							  HORIZONTAL_KEY
						  );

			float up = speed *
					   ((UnityEngine.Input.GetKey(
							 KeyCode.E
						 )
							 ? 1f
							 : 0f) -
						(UnityEngine.Input.GetKey(
							 KeyCode.Q
						 )
							 ? 1f
							 : 0f));

			transform.position += _cachedTransform.forward * forward + _cachedTransform.right * right + Vector3.up * up;
		}

		private void OnEnable()
		{
			if (enableInputCapture && !holdRightMouseCapture) CaptureInput();
		}

		private void OnDisable() =>
			ReleaseInput();

		private void OnApplicationFocus(bool focus)
		{
			if (_inputCaptured && !focus) ReleaseInput();
		}

		private void OnValidate()
		{
			if (UnityEngine.Application.isPlaying) enabled = enableInputCapture;
		}

		#endregion

		public void RefreshCameraRoll(float roll)
		{
			cameraRoll = roll;
			if (_cachedTransform.IsUnityNull())
				_cachedTransform = transform;

			var eulerAngles = _cachedTransform.eulerAngles;
			transform.eulerAngles = new(
				eulerAngles.x,
				eulerAngles.y,
				cameraRoll
			);
		}

		private void CaptureInput()
		{
			if (lockAndHideCursor)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible   = false;
			}

			_inputCaptured = true;

			if (_cachedTransform.IsUnityNull()) return;

			var eulerAngles = _cachedTransform.eulerAngles;
			_yaw   = eulerAngles.y;
			_pitch = eulerAngles.x;
		}

		private void ReleaseInput()
		{
			if (lockAndHideCursor)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible   = true;
			}

			_inputCaptured = false;
		}

		#region Serialized Fields

		[SerializeField] private bool enableInputCapture = true;
		[SerializeField] private bool lockAndHideCursor;
		[SerializeField] private bool holdRightMouseCapture = true;

		[SerializeField] private float lookSpeed              = 5f;
		[SerializeField] private float moveSpeed              = 5f;
		[SerializeField] private float sprintSpeed            = 50f;
		[SerializeField] private bool  useScrollSpeedIncrease = true;
		[SerializeField] private float speedIncreaseValue     = 100f;
		[SerializeField] private float cameraRoll;

		#endregion
	}
}
