using UnityEngine;
using UnityEngine.AI;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class NavMeshAgentExtensions
	{
		public static Vector3 GetRandomPoint( this NavMeshAgent navMeshAgent, float maxDistance )
		{
			var randomPosition =
				navMeshAgent.transform.position + Random.insideUnitSphere * maxDistance;

			NavMesh.SamplePosition(
				randomPosition,
				out var hit,
				maxDistance,
				NavMesh.AllAreas
			);

			return hit.position;
		}

		public static bool HasReachedDestination( this NavMeshAgent agent )
		{
			if ( agent.pathPending )
			{
				return false;
			}

			if ( !( agent.remainingDistance <= agent.stoppingDistance ) )
			{
				return false;
			}

			return !agent.hasPath || Mathf.Approximately( agent.velocity.sqrMagnitude, 0 );
		}
	}
}
