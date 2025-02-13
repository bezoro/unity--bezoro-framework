using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bezoro.Framework.Core.Tests.Edit_Mode.Eitor.Utilities;
using Moq;
using NUnit.Framework;

namespace Bezoro.Framework.Core.Tests.Edit_Mode.Array
{
	[TestFixture]
	[Category("Array Operations/Merging")]
	[TestOf(typeof(Core.Array.Merging))]
	public static class Merging_Tests
	{
		private static Mock<I_Logger> _Logger_Mock;

		public class Merging_Test_Parameter_Data
		{
			public object[]              Target;
			public object[]              Source;
			public Null_Element_Handling NullHandling;

			public object[] Expected;
		}

		/// <summary>
		///     Facilitates the operation of filling null elements in a target array with corresponding
		///     non-null values from a source array and appending any remaining source elements
		///     based on the specified null-handling policy.
		/// </summary>
		public class Fill_Null_Elements_And_Append
		{
			[SetUp]
			public void Setup()
			{
				_Logger_Mock = new();
			}

			// Tests for various invalid input scenarios to ensure the method handles them gracefully.
			public class Parameter_Validation
			{
				private static IEnumerable<TestCaseData> Source_Parameter_Validation_Test_Cases()
				{
					yield return Test_Utils.Create_Test_Case(
						"SHOULD_Not_Modify_Target_WHEN_Source_And_Target_Are_Null",
						new Merging_Test_Parameter_Data
						{
							Source       = null,
							Target       = null,
							Expected     = null,
							NullHandling = Null_Element_Handling.Exclude
						});

					yield return Test_Utils.Create_Test_Case(
						"SHOULD_Not_Modify_Target_WHEN_Source_Is_Null_And_Target_Has_Values",
						new Merging_Test_Parameter_Data
						{
							Source   = null,
							Target   = new[] { "A", "B", "C" },
							Expected = new[] { "A", "B", "C" }
						});

					yield return Test_Utils.Create_Test_Case(
						"SHOULD_Not_Modify_Target_WHEN_Source_Is_Null_And_Target_Has_Mixed_Values",
						new Merging_Test_Parameter_Data
						{
							Source   = null,
							Target   = new object[] { "A", 123, null },
							Expected = new object[] { "A", 123, null }
						});

					yield return Test_Utils.Create_Test_Case(
						"SHOULD_Not_Modify_Target_WHEN_Source_Is_Null_And_Target_Contains_Only_Nulls",
						new Merging_Test_Parameter_Data
						{
							Source   = null,
							Target   = new object[] { null, null },
							Expected = new object[] { null, null }
						});

					yield return Test_Utils.Create_Test_Case(
						"SHOULD_Not_Modify_Target_WHEN_Source_Is_Null_And_Target_Is_Empty",
						new Merging_Test_Parameter_Data
						{
							Source   = null,
							Target   = new object[] { },
							Expected = new object[] { }
						});

					yield return Test_Utils.Create_Test_Case(
						"SHOULD_Not_Modify_Target_WHEN_Source_Is_Null_And_Target_Contains_Custom_Reference_Objects",
						new Merging_Test_Parameter_Data
						{
							Source   = null,
							Target   = new[] { new Test_Object(), new Test_Object() },
							Expected = new[] { new Test_Object(), new Test_Object() }
						});
				}

				// [TestCaseSource(nameof(Target_Parameter_Validation_Test_Cases))]
				[TestCaseSource(nameof(Source_Parameter_Validation_Test_Cases))]
				public void Source_Parameter_Tests(Merging_Test_Parameter_Data data)
				{
					// Act
					Core.Array.Merging.Fill_Null_Elements_And_Append(
						data.Source, ref data.Target, data.NullHandling);

					// Assert
					Assert_Are_Equal(data.Expected, data.Target);
					Verify_Logger_Ran(_Logger_Mock, Log_Type.Warning);
				}

				public void SHOULD_Handle_Target_Correctly_Based_On_Null_Element_Handling(
					string[] source,
					string[] target,
					Null_Element_Handling handling,
					string[] expectedTarget
				)
				{
					// Act
					Core.Array.Merging.Fill_Null_Elements_And_Append(
						source,
						ref target,
						handling
					);

					// Assert
					Assert_Is_True(target.SequenceEqual(expectedTarget));
					Verify_Logger_Ran(_Logger_Mock, Log_Type.Success);
				}

