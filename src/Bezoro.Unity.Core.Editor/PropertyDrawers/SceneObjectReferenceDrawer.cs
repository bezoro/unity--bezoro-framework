using Bezoro.Unity.Core.Common.Serialization;
using Bezoro.Unity.Core.MonoBehaviours;
using Bezoro.Unity.Core.Scenes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bezoro.Unity.Core.Editor.PropertyDrawers
{
	[CustomPropertyDrawer(
		typeof(SceneObjectReference)
	)]
	/// <summary>
	/// Draws scene-object reference fields and stores selected SceneObjectIdentifier GUIDs.
	/// </summary>
	public class SceneObjectReferenceDrawer : PropertyDrawer
	{
		// Updated error message to reflect the need for SceneObjectIdentifier
		private const string NO_IDENTIFIER_COMPONENT_ERROR =
			"Selected object must have a <b>SceneObjectIdentifier</b> component.";

		private const string NOT_SCENE_OBJECT_ERROR =
			"Selected object must be a <b>scene object</b> (not a Prefab asset).";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			// Find the nested property for the Serializable_GUID instance
			var guidProperty = property.FindPropertyRelative(
				"guid"
			);

			if (guidProperty == null)
				return new Label(
					$"Error: Could not find nested 'guid' property in {property.propertyPath}"
				);

			// Find the actual string guid field within Serializable_GUID
			var guidStringProperty = guidProperty.FindPropertyRelative(
				"guid"
			);

			if (guidStringProperty == null)
				return new Label(
					$"Error: Could not find nested 'guid.guid' string property in {property.propertyPath}"
				);

			// --- Create Visual Elements ---

			var root = new VisualElement();

			var errorLabel = new Label
			{
				style =
				{
					// display = DisplayStyle.None, // Hide initially
					color        = Color.red,
					whiteSpace   = WhiteSpace.Normal, // Allow wrapping
					marginTop    = 2,
					marginBottom = 2
				}
			};

			// Hide initially by not adding it yet, or setting display = DisplayStyle.None

			var objectField = new ObjectField(
					property.displayName
				) // Use property's display name for the label
				{
					objectType        = typeof(GameObject),
					allowSceneObjects = true

					// Initial value will be set below by resolving the current GUID
				};

			// Attempt to resolve the current GUID to set the initial ObjectField value
			string      currentGuid   = guidStringProperty.stringValue;
			GameObject? initialObject = null;

			if (GUIDUtils.Check_GUID_Validity(
					currentGuid
				))
				initialObject = SceneObjectRegistry.Get_Object_By_GUID(
					currentGuid
				);

			objectField.value = initialObject; // Set initial value (will be null if GUID invalid or object destroyed)

			// Keep track if the initial object was valid, to avoid error messages if field starts empty correctly
			bool hadValidInitialObject = initialObject != null;

			var guidField = new TextField(
				"GUID"
			)
			{
				value      = currentGuid, // Set initial GUID text
				isReadOnly = true,
				style =
				{
					// Make it look obviously disabled
					opacity = 0.7f
				}

				// Consider binding later if needed: guidField.BindProperty(guidStringProperty);
			};

			guidField.SetEnabled(
				false
			); // Visually disable interaction

			// --- Add Elements to Root ---
			root.Add(
				objectField
			);

			root.Add(
				errorLabel
			); // Add error label container

			root.Add(
				guidField
			);

			// Hide error label initially
			errorLabel.style.display = DisplayStyle.None;

			// --- Register Callbacks ---

			objectField.RegisterValueChangedCallback(evt =>
				{
					var    assignedObject = evt.newValue as GameObject;
					string newGuid        = string.Empty; // Reset target GUID

					errorLabel.text          = "";                // Clear previous errors
					errorLabel.style.display = DisplayStyle.None; // Hide error label

					if (assignedObject == null)
					{
						// Object field cleared
						newGuid = string.Empty;
					}
					else if (EditorUtility.IsPersistent(
								 assignedObject
							 ))
					{
						// Assigned object is a prefab/asset, not a scene object
						errorLabel.text          = NOT_SCENE_OBJECT_ERROR;
						errorLabel.style.display = DisplayStyle.Flex;
						objectField.SetValueWithoutNotify(
							evt.previousValue
						); // Revert change visually

						return; // Stop processing
					}
					else
					{
						// Attempt to get the SceneObjectIdentifier component
						var identifier =
							assignedObject.GetComponent<SceneObjectIdentifier>(); // LOOK FOR SceneObjectIdentifier!

						if (identifier == null)
						{
							// Missing the required component
							errorLabel.text          = NO_IDENTIFIER_COMPONENT_ERROR;
							errorLabel.style.display = DisplayStyle.Flex;
							objectField.SetValueWithoutNotify(
								evt.previousValue
							); // Revert change visually

							return; // Stop processing
						}

						if (string.IsNullOrEmpty(
								identifier.GUID
							))
						{
							// Component exists but GUID is empty (shouldn't happen if OnValidate/Awake works)
							errorLabel.text          = "SceneObjectIdentifier on target has an empty GUID.";
							errorLabel.style.display = DisplayStyle.Flex;
							objectField.SetValueWithoutNotify(
								evt.previousValue
							); // Revert change visually

							return; // Stop processing
						}

						// Valid assignment! Get the GUID.
						newGuid = identifier.GUID ?? string.Empty;
					}

					// --- Apply Changes to SerializedProperty ---
					// Check if the GUID actually changed before applying
					if (guidStringProperty.stringValue != newGuid)
					{
						guidStringProperty.stringValue = newGuid;
						guidField.value                = newGuid; // Update the read-only field visually

						// Apply the changes to the serialized object to save and support undo
						property.serializedObject.ApplyModifiedProperties();
					}
				}
			);

			return root;
		}
	}
}
