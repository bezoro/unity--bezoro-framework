using Bezoro.Unity.Core.StateMachines.KCCBased;
using Drawing;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.StateMachine
{
	/// <summary>
	///     Draws scene gizmos for KCC state machine owners.
	/// </summary>
	internal static class KccStateMachineOwnerGizmoDrawer
	{
		[DrawGizmo(
			GizmoType.Active
		)]
		private static void DrawGizmos(KCCStateMachineOwner source, GizmoType gizmoType)
		{
			DrawStateLabels();

			void DrawStateLabels()
			{
				if (!UnityEngine.Application.isPlaying) return;

				var states = source.GetCurrentHierarchy();

				for (int i = states.Count - 1; i >= 0; i--)
				{
					var childRenderer = source.GetComponentInChildren<Renderer>();

					if (childRenderer is null) return;

					var bounds = childRenderer.bounds;

					var positionToDrawLabel = new float3(
						0f,
						bounds.size.y + i * 0.5f,
						0f
					);

					states.Reverse();

					using (Draw.InLocalSpace(
							   source.transform
						   ))
					{
						Draw.Label2D(
							positionToDrawLabel,
							$"{states[i].GetType().Name}",
							24f,
							LabelAlignment.Center,
							Color.blue
						);
					}

					states.Reverse();
				}
			}
		}
	}
}
