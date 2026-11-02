using Bezoro.Unity.Core.Editor.SceneView;
using UnityEditor.Overlays;
using UnityEngine.UIElements;

namespace Bezoro.Unity.Core.Editor.Overlays
{
	/// <summary>
	///     SceneView overlay for toggling automatic selection focus independently
	///     from the broader Game Settings overlay.
	/// </summary>
	[Overlay(typeof(UnityEditor.SceneView), "Scene View Auto Focus")]
	public class CanvasAutoFocusOverlay : Overlay
	{
		private const int    ITEM_SPACING = 5;
		private const int    ROOT_PADDING = 10;

		private Toggle?       _focus3DObjectsToggle;
		private Toggle?       _focusCanvasesUiToggle;

		/// <summary>
		///     Creates the overlay root for the SceneView auto-focus editor preferences.
		/// </summary>
		public override VisualElement CreatePanelContent()
		{
			var root = CreateRoot();
			BuildContent(root);
			return root;
		}

		private static Toggle CreateToggle(string label, bool value, EventCallback<ChangeEvent<bool>> callback)
		{
			var toggle = new Toggle(label)
			{
				value = value,
				style = { marginBottom = ITEM_SPACING }
			};

			toggle.RegisterValueChangedCallback(callback);
			return toggle;
		}

		private static VisualElement CreateRoot() =>
			new()
			{
				style =
				{
					paddingTop    = ROOT_PADDING,
					paddingBottom = ROOT_PADDING,
					paddingLeft   = ROOT_PADDING,
					paddingRight  = ROOT_PADDING
				}
			};

		private void BuildContent(VisualElement root)
		{
			root.Clear();
			root.Add(CreateToggle("Enabled", SceneViewSelectionAutoFocus.Enabled, OnCanvasAutoFocusToggleChanged));

			_focusCanvasesUiToggle = CreateToggle(
				"Canvases/UI", SceneViewSelectionAutoFocus.FocusCanvasesUi, OnFocusCanvasesUiToggleChanged
			);

			_focus3DObjectsToggle = CreateToggle(
				"3D Objects", SceneViewSelectionAutoFocus.Focus3DObjects, OnFocus3DObjectsToggleChanged
			);

			root.Add(_focusCanvasesUiToggle);
			root.Add(_focus3DObjectsToggle);
		}

		private void OnCanvasAutoFocusToggleChanged(ChangeEvent<bool> evt)
		{
			SceneViewSelectionAutoFocus.Enabled = evt.newValue;
		}

		private void OnFocus3DObjectsToggleChanged(ChangeEvent<bool> evt)
		{
			SceneViewSelectionAutoFocus.Focus3DObjects = evt.newValue;
			_focus3DObjectsToggle?.SetValueWithoutNotify(evt.newValue);
		}

		private void OnFocusCanvasesUiToggleChanged(ChangeEvent<bool> evt)
		{
			SceneViewSelectionAutoFocus.FocusCanvasesUi = evt.newValue;
			_focusCanvasesUiToggle?.SetValueWithoutNotify(evt.newValue);
		}
	}
}
