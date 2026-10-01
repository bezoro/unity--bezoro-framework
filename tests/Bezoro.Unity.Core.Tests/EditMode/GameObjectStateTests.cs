using System;
using Bezoro.Unity.Core.AsyncLoading;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies GameObjectState and ComponentState capture and restore helpers.
	/// </summary>
	[TestFixture]
	public sealed class GameObjectStateTests
	{
		private TestBehaviour _behaviour;
		private GameObject    _gameObject;

		[SetUp]
		public void SetUp()
		{
			_gameObject = new("GameObjectStateTestsObject");
			_behaviour  = _gameObject.AddComponent<TestBehaviour>();
		}

		[TearDown]
		public void TearDown()
		{
			if (_gameObject != null)
				Object.DestroyImmediate(_gameObject);
		}

		[Test]
		public void ComponentState_CapturesTargetAndEnabledState()
		{
			_behaviour.enabled = false;

			var state = new ComponentState(_behaviour);

			Assert.AreSame(_behaviour, state.Target);
			Assert.IsFalse(state.WasInitiallyEnabled);
			Assert.AreSame(_behaviour, (Component)state);
		}

		[Test]
		public void ComponentState_Equals_WhenCapturedEnabledStateDiffers_ReturnsFalse()
		{
			_behaviour.enabled = true;
			var enabledState = new ComponentState(_behaviour);

			_behaviour.enabled = false;
			var disabledState = new ComponentState(_behaviour);

			Assert.AreNotEqual(enabledState, disabledState);
		}

		[Test]
		public void ComponentState_Equals_WhenTargetAndEnabledStateMatch_ReturnsTrue()
		{
			_behaviour.enabled = false;

			var first  = new ComponentState(_behaviour);
			var second = new ComponentState(_behaviour);

			Assert.AreEqual(first,               second);
			Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
		}

		[Test]
		public void ComponentState_Target_WhenUnityTargetWasDestroyed_ReturnsNull()
		{
			var temporary = new GameObject("DestroyedComponentStateTarget");
			var behaviour = temporary.AddComponent<TestBehaviour>();
			var state     = new ComponentState(behaviour);

			Object.DestroyImmediate(temporary);

			Assert.IsNull(state.Target);
			Assert.AreEqual(0, state.GetHashCode());
		}

		[Test]
		public void ComponentState_WhenTargetIsNull_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new ComponentState(null));
		}

		[Test]
		public void DeactivateAll_DisablesObjectAndComponents()
		{
			var state = new GameObjectState(_gameObject);

			state.DeactivateAll();

			Assert.IsFalse(_gameObject.activeSelf);
			Assert.IsFalse(_behaviour.enabled);
		}

		[Test]
		public void GameObjectState_CapturesTargetNameActiveStateAndComponents()
		{
			var state = new GameObjectState(_gameObject);

			Assert.AreSame(_gameObject, state.Target);
			Assert.AreEqual(_gameObject.name, state.Name);
			Assert.IsTrue(state.WasInitiallyActive);
			Assert.IsTrue(state.IsActiveSelf);
			Assert.GreaterOrEqual(state.ComponentStates.Length, 2);
			Assert.AreSame(_gameObject, (GameObject)state);
		}

		[Test]
		public void GameObjectState_Equals_WhenTargetAndCapturedStateMatch_ReturnsTrue()
		{
			var first  = new GameObjectState(_gameObject);
			var second = new GameObjectState(_gameObject);

			Assert.AreEqual(first,               second);
			Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
		}

		[Test]
		public void GameObjectState_Target_WhenUnityTargetWasDestroyed_ReturnsNullNameAndInactive()
		{
			var temporary = new GameObject("DestroyedGameObjectStateTarget");
			var state     = new GameObjectState(temporary);

			Object.DestroyImmediate(temporary);

			Assert.IsNull(state.Target);
			Assert.IsNull(state.Name);
			Assert.IsFalse(state.IsActiveSelf);
			Assert.AreEqual(0, state.GetHashCode());
		}

		[Test]
		public void SetActive_ChangesTargetActiveSelf()
		{
			var state = new GameObjectState(_gameObject);

			state.SetActive(false);

			Assert.IsFalse(_gameObject.activeSelf);
		}

		[Test]
		public void ToComponents_FiltersDestroyedAndNullStates()
		{
			var state = new ComponentState(_behaviour);

			var components = new[] { state, null }.ToComponents();

			CollectionAssert.AreEqual(new Component[] { _behaviour }, components);
		}

		[Test]
		public void ToComponents_WhenStatesArrayIsNull_ReturnsEmptyArray()
		{
			ComponentState[] states = null;

			var components = states.ToComponents();

			Assert.IsEmpty(components);
		}

		private sealed class TestBehaviour : MonoBehaviour { }
	}
}
