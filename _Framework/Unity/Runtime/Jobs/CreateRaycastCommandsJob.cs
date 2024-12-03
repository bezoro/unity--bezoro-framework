using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Bezoro.Framework.Unity.Jobs
{
	[BurstCompile(
		FloatPrecision.Low,
		FloatMode.Fast )]
	public struct CreateRaycastCommandsJob : IJobParallelFor
	{
		[ReadOnly] [NativeDisableParallelForRestriction]
		public NativeArray<float3> Origins;

		[ReadOnly] public float3 Direction;

		[WriteOnly] [NativeDisableParallelForRestriction]
		public NativeArray<RaycastCommand> Commands;

		[BurstCompile]
		public void Execute( int index )
		{
			var queryParams = new QueryParameters
			{
				hitBackfaces     = false,
				hitMultipleFaces = false,
				layerMask        = -1,
				hitTriggers      = QueryTriggerInteraction.UseGlobal
			};

			Commands[ index ] = new(
				Origins[ index ],
				Direction,
				queryParams );
		}
	}
}
