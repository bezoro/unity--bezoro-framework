namespace Bezoro.StateMachines
{
	public interface ISimpleStateMachine
	{
		ISimpleState CurrentState { get; }
		public void SwitchCurrentState(ISimpleState stateToSwitchTo);
	}
}