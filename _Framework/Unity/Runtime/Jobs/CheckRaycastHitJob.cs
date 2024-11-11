using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Bezoro.Framework.Unity.Jobs
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
			{
				ValidHits[index] = Hits[index];
			}
		}
	}
}
