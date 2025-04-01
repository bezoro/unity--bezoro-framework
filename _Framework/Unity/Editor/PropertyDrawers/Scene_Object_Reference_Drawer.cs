using Bezoro.Framework.Core;
using Bezoro.Framework.Unity.Editor.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bezoro.Framework.Unity.Editor.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(Scene_Object_Reference))]
	public class Scene_Object_Reference_Drawer : PropertyDrawer
	{
		private const string _NO_COMPONENT_ERROR = "Selected object must have an <b>Serializable_GUID</b> component.";
		private const string _NOT_SCENE_OBJECT_ERROR = "Selected object must be a <b>scene object</b>.";

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var root           = new VisualElement();
			var property_value = SerializedPropertyUtils.GetValue(property) as Scene_Object_Reference;
			Assert.IsNotNull(property_value);

			var error_message = new Label
			{
				style =
				{
					alignSelf = Align.Center,
					color     = Color.red
				}
			};

			root.Add(error_message);

			var object_field = new ObjectField("Reference")
			{
				objectType        = typeof(GameObject),
				allowSceneObjects = true,
				value             = property_value?.Reference
			};

			Debug.Log(property_value?.GUID);
			Debug.Log(property_value?.Reference);
			root.Add(object_field);

			var guid_field = new TextField("GUID")
			{
				value       = property_value?.GUID,
				isReadOnly  = true,
				enabledSelf = false
			};

			root.Add(guid_field);

			object_field.RegisterValueChangedCallback(
				evt =>
				{
					if (evt.newValue == null) { return; }

					var object_uid = evt.newValue as GameObject;

					if (object_uid == null)
					{
						error_message.text    = _NO_COMPONENT_ERROR;
						error_message.visible = true;
						return;
					}

					if (EditorUtility.IsPersistent(object_uid))
					{
						error_message.text    = _NOT_SCENE_OBJECT_ERROR;
						error_message.visible = true;
						return;
					}

					if (property_value != null)
					{
						property_value.GUID = object_uid.GetComponent<Serializable_GUID>()?.Value;
						guid_field.value    = property_value.GUID;
					}

					error_message.visible = false;
				}
			);

			return root;
		}
	}
}
