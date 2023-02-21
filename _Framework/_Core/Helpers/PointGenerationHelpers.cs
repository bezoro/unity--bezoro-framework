using Unity.Jobs;
using Bezoro.Core;
using Unity.Collections;
using Unity.Mathematics;

namespace Bezoro
{
	public static class PointGenerationHelpers
	{
		public static void Generate2DRectanglePointsPersistent
		(float3 center,
		 float width,
		 float height,
		 int numPointsWidth,
		 int numPointsHeight,
		 float3 transformRight,
		 float3 transformUp,
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

			transformRight = math.normalize(transformRight);
			transformUp = math.normalize(transformUp);
			var generate2DRectanglePointsJob = new GeneratePointsOnPlaneJob()
											   {
												   Center = center,
												   Width = width,
												   Height = height,
												   NumPointsWidth = numPointsWidth,
												   NumPointsHeight = numPointsHeight,
												   RightAxis = transformRight,
												   UpAxis = transformUp,
												   ResultPoints = resultPoints
											   };

			generate2DRectanglePointsJob.Schedule(totalPoints, batchCount).Complete();
		}
	}
}