				public static IEnumerable<TestCaseData> Target_Parameter_Validation_Test_Cases()
				{
					yield return Test_Utils.Create_Test_Case(
						"SHOULD_Initialize_And_Match_Target_WHEN_Target_Is_Null",
						new Merging_Test_Parameter_Data
						{
							Expected     = new object[] { "A", "B", "C" },
							NullHandling = Null_Element_Handling.Exclude,
							Source       = new object[] { "A", "B", "C" },
							Target       = null
						});

					yield return Test_Utils.Create_Test_Case(
						"SHOULD_Ignore_Null_Elements_AND_Initialize_Target_WHEN_Target_Is_Null_With_Exclude",
						new Merging_Test_Parameter_Data
						{
							Source       = new[] { "A", "B", (object)null, "C" },
							Target       = null,
							NullHandling = Null_Element_Handling.Exclude,
							Expected     = new object[] { "A", "B", "C" }
						});

					yield return Test_Utils.Create_Test_Case(
						"SHOULD_Include_Null_Elements_AND_Initialize_Target_WHEN_Target_Is_Null_With_Preserve",
						new Merging_Test_Parameter_Data
						{
							Source       = new[] { "X", "Y", (object)null, "Z" },
							Target       = null,
							NullHandling = Null_Element_Handling.Include,
							Expected     = new[] { "X", "Y", (object)null, "Z" }
						});
				}
			}

			[Test]
			public void SHOULD_Return_Gracefully_WHEN_Both_Target_And_Source_Are_Null()
			{
				// Arrange
				string[] target = null;

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Append(
					null, ref target, Null_Element_Handling.Exclude
				);

				// Assert
				NUnit.Framework.Assert.That(
					target,
					Is.Null,
					"Target array should remain null when both the target and source arrays are null."
				);

				_Logger_Mock.Verify(
					log => log.Log_Warning(It.IsAny<string>()), Times.Once, "A warning should have been logged."
				);
			}

			// Tests to verify expected outcomes in ordinary use cases and valid inputs.

			#region 2. Core Functionality

			[TestCase(
				new[] { "A", "B", "C" }, null, Null_Element_Handling.Exclude, new[] { "A", "B", "C" },
				TestName = "SHOULD_Initialize_Target_WHEN_Target_Is_Null_And_Handling_Is_Exclude")]
			[TestCase(
				new[] { "A", "B", "C" }, new[] { "X", null, "Y" }, Null_Element_Handling.Exclude,
				new[] { "X", "A", "Y", "B", "C" },
				TestName = "SHOULD_Append_Source_To_Target_WHEN_Target_Contains_Elements_And_Handling_Is_Exclude")]
			[TestCase(
				new[] { "A", "B", "C" }, new[] { (object)null, null }, Null_Element_Handling.Include,
				new[] { null, null, "A", "B", "C" },
				TestName = "SHOULD_Include_Null_Elements_IN_Target_WHEN_Handling_Is_Include")]
			[TestCase(
				new[] { "A", "B", "C" }, new[] { (object)null, null }, Null_Element_Handling.Exclude,
				new[] { "A", "B", "C" },
				TestName = "SHOULD_Exclude_Null_Elements_From_Target_WHEN_Handling_Is_Exclude")]
			public void SHOULD_Correctly_Merge_Source_And_Target_Based_On_Null_Element_Handling(
				string[] source,
				string[] target,
				Null_Element_Handling nullElementHandling,
				string[] expected
			)
			{
				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Append(
					source, ref target, nullElementHandling);

				// Assert
				NUnit.Framework.Assert.That(
					target,
					Is.EqualTo(expected),
					"Target array was not merged correctly. " +
					$"Expected: {string.Join(", ", expected ?? System.Array.Empty<string>())}, " +
					$"but got: {string.Join(", ", target ?? System.Array.Empty<string>())}"
				);

				_Logger_Mock.Verify(
					log => log.Log_Success(It.IsAny<string>()), Times.Once,
					"Logger should have been called with Log_Success exactly once."
				);
			}

			[Test]
			public void SHOULD_Replace_Null_Target_Elements_WHEN_Source_Contains_Valid_Values()
			{
				// Arrange
				var source = new[] { "A", "B", "C" };
				var target = new[] { null, "X", null };

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Append(
					source,
					ref target,
					Null_Element_Handling.Exclude
				);

				// Assert
				NUnit.Framework.Assert.That(
					target,
					Is.EqualTo(new[] { "A", "X", "B", "C" }),
					"Target's null elements were not replaced correctly, or the remaining source elements were not appended in order."
				);
			}

			[Test]
			public void SHOULD_Append_Valid_Source_Elements_WHEN_Target_Has_Empty_Slots()
			{
				// Arrange
				var source = new[] { "D", "E", null };
				var target = new[] { null, "A", null };

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Append(
					source,
					ref target,
					Null_Element_Handling.Exclude
				);

				// Assert
				NUnit.Framework.Assert.That(
					target,
					Is.EqualTo(new[] { "D", "A", "E" }),
					"Source elements were not appended or inserted in the correct order, or null elements were incorrectly handled."
				);
			}

