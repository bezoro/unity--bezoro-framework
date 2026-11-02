using Bezoro.Core.Helpers;

namespace Bezoro.Unity.Core.StateMachines.TickableBased;

/// <summary>
///     Links tickable states with a condition and destination state.
/// </summary>
public class TickTransition : ITransition
{
	private BaseTickCondition[] _conditions         = new BaseTickCondition[4];
	private ConditionsCriteria  _conditionsCriteria = ConditionsCriteria.AnyTrue;
	private ITickableState      _fromState = null!;
	private ITickableState      _toState = null!;
	private TickableStateMachineOwner _owner = null!;
	public void Init(
		ITickableState            fromState,
		ITickableState            toState,
		TickableStateMachineOwner owner
	)
	{
		_fromState = fromState;
		_toState   = toState;
		_owner     = owner;
	}

	public void BecomeRelevant()
	{
		AllConditionsBecomeRelevant();

		void AllConditionsBecomeRelevant()
		{
			for (var i = 0; i < _conditions.Length; i++)
			{
				if (_conditions[i] == null) continue;

				_conditions[i].BecomeRelevant();
			}
		}
	}

	public void CeaseRelevant()
	{
		AllConditionsCeaseRelevant();

		void AllConditionsCeaseRelevant()
		{
			for (var i = 0; i < _conditions.Length; i++)
			{
				if (_conditions[i] == null) continue;

				_conditions[i].CeaseRelevant();
			}
		}
	}

	public void Tick()
	{
		TickConditions();

		void TickConditions()
		{
			if (_conditionsCriteria is ConditionsCriteria.AnyTrue)
				TickAnyTrue();

			else if (_conditionsCriteria is ConditionsCriteria.AllTrue) TickAllTrue();

			void TickAnyTrue()
			{
				for (var i = 0; i < _conditions.Length; i++)
				{
					if (_conditions[i] == null) continue;

					if (_conditions[i].Tick())
						_fromState.OnTransitionRaised(
							_toState
						);
				}
			}

			void TickAllTrue()
			{
				for (var i = 0; i < _conditions.Length; i++)
				{
					if (_conditions[i] == null) continue;

					if (!_conditions[i].Tick()) return;
				}

				_fromState.OnTransitionRaised(
					_toState
				);
			}
		}
	}

	public void WithCondition<T>(bool inverted = false)
		where T : BaseTickCondition, new()
	{
		var newCondition = new T();
		newCondition.Init(
			_owner,
			inverted
		);

		ArrayHelper.Add(
			ref _conditions,
			newCondition
		);
	}

	public void WithConditions<T0, T1>(
		ConditionsCriteria conditionsCriteria = ConditionsCriteria.AnyTrue,
		bool               inverted0          = false,
		bool               inverted1          = false
	)
		where T0 : BaseTickCondition, new()
		where T1 : BaseTickCondition, new()
	{
		var newCondition0 = new T0();
		newCondition0.Init(
			_owner,
			inverted0
		);

		ArrayHelper.Add(
			ref _conditions,
			newCondition0
		);

		var newCondition1 = new T1();
		newCondition1.Init(
			_owner,
			inverted1
		);

		ArrayHelper.Add(
			ref _conditions,
			newCondition1
		);

		_conditionsCriteria = conditionsCriteria;
	}

	public void WithConditions<T0, T1, T2>(
		ConditionsCriteria conditionsCriteria = ConditionsCriteria.AnyTrue,
		bool               inverted0          = false,
		bool               inverted1          = false,
		bool               inverted2          = false
	)
		where T0 : BaseTickCondition, new()
		where T1 : BaseTickCondition, new()
		where T2 : BaseTickCondition, new()
	{
		var newCondition0 = new T0();
		newCondition0.Init(
			_owner,
			inverted0
		);

		ArrayHelper.Add(
			ref _conditions,
			newCondition0
		);

		var newCondition1 = new T1();
		newCondition1.Init(
			_owner,
			inverted1
		);

		ArrayHelper.Add(
			ref _conditions,
			newCondition1
		);

		var newCondition2 = new T2();
		newCondition2.Init(
			_owner,
			inverted2
		);

		ArrayHelper.Add(
			ref _conditions,
			newCondition2
		);

		_conditionsCriteria = conditionsCriteria;
	}
}
