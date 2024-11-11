using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public class FreeCamera : MonoBehaviour
	{
		private void Awake()
		{
			enabled = enableInputCapture;
			_cachedTransform = transform;
		}

		private void OnEnable()
		{
			if (enableInputCapture && !holdRightMouseCapture)
			{
				CaptureInput();
			}
		}

		private void OnDisable()
		{
			ReleaseInput();
		}

		private void OnValidate()
		{
			if (Application.isPlaying)
			{
				enabled = enableInputCapture;
			}
		}

		private void CaptureInput()
		{
			if (lockAndHideCursor)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}

			_inputCaptured = true;

			var eulerAngles = _cachedTransform.eulerAngles;
			_yaw = eulerAngles.y;
			_pitch = eulerAngles.x;
		}

		private void ReleaseInput()
		{
			if (lockAndHideCursor)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}

			_inputCaptured = false;
		}

		private void OnApplicationFocus(bool focus)
		{
			if (_inputCaptured && !focus)
			{
				ReleaseInput();
			}
		}

		private void Update()
		{
			if (!enableInputCapture)
			{
				return;
			}

			if (!_inputCaptured)
			{
				if (!holdRightMouseCapture && Input.GetMouseButtonDown(0))
				{
					CaptureInput();
				}
				else if (holdRightMouseCapture && Input.GetMouseButtonDown(1))
				{
					CaptureInput();
				}
			}

			if (!_inputCaptured)
			{
				return;
			}

			if (_inputCaptured)
			{
				if (useScrollSpeedIncrease)
				{
					sprintSpeed += Input.GetAxis(ScrollWheelKey) * speedIncreaseValue;

					if (sprintSpeed < moveSpeed)
					{
						sprintSpeed = moveSpeed;
					}
				}

				if (!holdRightMouseCapture && Input.GetKeyDown(KeyCode.Escape))
				{
					ReleaseInput();
				}
				else if (holdRightMouseCapture && Input.GetMouseButtonUp(1))
				{
					ReleaseInput();
				}
			}

			var rotStrafe = Input.GetAxis(MouseXKey);
			var rotFwd = Input.GetAxis(MouseYKey);

			_yaw = ( _yaw + lookSpeed * rotStrafe ) % 360f;

			_pitch = ( _pitch - lookSpeed * rotFwd ) % 360f;

			transform.rotation = Quaternion.AngleAxis(_yaw, Vector3.up) * Quaternion.AngleAxis(_pitch, Vector3.right);

			var eulerAngles = _cachedTransform.eulerAngles;
			_cachedTransform.eulerAngles =
				new Vector3(eulerAngles.x, eulerAngles.y, cameraRoll);

			var speed = Time.deltaTime * ( Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed );

			var forward = speed * Input.GetAxis(VerticalKey);

			var right = speed * Input.GetAxis(HorizontalKey);

			var up = speed * ( ( Input.GetKey(KeyCode.E) ? 1f : 0f ) - ( Input.GetKey(KeyCode.Q) ? 1f : 0f ) );

			transform.position +=
				_cachedTransform.forward * forward + _cachedTransform.right * right + Vector3.up * up;
		}

		public void RefreshCameraRoll(float roll)
		{
			cameraRoll = roll;
			var eulerAngles = _cachedTransform.eulerAngles;
			transform.eulerAngles =
				new Vector3(eulerAngles.x, eulerAngles.y, cameraRoll);
		}

		[SerializeField]
		private bool enableInputCapture = true;
		[SerializeField]
		private bool lockAndHideCursor;
		[SerializeField]
		private bool holdRightMouseCapture = true;

		[SerializeField]
		private float lookSpeed = 5f;
		[SerializeField]
		private float moveSpeed = 5f;
		[SerializeField]
		private float sprintSpeed = 50f;
		[SerializeField]
		private bool useScrollSpeedIncrease = true;
		[SerializeField]
		private float speedIncreaseValue = 100f;
		[SerializeField]
		private float cameraRoll;

		private bool _inputCaptured;
		private float _yaw;
		private float _pitch;
		private Transform _cachedTransform;

		private const string ScrollWheelKey = "Mouse ScrollWheel";
		private const string MouseXKey = "Mouse X";
		private const string MouseYKey = "Mouse Y";
		private const string VerticalKey = "Vertical";
		private const string HorizontalKey = "Horizontal";
	}
}
