using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class JobHelpersTests
	{
		[Test]
		public void CheckRaycastHitsPersistent_MissClearsPreviouslyPopulatedSlot()
		{
			var input = new NativeArray<RaycastHit>(1, Allocator.TempJob);
			var output = default(NativeArray<RaycastHit>);
			try
			{
				input[0] = new RaycastHit { distance = 4f };
				JobHelpers.CheckRaycastHitsPersistent(input, ref output);
				Assert.AreEqual(4f, output[0].distance);
				input[0] = default;
				JobHelpers.CheckRaycastHitsPersistent(input, ref output);
				Assert.AreEqual(0f, output[0].distance);
			}
			finally
			{
				if (output.IsCreated)
					output.Dispose();
				input.Dispose();
			}
		}

		[TestCase(1, 1)]
		[TestCase(1, 3)]
		[TestCase(3, 1)]
		public void GeneratePointsOnPlanePersistent_CentersSingletonAxis(int columns, int rows)
		{
			var points = default(NativeArray<float3>);
			try
			{
				JobHelpers.GeneratePointsOnPlanePersistent(new(3f, 4f, 5f), 4f, 6f, columns, rows,
					new(1f, 0f, 0f), new(0f, 1f, 0f), ref points);
				for (var i = 0; i < points.Length; i++)
				{
					float x = columns == 1 ? 3f : 5f - 2f * (i % columns);
					float y = rows == 1 ? 4f : 7f - 3f * (i / columns);
					AssertFloat3(new(x, y, 5f), points[i]);
				}
			}
			finally
			{
				if (points.IsCreated)
					points.Dispose();
			}
		}

		[Test]
		public void CreateRaycastCommandsPersistent_WhenOriginCountChanges_ReallocatesCommands()
		{
			var origins  = new NativeArray<float3>(2, Allocator.TempJob);
			var commands = new NativeArray<RaycastCommand>(1, Allocator.Persistent);

			try
			{
				origins[0] = new(1f, 2f, 3f);
				origins[1] = new(4f, 5f, 6f);

				JobHelpers.CreateRaycastCommandsPersistent(origins, new(0f, -1f, 0f), ref commands);

				Assert.AreEqual(2, commands.Length);
				AssertVector3(new(1f, 2f, 3f), commands[0].from);
				AssertVector3(Vector3.down,    commands[0].direction);
				AssertVector3(new(4f, 5f, 6f), commands[1].from);
			}
			finally
			{
				if (commands.IsCreated)
					commands.Dispose();

				origins.Dispose();
			}
		}

		[Test]
		public void GeneratePointsOnPlanePersistent_NormalizesAxesAndAllocatesExpectedPointCount()
		{
			var points = default(NativeArray<float3>);

			try
			{
				JobHelpers.GeneratePointsOnPlanePersistent(
					new(10f, 0f, 20f),
					2f,
					4f,
					2,
					2,
					new(2f, 0f, 0f),
					new(0f, 0f, 3f),
					ref points
				);

				Assert.AreEqual(4, points.Length);
				AssertFloat3(new(11f, 0f, 22f), points[0]);
				AssertFloat3(new(9f, 0f, 22f),  points[1]);
				AssertFloat3(new(11f, 0f, 18f), points[2]);
				AssertFloat3(new(9f, 0f, 18f),  points[3]);
			}
			finally
			{
				if (points.IsCreated)
					points.Dispose();
			}
		}

		[Test]
		public void GeneratePointsOnPlanePersistent_WhenPointCountIsZero_LeavesResultUncreated()
		{
			var points = default(NativeArray<float3>);

			JobHelpers.GeneratePointsOnPlanePersistent(
				float3.zero,
				1f,
				1f,
				0,
				2,
				new(1f, 0f, 0f),
				new(0f, 1f, 0f),
				ref points
			);

			Assert.IsFalse(points.IsCreated);
		}

		private static void AssertFloat3(float3 expected, float3 actual)
		{
			Assert.AreEqual(expected.x, actual.x, 0.0001f);
			Assert.AreEqual(expected.y, actual.y, 0.0001f);
			Assert.AreEqual(expected.z, actual.z, 0.0001f);
		}

		private static void AssertVector3(Vector3 expected, Vector3 actual)
		{
			Assert.AreEqual(expected.x, actual.x, 0.0001f);
			Assert.AreEqual(expected.y, actual.y, 0.0001f);
			Assert.AreEqual(expected.z, actual.z, 0.0001f);
		}
	}
}
