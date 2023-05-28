using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace Bezoro.Framework.Core.Jobs
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