using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Bezoro.Unity.Core.Jobs;

[BurstCompile]
/// <summary>
/// Checks batched raycast results for any successful hit.
/// </summary>
public struct CheckRaycastHitJob : IJobParallelFor
{
	[ReadOnly] public NativeArray<RaycastHit> Hits;

	[WriteOnly] public NativeArray<RaycastHit> ValidHits;

	#region Interface Implementations

	public void Execute(int index)
	{
		if (Hits[index].distance > 0) ValidHits[index] = Hits[index];
	}

	#endregion
}
