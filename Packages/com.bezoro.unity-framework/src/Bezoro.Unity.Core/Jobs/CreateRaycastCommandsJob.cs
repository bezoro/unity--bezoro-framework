using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Bezoro.Unity.Core.Jobs;

[BurstCompile(
	FloatPrecision.Low,
	FloatMode.Fast
)]
/// <summary>
/// Builds RaycastCommand entries from ray origins and directions.
/// </summary>
public struct CreateRaycastCommandsJob : IJobParallelFor
{
	[ReadOnly] public float3 Direction;
	[ReadOnly] [NativeDisableParallelForRestriction]
	public NativeArray<float3> Origins;

	[WriteOnly] [NativeDisableParallelForRestriction]
	public NativeArray<RaycastCommand> Commands;

	#region Interface Implementations

	[BurstCompile]
	public void Execute(int index)
	{
		var queryParams = new QueryParameters
		{
			hitBackfaces     = false,
			hitMultipleFaces = false,
			layerMask        = -1,
			hitTriggers      = QueryTriggerInteraction.UseGlobal
		};

		Commands[index] = new(
			Origins[index],
			Direction,
			queryParams
		);
	}

	#endregion
}
