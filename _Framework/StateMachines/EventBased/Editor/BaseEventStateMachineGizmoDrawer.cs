using UnityEditor;
using UnityEngine;

namespace Bezoro.StateMachines.Editor
{
	public static class BaseEventStateMachineGizmoDrawer
	{
		[DrawGizmo(GizmoType.Active)]
		private static void DrawGizmos(BaseEventStateMachine source, GizmoType gizmoType)
		{
			DrawStateLabels(source);

			void DrawStateLabels(IEventStateMachine stateMachine, int index = 0)
			{
				const float offset = 4f;
				var labelPosition = source.transform.position + Vector3.up * offset;

				Handles.Label
					(
					 labelPosition - (Vector3.up * index) * 0.75f,
					 stateMachine.CurrentState.GetType().Name
					);

				if (stateMachine.CurrentState is IEventStateMachine fsm)
				{
					index++;
					DrawStateLabels(fsm, index);
				}
			}
		}
	}
}