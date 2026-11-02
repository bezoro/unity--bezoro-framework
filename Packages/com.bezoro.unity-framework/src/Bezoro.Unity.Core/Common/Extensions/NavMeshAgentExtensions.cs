using UnityEngine;
using UnityEngine.AI;

namespace Bezoro.Unity.Core.Common.Extensions;

/// <summary>
///     Contains extension methods for Nav Mesh Agent.
/// </summary>
public static class NavMeshAgentExtensions
{
	public static bool HasReachedDestination(this NavMeshAgent agent)
	{
		if (agent.pathPending) return false;

		if (agent.remainingDistance > agent.stoppingDistance) return false;

		return !agent.hasPath ||
			   Mathf.Approximately(
				   agent.velocity.sqrMagnitude,
				   0
			   );
	}

	public static Vector3 GetRandomPosition(this NavMeshAgent navMeshAgent, float maxDistance = Mathf.Infinity)
	{
		var randomPosition = navMeshAgent.transform.position + Random.insideUnitSphere * maxDistance;
		NavMesh.SamplePosition(
			randomPosition,
			out var hit,
			maxDistance,
			NavMesh.AllAreas
		);

		return hit.position;
	}
}
