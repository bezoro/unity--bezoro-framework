using System;
using System.Collections.Generic;
using Bezoro.Unity.Core.Editor.PropertyDrawers;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     One-stop helper for drawing a SerializedProperty in IMGUI code.
	/// </summary>
	public static class EditorPropertyDrawUtility
	{
		/// <summary>
		///     Layout version of DrawPropertyAsFoldout.
		/// </summary>
		/// <returns>The height that was used.</returns>
		public static float DrawPropertyAsFoldoutLayout(
			SerializedProperty property,
			ref bool           foldoutState,
			GUIContent?        label = null
		)
		{
			if (property == null) return 0f;

			if (label == null)
				label = new(
					property.displayName
				);

			float totalHeight = EditorGUIUtility.singleLineHeight;

			foldoutState = EditorGUILayout.Foldout(
				foldoutState,
				label,
				true
			);

			if (foldoutState)
			{
				EditorGUI.indentLevel++;

				// Draw each visible child property
				var childProperty = property.Copy();
				var endProperty   = childProperty.GetEndProperty();

				if (childProperty.NextVisible(
						true
					))
					do
					{
						// Stop if we reach the end property
						if (SerializedProperty.EqualContents(
								childProperty,
								endProperty
							)) break;

						totalHeight += DrawPropertyLayout(
										   childProperty
									   ) +
									   EditorGUIUtility.standardVerticalSpacing;
					} while (childProperty.NextVisible(
								 false
							 ));

				EditorGUI.indentLevel--;
			}

			return totalHeight;
		}

		/// <summary>
		///     Draws an entire grid of properties with proper row and column handling
		/// </summary>
		public static float DrawPropertyGrid(
			Rect                     position,
			SerializedProperty       dataProp,
			int                      width,
			int                      height,
			string                   parentPath,
			Dictionary<string, bool> expandedStates,
			float                    indexLabelWidth = 26f,
			float                    cellPadding     = 4f,
			float                    minColumnWidth  = 50f
		)
		{
			if (width <= 0 || height <= 0 || dataProp == null) return position.y;

			// Calculate optimal column widths based on content
			var gridCalcParams = new EditorPropertyRectUtility.GridCalculationParams
			{
				DataProperty    = dataProp,
				ExpandedStates  = expandedStates,
				Height          = height,
				Width           = width,
				TotalWidth      = width,
				MinColumnWidth  = SerializableArray2DConfig.MIN_COLUMN_WIDTH,
				IndexLabelWidth = SerializableArray2DConfig.INDEX_LABEL_WIDTH
			};

			float[] columnWidths = EditorPropertyRectUtility.CalculateOptimalColumnWidths(
				gridCalcParams
			);

			// Calculate column positions with the new widths
			float[] columnPositions = EditorPropertyRectUtility.CalculateColumnPositions(
				position.x + EditorGUI.indentLevel * 15 + indexLabelWidth,
				columnWidths,
				cellPadding
			);

			// Draw column headers
			position = EditorPropertyRectUtility.NextLine(
				position
			);

			DrawColumnHeaders(
				position,
				columnPositions,
				columnWidths,
				width
			);

			// Calculate row heights
			float[] rowHeights = EditorPropertyRectUtility.CalculateRowHeights(
				gridCalcParams,
				columnWidths
			);

			// Draw grid cells
			float currentY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

			for (var row = 0; row < height; row++)
			{
				// Position for this row
				float rowHeight = rowHeights[row];
				var rowPosition = new Rect(
					position.x,
					currentY,
					position.width,
					rowHeight
				);

				// Draw row label
				EditorGUI.LabelField(
					new(
						rowPosition.x + EditorGUI.indentLevel * 15,
						rowPosition.y,
						indexLabelWidth,
						EditorGUIUtility.singleLineHeight
					),
					row.ToString(),
					EditorStyles.miniLabel
				);

				// Draw cells in this row
				for (var col = 0; col < width; col++)
				{
					int index = row * width + col;
					if (index >= dataProp.arraySize) continue;

					var cellRect = new Rect(
						columnPositions[col],
						rowPosition.y,
						columnWidths[col],
						rowHeight
					);

					var cellProperty = dataProp.GetArrayElementAtIndex(
						index
					);

					// Handle property drawing
					DrawGridCell(
						cellRect,
						cellProperty,
						parentPath,
						index,
						expandedStates
					);
				}

				// Move to next row position
				currentY += rowHeight + EditorGUIUtility.standardVerticalSpacing;
			}

			return currentY;
		}

		/// <summary>
		///     Convenience wrapper for the layout system
		///     ("EditorGUILayout"-style).
		/// </summary>
		public static float DrawPropertyLayout(
			SerializedProperty                                       property,
			GUIContent?                                              label           = null,
			bool                                                     includeChildren = true,
			Func<Rect, SerializedProperty, GUIContent, bool, float>? customDrawer    = null
		)
		{
			if (property == null) return 0f;

			if (label == null)
				label = new(
					property.displayName
				);

			float height = EditorGUI.GetPropertyHeight(
				property,
				label,
				includeChildren
			);

			var rect = EditorGUILayout.GetControlRect(
				false,
				height
			);

			if (customDrawer != null)
				return customDrawer(
					rect,
					property,
					label,
					includeChildren
				);

			EditorGUI.PropertyField(
				rect,
				property,
				label,
				includeChildren
			);

			return height;
		}

		/// <summary>
		///     Draws <paramref name="property" /> at <paramref name="position" /> and
		///     returns a Rect advanced to the next line.
		/// </summary>
		/// <param name="position">Top-left corner where the field should be drawn.</param>
		/// <param name="property">Property to draw.</param>
		/// <param name="label">
		///     Optional label. Pass <see langword="null" /> to use the property's
		///     own displayName, or <see cref="GUIContent.none" /> to draw it label-less.
		/// </param>
		/// <param name="includeChildren">
		///     Whether to show child properties (true is the usual choice).
		/// </param>
		/// <param name="customDrawer">Optional custom property drawer function.</param>
		/// <returns>
		///     A Rect whose y has been moved down by the exact amount that was
		///     just consumed, plus <see cref="EditorGUIUtility.standardVerticalSpacing" />.
		/// </returns>
		public static Rect DrawProperty(
			Rect                                                     position,
			SerializedProperty                                       property,
			GUIContent?                                              label           = null,
			bool                                                     includeChildren = true,
			Func<Rect, SerializedProperty, GUIContent, bool, float>? customDrawer    = null
		)
		{
			if (property == null) return position; // nothing to draw, nothing to advance

			// Fall back to default label handling
			if (label == null)
				label = new(
					property.displayName
				);

			float height;

			// Use custom drawer if provided
			if (customDrawer != null)
			{
				height = customDrawer(
					position,
					property,
					label,
					includeChildren
				);
			}
			else
			{
				// Let Unity tell us how tall this field really is
				height = EditorGUI.GetPropertyHeight(
					property,
					label,
					includeChildren
				);

				var fieldRect = new Rect(
					position.x,
					position.y,
					position.width,
					height
				);

				EditorGUI.PropertyField(
					fieldRect,
					property,
					label,
					includeChildren
				);
			}

			// Advance to the next line
			position.y += height + EditorGUIUtility.standardVerticalSpacing;
			return position;
		}

		/// <summary>
		///     Draws a property and all of its children as a foldout.
		/// </summary>
		/// <param name="position">Position to draw at.</param>
		/// <param name="property">Property to draw.</param>
		/// <param name="foldoutState">Reference to a boolean that tracks the foldout state.</param>
		/// <param name="label">Optional custom label.</param>
		/// <returns>Updated position rect.</returns>
		public static Rect DrawPropertyAsFoldout(
			Rect               position,
			SerializedProperty property,
			ref bool           foldoutState,
			GUIContent?        label = null
		)
		{
			if (property == null) return position;

			if (label == null)
				label = new(
					property.displayName
				);

			foldoutState = EditorGUI.Foldout(
				new(
					position.x,
					position.y,
					position.width,
					EditorGUIUtility.singleLineHeight
				),
				foldoutState,
				label,
				true
			);

			if (foldoutState)
			{
				EditorGUI.indentLevel++;
				position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

				// Draw each visible child property
				var childProperty = property.Copy();
				var endProperty   = childProperty.GetEndProperty();

				if (childProperty.NextVisible(
						true
					))
					do
					{
						// Stop if we reach the end property
						if (SerializedProperty.EqualContents(
								childProperty,
								endProperty
							)) break;

						position = DrawProperty(
							position,
							childProperty
						);
					} while (childProperty.NextVisible(
								 false
							 ));

				EditorGUI.indentLevel--;
			}
			else
			{
				position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
			}

			return position;
		}

		/// <summary>
		///     Draws all properties of a serialized object.
		/// </summary>
		/// <param name="serializedObject">The serialized object whose properties to draw.</param>
		/// <param name="drawScript">Whether to draw the Script field.</param>
		public static void DrawAllProperties(SerializedObject serializedObject, bool drawScript = false)
		{
			if (serializedObject == null) return;

			serializedObject.Update();

			var iterator      = serializedObject.GetIterator();
			var enterChildren = true;

			while (iterator.NextVisible(
					   enterChildren
				   ))
			{
				enterChildren = false;

				// Skip script field if not wanted
				if (!drawScript && iterator.propertyPath == "m_Script") continue;

				DrawPropertyLayout(
					iterator
				);
			}

			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		///     Draws all properties of a serialized object within a vertical box.
		/// </summary>
		/// <param name="serializedObject">The serialized object whose properties to draw.</param>
		/// <param name="title">Optional title for the box.</param>
		/// <param name="drawScript">Whether to draw the Script field.</param>
		public static void DrawAllPropertiesInBox(
			SerializedObject serializedObject,
			string?          title      = null,
			bool             drawScript = false
		)
		{
			if (serializedObject == null) return;

			EditorGUILayout.BeginVertical(
				GUI.skin.box
			);

			if (!string.IsNullOrEmpty(
					title
				))
				EditorGUILayout.LabelField(
					title,
					EditorStyles.boldLabel
				);

			DrawAllProperties(
				serializedObject,
				drawScript
			);

			EditorGUILayout.EndVertical();
		}

		/// <summary>
		///     Draws column headers for a grid
		/// </summary>
		public static void DrawColumnHeaders(Rect position, float[] columnPositions, float[] columnWidths, int columns)
		{
			for (var col = 0; col < columns; col++)
			{
				var colHeaderRect = new Rect(
					columnPositions[col],
					position.y,
					columnWidths[col],
					EditorGUIUtility.singleLineHeight
				);

				var centeredStyle = new GUIStyle(
					EditorStyles.miniLabel
				) { alignment = TextAnchor.MiddleCenter };

				EditorGUI.LabelField(
					colHeaderRect,
					col.ToString(),
					centeredStyle
				);
			}
		}

		/// <summary>
		///     Draws a cell in a grid and tracks its expanded state
		/// </summary>
		public static void DrawGridCell(
			Rect                     position,
			SerializedProperty       property,
			string                   parentPath,
			int                      index,
			Dictionary<string, bool> expandedStates
		)
		{
			// Create a unique key for this property
			string? propertyPath = property.propertyPath;

			// Store previous expanded state
			bool wasExpanded = expandedStates.TryGetValue(
								   propertyPath,
								   out bool state
							   ) &&
							   state;

			// Draw the property field
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(
				position,
				property,
				GUIContent.none,
				true
			);

			// Track expanded state changes
			if (EditorGUI.EndChangeCheck() && property.hasChildren)
			{
				bool isNowExpanded = property.isExpanded;
				expandedStates[propertyPath] = isNowExpanded;

				// If expansion state changed, force a repaint to recalculate layout
				if (wasExpanded != isNowExpanded)
					EditorUtility.SetDirty(
						property.serializedObject.targetObject
					);
			}
			else if (property.hasChildren &&
					 !expandedStates.ContainsKey(
						 propertyPath
					 ))
			{
				expandedStates[propertyPath] = property.isExpanded;
			}
		}
	}
}
