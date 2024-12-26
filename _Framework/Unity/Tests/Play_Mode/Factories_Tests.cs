using System;
using System.Collections;
using System.Threading.Tasks;
using Bezoro.Framework.Core;
using Bezoro.Framework.Core.Tests.Edit_Mode.Eitor.Utilities;
using Moq;
using NUnit.Framework;
using UnityEngine;
using Assert = NUnit.Framework.Assert;

namespace Bezoro.Framework.Unity.Tests.PlayMode
{
	[TestFixture]
	public class Factories_Tests
	{
		private static Mock<I_Logger> _Logger_Mock;

		public class GameObject
		{
			[SetUp]
			public void SetUp()
			{
				Application.runInBackground = true;
				_Logger_Mock                = new();
			}

			#region Test Data Model

			public class Create_Async_Test_Parameter_Data
			{
				public string         ObjectName               { get; set; }
				public Transform_Data TransformData            { get; set; }
				public Func<bool>     EnableCondition          { get; set; }
				public Type[]         ComponentTypes           { get; set; }
				public Type[]         ExpectedComponents       { get; set; }
				public bool           ExpectedActiveState      { get; set; }
				public string         ExpectedExceptionMessage { get; set; }
				public int            ConcurrentCalls          { get; set; }
			}

			#endregion

			#region 1. Parameter Validation

			//

			#endregion

			#region 2. Core Functionality

			[TestCaseSource(nameof(GameObject.Core_Functionality_Test_Cases))]
			public async Task SHOULD_Create_GameObject_As_Expected(Create_Async_Test_Parameter_Data testData)
			{
				// Act
				var game_object = await Factories.GameObject.Create_Async(
					testData.ObjectName,
					testData.TransformData,
					testData.EnableCondition,
					_Logger_Mock.Object,
					testData.ComponentTypes
				);

				// Assert
				Assert.IsNotNull(game_object, "GameObject should not be null.");

				Assert.AreEqual(
					testData.ExpectedActiveState, game_object.activeSelf, "GameObject active state mismatch.");

				foreach ( var type in testData.ExpectedComponents )
				{
					Assert.IsNotNull(
						game_object.GetComponent(type), $"Expected component '{type.Name}' was not added.");
				}
			}

			private static IEnumerable Core_Functionality_Test_Cases()
			{
				yield return Test_Utils.Create_Test_Case(
					"SHOULD_Create_Disabled_Object_With_Specified_Components",
					new Create_Async_Test_Parameter_Data
					{
						ObjectName          = "TestObject",
						TransformData       = default,
						EnableCondition     = null,
						ComponentTypes      = new[] { typeof(Rigidbody) },
						ExpectedComponents  = new[] { typeof(Rigidbody) },
						ExpectedActiveState = false
					});

				yield return Test_Utils.Create_Test_Case(
					"SHOULD_Activate_Object_When_EnableCondition_Is_True",
					new Create_Async_Test_Parameter_Data
					{
						ObjectName          = "TestObject",
						TransformData       = default,
						EnableCondition     = () => true,
						ComponentTypes      = new Type[] { },
						ExpectedComponents  = new Type[] { },
						ExpectedActiveState = true
					});

				yield return Test_Utils.Create_Test_Case(
					"SHOULD_Add_Multiple_Specified_Components_To_Object",
					new Create_Async_Test_Parameter_Data
					{
						ObjectName          = "TestObjectWithComponents",
						TransformData       = default,
						EnableCondition     = null,
						ComponentTypes      = new[] { typeof(Rigidbody), typeof(BoxCollider) },
						ExpectedComponents  = new[] { typeof(Rigidbody), typeof(BoxCollider) },
						ExpectedActiveState = true
					});

				yield return Test_Utils.Create_Test_Case(
					"SHOULD_Enable_Added_Behaviours_Correctly",
					new Create_Async_Test_Parameter_Data
					{
						ObjectName          = "TestObjectWithBehaviours",
						TransformData       = default,
						EnableCondition     = () => true,
						ComponentTypes      = new[] { typeof(Rigidbody), typeof(BoxCollider) },
						ExpectedComponents  = new[] { typeof(Rigidbody), typeof(BoxCollider) },
						ExpectedActiveState = true
					});
			}

			#endregion

			#region 3. Edge Cases

			[TestCaseSource(nameof(GameObject.EdgeCaseTestCases))]
			public async Task SHOULD_Handle_Edge_Cases_Gracefully(Create_Async_Test_Parameter_Data testData)
			{
				// Act
				var game_object = await Factories.GameObject.Create_Async(
					testData.ObjectName,
					testData.TransformData,
					testData.EnableCondition,
					_Logger_Mock.Object,
					testData.ComponentTypes);

				// Assert
				Assert.IsNotNull(game_object, "GameObject should not be null in edge case.");

				Assert.AreEqual(
					testData.ExpectedActiveState, game_object.activeSelf,
					"GameObject active state mismatch in edge case.");
			}

			private static IEnumerable EdgeCaseTestCases()
			{
				yield return Test_Utils.Create_Test_Case(
					"SHOULD_Handle_Long_ObjectName_And_Valid_Components",
					new Create_Async_Test_Parameter_Data
					{
						ObjectName          = new( 'A', 5000 ), // Large object name
						TransformData       = default,
						EnableCondition     = null,
						ComponentTypes      = new[] { typeof(BoxCollider), typeof(Collider) },
						ExpectedComponents  = new[] { typeof(BoxCollider), typeof(Collider) },
						ExpectedActiveState = false
					});

				yield return Test_Utils.Create_Test_Case(
					"SHOULD_Not_Activate_Object_When_EnableCondition_Is_False",
					new Create_Async_Test_Parameter_Data
					{
						ObjectName          = "TestObject",
						TransformData       = default,
						EnableCondition     = () => false,
						ComponentTypes      = new Type[] { },
						ExpectedComponents  = new Type[] { },
						ExpectedActiveState = false
					});
			}

			#endregion

			#region 4. Thread Safety

			//

			#endregion
		}
	}
}