			[Test]
			public void SHOULD_Include_Null_Elements_WHEN_Null_Handling_Is_Preserve()
			{
				// Arrange
				var source = new[] { "A", null, "C" };
				var target = new[] { null, "B", null };

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Append(
					source,
					ref target,
					Null_Element_Handling.Include
				);

				// Assert
				NUnit.Framework.Assert.That(
					target,
					Is.EqualTo(new[] { "A", "B", null, "C" }),
					"Null elements were not preserved when the Null_Element_Handling mode was set to Include."
				);
			}

			#endregion

			// Tests that cover rare, unexpected, or extreme conditions that might break regular functionality.

			#region 3. Edge Cases

			[Test]
			public void SHOULD_Do_Nothing_WHEN_Target_Has_No_Null_And_Source_Is_Empty()
			{
				// Arrange
				var source = System.Array.Empty<string>();
				var target = new[] { "A", "B", "C" };

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Append(
					source, ref target, Null_Element_Handling.Exclude
				);

				// Assert
				NUnit.Framework.Assert.That(
					target,
					Is.EqualTo(new[] { "A", "B", "C" }),
					"Target array was incorrectly modified when the source array was empty, despite there being no null slots to fill."
				);
			}

			[Test]
			public void SHOULD_Populate_And_Append_WHEN_Arrays_Are_Large()
			{
				// Arrange
				var source = Enumerable.Range(1, 10000).Select(i => $"S{i}").ToArray();
				var target = new string[5000];

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Append(
					source, ref target, Null_Element_Handling.Exclude
				);

				// Assert
				NUnit.Framework.Assert.That(
					target.Length,
					Is.EqualTo(10000),
					"Target array does not have the correct final length after appending large arrays."
				);

				NUnit.Framework.Assert.That(
					target.All(element => !string.IsNullOrEmpty(element)),
					Is.True,
					"No null or empty elements were expected in the target array."
				);
			}

			#endregion

			// Tests to validate overall efficiency and scalability.

			#region 4. Performance

			[Test]
			public void SHOULD_Execute_In_Reasonable_Time_WHEN_Source_Has_Large_Array()
			{
				// Arrange
				var source    = Enumerable.Range(1, 100000).Select(i => $"S{i}").ToArray();
				var target    = new string[50000];
				var stopwatch = new Stopwatch();

				// Act
				stopwatch.Start();

				Core.Array.Merging.Fill_Null_Elements_And_Append(
					source, ref target, Null_Element_Handling.Exclude
				);

				stopwatch.Stop();

				// Assert
				NUnit.Framework.Assert.DoesNotThrow(
					() => Core.Array.Merging.Fill_Null_Elements_And_Append(source, ref target),
					"The method failed to handle large inputs and threw an exception."
				);

				NUnit.Framework.Assert.That(
					stopwatch.ElapsedMilliseconds,
					Is.LessThan(100),
					$"Performance test failed: Operation took {stopwatch.ElapsedMilliseconds}ms, exceeding the acceptable threshold."
				);
			}

			#endregion

			// Private members used by the tests

			#region Plumbing

			private void Fill_Null_Elements_And_Append_Excluding_Source_Nulls(object[] source, object[] target)
			{
				Core.Array.Merging.Fill_Null_Elements_And_Append(
					source, ref target, Null_Element_Handling.Exclude
				);
			}

			private void Fill_Null_Elements_And_Append_Including_Source_Nulls(object[] source, object[] target)
			{
				Core.Array.Merging.Fill_Null_Elements_And_Append(
					source, ref target, Null_Element_Handling.Include
				);
			}

			private static void Assert_Is_True(bool condition)
			{
				NUnit.Framework.Assert.That(condition, Is.True, $"Condition was not true: {condition}");
			}

			private static void Assert_Are_Equal(object[] expected, object[] actual)
			{
				NUnit.Framework.Assert.That(
					actual, Is.EqualTo(expected),
					$"Arrays are not equal. Expected: {expected}, Actual: {actual}");
			}

			private void Assert_Are_Equivalent<T>(T[] expected, T[] actual)
			{
				NUnit.Framework.Assert.That(
					actual,
					Is.EquivalentTo(expected),
					$"Arrays do not contain the same elements. Expected: {string.Join(", ", expected)}, Actual: {string.Join(", ", actual)}");
			}

