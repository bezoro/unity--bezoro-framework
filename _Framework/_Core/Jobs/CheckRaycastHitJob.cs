using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;

namespace Bezoro.Framework.Core.Jobs
{
	[BurstCompile]
	public struct CheckRaycastHitJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<RaycastHit> Hits;

		[WriteOnly]
		public NativeArray<RaycastHit> ValidHits;

		public void Execute(int index)
		{
			if (Hits[index].distance > 0)
				ValidHits[index] = Hits[index];
		}
	}
}