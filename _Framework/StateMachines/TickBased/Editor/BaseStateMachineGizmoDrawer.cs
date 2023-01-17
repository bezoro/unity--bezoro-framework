using UnityEditor;
using UnityEngine;

namespace Bezoro.StateMachines.Editor
{
	public static class BaseStateMachineGizmoDrawer
	{
		[DrawGizmo(GizmoType.Active)]
		private static void DrawGizmos(BaseStateMachine source, GizmoType gizmoType)
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

					if (stateMachine.CurrentState is not IStateMachine fsm)
						return;

					index++;
					stateMachine = fsm;
				}
			}
		}
	}
}