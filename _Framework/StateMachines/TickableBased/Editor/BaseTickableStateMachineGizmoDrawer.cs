using UnityEditor;
using UnityEngine;

namespace Bezoro.StateMachines.Editor
{
	public static class BaseTickableStateMachineGizmoDrawer
	{
		[DrawGizmo(GizmoType.Active)]
		private static void DrawGizmos(BaseTickableStateMachine source, GizmoType gizmoType)
		{
			DrawStateLabels(source);

			void DrawStateLabels(IStateMachine stateMachine, int index = 0)
			{
				while (true)
				{
					const float offset = 4f;
					var labelPosition = source.transform.position + Vector3.up * offset;

					Handles.Label
						(
						 labelPosition - (Vector3.up * index) * 0.75f,
						 stateMachine.CurrentState.GetType().Name
						);

					if (stateMachine.CurrentState is not ITickableStateMachine fsm)
						return;

					index++;
					stateMachine = fsm;
				}
			}
		}
	}
}