using System;
using System.Collections.Generic;
using System.Diagnostics;
using Bezoro.Unity.Core.Common.Extensions;
using NUnit.Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.PlayMode.GameObjectExtensions
{
	[TestFixture]
	/// <summary>
	/// Verifies recursive hierarchy activation, destroyed-child handling, and deep-tree performance.
	/// </summary>
	public sealed class ActivateHierarchyTests
	{
		#region Test Methods

		[Test]
		public void Activates_AllNodes_EvenIf_SomeAreInactive()
		{
			var root = new GameObject(
				"Root"
			) { name = "Root" };

			var child = new GameObject(
				"Child"
			);

			child.transform.SetParent(
				root.transform,
				false
			);

			// set different activation states
			root.SetActive(
				false
			);

			child.SetActive(
				false
			);

			try
			{
				// Act
				root.ActivateHierarchy(
					true
				);

				// Assert
				Assert.IsTrue(
					root.activeSelf
				);

				Assert.IsTrue(
					child.activeSelf
				);
			}
			finally
			{
				DestroyHierarchy(
					root
				);
			}
		}

		[Test]
		public void Deactivates_AllNodes_InHierarchy()
		{
			// Arrange
			var root = new GameObject(
				"Root"
			);

			var childA = new GameObject(
				"ChildA"
			);

			var childB = new GameObject(
				"ChildB"
			);

			var grand = new GameObject(
				"GrandChild"
			);

			childA.transform.SetParent(
				root.transform,
				false
			);

			childB.transform.SetParent(
				root.transform,
				false
			);

			grand.transform.SetParent(
				childA.transform,
				false
			);

			try
			{
				// Act
				root.ActivateHierarchy(
					false
				);

				// Assert
				Assert.IsFalse(
					root.activeSelf
				);

				Assert.IsFalse(
					childA.activeSelf
				);

				Assert.IsFalse(
					childB.activeSelf
				);

				Assert.IsFalse(
					grand.activeSelf
				);
			}
			finally
			{
				DestroyHierarchy(
					root
				);
			}
		}

		[Test]
		public void Handles_VeryDeepHierarchy_WithoutStackOverflow()
		{
			const int DEPTH = 2_000; // far beyond Unity's default call-stack depth

			var root = BuildLinearHierarchy(DEPTH);

			try
			{
				Assert.DoesNotThrow(() => root.ActivateHierarchy(
										false
									)
				);

				// Spot-check a few nodes
				Assert.IsFalse(
					root.activeSelf
				);

				Assert.IsFalse(
					root.transform.GetChild(
						0
					).gameObject.activeSelf
				);

				// Get the deepest child in the linear chain
				var deepestChild = GetDeepestChild(
					root
				);

				Assert.IsFalse(
					deepestChild.activeSelf
				);
			}
			finally
			{
				DestroyHierarchy(
					root
				);
			}
		}

		[Test]
		public void Performance_LargeHierarchy()
		{
			// Arrange - Create a wider hierarchy (breadth vs depth)
			const int    LEVELS             = 5;
			const int    CHILDREN_PER_LEVEL = 5;
			const double MAX_ELAPSED_MILLIS = 50d;

			var root         = new GameObject("Root");
			var currentLevel = new List<GameObject> { root };

			// Build a tree with multiple levels and many children per level
			for (var level = 0; level < LEVELS; level++)
			{
				var nextLevel = new List<GameObject>();
				foreach (var parent in currentLevel)
				{
					for (var i = 0; i < CHILDREN_PER_LEVEL; i++)
					{
						var child = new GameObject($"L{level}_Child_{i}");
						child.transform.SetParent(parent.transform, false);
						nextLevel.Add(child);
					}
				}

				currentLevel = nextLevel;
			}

			var totalObjects   = 1;
			var objectsAtLevel = 1;
			for (var i = 0; i < LEVELS; i++)
			{
				objectsAtLevel *= CHILDREN_PER_LEVEL;
				totalObjects   += objectsAtLevel;
			}

			try
			{
				// Act & Assert - Measure performance
				var stopwatch = Stopwatch.StartNew();

				root.ActivateHierarchy(
					false
				);

				stopwatch.Stop();

				// Should complete within reasonable time (adjust threshold as needed)
				Assert.That(
					stopwatch.Elapsed.TotalMilliseconds, Is.LessThan(MAX_ELAPSED_MILLIS),
					$"ActivateHierarchy took {stopwatch.Elapsed.TotalMilliseconds:F2}ms for {totalObjects} objects, expected < {MAX_ELAPSED_MILLIS}ms"
				);

				// Verify correctness - spot check some nodes
				Assert.IsFalse(root.activeSelf, "Root should be deactivated");

				Assert.IsFalse(root.transform.GetChild(0).gameObject.activeSelf, "First child should be deactivated");

				Assert.IsFalse(
					root.transform.GetChild(CHILDREN_PER_LEVEL - 1).gameObject.activeSelf,
					"Last child should be deactivated"
				);

				Debug.Log(
					$"ActivateHierarchy performance: {stopwatch.ElapsedMilliseconds}ms fo {totalObjects} GameObjects"
				);

				Debug.Log($"Average time per GameObject: {(float)stopwatch.ElapsedMilliseconds / totalObjects:F6}ms");
			}
			finally
			{
				DestroyHierarchy(
					root
				);
			}
		}

		[Test]
		public void Skips_DestroyedChildren_Gracefully()
		{
			var root = new GameObject(
				"Root"
			);

			var child = new GameObject(
				"Child"
			);

			child.transform.SetParent(
				root.transform,
				false
			);

			// Destroy the child before invoking the method
			Object.DestroyImmediate(
				child
			);

			try
			{
				// Should not throw even though one child has been destroyed
				Assert.DoesNotThrow(() => root.ActivateHierarchy(
										false
									)
				);

				Assert.IsFalse(
					root.activeSelf
				);
			}
			finally
			{
				DestroyHierarchy(
					root
				);
			}
		}

		[Test]
		public void Throws_OnNull_GameObject()
		{
			GameObject target = null!;
			Assert.Throws<ArgumentNullException>(() => target.ActivateHierarchy(
													 true
												 )
			);
		}

		#endregion

		#region Helpers

		private static GameObject BuildLinearHierarchy(int depth)
		{
			var root = new GameObject(
				"Root"
			);

			var current = root;

			for (var i = 0; i < depth; i++)
			{
				var child = new GameObject(
					$"Node_{i}"
				);

				child.transform.SetParent(
					current.transform,
					false
				);

				current = child;
			}

			return root;
		}

		private static GameObject GetDeepestChild(GameObject root)
		{
			var current = root;
			while (current.transform.childCount > 0)
			{
				current = current.transform.GetChild(
					0
				).gameObject;
			}

			return current;
		}

		private static void DestroyHierarchy(GameObject root)
		{
			if (root != null)
				Object.DestroyImmediate(
					root
				);
		}

		#endregion
	}
}
