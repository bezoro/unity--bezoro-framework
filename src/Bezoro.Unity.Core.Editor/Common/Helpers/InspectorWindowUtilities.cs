using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bezoro.Unity.Core.Editor.Common.Helpers
{
	/// <summary>
	///     Finds and repaints Unity Inspector windows from editor code.
	/// </summary>
	public static class InspectorWindowUtilities
	{
		public static void Open_Inspector_Window(
			ObjectField         objectFieldToInspect,
			InspectorWindowType type = InspectorWindowType.Normal
		)
		{
			if (objectFieldToInspect == null) return;

			Open_Inspector_Window(
				objectFieldToInspect.value,
				type
			);
		}

		public static void Open_Inspector_Window(
			Object              objectToInspect,
			InspectorWindowType type = InspectorWindowType.Normal
		)
		{
			if (objectToInspect == null) return;

			var inspectorWindow = ScriptableObject.CreateInstance<EditorWindow>();
			inspectorWindow.titleContent = new(
				"Inspector"
			);

			var editor = UnityEditor.Editor.CreateEditor(
				objectToInspect
			);

			inspectorWindow.rootVisualElement.Add(
				new IMGUIContainer(() => editor.OnInspectorGUI()
				)
			);

			switch (type)
			{
				case InspectorWindowType.Normal:
					inspectorWindow.Show();
					break;

				case InspectorWindowType.Auxiliary:
					inspectorWindow.ShowAuxWindow();
					break;
			}
		}
	}

	/// <summary>
	///     Lists the supported Inspector Window Type values.
	/// </summary>
	public enum InspectorWindowType
	{
		Normal = 0,
		Auxiliary
	}
}
