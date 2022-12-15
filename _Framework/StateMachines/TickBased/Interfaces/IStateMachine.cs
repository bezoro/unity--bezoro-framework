namespace Bezoro.StateMachines
{
	public interface IStateMachine
	{
		IState CurrentState { get; }

		public void SwitchCurrentState(IState stateToSwitchTo);
	}
}