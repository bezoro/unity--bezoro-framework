using Bezoro.Unity.Core.Attributes;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.PropertyDrawers;

[CustomPropertyDrawer(
	typeof(CurveBoundsAttribute)
)]
/// <summary>
/// Draws curve fields constrained by CurveBoundsAttribute.
/// </summary>
public sealed class CurveBoundsDrawer : PropertyDrawer
{
	private static readonly GUIContent LockIcon =
		EditorGUIUtility.IconContent(
			"LockIcon-On",
			"Keys are clamped to bounds"
		);

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
		EditorGUIUtility.singleLineHeight;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var attr = (CurveBoundsAttribute)attribute;

		// ------------------------------------------------------------------
		// 1. Draw the curve (with or without locked view rect)
		// ------------------------------------------------------------------
		EditorGUI.BeginChangeCheck();

		var newCurve = attr.LockViewRect
						   ? EditorGUI.CurveField(
							   position,
							   label,
							   property.animationCurveValue,
							   Color.green,
							   attr.ViewRect
						   )
						   : EditorGUI.CurveField(
							   position,
							   label,
							   property.animationCurveValue
						   );

		// ------------------------------------------------------------------
		// 2. Clamp keys that were moved by the user
		// ------------------------------------------------------------------
		if (EditorGUI.EndChangeCheck())
		{
			for (var i = 0; i < newCurve.length; i++)
			{
				var k = newCurve.keys[i];
				k.time = Mathf.Clamp(
					k.time,
					attr.MinTime,
					attr.MaxTime
				);

				k.value = Mathf.Clamp(
					k.value,
					attr.MinValue,
					attr.MaxValue
				);

				newCurve.MoveKey(
					i,
					k
				);
			}

			property.animationCurveValue = newCurve;
		}

		// ------------------------------------------------------------------
		// 3. Visual overlay (border + tiny lock icon)
		// ------------------------------------------------------------------
		if (attr.ShowOverlay)
		{
			// The preview area (excluding the label) so the border hugs the curve
			var previewRect = position;
			previewRect.xMin += EditorGUIUtility.labelWidth;

			// Semi-transparent fill to make it pop in dark & light skins alike
			var fillCol = new Color(
				0f,
				1f,
				0f,
				0.05f
			);

			EditorGUI.DrawRect(
				previewRect,
				fillCol
			);

			// Solid border
			Handles.BeginGUI();
			Handles.color = new(
				0f,
				1f,
				0f,
				0.6f
			);

			Handles.DrawAAPolyLine(
				2f,
				new Vector3(
					previewRect.xMin,
					previewRect.yMin
				),
				new Vector3(
					previewRect.xMax,
					previewRect.yMin
				),
				new Vector3(
					previewRect.xMax,
					previewRect.yMax
				),
				new Vector3(
					previewRect.xMin,
					previewRect.yMax
				),
				new Vector3(
					previewRect.xMin,
					previewRect.yMin
				)
			);

			Handles.EndGUI();

			// Lock icon in the top-right corner of the preview
			const float ICON_SIZE = 13f;
			var iconRect = new Rect(
				previewRect.xMax - ICON_SIZE,
				previewRect.yMin,
				ICON_SIZE,
				ICON_SIZE
			);

			GUI.Label(
				iconRect,
				LockIcon
			);
		}
	}
}
