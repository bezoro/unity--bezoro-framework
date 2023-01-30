using UnityEngine;

namespace Bezoro.StateMachines
{
	public interface IKCCCondition : ITickCondition { }

	public interface ITickCondition : ICondition
	{
		public void Init(MonoBehaviour stateMachine, bool inverted = false);

		public bool Inverted { get; }

		public bool Tick();

		public bool OnTick();
	}

	public interface IEventCondition : ICondition
	{
		public void Init(IEventTransition transition, MonoBehaviour stateMachine);
	}

	public interface ICondition
	{
		public void Init(ITransition transition, MonoBehaviour stateMachine);

		public MonoBehaviour StateMachine { get; }

		public void BecomeRelevant();

		public void CeaseRelevant();
	}
}