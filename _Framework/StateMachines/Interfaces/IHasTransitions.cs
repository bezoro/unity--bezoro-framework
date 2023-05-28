namespace Bezoro.Framework.StateMachines
{
	public interface IHasTransitions<T0, T1>
	where T0 : ITransition
	where T1 : IState
	{
		T0[] Transitions { get; }

		void OnTransitionRaised(T1 toState);
	}
}