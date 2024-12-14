using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public class Free_Camera : MonoBehaviour
	{
		private const string _HORIZONTAL_KEY   = "Horizontal";
		private const string _MOUSE_X_KEY      = "Mouse X";
		private const string _MOUSE_Y_KEY      = "Mouse Y";
		private const string _SCROLL_WHEEL_KEY = "Mouse ScrollWheel";
		private const string _VERTICAL_KEY     = "Vertical";

		[SerializeField] private bool enableInputCapture = true;
		[SerializeField] private bool lockAndHideCursor;
		[SerializeField] private bool holdRightMouseCapture = true;

		[SerializeField] private float     lookSpeed              = 5f;
		[SerializeField] private float     moveSpeed              = 5f;
		[SerializeField] private float     sprintSpeed            = 50f;
		[SerializeField] private bool      useScrollSpeedIncrease = true;
		[SerializeField] private float     speedIncreaseValue     = 100f;
		[SerializeField] private float     cameraRoll;
		private                  Transform _cachedTransform;

		private bool  _inputCaptured;
		private float _pitch;
		private float _yaw;

		public void RefreshCameraRoll(float roll)
		{
			cameraRoll = roll;
			var euler_angles = _cachedTransform.eulerAngles;
			transform.eulerAngles = new( euler_angles.x, euler_angles.y, cameraRoll );
		}

		private void Awake()
		{
			enabled          = enableInputCapture;
			_cachedTransform = transform;
		}

		private void CaptureInput()
		{
			if (lockAndHideCursor)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible   = false;
			}

			_inputCaptured = true;

			var euler_angles = _cachedTransform.eulerAngles;
			_yaw   = euler_angles.y;
			_pitch = euler_angles.x;
		}

		private void OnApplicationFocus(bool focus)
		{
			if (_inputCaptured && !focus)
				ReleaseInput();
		}

		private void OnDisable()
		{
			ReleaseInput();
		}

		private void OnEnable()
		{
			if (enableInputCapture && !holdRightMouseCapture)
				CaptureInput();
		}

		private void OnValidate()
		{
			if (Application.isPlaying)
				enabled = enableInputCapture;
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

		private void Update()
		{
			if (!enableInputCapture) return;

			if (!_inputCaptured)
			{
				if (!holdRightMouseCapture && Input.GetMouseButtonDown(0))
					CaptureInput();
				else if (holdRightMouseCapture && Input.GetMouseButtonDown(1))
					CaptureInput();
			}

			if (!_inputCaptured) return;

			if (_inputCaptured)
			{
				if (useScrollSpeedIncrease)
				{
					sprintSpeed += Input.GetAxis(_SCROLL_WHEEL_KEY) * speedIncreaseValue;

					if (sprintSpeed < moveSpeed)
						sprintSpeed = moveSpeed;
				}

				if (!holdRightMouseCapture && Input.GetKeyDown(KeyCode.Escape))
					ReleaseInput();
				else if (holdRightMouseCapture && Input.GetMouseButtonUp(1))
					ReleaseInput();
			}

			var rot_strafe = Input.GetAxis(_MOUSE_X_KEY);
			var rot_fwd    = Input.GetAxis(_MOUSE_Y_KEY);

			_yaw   = ( _yaw + lookSpeed * rot_strafe ) % 360f;
			_pitch = ( _pitch - lookSpeed * rot_fwd ) % 360f;

			transform.rotation =
				Quaternion.AngleAxis(_yaw, Vector3.up) *
				Quaternion.AngleAxis(_pitch, Vector3.right);

			var euler_angles = _cachedTransform.eulerAngles;
			_cachedTransform.eulerAngles = new( euler_angles.x, euler_angles.y, cameraRoll );
			var speed   = Time.deltaTime * ( Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed );
			var forward = speed * Input.GetAxis(_VERTICAL_KEY);
			var right   = speed * Input.GetAxis(_HORIZONTAL_KEY);
			var up      = speed * ( ( Input.GetKey(KeyCode.E) ? 1f : 0f ) - ( Input.GetKey(KeyCode.Q) ? 1f : 0f ) );
			transform.position += _cachedTransform.forward * forward + _cachedTransform.right * right + Vector3.up * up;
		}
	}
}