			private static void Verify_Logger_Ran(Mock<I_Logger> logger, Log_Type type, int times = 1)
			{
				switch ( type )
				{
					case Log_Type.Success:
						logger.Verify(
							log => log.Log_Success(It.IsAny<string>()), Times.Exactly(times),
							$"Expected '{Log_Type.Success}' to be logged {times} time(s).");

						break;

					case Log_Type.Warning:
						_Logger_Mock.Verify(
							log => log.Log_Warning(It.IsAny<string>()), Times.Exactly(times),
							$"Expected '{Log_Type.Warning}' to be logged {times} time(s).");

						break;

					case Log_Type.Error:
						_Logger_Mock.Verify(
							log => log.Log_Error(It.IsAny<string>()), Times.Exactly(times),
							$"Expected '{Log_Type.Error}' to be logged {times} time(s).");

						break;

					case Log_Type.Log:
						_Logger_Mock.Verify(
							log => log.Log(It.IsAny<string>()), Times.Exactly(times),
							$"Expected '{Log_Type.Log}' to be logged {times} time(s).");

						break;
					default:
						NUnit.Framework.Assert.Fail($"Unexpected log type: {type}");
						break;
				}
			}

			#endregion
		}

		// TODO: WIP
		public class Fill_Null_Elements_And_Prepend
		{
			// Tests for various invalid input scenarios to ensure the method handles them gracefully.

			#region 1. Parameter Validation

			[Test]
			public void SHOULD_Not_Modify_Target_WHEN_Source_Is_Null()
			{
				// Arrange
				Test_Object[] source = null; // Null source
				var           target = new[] { new Test_Object(), null };

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Prepend(source, ref target);

				// Assert
				NUnit.Framework.Assert.That(target.Length, Is.EqualTo(2));
				NUnit.Framework.Assert.That(target[0], Is.Not.Null);
				NUnit.Framework.Assert.That(target[1], Is.Null); // Target should remain unchanged.
			}

			[Test]
			public void SHOULD_Not_Throw_Exception_WHEN_Target_Is_Null()
			{
				// Arrange
				var           source = new[] { new Test_Object() };
				Test_Object[] target = null;

				// Act
				NUnit.Framework.Assert.DoesNotThrow(
					() =>
					{
						Core.Array.Merging.Fill_Null_Elements_And_Prepend(source, ref target);
					}
				);

				// Assert
				NUnit.Framework.Assert.That(target, Is.Not.Null);
				NUnit.Framework.Assert.That(target.Length == 1); // One item from source prepended.
			}

			#endregion

			// Tests to verify expected outcomes in ordinary use cases and valid inputs.

			#region 2. Core Functionality

			[Test]
			public void SHOULD_Fill_Null_Slots_AND_Prepend_Extra_Source_Elements()
			{
				// Arrange
				var source = new[]
				{
					new Test_Object { Id = 1 },
					new Test_Object { Id = 2 },
					new Test_Object { Id = 3 }
				};

				var target = new[]
				{
					null, // First element is null and should be filled
					new Test_Object { Id = 4 }
				};

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Prepend(source, ref target);

				// Assert
				NUnit.Framework.Assert.That(target.Length, Is.EqualTo(4)); // 1 source filled + 2 additional prepended.
				NUnit.Framework.Assert.That(target[0].Id, Is.EqualTo(1));  // Null slot filled from source.
				NUnit.Framework.Assert.That(target[1].Id, Is.EqualTo(2));  // New source element prepended.
				NUnit.Framework.Assert.That(target[2].Id, Is.EqualTo(3));  // Another source element prepended.
				NUnit.Framework.Assert.That(target[3].Id, Is.EqualTo(4));  // Original non-null target preserved.
			}

			[TestCase(Null_Element_Handling.Include)]
			[TestCase(Null_Element_Handling.Exclude)]
			public void SHOULD_Respect_Null_Element_Handling_Mode(Null_Element_Handling mode)
			{
				// Arrange
				var source = new[]
				{
					null,
					new Test_Object { Id = 2 }
				};

				var target = new Test_Object[]
				{
					null,
					null
				};

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Prepend(source, ref target, mode);

				// Assert
				if (mode == Null_Element_Handling.Include)
				{
					NUnit.Framework.Assert.That(target[0], Is.Null);          // First null preserved
					NUnit.Framework.Assert.That(target[1].Id, Is.EqualTo(2)); // Second source element used
				}
				else
				{
					NUnit.Framework.Assert.That(target[0].Id, Is.EqualTo(2)); // Non-null fills first.

					NUnit.Framework.Assert.That(
						target[1], Is.Null
					); // Nothing prepended since other source entry is null.
				}
			}

			#endregion

			// Tests that cover rare, unexpected, or extreme conditions that might break regular functionality.

			#region 3. Edge Cases

			[Test]
			public void SHOULD_Do_Nothing_With_Empty_Source_And_Empty_Target()
			{
				// Arrange
				var source = System.Array.Empty<Test_Object>();
				var target = System.Array.Empty<Test_Object>();

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Prepend(source, ref target);

				// Assert
				NUnit.Framework.Assert.That(target.Length, Is.EqualTo(0));
			}

