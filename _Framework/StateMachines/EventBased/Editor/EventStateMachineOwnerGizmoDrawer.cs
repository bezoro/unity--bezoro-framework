using UnityEditor;
using UnityEngine;

namespace Bezoro.StateMachines.Editor
{
	public static class EventStateMachineOwnerGizmoDrawer
	{
		[DrawGizmo(GizmoType.Active)]
		private static void DrawGizmos(EventStateMachineOwner source, GizmoType gizmoType)
		{
			DrawStateLabels(source.StateMachine);

			void DrawStateLabels(IEventStateMachine stateMachine, int index = 0)
			{
				if (!Application.isPlaying)
					return;

				const float offset = 4f;

				while (true)
				{
					var labelPosition = source.transform.position + Vector3.up * offset;

					Handles.Label(labelPosition - (Vector3.up * index) * 0.75f,
								  stateMachine.CurrentState.GetType().Name);

					if (stateMachine.CurrentState is IEventStateMachine fsm)
					{
						index++;
						stateMachine = fsm;
						continue;
					}

					break;
				}
			}
		}
	}
}