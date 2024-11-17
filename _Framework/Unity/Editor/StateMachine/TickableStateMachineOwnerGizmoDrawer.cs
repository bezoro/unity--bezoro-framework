using Bezoro.Framework.Unity.StateMachines;
using Bezoro.Framework.Unity.StateMachines.TickableBased;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.StateMachine
{
	internal static class TickableStateMachineOwnerGizmoDrawer
	{
		[DrawGizmo( GizmoType.Active )]
		private static void DrawGizmos( TickableStateMachineOwner source, GizmoType gizmoType )
		{
			DrawStateLabels( source.StateMachine );

			void DrawStateLabels( ITickableStateMachine stateMachine, int index = 0 )
			{
				if ( !Application.isPlaying )
				{
					return;
				}

				const float offset = 4f;

				while (true)
				{
					var labelPosition = source.transform.position + Vector3.up * offset;

					Handles.Label(
						labelPosition - Vector3.up * index * 0.75f,
						stateMachine.CurrentState.GetType().Name
					);

					if ( stateMachine.CurrentState is ITickableStateMachine fsm )
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
