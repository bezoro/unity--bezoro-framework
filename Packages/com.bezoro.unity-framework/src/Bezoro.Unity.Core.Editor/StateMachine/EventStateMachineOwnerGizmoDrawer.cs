using Bezoro.Unity.Core.StateMachines.EventBased;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.StateMachine
{
	/// <summary>
	///     Draws scene gizmos for event-based state machine owners.
	/// </summary>
	internal static class EventStateMachineOwnerGizmoDrawer
	{
		[DrawGizmo(
			GizmoType.Active
		)]
		private static void DrawGizmos(EventStateMachineOwner source, GizmoType gizmoType)
		{
			DrawStateLabels(
				source.StateMachine
			);

			void DrawStateLabels(IEventStateMachine stateMachine, int index = 0)
			{
				if (!UnityEngine.Application.isPlaying) return;

				const float OFFSET = 4f;

				while (true)
				{
					var labelPosition = source.transform.position + Vector3.up * OFFSET;

					Handles.Label(
						labelPosition - Vector3.up * index * 0.75f,
						stateMachine.CurrentState.GetType().Name
					);

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