			[Test]
			public void SHOULD_Handle_Very_Large_Target_And_Source()
			{
				// Arrange
				const int size = 1000000;

				var source = Enumerable.Range(0, size)
					.Select(i => new Test_Object { Id = i }).ToArray();

				var target = new Test_Object[size]; // Entire target is null.

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Prepend(source, ref target);

				// Assert
				NUnit.Framework.Assert.That(target.Length, Is.EqualTo(size * 2)); // Entire source added as prepend.

				NUnit.Framework.Assert.That(
					target.Skip(size).Take(size)
						.Select(obj => obj.Id)
						.SequenceEqual(Enumerable.Range(0, size))
				); // All IDs match prepended.
			}

			[Test]
			public void SHOULD_Append_All_Source_WHEN_Target_Has_No_Nulls()
			{
				// Arrange
				var source = new[]
				{
					new Test_Object { Id = 1 },
					new Test_Object { Id = 2 }
				};

				var target = new[]
				{
					new Test_Object(),
					new Test_Object()
				}; // Fully populated target

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Prepend(source, ref target);

				// Assert
				NUnit.Framework.Assert.That(target.Length, Is.EqualTo(4)); // Original target + source prepended.
				NUnit.Framework.Assert.That(target[0].Id, Is.EqualTo(1));  // First source element prepended.
				NUnit.Framework.Assert.That(target[1].Id, Is.EqualTo(2));  // Second source element prepended.
			}

			#endregion

			// Tests to validate overall efficiency and scalability.

			#region 4. Performance

			[Test]
			public void SHOULD_Perform_Efficiently_With_Large_Arrays()
			{
				// Arrange
				const int size = 1000000;

				var source = Enumerable.Range(0, size)
					.Select(i => new Test_Object { Id = i }).ToArray();

				var target = Enumerable.Repeat<Test_Object>(null, size).ToArray();

				// Act & Assert
				NUnit.Framework.Assert.That(
					() =>
					{
						Core.Array.Merging.Fill_Null_Elements_And_Prepend(source, ref target);
					}, Throws.Nothing
				);
			}

			#endregion

			[Test]
			public void Initializes_Null_Target()
			{
				// Arrange
				var           source = new[] { new Test_Object(), new Test_Object() };
				Test_Object[] target = null; // Target starts as null

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Prepend(source, ref target);

				// Assert
				NUnit.Framework.Assert.That(target, Is.Not.Null, "Target should be initialized.");

				NUnit.Framework.Assert.That(
					target.Length, Is.EqualTo(source.Length), "Target length should match source length."
				);

				NUnit.Framework.Assert.That(
					source.All(item => target.Contains(item)),
					"All elements from source should be in target."
				);
			}

			[Test]
			public void Prepends_Empty_Target()
			{
				// Arrange
				var source = new[] { new Test_Object(), new Test_Object() };
				var target = System.Array.Empty<Test_Object>(); // Empty target array

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Prepend(source, ref target);

				// Assert
				NUnit.Framework.Assert.That(target, Is.Not.Null, "Target should not be null after operation.");

				NUnit.Framework.Assert.That(
					target.Length, Is.EqualTo(source.Length), "Target length should match source length."
				);

				NUnit.Framework.Assert.That(
					source.All(item => target.Contains(item)),
					"All elements from source should be prepended into target."
				);

				NUnit.Framework.Assert.That(
					target, Is.EqualTo(source), "Target contents should match prepended source."
				);
			}

			[Test]
			public void Fills_Target_With_Enough_Empty_Elements()
			{
				// Arrange
				var source = new[] { new Test_Object(), new Test_Object(), new Test_Object() };
				var target = new Test_Object[5];

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Prepend(source, ref target);

				// Assert
				NUnit.Framework.Assert.That(target, Is.Not.Null, "Target should not be null after operation.");

				NUnit.Framework.Assert.That(
					target.Length, Is.GreaterThanOrEqualTo(source.Length),
					"Target should have enough space for source elements."
				);

				NUnit.Framework.Assert.That(
					source.All(item => target.Contains(item)),
					"Target should contain all elements from the source."
				);
			}

			[Test]
			public void Prepends_To_Full_Target()
			{
				// Arrange
				var source = new[] { new Test_Object(), new Test_Object() };
				var target = new[] { new Test_Object(), new Test_Object() }; // Full target array

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Prepend(
					source, ref target, Null_Element_Handling.Exclude
				);

				// Assert
				NUnit.Framework.Assert.That(
					target.Length, Is.EqualTo(source.Length + 2),
					"Target length should expand to include prepended source elements."
				);

				NUnit.Framework.Assert.That(
					source.All(item => target.Contains(item)),
					"All elements from source should be in the target."
				);

				NUnit.Framework.Assert.That(
					target.Take(source.Length), Is.EqualTo(source),
					"Prepended elements should be at the beginning of the target."
				);
			}

