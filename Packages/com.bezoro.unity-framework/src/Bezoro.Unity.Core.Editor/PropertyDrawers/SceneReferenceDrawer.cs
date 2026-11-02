using Bezoro.Unity.Core.Scenes;
using Bezoro.Unity.Core.ScriptableObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bezoro.Unity.Core.Editor.PropertyDrawers
{
	/// <summary>
	///     Custom Property Drawer for the SceneReference class.
	///     Displays the assigned Scene asset, its path and GUID.
	/// </summary>
	[CustomPropertyDrawer(
		typeof(SceneReference)
	)]
	/// <summary>
	/// Draws scene asset references with path, GUID, and build-settings validation.
	/// </summary>
	public class SceneReferenceDrawer : PropertyDrawer
	{
		private const float FRAME_PADDING      = 4f; // space between border and content
		private const float LINE_SPACING       = 2f;
		private const float WARNING_BOX_HEIGHT = 30f;

		private static readonly float LineHeight = EditorGUIUtility.singleLineHeight;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = LineHeight; // object field

			if (ShouldShowWarning(
					property
				))
				height += WARNING_BOX_HEIGHT + LINE_SPACING;

			height += LineHeight + LINE_SPACING;

			// Path + GUID (if a scene is assigned)
			var sceneAssetProp = property.FindPropertyRelative(
				"sceneAsset"
			);

			if (sceneAssetProp.objectReferenceValue != null)
				height += (LineHeight + LINE_SPACING) * 2;

			return height + FRAME_PADDING * 2; // frame padding
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.Box(
				position,
				GUIContent.none,
				EditorStyles.helpBox
			);

			var contentRect = new Rect(
				position.x + FRAME_PADDING,
				position.y + FRAME_PADDING,
				position.width - FRAME_PADDING * 2,
				position.height - FRAME_PADDING * 2
			);

			EditorGUI.BeginProperty(
				contentRect,
				label,
				property
			);

			var current = contentRect;

			var sceneAssetProperty = property.FindPropertyRelative(
				"sceneAsset"
			);

			var loadSceneModeProperty = property.FindPropertyRelative(
				"loadSceneMode"
			);

			var scenePathProperty = property.FindPropertyRelative(
				"scenePath"
			);

			var sceneGuidProperty = property.FindPropertyRelative(
				"sceneGuid"
			);


			// Warning box (if scene not in build settings)
			if (ShouldShowWarning(
					property
				))
			{
				var warningRect = new Rect(
					current.x,
					current.y,
					current.width,
					WARNING_BOX_HEIGHT
				);

				EditorGUI.HelpBox(
					warningRect,
					"Scene is not in Build Settings",
					MessageType.Warning
				);

				current.y += WARNING_BOX_HEIGHT + LINE_SPACING;
			}

			// --- Scene Asset field ---
			var newObject = EditorGUI.ObjectField(
				new(
					current.x,
					current.y,
					current.width,
					LineHeight
				),
				label,
				sceneAssetProperty.objectReferenceValue,
				typeof(SceneAsset),
				false
			);

			// Apply changes (if any)
			if (newObject != sceneAssetProperty.objectReferenceValue)
			{
				sceneAssetProperty.objectReferenceValue = newObject;
				scenePathProperty.stringValue = newObject != null
													? AssetDatabase.GetAssetPath(
														newObject
													)
													: string.Empty;

				sceneGuidProperty.stringValue = newObject != null
													? AssetDatabase.AssetPathToGUID(
														scenePathProperty.stringValue
													)
													: string.Empty;
			}

			using (EditorGUI.DisabledScope disabled = new(
					   IsInsideSceneCollection(
						   property
					   )
				   ))
			{
				// Draw Load-Mode only when allowed
				current.y += LineHeight + LINE_SPACING;

				EditorGUI.BeginChangeCheck();
				loadSceneModeProperty.enumValueIndex = (int)(LoadSceneMode)EditorGUI.EnumPopup(
					new(
						current.x,
						current.y,
						current.width,
						LineHeight
					),
					"Load Mode",
					(LoadSceneMode)loadSceneModeProperty.enumValueIndex
				);

				if (EditorGUI.EndChangeCheck())
					property.serializedObject.ApplyModifiedProperties();
			}

			current.y += LineHeight + LINE_SPACING;

			// --- Draw Path & GUID (read-only) ---
			if (sceneAssetProperty.objectReferenceValue != null)
			{
				EditorGUI.BeginDisabledGroup(
					true
				);

				EditorGUI.LabelField(
					new(
						current.x,
						current.y,
						current.width,
						LineHeight
					),
					"Path",
					scenePathProperty.stringValue
				);

				current.y += LineHeight + LINE_SPACING;

				EditorGUI.LabelField(
					new(
						current.x,
						current.y,
						current.width,
						LineHeight
					),
					"GUID",
					sceneGuidProperty.stringValue
				);

				EditorGUI.EndDisabledGroup();
			}

			EditorGUI.EndProperty();
		}

		// ----------------------------------------------------------------------
		// Early-out helpers
		// ----------------------------------------------------------------------
		private static bool HasParentDrawer(SerializedProperty property) =>
			property.depth > 0; // any depth above 0 means we're nested

		private static bool IsInsideSceneCollection(SerializedProperty property) =>
			property.serializedObject.targetObject is SceneCollection;

		private bool ShouldShowWarning(SerializedProperty property)
		{
			var pathProp = property.FindPropertyRelative(
				"scenePath"
			);

			return !string.IsNullOrEmpty(
					   pathProp.stringValue
				   ) &&
				   SceneUtility.GetBuildIndexByScenePath(
					   pathProp.stringValue
				   ) <
				   0;
		}
	}
}
