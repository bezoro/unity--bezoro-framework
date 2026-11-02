using Bezoro.Unity.Core.Editor.Validation;
using Bezoro.Unity.Core.ScriptableObjects;
using UnityEditor;

namespace Bezoro.Unity.Core.Editor.Inspectors
{
	/// <summary>
	///     Edits ArrayHelpersConfig and reports invalid asset locations in the native Inspector.
	/// </summary>
	[CustomEditor(typeof(ArrayHelpersConfig))]
	[CanEditMultipleObjects]
	public sealed class ArrayHelpersConfigInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			foreach (var inspected in targets)
			{
				var config = inspected as ArrayHelpersConfig;
				if (config == null) continue;
				foreach (var error in ArrayHelpersConfigValidator.GetValidationErrors(config))
					EditorGUILayout.HelpBox(error, MessageType.Error);
			}

			DrawDefaultInspector();
		}
	}
}
