// Behaviour_Extensions_Tests.cs

using System.Threading.Tasks;
using Bezoro.Framework.Unity.Utilities;
using NUnit.Framework;
using UnityEngine;
using Array_Helpers = Bezoro.Framework.Core.Array_Helpers;

namespace Bezoro.Framework.Unity.Tests.PlayMode
{
	[TestFixture]
	public class Behaviour_Extensions_Tests
	{
		private Test_Behaviour[] _test_behaviours = { };
		private GameObject       _test_game_object;
		private Test_Behaviour   _test_behaviour;

		[SetUp]
		public void SetUp()
		{
			_test_game_object = new();
			_test_behaviour   = _test_game_object.AddComponent<Test_Behaviour>();

			Array_Helpers.Add(ref _test_behaviours, _test_game_object.AddComponent<Test_Behaviour>());
			Array_Helpers.Add(ref _test_behaviours, _test_game_object.AddComponent<Test_Behaviour>());
			Array_Helpers.Add(ref _test_behaviours, _test_game_object.AddComponent<Test_Behaviour>());
		}

		// [TearDown]
		public async Task TearDown()
		{
			await Task.Yield();

			Object.DestroyImmediate(_test_game_object);
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task Toggle_Behaviours__Should_Toggle__All_Behaviour_States(bool initial_state)
		{
			// Arrange
			foreach ( var b in _test_behaviours )
				b.enabled = initial_state;

			// Act
			await _test_behaviours.Toggle_Behaviours();

			// Assert
			foreach ( var t in _test_behaviours )
				Assert.IsTrue(t.enabled != initial_state);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void Toggle_Behaviour__Should_Toggle__Behaviour_State(bool initial_state)
		{
			// Arrange
			_test_behaviour.enabled = initial_state;

			// Act
			_test_behaviour.Toggle_Behaviour();
			var toggled_state = _test_behaviour.enabled;

			// Assert
			Assert.IsTrue(initial_state != toggled_state);
		}

		public class Test_Behaviour : MonoBehaviour { }
	}
}
