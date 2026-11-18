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

	/// <summary>Samples the agent's NavMesh within the radius. Falls back to the current position if sampling fails.</summary>
	public static Vector3 GetRandomPosition(this NavMeshAgent navMeshAgent, float maxDistance = 10f)
	{
		if (!(maxDistance > 0f) || float.IsInfinity(maxDistance))
			throw new System.ArgumentOutOfRangeException(nameof(maxDistance), "The search radius must be finite and positive.");

		var origin = navMeshAgent.transform.position;
		var candidate = origin + Random.insideUnitSphere * maxDistance;
		var filter = new NavMeshQueryFilter
		{
			agentTypeID = navMeshAgent.agentTypeID,
			areaMask = navMeshAgent.areaMask
		};

		return NavMesh.SamplePosition(candidate, out var hit, maxDistance, filter) ? hit.position : origin;
	}
}