			[Test]
			public void Fills_Null_Elements_And_Prepends_The_Rest()
			{
				// Arrange
				var source = new[] { new Test_Object(), new Test_Object(), new Test_Object(), new Test_Object() };
				var target = new Test_Object[2];

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Prepend(source, ref target);

				// Assert
				NUnit.Framework.Assert.That(
					target, Is.Not.Null, "Target array should not be null after the operation."
				);

				NUnit.Framework.Assert.That(
					target.Length, Is.GreaterThanOrEqualTo(source.Length),
					"Target should have enough space to hold the source elements."
				);

				NUnit.Framework.Assert.That(
					source.Where(item => item != null).All(item => target.Contains(item)),
					"Target should contain all non-null elements from the source."
				);

				NUnit.Framework.Assert.That(
					target.Take(source.Length).Where(item => item != null),
					Is.EqualTo(source.Where(item => item != null)),
					"Non-null elements from the source should be prepended in the target."
				);
			}
		}

		// TODO: WIP
		public class Fill_Null_Elements_Only
		{
			#region 1. Parameter Validation

			[Test]
			public void SHOULD_Do_Nothing_WHEN_Source_Is_Null()
			{
				// Arrange
				var target = new string[3];

				// Act
				Core.Array.Merging.Fill_Null_Elements_Only(null, ref target);

				// Assert
				NUnit.Framework.Assert.That(
					target, Is.All.Null, "Target array should remain unchanged when the source is null."
				);

				_Logger_Mock.Verify(
					logger => logger.Log_Warning(It.IsAny<string>()),
					Times.Once,
					"A warning should have been logged."
				);
			}

			[Test]
			public void SHOULD_Initialize_Target_WHEN_Target_Is_Null()
			{
				// Arrange
				var      source = new[] { "A", "B" };
				string[] target = null;

				// Act
				Core.Array.Merging.Fill_Null_Elements_Only(source, ref target);

				// Assert
				NUnit.Framework.Assert.IsNotNull(target, "Target should be initialized when it is null.");

				NUnit.Framework.Assert.AreEqual(
					source, target, "Target array should match the source array after initialization."
				);
			}

			[Test]
			public void SHOULD_Exit_Gracefully_WHEN_Source_And_Target_Are_Null()
			{
				// Arrange
				string[] target = null;

				// Act
				Core.Array.Merging.Fill_Null_Elements_Only(null, ref target);

				// Assert
				NUnit.Framework.Assert.IsNull(
					target, "Target array should remain null when both source and target are null."
				);
			}

			[Test]
			public void SHOULD_Fill_Target_Null_Slots_WHEN_Source_Is_Valid()
			{
				// Arrange
				var source = new[] { "A", "B", "C" };
				var target = new string[] { null, null, null };

				// Act
				Core.Array.Merging.Fill_Null_Elements_Only(source, ref target);

				// Assert
				NUnit.Framework.Assert.AreEqual(new[] { "A", "B", "C" }, target);
			}

			[Test]
			public void SHOULD_Partially_Fill_Target_Until_Source_Exhausted_WHEN_Source_Is_Smaller_Than_Target()
			{
				// Arrange
				var source = new[] { "A", "B" };
				var target = new string[] { null, null, null };

				// Act
				Core.Array.Merging.Fill_Null_Elements_Only(source, ref target);

				// Assert
				NUnit.Framework.Assert.AreEqual(new[] { "A", "B", null }, target);
			}

			[Test]
			public void SHOULD_Not_Change_Target_WHEN_No_Null_Slots_In_Target()
			{
				// Arrange
				var source = new[] { "A", "B" };
				var target = new[] { "X", "Y", "Z" };

				// Act
				Core.Array.Merging.Fill_Null_Elements_Only(source, ref target);

				// Assert
				NUnit.Framework.Assert.AreEqual(new[] { "X", "Y", "Z" }, target);
			}

			[Test]
			public void SHOULD_Not_Use_Extra_Source_Elements_WHEN_Source_Has_More_Elements_Than_Target_Null_Slots()
			{
				// Arrange
				var source = new[] { "A", "B", "C", "D" };
				var target = new string[] { null, null };

				// Act
				Core.Array.Merging.Fill_Null_Elements_Only(source, ref target);

				// Assert
				NUnit.Framework.Assert.AreEqual(new[] { "A", "B" }, target);
			}

			#endregion

