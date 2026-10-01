using System.Collections.Generic;
using System.Threading.Tasks;
using Bezoro.Unity.Core.Common.Extensions;
using NUnit.Framework;
using UnityEngine;
using Assert = NUnit.Framework.Assert;

namespace Bezoro.Unity.Core.Tests.PlayMode
{
	[TestFixture]
	/// <summary>
	/// Verifies behaviour extension methods against Unity component enabled states.
	/// </summary>
	public class BehaviourExtensionsTests
	{
		private List<TestBehaviour> _testBehavioursList;
		private GameObject          _testGameObject;
		private TestBehaviour       _testSingleBehaviour;

		#region Setup/Teardown Methods

		[SetUp]
		public void SetUp()
		{
			_testGameObject = new(
				"TestGameObject"
			);

			_testSingleBehaviour = _testGameObject.AddComponent<TestBehaviour>();

			_testBehavioursList = new()
			{
				_testGameObject.AddComponent<TestBehaviour>(),
				_testGameObject.AddComponent<TestBehaviour>(),
				_testGameObject.AddComponent<TestBehaviour>()
			};
		}

		#endregion

		#region Setup/Teardown Methods

		[TearDown]
		public async Task TearDown()
		{
			await Task.Yield(); // Ensures operations on Unity objects are on the main thread if needed

			if (_testGameObject != null)
				Object.DestroyImmediate(
					_testGameObject
				);
		}

		#endregion

		#region Helper Methods/Other Members

		// Renamed nested class
		/// <summary>
		///     Test component exposing the extension toggle through an instance method.
		/// </summary>
		public class TestBehaviour : MonoBehaviour
		{
			public void ToggleBehaviour() =>
				BehaviourExtensions.ToggleBehaviour(
					this
				);
		}

		#endregion

		#region Test Methods

		[TestCase(
			true
		)]
		[TestCase(
			false
		)]
		public void ToggleBehaviour_ShouldToggleState(bool initialState)
		{
			// Arrange
			_testSingleBehaviour.enabled = initialState;

			// Act
			_testSingleBehaviour.ToggleBehaviour();
			bool toggledState = _testSingleBehaviour.enabled;

			// Assert
			Assert.AreNotEqual(
				initialState,
				toggledState,
				"Behaviour state was not toggled correctly."
			);
		}

		[TestCase(
			true
		)]
		[TestCase(
			false
		)]
		public async Task ToggleBehaviours_ShouldToggleAllStates(bool initialState)
		{
			// Arrange
			foreach (var b in _testBehavioursList) b.enabled = initialState;

			// Act
			await _testBehavioursList.ToggleBehaviours();

			// Assert
			foreach (var t in _testBehavioursList)
			{
				Assert.AreNotEqual(
					initialState,
					t.enabled,
					"Behaviour state was not toggled correctly."
				);
			}
		}

		#endregion
	}
}
