using Bezoro.Unity.Core.Common.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies synchronous Behaviour extension methods.
	/// </summary>
	[TestFixture]
	public sealed class BehaviourExtensionsEditModeTests
	{
		private TestBehaviour _behaviour;
		private GameObject    _gameObject;

		[SetUp]
		public void SetUp()
		{
			_gameObject = new("BehaviourExtensionsEditModeTestsObject");
			_behaviour  = _gameObject.AddComponent<TestBehaviour>();
		}

		[TearDown]
		public void TearDown()
		{
			if (_gameObject != null)
				Object.DestroyImmediate(_gameObject);
		}

		[Test]
		public void EnableAndDisableBehaviour_SetExpectedEnabledState()
		{
			_behaviour.DisableBehaviour();
			Assert.IsFalse(_behaviour.enabled);

			_behaviour.EnableBehaviour();
			Assert.IsTrue(_behaviour.enabled);
		}

		[Test]
		public void GetBehaviourState_ReturnsCurrentEnabledState()
		{
			_behaviour.enabled = false;

			Assert.IsFalse(_behaviour.GetBehaviourState());
		}

		[Test]
		public void SetBehaviourState_SetsExpectedEnabledState()
		{
			_behaviour.SetBehaviourState(false);

			Assert.IsFalse(_behaviour.enabled);
		}

		[Test]
		public void ToggleBehaviour_InvertsEnabledState()
		{
			_behaviour.enabled = true;

			_behaviour.ToggleBehaviour();

			Assert.IsFalse(_behaviour.enabled);
		}

		private sealed class TestBehaviour : MonoBehaviour { }
	}
}
