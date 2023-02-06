using Bezoro.Core;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Bezoro.Core.Editor.Extensions;

namespace Bezoro.StateMachines.Editor
{
	[CustomEditor(typeof(BaseMonoTickableStateMachine), true)]
	public class BaseTickableStateMachineEditor : OdinEditor
	{
		private BaseMonoTickableStateMachine _obj;

		private void Awake() =>
			_obj = target as BaseMonoTickableStateMachine;

		public override void OnInspectorGUI()
		{
			if (!_obj.TickType.HasFlag(TickType.Late))
			{
				EditorGUILayout.HelpBox
					(
					 "Tickable State Machines need at least LateTick to be called, " +
					 "otherwise transitions will not be evaluated.",
					 MessageType.Warning
					);

				if (GUILayout.Button("Fix"))
				{
					var tickTypeProperty =
						serializedObject.FindPropertyByAutoPropertyName("TickType");

					tickTypeProperty.enumValueFlag += 8;
					serializedObject.ApplyModifiedProperties();
				}
			}

			base.OnInspectorGUI();
		}
	}
}