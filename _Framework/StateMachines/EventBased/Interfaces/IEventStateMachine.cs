namespace Bezoro.StateMachines
{
	public interface IEventStateMachine
	{
		IEventState CurrentState { get; }
		public void SwitchCurrentState(IEventState stateToSwitchTo);
	}
}