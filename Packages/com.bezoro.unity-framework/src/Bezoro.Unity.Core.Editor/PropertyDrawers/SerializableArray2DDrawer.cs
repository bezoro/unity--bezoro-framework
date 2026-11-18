using System.Collections.Generic;
using Bezoro.Unity.Core.Collections;
using Bezoro.Unity.Core.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.PropertyDrawers
{
	/// <summary>
	///     Stores layout constants and labels for SerializableArray2D inspector rendering.
	/// </summary>
	public class SerializableArray2DConfig
	{
		public const float INDEX_LABEL_WIDTH                  = 26f;
		public const float INVALID_DIMENSIONS_HELP_BOX_HEIGHT = 40f;
		public const float MIN_COLUMN_WIDTH                   = 50f;
		public static readonly GUIContent HeightLabel = new(
			"Height"
		);

		public static readonly GUIContent WidthLabel = new(
			"Width"
		);
	}

	[CustomPropertyDrawer(
		typeof(SerializableArray2D<>),
		true
	)]
	/// <summary>
	/// Draws SerializableArray2D values as editable width, height, and element grids.
	/// </summary>
	public class SerializableArray2DDrawer : PropertyDrawer
	{
		private readonly Dictionary<string, bool>            _expandedStates   = new();
		private readonly SerializableArray2DConfig           _config           = new();
		private readonly SerializableArray2DLayoutCalculator _layoutCalculator = new();
		private readonly SerializableArray2DRenderer         _renderer         = new();

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			property.serializedObject.isEditingMultipleObjects ? EditorGUIUtility.singleLineHeight :
			_layoutCalculator.CalculatePropertyHeight(
				property,
				EditorGUIUtility.currentViewWidth,
				_expandedStates,
				_config
			);

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.serializedObject.isEditingMultipleObjects)
			{
				EditorGUI.LabelField(position, label, new GUIContent("Select one object to edit this grid."));
				return;
			}

			var foldoutRect = DrawFoldoutHeader(
				position,
				property,
				label
			);

			if (!property.isExpanded) return;

			EditorGUI.indentLevel++;

			var drawPosition = _renderer.DrawDimensionFields(
				foldoutRect,
				property
			);

			var properties = GetArray2DProperties(property);
			var dimensions = GetValidatedDimensions(
				properties
			);

			if (!dimensions.IsValid())
			{
				_renderer.DrawInvalidDimensionsHelp(
					drawPosition
				);

				EditorGUI.indentLevel--;
				return;
			}

			_renderer.DrawPropertyGrid(
				drawPosition,
				properties.data,
				dimensions,
				property.propertyPath,
				_expandedStates,
				_config
			);

			EditorGUI.indentLevel--;
		}

		private (SerializedProperty width, SerializedProperty height, SerializedProperty data) GetArray2DProperties(
			SerializedProperty property) =>
			(
				property.FindPropertyRelative(
					"width"
				),
				property.FindPropertyRelative(
					"height"
				),
				property.FindPropertyRelative(
					"elements"
				)
			);

		private Array2DDimensions GetValidatedDimensions(
			(SerializedProperty width, SerializedProperty height, SerializedProperty data) properties)
		{
			int width = Mathf.Max(
				0,
				properties.width.intValue
			);

			int height = Mathf.Max(
				0,
				properties.height.intValue
			);

			return new(
				width,
				height
			);
		}

		private Rect DrawFoldoutHeader(Rect position, SerializedProperty property, GUIContent label)
		{
			position.height = EditorGUIUtility.singleLineHeight;
			property.isExpanded = EditorGUI.Foldout(
				position,
				property.isExpanded,
				label,
				true
			);

			return position;
		}
	}

	/// <summary>
	///     Calculates inspector height for expanded SerializableArray2D grids.
	/// </summary>
	public class SerializableArray2DLayoutCalculator
	{
		public float CalculatePropertyHeight(
			SerializedProperty        property,
			float                     totalWidth,
			Dictionary<string, bool>  expandedStates,
			SerializableArray2DConfig config)
		{
			if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;

			var properties =
				GetArray2DProperties(
					property
				);

			var dimensions = GetDimensions(
				properties
			);

			float height = CalculateBaseHeight(
				properties
			);

			if (!dimensions.IsValid()) return height + SerializableArray2DConfig.INVALID_DIMENSIONS_HELP_BOX_HEIGHT;

			return height +
				   CalculateGridHeight(
					   properties.data,
					   dimensions,
					   totalWidth,
					   expandedStates,
					   config
				   );
		}

		private (SerializedProperty width, SerializedProperty height, SerializedProperty data) GetArray2DProperties(
			SerializedProperty property) =>
			(
				property.FindPropertyRelative(
					"width"
				),
				property.FindPropertyRelative(
					"height"
				),
				property.FindPropertyRelative(
					"elements"
				)
			);

		private Array2DDimensions GetDimensions(
			(SerializedProperty width, SerializedProperty height, SerializedProperty data) properties)
		{
			int width = Mathf.Max(
				0,
				properties.width.intValue
			);

			int height = Mathf.Max(
				0,
				properties.height.intValue
			);

			return new(
				width,
				height
			);
		}

		private float CalculateBaseHeight(
			(SerializedProperty width, SerializedProperty height, SerializedProperty data) properties)
		{
			float spacing = EditorGUIUtility.standardVerticalSpacing;
			float height  = EditorGUIUtility.singleLineHeight; // Foldout line

			height += spacing +
					  EditorGUI.GetPropertyHeight(
						  properties.width
					  );

			height += spacing +
					  EditorGUI.GetPropertyHeight(
						  properties.height
					  );

			height += spacing;

			return height;
		}

		private float CalculateGridHeight(
			SerializedProperty        dataProp,
			Array2DDimensions         dimensions,
			float                     totalWidth,
			Dictionary<string, bool>  expandedStates,
			SerializableArray2DConfig config)
		{
			var gridCalcParams = new EditorPropertyRectUtility.GridCalculationParams
			{
				DataProperty    = dataProp,
				ExpandedStates  = expandedStates,
				Height          = dimensions.Height,
				Width           = dimensions.Width,
				TotalWidth      = totalWidth,
				MinColumnWidth  = SerializableArray2DConfig.MIN_COLUMN_WIDTH,
				IndexLabelWidth = SerializableArray2DConfig.INDEX_LABEL_WIDTH
			};

			float[] columnWidths = EditorPropertyRectUtility.CalculateOptimalColumnWidths(
				gridCalcParams
			);

			float[] rowHeights = EditorPropertyRectUtility.CalculateRowHeights(
				gridCalcParams,
				columnWidths
			);

			float spacing    = EditorGUIUtility.standardVerticalSpacing;
			float gridHeight = spacing + EditorGUIUtility.singleLineHeight * 2; // Column headers

			for (var row = 0; row < dimensions.Height; row++) gridHeight += spacing + rowHeights[row];

			return gridHeight;
		}
	}

	/// <summary>
	///     Renders SerializableArray2D dimension fields, validation messages, and grid cells.
	/// </summary>
	public class SerializableArray2DRenderer
	{
		public Rect DrawDimensionFields(
			Rect                                                                           startPosition,
			(SerializedProperty width, SerializedProperty height, SerializedProperty data) properties,
			SerializableArray2DConfig                                                      config)
		{
			var widthPath = properties.width.propertyPath;
			var gridPath = widthPath.Substring(0, widthPath.LastIndexOf('.'));
			return DrawDimensionFields(startPosition, properties.width.serializedObject.FindProperty(gridPath));
		}

		internal Rect DrawDimensionFields(Rect startPosition, SerializedProperty grid)
		{
			var width = grid.FindPropertyRelative("width").intValue;
			var height = grid.FindPropertyRelative("height").intValue;
			var position = EditorPropertyRectUtility.NextLine(startPosition);
			int newWidth = EditorGUI.DelayedIntField(position, SerializableArray2DConfig.WidthLabel, width);
			position = EditorPropertyRectUtility.NextLine(position);
			int newHeight = EditorGUI.DelayedIntField(position, SerializableArray2DConfig.HeightLabel, height);

			if (newWidth != width && newWidth > 0)
				ResizeGrid(grid, newWidth, Mathf.Max(1, height));
			else if (newHeight != height && newHeight > 0)
				ResizeGrid(grid, Mathf.Max(1, width), newHeight);

			return EditorPropertyRectUtility.NextLine(position);
		}

		internal static void ResizeGrid(SerializedProperty grid, int width, int height)
		{
			if (width <= 0 || height <= 0 || (long)width * height > int.MaxValue ||
				grid.serializedObject.isEditingMultipleObjects)
				return;

			var value = (IResizableSerializedGrid)grid.boxedValue;
			grid.boxedValue = value.ResizedCopy(width, height);
		}

		public void DrawInvalidDimensionsHelp(Rect position)
		{
			position = EditorPropertyRectUtility.NextLine(
				position
			);

			var helpBoxRect = new Rect(
				position.x,
				position.y,
				position.width,
				SerializableArray2DConfig.INVALID_DIMENSIONS_HELP_BOX_HEIGHT
			);

			EditorGUI.HelpBox(
				helpBoxRect,
				"Width and Height must both be > 0",
				MessageType.Info
			);
		}

		public void DrawPropertyGrid(
			Rect                      position,
			SerializedProperty        dataProp,
			Array2DDimensions         dimensions,
			string                    propertyPath,
			Dictionary<string, bool>  expandedStates,
			SerializableArray2DConfig config) =>
			EditorPropertyDrawUtility.DrawPropertyGrid(
				position,
				dataProp,
				dimensions.Width,
				dimensions.Height,
				propertyPath,
				expandedStates
			);
	}

	/// <summary>
	///     Stores validated SerializableArray2D width and height values.
	/// </summary>
	public readonly struct Array2DDimensions
	{
		public readonly int Height;
		public readonly int Width;

		public Array2DDimensions(int width, int height)
		{
			Width  = width;
			Height = height;
		}

		public bool IsValid() => Width > 0 && Height > 0;
	}
}
