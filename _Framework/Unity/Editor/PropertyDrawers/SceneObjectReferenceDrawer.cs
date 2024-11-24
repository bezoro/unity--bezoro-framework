using Bezoro.Framework.Core;
using Bezoro.Framework.Unity;
using Bezoro.Framework.Unity.Editor.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer( typeof(SceneObjectReference) )]
public class SceneObjectReferenceDrawer : PropertyDrawer
{
	private const string NO_COMPONENT_ERROR     = "Selected object must have an <b>ObjectUID</b> component.";
	private const string NOT_SCENE_OBJECT_ERROR = "Selected object must be a <b>scene object</b>.";
	private const string WILL_NOT_SAVE_ERROR    = "The reference will <b>NOT</b> be serialized.";

	public override VisualElement CreatePropertyGUI( SerializedProperty property )
	{
		var root          = new VisualElement();
		var propertyValue = SerializedPropertyUtils.GetValue( property ) as SceneObjectReference;
		Assert.IsNotNull( propertyValue );

		var errorMessage = new Label
		{
			style =
			{
				alignSelf = Align.Center,
				color     = Color.red
			}
		};
		root.Add( errorMessage );

		var objectField = new ObjectField( "Reference" )
		{
			objectType        = typeof(ObjectUID),
			allowSceneObjects = true,
			value             = propertyValue.Reference
		};
		Debug.Log( propertyValue.GUID );
		Debug.Log( propertyValue.Reference );
		root.Add( objectField );

		var guidField = new TextField( "GUID" )
		{
			value       = propertyValue.GUID,
			isReadOnly  = true,
			enabledSelf = false
		};
		root.Add( guidField );

		objectField.RegisterValueChangedCallback(
			evt =>
			{
				if ( evt.newValue == null ) { return; }
				var objectUID = evt.newValue as ObjectUID;
				if ( objectUID == null )
				{
					errorMessage.text    = NO_COMPONENT_ERROR;
					errorMessage.visible = true;
					return;
				}
				if ( EditorUtility.IsPersistent( objectUID.gameObject ) )
				{
					errorMessage.text    = NOT_SCENE_OBJECT_ERROR;
					errorMessage.visible = true;
					return;
				}
				propertyValue.GUID   = objectUID.UID;
				guidField.value      = propertyValue.GUID;
				errorMessage.visible = false;
			} );

		return root;
	}
}
