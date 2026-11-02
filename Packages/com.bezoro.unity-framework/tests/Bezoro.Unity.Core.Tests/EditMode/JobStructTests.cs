using Bezoro.Unity.Core.Jobs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class JobStructTests
	{
		[Test]
		public void CheckRaycastHitJob_Execute_CopiesOnlyHitsWithPositiveDistance()
		{
			var hits      = new NativeArray<RaycastHit>(2, Allocator.Temp);
			var validHits = new NativeArray<RaycastHit>(2, Allocator.Temp);

			try
			{
				hits[0] = new() { distance = 0f };
				hits[1] = new() { distance = 3.5f };

				var job = new CheckRaycastHitJob
				{
					Hits      = hits,
					ValidHits = validHits
				};

				job.Execute(0);
				job.Execute(1);

				Assert.AreEqual(0f,   validHits[0].distance);
				Assert.AreEqual(3.5f, validHits[1].distance);
			}
			finally
			{
				validHits.Dispose();
				hits.Dispose();
			}
		}

		[Test]
		public void CreateRaycastCommandsJob_Execute_UsesMatchingOriginAndConfiguredDirection()
		{
			var origins  = new NativeArray<float3>(2, Allocator.Temp);
			var commands = new NativeArray<RaycastCommand>(2, Allocator.Temp);

			try
			{
				origins[0] = new(1f, 2f, 3f);
				origins[1] = new(4f, 5f, 6f);

				var job = new CreateRaycastCommandsJob
				{
					Origins   = origins,
					Direction = new(0f, -1f, 0f),
					Commands  = commands
				};

				job.Execute(1);

				AssertVector3(new(4f, 5f, 6f), commands[1].from);
				AssertVector3(Vector3.down,    commands[1].direction);
			}
			finally
			{
				commands.Dispose();
				origins.Dispose();
			}
		}

		[Test]
		public void GeneratePointsOnPlaneJob_Execute_GeneratesEvenGridAroundCenter()
		{
			var points = new NativeArray<float3>(4, Allocator.Temp);

			try
			{
				var job = new GeneratePointsOnPlaneJob
				{
					Center          = new(10f, 0f, 20f),
					Width           = 2f,
					Height          = 4f,
					NumPointsWidth  = 2,
					NumPointsHeight = 2,
					RightAxis       = new(1f, 0f, 0f),
					UpAxis          = new(0f, 0f, 1f),
					ResultPoints    = points
				};

				for (var i = 0; i < points.Length; i++)
					job.Execute(i);

				AssertFloat3(new(11f, 0f, 22f), points[0]);
				AssertFloat3(new(9f, 0f, 22f),  points[1]);
				AssertFloat3(new(11f, 0f, 18f), points[2]);
				AssertFloat3(new(9f, 0f, 18f),  points[3]);
			}
			finally
			{
				points.Dispose();
			}
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
