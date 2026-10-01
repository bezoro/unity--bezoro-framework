using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Calculates inspector rects, widths, and heights for custom property grids.
	/// </summary>
	public static class EditorPropertyRectUtility
	{
		private const float COLOR_WIDTH              = 120f;
		private const float DEFAULT_BORDER_THICKNESS = 1f;
		// Constants for better maintainability
		private const float DEFAULT_CELL_PADDING        = 4f;
		private const float DEFAULT_MIN_WIDTH           = 100f;
		private const float ENUM_WIDTH                  = 100f;
		private const float EXPANDED_FIELD_EXTRA_WIDTH  = 100f;
		private const float EXPANDED_FIELD_INDENT_WIDTH = 15f;
		private const float INDENT_PER_LEVEL            = 15f;

		// Width constants for different property types
		private const float INTEGER_FLOAT_BOOL_WIDTH = 60f;
		private const float OBJECT_REFERENCE_WIDTH   = 150f;
		private const float RECT_WIDTH               = 200f;
		private const float STRING_WIDTH             = 120f;
		private const float VECTOR2_WIDTH            = 120f;
		private const float VECTOR3_WIDTH            = 180f;
		private const float VECTOR4_WIDTH            = 220f;

		// Calculate the height needed for a property
		public static float GetElementHeight(SerializedProperty property)
		{
			if (property == null) return EditorGUIUtility.singleLineHeight;

			if (property.propertyType != SerializedPropertyType.Generic) return EditorGUIUtility.singleLineHeight;

			return CalculateGenericPropertyHeight(
				property
			);
		}

		// Calculate the minimum width needed for a property
		public static float GetElementWidth(SerializedProperty property, bool isExpanded = false)
		{
			if (property == null) return DEFAULT_MIN_WIDTH;

			if (!isExpanded || property.propertyType != SerializedPropertyType.Generic)
				return GetPropertyTypeWidth(
					property.propertyType
				);

			return CalculateExpandedPropertyWidth(
				property
			);
		}

		// Get height for a property considering its expanded state
		public static float GetExpandedPropertyHeight(SerializedProperty property, bool isExpanded)
		{
			if (property == null) return EditorGUIUtility.singleLineHeight;

			if (!isExpanded)
				return EditorGUI.GetPropertyHeight(
					property,
					true
				);

			if (property.propertyType == SerializedPropertyType.Generic)
				return CalculateExpandedGenericHeight(
					property
				);

			if (property.isArray)
				return CalculateExpandedArrayHeight(
					property
				);

			return EditorGUI.GetPropertyHeight(
				property,
				true
			);
		}

		// Calculate column positions based on widths
		public static float[] CalculateColumnPositions(
			float   startX,
			float[] columnWidths,
			float   cellPadding = DEFAULT_CELL_PADDING)
		{
			var positions = new float[columnWidths.Length];
			positions[0] = startX;
			for (var i = 1; i < columnWidths.Length; i++)
				positions[i] = positions[i - 1] + columnWidths[i - 1] + cellPadding;

			return positions;
		}

		// Calculate optimal column widths for a grid
		public static float[] CalculateOptimalColumnWidths(GridCalculationParams parameters)
		{
			float[] columnWidths = InitializeColumnWidths(
				parameters.Width,
				parameters.MinColumnWidth
			);

			// Find required width for each column based on content
			for (var col = 0; col < parameters.Width; col++)
			{
				columnWidths[col] = CalculateMaxColumnWidth(
					parameters,
					col
				);
			}

			// Adjust column widths to fit available width
			AdjustColumnWidthsToFit(
				columnWidths,
				parameters.Width,
				parameters.TotalWidth,
				parameters.IndexLabelWidth,
				parameters.MinColumnWidth
			);

			return columnWidths;
		}

		// Calculate row heights for a grid
		public static float[] CalculateRowHeights(GridCalculationParams parameters, float[] columnWidths)
		{
			var rowHeights = new float[parameters.Height];
			for (var row = 0; row < parameters.Height; row++)
			{
				rowHeights[row] = CalculateMaxRowHeight(
					parameters,
					row
				);
			}

			return rowHeights;
		}

		// Helper – get Rect for the next line down
		public static Rect NextLine(Rect r)
		{
			r.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			return r;
		}

		public static void DrawBorder(in Rect rect, in Color color, float thickness = DEFAULT_BORDER_THICKNESS)
		{
			// Top
			EditorGUI.DrawRect(
				new(
					rect.x,
					rect.y,
					rect.width,
					thickness
				),
				color
			);

			// Left
			EditorGUI.DrawRect(
				new(
					rect.x,
					rect.y,
					thickness,
					rect.height
				),
				color
			);

			// Bottom
			EditorGUI.DrawRect(
				new(
					rect.x,
					rect.yMax - thickness,
					rect.width,
					thickness
				),
				color
			);

			// Right
			EditorGUI.DrawRect(
				new(
					rect.xMax - thickness,
					rect.y,
					thickness,
					rect.height
				),
				color
			);
		}

		private static bool IsPropertyExpanded(
			SerializedProperty       dataProp,
			int                      index,
			Dictionary<string, bool> expandedStates)
		{
			if (index < 0 || index >= dataProp.arraySize)
				return false;

			string? propertyPath = dataProp.GetArrayElementAtIndex(index).propertyPath;
			return expandedStates.TryGetValue(
					   propertyPath,
					   out bool state
				   ) &&
				   state;
		}

		private static float CalculateExpandedArrayHeight(SerializedProperty property)
		{
			float totalHeight = EditorGUIUtility.singleLineHeight;
			for (var i = 0; i < property.arraySize; i++)
			{
				totalHeight += EditorGUIUtility.standardVerticalSpacing;
				var element = property.GetArrayElementAtIndex(
					i
				);

				totalHeight += EditorGUI.GetPropertyHeight(
					element,
					true
				);
			}

			totalHeight += EditorGUIUtility.standardVerticalSpacing;
			totalHeight += EditorGUIUtility.singleLineHeight;
			return totalHeight;
		}

		private static float CalculateExpandedGenericHeight(SerializedProperty property)
		{
			float totalHeight = EditorGUIUtility.singleLineHeight;
			var   iterator    = property.Copy();
			var   endProperty = iterator.GetEndProperty();

			if (iterator.NextVisible(
					true
				))
				do
				{
					if (SerializedProperty.EqualContents(
							iterator,
							endProperty
						)) break;

					totalHeight += EditorGUIUtility.standardVerticalSpacing;
					totalHeight += EditorGUI.GetPropertyHeight(
						iterator,
						true
					);
				} while (iterator.NextVisible(
							 false
						 ));

			return totalHeight;
		}

		private static float CalculateExpandedPropertyWidth(SerializedProperty property)
		{
			float maxWidth    = DEFAULT_MIN_WIDTH;
			var   iterator    = property.Copy();
			var   endProperty = iterator.GetEndProperty();
			var   style       = EditorStyles.label;

			if (iterator.NextVisible(
					true
				))
				do
				{
					if (SerializedProperty.EqualContents(
							iterator,
							endProperty
						)) break;

					float fieldWidth = style.CalcSize(
						new(
							iterator.displayName
						)
					).x;

					fieldWidth += EXPANDED_FIELD_EXTRA_WIDTH + EXPANDED_FIELD_INDENT_WIDTH;
					maxWidth = Mathf.Max(
						maxWidth,
						fieldWidth
					);
				} while (iterator.NextVisible(
							 false
						 ));

			return maxWidth;
		}

		private static float CalculateGenericPropertyHeight(SerializedProperty property)
		{
			float height   = 0;
			var   copy     = property.Copy();
			var   iterator = copy.Copy();

			if (iterator.NextVisible(
					true
				))
				do
				{
					if (SerializedProperty.EqualContents(
							iterator,
							property
						) ||
						!iterator.propertyPath.StartsWith(
							property.propertyPath + "."
						))
						break;

					height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				} while (iterator.NextVisible(
							 false
						 ));

			return height > 0 ? height : EditorGUIUtility.singleLineHeight;
		}

		private static float CalculateMaxColumnWidth(GridCalculationParams parameters, int col)
		{
			float maxWidth = parameters.MinColumnWidth;
			for (var row = 0; row < parameters.Height; row++)
			{
				int index = row * parameters.Width + col;
				if (index >= parameters.DataProperty.arraySize) continue;

				var element = parameters.DataProperty.GetArrayElementAtIndex(
					index
				);

				bool isExpanded = IsPropertyExpanded(
					parameters.DataProperty,
					index,
					parameters.ExpandedStates
				);

				float requiredWidth = GetElementWidth(
					element,
					isExpanded
				);

				maxWidth = Mathf.Max(
					maxWidth,
					requiredWidth
				);
			}

			return maxWidth;
		}

		private static float CalculateMaxRowHeight(GridCalculationParams parameters, int row)
		{
			float maxHeight = EditorGUIUtility.singleLineHeight;
			for (var col = 0; col < parameters.Width; col++)
			{
				int index = row * parameters.Width + col;
				if (index >= parameters.DataProperty.arraySize) continue;

				var element = parameters.DataProperty.GetArrayElementAtIndex(
					index
				);

				bool isExpanded = IsPropertyExpanded(
					parameters.DataProperty,
					index,
					parameters.ExpandedStates
				);

				float elementHeight = GetExpandedPropertyHeight(
					element,
					isExpanded
				);

				maxHeight = Mathf.Max(
					maxHeight,
					elementHeight
				);
			}

			return maxHeight;
		}

		private static float CalculateTotalContentWidth(float[] columnWidths, int width)
		{
			float totalContentWidth                                 = 0;
			for (var col = 0; col < width; col++) totalContentWidth += columnWidths[col];

			return totalContentWidth + (width - 1) * DEFAULT_CELL_PADDING;
		}

		private static float GetPropertyTypeWidth(SerializedPropertyType propertyType) =>
			propertyType switch
			{
				SerializedPropertyType.Integer or SerializedPropertyType.Float or SerializedPropertyType.Boolean =>
					INTEGER_FLOAT_BOOL_WIDTH,
				SerializedPropertyType.String          => STRING_WIDTH,
				SerializedPropertyType.Enum            => ENUM_WIDTH,
				SerializedPropertyType.Vector2         => VECTOR2_WIDTH,
				SerializedPropertyType.Vector3         => VECTOR3_WIDTH,
				SerializedPropertyType.Vector4         => VECTOR4_WIDTH,
				SerializedPropertyType.Rect            => RECT_WIDTH,
				SerializedPropertyType.Color           => COLOR_WIDTH,
				SerializedPropertyType.ObjectReference => OBJECT_REFERENCE_WIDTH,
				_                                      => DEFAULT_MIN_WIDTH
			};

		private static float[] InitializeColumnWidths(int width, float minColumnWidth)
		{
			var columnWidths                                        = new float[width];
			for (var col = 0; col < width; col++) columnWidths[col] = minColumnWidth;

			return columnWidths;
		}

		private static void AdjustColumnWidthsToFit(
			float[] columnWidths,
			int     width,
			float   totalWidth,
			float   indexLabelWidth,
			float   minColumnWidth)
		{
			float totalContentWidth = CalculateTotalContentWidth(
				columnWidths,
				width
			);

			float availableWidth = totalWidth - indexLabelWidth - EditorGUI.indentLevel * INDENT_PER_LEVEL;

			if (totalContentWidth > availableWidth)
			{
				float scaleFactor = availableWidth / totalContentWidth;
				for (var col = 0; col < width; col++)
				{
					columnWidths[col] = Mathf.Max(
						minColumnWidth,
						columnWidths[col] * scaleFactor
					);
				}
			}
		}

		/// <summary>
		///     Collects inputs used to calculate custom property grid row and column sizes.
		/// </summary>
		public struct GridCalculationParams
		{
			public Dictionary<string, bool> ExpandedStates  { get; set; }
			public float                    IndexLabelWidth { get; set; }
			public float                    MinColumnWidth  { get; set; }
			public float                    TotalWidth      { get; set; }
			public int                      Height          { get; set; }
			public int                      Width           { get; set; }
			public SerializedProperty       DataProperty    { get; set; }
		}
	}
}
