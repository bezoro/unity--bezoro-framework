#nullable enable

using Bezoro.Unity.Core.Attributes;
using TMPro;
using UnityEngine;

namespace Bezoro.Unity.Core.UI
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	public sealed class TextBubbleView : MonoBehaviour
	{
		[EditorGetComponent(typeof(TMP_Text), true, true)]
		[SerializeField] private TMP_Text? text;

		[EditorGetComponent(typeof(CanvasGroup), true, true)]
		[SerializeField] private CanvasGroup? canvasGroup;

		private RectTransform? _rectTransform;

		public RectTransform RectTransform
		{
			get
			{
				ResolveReferences();
				return _rectTransform!;
			}
		}

		public string CurrentText { get; private set; } = string.Empty;
		public bool   IsVisible   { get; private set; }

		#region Unity Lifecycle

		private void Awake()
		{
			ResolveReferences();
		}

		private void Reset()
		{
			ResolveReferences();
		}

		#endregion

		public void SetText(string value)
		{
			CurrentText = value ?? string.Empty;
			ResolveReferences();

			if (text != null)
				text.text = CurrentText;
		}

		public void SetAnchoredPosition(Vector2 anchoredPosition)
		{
			RectTransform.anchoredPosition = anchoredPosition;
		}

		public void SetVisible(bool visible)
		{
			IsVisible = visible;

			ResolveReferences();

			if (canvasGroup != null)
			{
				canvasGroup.alpha          = visible ? 1f : 0f;
				canvasGroup.interactable   = false;
				canvasGroup.blocksRaycasts = false;
			}

			if (visible && !gameObject.activeSelf)
				gameObject.SetActive(true);
			else if (!visible && gameObject.activeSelf)
				gameObject.SetActive(false);
		}

		public void ResetView()
		{
			ResolveReferences();
			SetText(string.Empty);
			SetVisible(false);
			RectTransform.anchoredPosition = Vector2.zero;
		}

		private void ResolveReferences()
		{
			if (_rectTransform == null)
				_rectTransform = GetComponent<RectTransform>();

			if (text == null)
				text = GetComponentInChildren<TMP_Text>(true);

			if (canvasGroup == null)
				canvasGroup = GetComponent<CanvasGroup>();
		}
	}
}
