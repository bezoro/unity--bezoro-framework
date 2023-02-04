namespace Bezoro.StateMachines
{
	public interface IHasTransitions<T>
	where T : IState
	{
		void OnTransitionRaised(T toState);
	}
}