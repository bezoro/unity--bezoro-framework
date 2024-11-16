using Bezoro.Framework.Unity.States;

namespace Bezoro.Framework.Unity
{
	public interface IHasTransitions<T0, T1>
	where T0 : ITransition
	where T1 : IState
	{
		T0[] Transitions { get; }

		void OnTransitionRaised( T1 toState );
	}
}
