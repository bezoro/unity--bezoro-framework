using System;
using System.Collections.Generic;
using System.Reflection;
using Bezoro.Unity.Core.Tickable;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies tickable registration, de-duplication, removal, and phase dispatch order.
	/// </summary>
	[TestFixture]
	public sealed class TickableManagerTests
	{
		private GameObject      _gameObject;
		private TickableManager _manager;

		[SetUp]
		public void SetUp()
		{
			_gameObject = new("Tickable Manager");
			_manager    = _gameObject.AddComponent<TickableManager>();
		}

		[TearDown]
		public void TearDown()
		{
			if (_gameObject != null)
				Object.DestroyImmediate(_gameObject);
		}

		[Test]
		public void AddUnique_WhenTickableIsNull_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _manager.AddUnique(null));
		}

		[Test]
		public void AddUnique_WhenTickableRegisteredTwice_DispatchesOncePerPhase()
		{
			var tickable = new AllPhaseTickable();

			_manager.AddUnique(tickable);
			_manager.AddUnique(tickable);

			InvokeGroup("ProcessEarlyTickablesGroup");
			InvokeGroup("ProcessRegularTickablesGroup");
			InvokeGroup("ProcessLateTickablesGroup");
			InvokeGroup("ProcessFixedTickablesGroup");

			CollectionAssert.AreEqual(
				new[]
				{
					"PreEarly", "Early", "PostEarly",
					"PreRegular", "Regular", "PostRegular",
					"PreLate", "Late", "PostLate",
					"PreFixed", "Fixed", "PostFixed"
				},
				tickable.Calls
			);
		}

		[Test]
		public void Remove_WhenTickableIsNull_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _manager.Remove(null));
		}

		[Test]
		public void Remove_WhenTickableWasRegistered_PreventsFurtherDispatch()
		{
			var tickable = new AllPhaseTickable();
			_manager.AddUnique(tickable);

			InvokeGroup("ProcessRegularTickablesGroup");
			_manager.Remove(tickable);
			InvokeGroup("ProcessRegularTickablesGroup");

			CollectionAssert.AreEqual(
				new[] { "PreRegular", "Regular", "PostRegular" },
				tickable.Calls
			);
		}

		private void InvokeGroup(string methodName)
		{
			typeof(TickableManager)
				.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
				.Invoke(_manager, null);
		}

		private sealed class AllPhaseTickable :
			IPreEarlyTickable,
			IEarlyTickable,
			IPostEarlyTickable,
			IPreRegularTickable,
			IRegularTickable,
			IPostRegularTickable,
			IPreLateTickable,
			ILateTickable,
			IPostLateTickable,
			IPreFixedTickable,
			IFixedTickable,
			IPostFixedTickable
		{
			public List<string> Calls { get; } = new();

			public void EarlyTick() => Calls.Add("Early");

			public void FixedTick() => Calls.Add("Fixed");

			public void LateTick() => Calls.Add("Late");

			public void PostEarlyTick() => Calls.Add("PostEarly");

			public void PostFixedTick() => Calls.Add("PostFixed");

			public void PostLateTick() => Calls.Add("PostLate");

			public void PostRegularTick() => Calls.Add("PostRegular");

			public void PreEarlyTick() => Calls.Add("PreEarly");

			public void PreFixedTick() => Calls.Add("PreFixed");

			public void PreLateTick() => Calls.Add("PreLate");

			public void PreRegularTick() => Calls.Add("PreRegular");

			public void RegularTick() => Calls.Add("Regular");
		}
	}
}
