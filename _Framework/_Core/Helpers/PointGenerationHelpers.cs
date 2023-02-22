using Unity.Jobs;
using Bezoro.Core;
using Unity.Collections;
using Unity.Mathematics;

namespace Bezoro
{
	public static class PointGenerationHelpers
	{
		public static void GeneratePointsOnPlanePersistent
		(float3 center,
		 float width,
		 float height,
		 int numPointsWidth,
		 int numPointsHeight,
		 float3 rightAxis,
		 float3 upAxis,
		 ref NativeArray<float3> resultPoints,
		 int batchCount = 64)
		{
			var totalPoints = numPointsWidth * numPointsHeight;
			if (totalPoints <= 0)
				return;

			if (resultPoints.Length != totalPoints)
			{
				if (resultPoints.IsCreated)
					resultPoints.Dispose();

				resultPoints = new NativeArray<float3>(totalPoints, Allocator.Persistent);
			}

			rightAxis = math.normalize(rightAxis);
			upAxis = math.normalize(upAxis);
			var generatePointsOnPlaneJob = new GeneratePointsOnPlaneJob()
										   {
											   Center = center,
											   Width = width,
											   Height = height,
											   NumPointsWidth = numPointsWidth,
											   NumPointsHeight = numPointsHeight,
											   RightAxis = rightAxis,
											   UpAxis = upAxis,
											   ResultPoints = resultPoints
										   };

			generatePointsOnPlaneJob.Schedule(totalPoints, batchCount).Complete();
		}
	}
}