namespace Bezoro.Framework.Unity.StateMachines
{
	public interface IHasTransitions<T0, T1>
	where T0 : ITransition
	where T1 : IState
	{
		public T0[] Transitions { get; }

		public void OnTransitionRaised( T1 toState );
	}
}
