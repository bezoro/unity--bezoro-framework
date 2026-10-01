using Bezoro.Unity.Core.StateMachines.TickableBased;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.StateMachine
{
	/// <summary>
	///     Draws scene gizmos for tickable state machine owners.
	/// </summary>
	internal static class TickableStateMachineOwnerGizmoDrawer
	{
		[DrawGizmo(
			GizmoType.Active
		)]
		private static void DrawGizmos(TickableStateMachineOwner source, GizmoType gizmoType)
		{
			DrawStateLabels(
				source.StateMachine
			);

			void DrawStateLabels(ITickableStateMachine stateMachine, int index = 0)
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

					if (stateMachine.CurrentState is ITickableStateMachine fsm)
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