			#region 2. Core Functionality

			[Test]
			public void SHOULD_Fill_Target_Completely_WHEN_Source_And_Target_Sizes_Match()
			{
				// Arrange
				var source = new[] { "A", "B", "C" };
				var target = new string[] { null, null, null };
				var logger = new Mock<I_Logger>();

				// Act
				Core.Array.Merging.Fill_Null_Elements_Only(source, ref target);

				// Assert
				NUnit.Framework.Assert.AreEqual(new[] { "A", "B", "C" }, target);
			}

			[Test]
			public void SHOULD_Fill_Target_Until_Source_Is_Exhausted_WHEN_Source_Is_Smaller_Than_Target_Null_Slots()
			{
				// Arrange
				var source = new[] { "A", "B" };
				var target = new string[] { null, null, null };
				var logger = new Mock<I_Logger>();

				// Act
				Core.Array.Merging.Fill_Null_Elements_Only(source, ref target);

				// Assert
				NUnit.Framework.Assert.AreEqual(new[] { "A", "B", null }, target);
			}

			[Test]
			public void SHOULD_NotChangeTarget_WHEN_NoNullSlotsInTarget()
			{
				// Arrange
				var source = new[] { "A", "B" };
				var target = new[] { "X", "Y", "Z" };
				var logger = new Mock<I_Logger>();

				// Act
				Core.Array.Merging.Fill_Null_Elements_Only(source, ref target);

				// Assert
				NUnit.Framework.Assert.AreEqual(new[] { "X", "Y", "Z" }, target);
			}

			[Test]
			public void SHOULD_NotFillExtraSourceElements_WHEN_SourceHasMoreElementsThanTargetNullSlots()
			{
				// Arrange
				var source = new[] { "A", "B", "C", "D" };
				var target = new string[] { null, null };
				var logger = new Mock<I_Logger>();

				// Act
				Core.Array.Merging.Fill_Null_Elements_Only(source, ref target);

				// Assert
				NUnit.Framework.Assert.AreEqual(new[] { "A", "B" }, target);
			}

			#endregion

			#region 3. Edge Cases

			[Test]
			public void SHOULD_Initialize_Target_With_Source_WHEN_Target_Array_Is_Empty()
			{
				// Arrange
				var source = new[] { "A", "B" };
				var target = System.Array.Empty<string>();

				// Act
				Core.Array.Merging.Fill_Null_Elements_Only(source, ref target);

				// Assert
				NUnit.Framework.Assert.IsNotNull(target, "Target should be initialized when it is empty.");

				NUnit.Framework.Assert.AreEqual(
					source, target, "Target array should match the source array after initialization."
				);
			}

			[Test]
			public void SHOULD_Not_Change_Target_WHEN_Both_Source_And_Target_Contain_Only_Null_Elements()
			{
				// Arrange
				var source = new string[3];
				var target = new string[3];

				// Act
				Core.Array.Merging.Fill_Null_Elements_Only(source, ref target);

				// Assert
				NUnit.Framework.Assert.AreEqual(
					new string[3], target,
					"Target array should remain unchanged since all elements in the source and target arrays are null."
				);
			}

			[Test]
			public void SHOULD_Handle_Large_Arrays_Effectively()
			{
				// Arrange
				var source = new string[10000];
				var target = new string[20000];

				for (var i = 0 ; i < 5000 ; i++)
					source[i] = $"Item {i}";

				for (var i = 0 ; i < 10000 ; i++)
				{
					if (i % 2 == 0)
						target[i] = $"Old {i}";
				}

				// Act
				Core.Array.Merging.Fill_Null_Elements_Only(source, ref target);

				// Assert
				var filled_count = target.Count(x => x?.StartsWith("Item") == true);
				NUnit.Framework.Assert.AreEqual(5000, filled_count); // Only 5000 non-null elements from source filled.
			}

			#endregion

			#region 4. Performance

			[Test]
			public void SHOULD_CompleteQuickly_WHEN_LargeInputArraysProvided()
			{
				// Arrange
				var source                                   = new string[1000000];
				for (var i = 0 ; i < 500000 ; i++) source[i] = $"Element {i}";
				var target                                   = new string[5000000];
				var logger                                   = new Mock<I_Logger>();

				// Act & Assert (with timeout)
				NUnit.Framework.Assert.That(
					() =>
					{
						Core.Array.Merging.Fill_Null_Elements_Only(source, ref target);
					}, Throws.Nothing.After(2000)
				); // Should complete within 2 seconds.
			}

			#endregion
		}

