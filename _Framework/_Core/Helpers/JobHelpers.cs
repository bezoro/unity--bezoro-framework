using Unity.Jobs;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace Bezoro.Core.Helpers
{
	public static class JobHelpers
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

		public static void CreateRaycastCommandsPersistent
		(NativeArray<float3> origins,
		 float3 direction,
		 ref NativeArray<RaycastCommand> commands,
		 int batchCount = 64)
		{
			if (origins.Length == 0)
				return;

			if (origins.Length != commands.Length)
			{
				if (commands.IsCreated)
					commands.Dispose();

				commands = new NativeArray<RaycastCommand>(origins.Length, Allocator.Persistent);
			}

			var createRaycastCommandsJob = new CreateRaycastCommandsJob()
										   {
											   Origins = origins,
											   Direction = direction,
											   Commands = commands
										   };

			createRaycastCommandsJob.Schedule(origins.Length, batchCount).Complete();
		}

		public static void ExecuteRaycastCommandsPersistent
		(NativeArray<RaycastCommand> commands,
		 ref NativeArray<RaycastHit> hits,
		 int minCommandsPerJob = 1)
		{
			if (commands.Length == 0)
				return;

			if (commands.Length != hits.Length)
			{
				if (hits.IsCreated)
					hits.Dispose();

				hits = new NativeArray<RaycastHit>(commands.Length, Allocator.Persistent);
			}

			RaycastCommand.ScheduleBatch(commands, hits, minCommandsPerJob).Complete();
		}

		/// <summary>
		/// Loops through 'hits', returning a 'validHits' array of RaycastHits that actually hit something.
		/// </summary>
		/// <param name="hits">Array to check.</param>
		/// <param name="validHits"> Array of hits that actually hit something. <b>Allocator.Persistent</b>.</param>
		/// <param name="batchCount">Job batch count.</param>
		public static void CheckRaycastHitsPersistent
		(NativeArray<RaycastHit> hits,
		 ref NativeArray<RaycastHit> validHits,
		 int batchCount = 64)
		{
			if (hits.Length == 0)
				return;

			if (hits.Length != validHits.Length)
			{
				if (validHits.IsCreated)
					validHits.Dispose();

				validHits = new NativeArray<RaycastHit>(hits.Length, Allocator.Persistent);
			}

			var checkRaycastHitJob = new CheckRaycastHitJob()
									 {
										 Hits = hits,
										 ValidHits = validHits
									 };

			checkRaycastHitJob.Schedule(hits.Length, batchCount).Complete();
		}
	}
}