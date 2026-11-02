using Bezoro.Unity.Core.Attributes;
using Bezoro.Unity.Core.Loading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Updates loading screen fill and percent text from LoadingProgressService progress.
	/// </summary>
	public class LoadingScreen : MonoBehaviour
	{
		private readonly LoadingScreenModel _model = new();

		[EditorGetComponent(
			typeof(Image),
			true
		)]
		[SerializeField] private Image? image;

		[EditorGetComponent(
			typeof(TMP_Text),
			true
		)]
		[SerializeField] private TMP_Text? text;

		#region Unity Lifecycle

		private void Reset() =>
			ResolveReferences();

		#endregion

		private void OnOverallProgressChanged(float value)
		{
			if (image == null || text == null)
				return;

			var presentation = _model.GetProgressPresentation(value, image.fillAmount);

			image.fillAmount = presentation.FillAmount;
			text.text        = presentation.PercentText;
		}

		private void ResolveReferences()
		{
			image ??= GetComponentInChildren<Image>(true);
			text  ??= GetComponentInChildren<TMP_Text>(true);
		}

		#region Unity lifecycle

		private void Awake()
		{
			ResolveReferences();

			if (image == null || text == null)
			{
				Debug.LogError($"{nameof(LoadingScreen)} requires an Image and TMP_Text reference.", this);
				enabled = false;
				return;
			}

			var presentation = _model.GetInitialPresentation();
			image.fillAmount = presentation.FillAmount;
			text.text        = presentation.PercentText;
		}

		private void OnEnable()
		{
			ResolveReferences();

			if (image == null || text == null)
			{
				enabled = false;
				return;
			}

			if (!LoadingProgressService.TryGetInstance(
					out var loadingProgressService
				)) return;

			loadingProgressService.ProgressChanged += OnOverallProgressChanged;
		}

		private void OnDisable()
		{
			if (!LoadingProgressService.TryGetInstance(
					out var loadingProgressService
				)) return;

			loadingProgressService.ProgressChanged -= OnOverallProgressChanged;
		}

		#endregion
	}

	public readonly struct LoadingScreenProgressPresentation
	{
		public LoadingScreenProgressPresentation(float fillAmount, string percentText)
		{
			FillAmount  = fillAmount;
			PercentText = percentText;
		}

		public float FillAmount { get; }
		public string PercentText { get; }
	}

	public sealed class LoadingScreenModel
	{
		public LoadingScreenProgressPresentation GetInitialPresentation() =>
			new(0f, GetPercentText(0f));

		public LoadingScreenProgressPresentation GetProgressPresentation(float requestedProgress, float currentFillAmount)
		{
			float fillAmount = Mathf.Min(1f, Mathf.Max(requestedProgress, currentFillAmount));
			return new(fillAmount, GetPercentText(fillAmount));
		}

		private static string GetPercentText(float fillAmount) =>
			$"{Mathf.RoundToInt(fillAmount * 100f)} %";
	}
}