		// TODO: WIP
		public class Fill_Null_Elements_And_Resize
		{
			[Test]
			public void Fill_Null_Elements_And_Resize_Expands_Target_Array()
			{
				// Arrange
				var source = new[] { new Test_Object(), new Test_Object(), null, new Test_Object() };
				var target = new Test_Object[2];

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Resize(source, ref target);

				// Assert
				NUnit.Framework.Assert.That(target.Length, Is.EqualTo(5), "Target array should be resized.");
				NUnit.Framework.Assert.That(target[0], Is.EqualTo(source[0]), "Element should be copied to target.");
				NUnit.Framework.Assert.That(target[1], Is.EqualTo(source[1]), "Element should be copied to target.");

				NUnit.Framework.Assert.That(
					target[2], Is.Null, "Null from source should create null in resized array."
				);

				NUnit.Framework.Assert.That(
					target[4], Is.EqualTo(source[3]), "Last element should be appended to resized array."
				);
			}

			[Test]
			public void Fill_Null_Elements_And_Resize_Handles_Full_Target()
			{
				// Arrange
				var source = new[] { new Test_Object(), new Test_Object() };
				var target = new[] { new Test_Object(), new Test_Object() };

				// Act
				Core.Array.Merging.Fill_Null_Elements_And_Resize(source, ref target);

				// Assert
				NUnit.Framework.Assert.That(
					target.Length, Is.EqualTo(4), "Target should grow to accommodate source elements."
				);

				NUnit.Framework.Assert.That(target.Skip(2), Is.EqualTo(source), "Source elements should be appended.");
			}
		}

		// TODO: WIP
		public class Append_Only
		{
			[Test]
			public void Append_Adds_Source_To_End_Of_Target()
			{
				// Arrange
				var source = new[] { new Test_Object(), null, new Test_Object() };
				var target = new[] { new Test_Object(), new Test_Object() };

				// Act
				Core.Array.Merging.Append_Only(source, ref target, Null_Element_Handling.Include);

				// Assert
				NUnit.Framework.Assert.That(target.Length, Is.EqualTo(5), "Target should include all source elements.");

				NUnit.Framework.Assert.That(
					target[2], Is.EqualTo(source[0]), "First source element should be appended."
				);

				NUnit.Framework.Assert.That(target[3], Is.Null, "Null element from source should be included.");

				NUnit.Framework.Assert.That(
					target[4], Is.EqualTo(source[2]), "Final source element should be appended."
				);
			}

			[Test]
			public void Append_Skips_Null_Elements_When_Flag_Is_Disabled()
			{
				// Arrange
				var source = new[] { new Test_Object(), null, new Test_Object() };
				var target = new[] { new Test_Object(), new Test_Object() };

				// Act
				Core.Array.Merging.Append_Only(source, ref target);

				// Assert
				NUnit.Framework.Assert.That(
					target.Length, Is.EqualTo(4), "Null elements from source should be excluded."
				);

				NUnit.Framework.Assert.That(
					target.Skip(2), Is.EquivalentTo(new[] { source[0], source[2] }),
					"Only non-null elements should be appended."
				);
			}
		}

		// TODO: WIP
		public class Prepend_Only
		{
			[Test]
			public void Prepend_Adds_Source_To_Beginning_Of_Target()
			{
				// Arrange
				var source = new[] { new Test_Object(), null, new Test_Object() };
				var target = new[] { new Test_Object(), new Test_Object() };

				// Act
				Core.Array.Merging.Prepend_Only(source, ref target, Null_Element_Handling.Include);

				// Assert
				NUnit.Framework.Assert.That(
					target.Length, Is.EqualTo(5), "Target should include all source elements at the start."
				);

				NUnit.Framework.Assert.That(
					target[0], Is.EqualTo(source[0]), "First source element should be prepended."
				);

				NUnit.Framework.Assert.That(target[2], Is.Null, "Existing target elements should shift.");
			}

			[Test]
			public void Prepend_Skips_Null_Elements_When_Flag_Is_Disabled()
			{
				// Arrange
				var source = new[] { new Test_Object(), null };
				var target = new[] { new Test_Object(), new Test_Object() };

				// Act
				Core.Array.Merging.Prepend_Only(source, ref target);

				// Assert
				NUnit.Framework.Assert.That(
					target.Length, Is.EqualTo(3), "Null elements from source should be excluded."
				);

				NUnit.Framework.Assert.That(
					target[0], Is.EqualTo(source[0]), "First valid element of source should be prepended."
				);

				NUnit.Framework.Assert.That(
					target[1..], Is.EquivalentTo(new[] { source[0], target[1] }),
					"Existing elements should maintain order."
				);
			}
		}

		/// <summary>
		///     Represents an object used for testing purposes in the context of
		///     array operations and merging functionalities within the test cases.
		/// </summary>
		private class Test_Object
		{
			public int Id { get; set; }
		}
	}
}
