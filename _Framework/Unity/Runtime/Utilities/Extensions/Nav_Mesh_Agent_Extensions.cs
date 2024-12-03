using UnityEngine;
using UnityEngine.AI;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Nav_Mesh_Agent_Extensions
	{
		public static Vector3 Get_Random_Point( this NavMeshAgent nav_mesh_agent, float max_distance )
		{
			var random_position = nav_mesh_agent.transform.position + Random.insideUnitSphere * max_distance;
			NavMesh.SamplePosition( random_position, out var hit, max_distance, NavMesh.AllAreas );
			return hit.position;
		}

		public static bool HasReachedDestination( this NavMeshAgent agent )
		{
			if ( agent.pathPending ) return false;
			if ( !( agent.remainingDistance <= agent.stoppingDistance ) ) return false;

			return !agent.hasPath || Mathf.Approximately( agent.velocity.sqrMagnitude, 0 );
		}
	}
}
