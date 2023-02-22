using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Bezoro
{
	[BurstCompile]
	public struct CreateRaycastCommandsJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<float3> Origins;
		[ReadOnly]
		public float3 Direction;

		[WriteOnly]
		public NativeArray<RaycastCommand> Commands;

		public void Execute(int index) =>
			Commands[index] = new RaycastCommand(Origins[index], Direction);
	}
}