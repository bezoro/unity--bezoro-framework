using Drawing;
using UnityEditor;
using UnityEngine;

namespace Bezoro.StateMachines.Editor
{
	public static class BaseMonoKCCStateMachineGizmoDrawer
	{
		[DrawGizmo(GizmoType.Active)]
		private static void DrawGizmos(BaseMonoKCCStateMachine source, GizmoType gizmoType)
		{
			DrawDebugInfo();

			void DrawDebugInfo()
			{
				DrawStateLabels();

				void DrawStateLabels()
				{
					var states = source.GetCurrentHierarchy();

					for (var i = states.Count - 1; i >= 0; i--)
					{
						var childRenderer = source.GetComponentInChildren<Renderer>();
						if (childRenderer is null)
							return;

						var bounds = childRenderer.bounds;

						var positionToDrawLabel = new Vector3(
															  0f,
															  bounds.size.y + i * 0.5f,
															  0f
															 );

						states.Reverse();

						using (Draw.InLocalSpace(source.transform))
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
}