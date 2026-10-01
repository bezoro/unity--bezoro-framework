using Bezoro.Core.Helpers;

namespace Bezoro.Unity.Core.StateMachines.EventBased;

/// <summary>
///     Links event states with a condition and destination state.
/// </summary>
public sealed class EventTransition : ITransition
{
	private BaseEventCondition[] _conditions = new BaseEventCondition[4];

	private EventStateMachineOwner _owner = null!;
	private IEventState            _fromState = null!;
	private IEventState            _toState = null!;
	public void Init(IEventState fromState, IEventState toState, EventStateMachineOwner owner)
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

	public void OnConditionRaised() =>
		_fromState.OnTransitionRaised(
			_toState
		);

	public void WithCondition<T>()
		where T : BaseEventCondition, new()
	{
		var newCondition = new T();
		newCondition.Init(
			_owner,
			this
		);

		ArrayHelper.Add(
			ref _conditions,
			newCondition
		);
	}

	public void WithConditions<T0, T1>()
		where T0 : BaseEventCondition, new()
		where T1 : BaseEventCondition, new()
	{
		var condition1 = new T0();
		condition1.Init(
			_owner,
			this
		);

		ArrayHelper.Add(
			ref _conditions,
			condition1
		);

		var condition2 = new T1();
		condition2.Init(
			_owner,
			this
		);

		ArrayHelper.Add(
			ref _conditions,
			condition2
		);
	}

	public void WithConditions<T0, T1, T2>()
		where T0 : BaseEventCondition, new()
		where T1 : BaseEventCondition, new()
		where T2 : BaseEventCondition, new()
	{
		var condition1 = new T0();
		condition1.Init(
			_owner,
			this
		);

		ArrayHelper.Add(
			ref _conditions,
			condition1
		);

		var condition2 = new T1();
		condition2.Init(
			_owner,
			this
		);

		ArrayHelper.Add(
			ref _conditions,
			condition2
		);

		var condition3 = new T2();
		condition3.Init(
			_owner,
			this
		);

		ArrayHelper.Add(
			ref _conditions,
			condition3
		);
	}
}
