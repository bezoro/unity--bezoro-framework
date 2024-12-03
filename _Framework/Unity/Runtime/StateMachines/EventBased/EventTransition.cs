using Bezoro.Framework.Core;

namespace Bezoro.Framework.Unity.StateMachines.EventBased
{
	public sealed class EventTransition : ITransition
	{
		private BaseEventCondition[] _conditions = new BaseEventCondition[4];
		private IEventState          _fromState;

		private EventStateMachineOwner _owner;
		private IEventState            _toState;

		public void Init( IEventState fromState, IEventState toState, EventStateMachineOwner owner )
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
				for ( var i = 0 ; i < _conditions.Length ; i++ )
				{
					if ( _conditions[ i ] == null )
					{
						continue;
					}

					_conditions[ i ].BecomeRelevant();
				}
			}
		}

		public void OnConditionRaised()
		{
			_fromState.OnTransitionRaised( _toState );
		}

		public void CeaseRelevant()
		{
			AllConditionsCeaseRelevant();

			void AllConditionsCeaseRelevant()
			{
				for ( var i = 0 ; i < _conditions.Length ; i++ )
				{
					if ( _conditions[ i ] == null )
					{
						continue;
					}

					_conditions[ i ].CeaseRelevant();
				}
			}
		}

		public void WithCondition<T>()
		where T : BaseEventCondition, new()
		{
			var newCondition = new T();
			newCondition.Init(
				_owner,
				this );

			Array_Helpers.Add(
				ref _conditions,
				newCondition );
		}

		public void WithConditions<T0, T1>()
		where T0 : BaseEventCondition, new()
		where T1 : BaseEventCondition, new()
		{
			var newCondition0 = new T0();
			newCondition0.Init(
				_owner,
				this );

			Array_Helpers.Add(
				ref _conditions,
				newCondition0 );

			var newCondition1 = new T1();
			newCondition1.Init(
				_owner,
				this );

			Array_Helpers.Add(
				ref _conditions,
				newCondition1 );
		}

		public void WithConditions<T0, T1, T2>()
		where T0 : BaseEventCondition, new()
		where T1 : BaseEventCondition, new()
		where T2 : BaseEventCondition, new()
		{
			var newCondition0 = new T0();
			newCondition0.Init(
				_owner,
				this );

			Array_Helpers.Add(
				ref _conditions,
				newCondition0 );

			var newCondition1 = new T1();
			newCondition1.Init(
				_owner,
				this );

			Array_Helpers.Add(
				ref _conditions,
				newCondition1 );

			var newCondition2 = new T2();
			newCondition2.Init(
				_owner,
				this );

			Array_Helpers.Add(
				ref _conditions,
				newCondition2 );
		}
	}
}
