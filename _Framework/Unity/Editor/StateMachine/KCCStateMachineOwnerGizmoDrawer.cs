using Bezoro.Framework.Unity.StateMachines.KCCBased;
using Drawing;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.StateMachine
{
	internal static class KCCStateMachineOwnerGizmoDrawer
	{
		[DrawGizmo( GizmoType.Active )]
		private static void DrawGizmos( KCCStateMachineOwner source, GizmoType gizmoType )
		{
			DrawStateLabels();

			void DrawStateLabels()
			{
				if ( !Application.isPlaying )
				{
					return;
				}

				var states = source.GetCurrentHierarchy();

				for ( var i = states.Count - 1 ; i >= 0 ; i-- )
				{
					var childRenderer = source.GetComponentInChildren<Renderer>();

					if ( childRenderer is null )
					{
						return;
					}

					var bounds = childRenderer.bounds;

					var positionToDrawLabel = new Vector3(
						0f,
						bounds.size.y + i * 0.5f,
						0f
					);

					states.Reverse();

					using ( Draw.InLocalSpace( source.transform ) )
					{
						Draw.Label2D(
							positionToDrawLabel,
							$"{states[ i ].GetType().Name}",
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
