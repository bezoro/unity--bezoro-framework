using UnityEngine;

namespace Bezoro
{
	public class FreeCamera : MonoBehaviour
	{
		private void Awake() =>
			enabled = enableInputCapture;

		private void OnEnable()
		{
			if (enableInputCapture && !holdRightMouseCapture)
				CaptureInput();
		}

		private void OnDisable() =>
			ReleaseInput();

		private void OnValidate()
		{
			if (Application.isPlaying)
				enabled = enableInputCapture;
		}

		private void CaptureInput()
		{
			if (lockAndHideCursor)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}

			_inputCaptured = true;

			_yaw = transform.eulerAngles.y;
			_pitch = transform.eulerAngles.x;
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
				ReleaseInput();
		}

		private void Update()
		{
			if (!enableInputCapture)
				return;

			if (!_inputCaptured)
			{
				if (!holdRightMouseCapture && Input.GetMouseButtonDown(0))
					CaptureInput();
				else if (holdRightMouseCapture && Input.GetMouseButtonDown(1))
					CaptureInput();
			}

			if (!_inputCaptured)
				return;

			if (_inputCaptured)
			{
				if (useScrollSpeedIncrease)
				{
					sprintSpeed += (Input.GetAxis(ScrollWheelKey) * speedIncreaseValue);

					if (sprintSpeed < moveSpeed)
						sprintSpeed = moveSpeed;
				}

				if (!holdRightMouseCapture && Input.GetKeyDown(KeyCode.Escape))
					ReleaseInput();

				else if (holdRightMouseCapture && Input.GetMouseButtonUp(1))
					ReleaseInput();
			}

			var rotStrafe = Input.GetAxis(MouseXKey);
			var rotFwd = Input.GetAxis(MouseYKey);

			_yaw = (_yaw + lookSpeed * rotStrafe) % 360f;

			_pitch = (_pitch - lookSpeed * rotFwd) % 360f;

			transform.rotation = Quaternion.AngleAxis(_yaw, Vector3.up) *
								 Quaternion.AngleAxis(_pitch, Vector3.right);

			transform.eulerAngles =
				new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, cameraRoll);

			var speed = Time.deltaTime *
						(Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed);

			var forward = speed * Input.GetAxis(VerticalKey);

			var right = speed * Input.GetAxis(HorizontalKey);

			var up = speed *
					 ((Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f));

			transform.position +=
				transform.forward * forward + transform.right * right + Vector3.up * up;
		}

		public void RefreshCameraRoll(float roll)
		{
			cameraRoll = roll;
			transform.eulerAngles =
				new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, cameraRoll);
		}

		[SerializeField]
		private bool enableInputCapture = true;
		[SerializeField]
		private bool lockAndHideCursor = false;
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
		private float cameraRoll = 0f;

		private bool _inputCaptured;
		private float _yaw;
		private float _pitch;

		private const string ScrollWheelKey = "Mouse ScrollWheel";
		private const string MouseXKey = "Mouse X";
		private const string MouseYKey = "Mouse Y";
		private const string VerticalKey = "Vertical";
		private const string HorizontalKey = "Horizontal";
	}
}