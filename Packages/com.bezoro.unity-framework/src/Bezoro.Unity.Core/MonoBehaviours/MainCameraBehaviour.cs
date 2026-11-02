using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Bezoro.Unity.Core.Attributes;
using Bezoro.Unity.Core.Common.Enums;
using Bezoro.Unity.Core.Common.Extensions;
using Bezoro.Unity.Core.Loading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Keeps the main camera overlay alive and fades it around scene loading.
	/// </summary>
	[SelectionBase]
	[RequireComponent(typeof(SingletonScope), typeof(GlobalReference))]
	[DefaultExecutionOrder(ExecutionOrder.Manager)]
	public class MainCameraBehaviour : MonoBehaviour
	{
		[SerializeField] private float fadeStartDelay = 1f;
		[SerializeField] private Color fadeColor      = Color.black;
		[SerializeField] private float fadeDuration   = 1f;
		[SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(
			0,
			0,
			1,
			1
		);
		[SerializeField] private float fadeOutDelay;

		[EditorGetComponent(
			typeof(Canvas),
			true,
			true
		)]
		[SerializeField] private Canvas? canvas;

		[EditorGetComponent(
			typeof(Image),
			true,
			true
		)]
		[SerializeField] private Image? fadeImage;

		private CancellationTokenSource _fadeCts = new();
		private readonly MainCameraModel _model = new();
		private bool _isSingletonInstance;

		public static event Action? FadeInEnded;
		public static event Action? FadeInStarted;
		public static event Action? FadeOutEnded;
		public static event Action? FadeOutStarted;

		public static bool IsAvailable => TryGetInstance(out _);

		public static MainCameraBehaviour Instance => GlobalReferenceAccess<MainCameraBehaviour>.Instance;

		public static bool TryGetInstance([NotNullWhen(true)] out MainCameraBehaviour? instance) =>
			GlobalReferenceAccess<MainCameraBehaviour>.TryGetInstance(out instance);

		#region Unity Lifecycle

		private void Awake()
		{
			_isSingletonInstance = GlobalReferenceAccess<MainCameraBehaviour>.IsCurrent(this);

			if (!_isSingletonInstance)
				return;

			var camera = GetComponent<Camera>();
			if (camera != null)
				CameraManager.Register(camera);

			if (_model.ShouldActivateCanvas(canvas != null && !canvas.gameObject.IsUnityNull()))
				canvas!.gameObject.SetActive(
					true
				);

			if (_model.ShouldActivateFadeImage(!fadeImage.IsUnityNull()))
				fadeImage!.gameObject.SetActive(true);

			if (_model.ShouldApplyFadeColor(!fadeImage.IsUnityNull()))
				fadeImage!.color = fadeColor;

		}

		private void OnEnable()
		{
			LoadingScreenManager.LoadStarted += OnSceneLoadStarted;

			if (LoadingProgressService.TryGetInstance(
					out var loadingProgressService
				))
				loadingProgressService.Completed += OnLoadingCompleted;
		}

		private void OnDisable()
		{
			LoadingScreenManager.LoadStarted -= OnSceneLoadStarted;
			var camera = GetComponent<Camera>();
			if (camera != null)
				CameraManager.Unregister(camera);

			if (LoadingProgressService.TryGetInstance(out var loadingProgressService))
				loadingProgressService.Completed -= OnLoadingCompleted;
		}

		private void OnDestroy()
		{
			if (!_isSingletonInstance)
				return;

			_fadeCts.Cancel();
			_fadeCts.Dispose();
			FadeInEnded    = null;
			FadeInStarted  = null;
			FadeOutEnded   = null;
			FadeOutStarted = null;
		}

		#endregion

		private async UniTask DoFadeOut(CancellationToken ct)
		{
			var targetCanvas = canvas;
			var hasCanvas = targetCanvas is not null && !targetCanvas.IsUnityNull();
			var hasCanvasGameObject = false;

			if (targetCanvas is not null)
				hasCanvasGameObject = hasCanvas && !targetCanvas.gameObject.IsUnityNull();

			if (!_model.ShouldFadeOut(hasCanvas, hasCanvasGameObject))
				return;

			if (targetCanvas is null)
				return;

			FadeOutStarted?.Invoke();

			var targetFadeImage = fadeImage;
			if (targetFadeImage is not null && _model.ShouldFadeImage(!targetFadeImage.IsUnityNull()))
				await targetFadeImage.DoFadeToAlpha(0, fadeDuration, fadeStartDelay, fadeCurve, ct);

			targetCanvas.gameObject.SetActive(false);
			FadeOutEnded?.Invoke();
		}

		private async void OnLoadingCompleted()
		{
			_fadeCts.Cancel();
			_fadeCts.Dispose();
			_fadeCts = new();
			var ct = _fadeCts.Token;

			try
			{
				await UniTask.WaitForSeconds(fadeOutDelay, cancellationToken: ct);
			}
			catch (OperationCanceledException)
			{
				return;
			}

			DoFadeOut(ct).Forget();
		}

		private void OnSceneLoadStarted()
		{
			_fadeCts.Cancel();
			FadeInStarted?.Invoke();

			var targetFadeImage = fadeImage;
			if (targetFadeImage is not null && _model.ShouldResetFadeAlpha(!targetFadeImage.IsUnityNull()))
			{
				var color = targetFadeImage.color;
				color.a         = _model.GetFadeInAlpha();
				targetFadeImage.color = color;
			}

			if (_model.ShouldActivateCanvas(canvas != null && !canvas.gameObject.IsUnityNull()))
				canvas!.gameObject.SetActive(true);

			FadeInEnded?.Invoke();
		}
	}

	public sealed class MainCameraModel
	{
		public bool ShouldActivateCanvas(bool hasCanvasGameObject)
		{
			return hasCanvasGameObject;
		}

		public bool ShouldActivateFadeImage(bool hasFadeImage)
		{
			return hasFadeImage;
		}

		public bool ShouldApplyFadeColor(bool hasFadeImage)
		{
			return hasFadeImage;
		}

		public bool ShouldFadeOut(bool hasCanvas, bool hasCanvasGameObject)
		{
			return hasCanvas && hasCanvasGameObject;
		}

		public bool ShouldFadeImage(bool hasFadeImage)
		{
			return hasFadeImage;
		}

		public bool ShouldResetFadeAlpha(bool hasFadeImage)
		{
			return hasFadeImage;
		}

		public float GetFadeInAlpha()
		{
			return 1f;
		}
	}
}
