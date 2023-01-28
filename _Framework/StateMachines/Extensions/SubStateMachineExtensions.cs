namespace Bezoro.StateMachines
{
	public static class SubStateMachineExtensions
	{
		public static void Enter_Ext<T>(this T subStateMachine)
		where T : ISubStateMachine
		{
			subStateMachine.AllTransitionsBecomeRelevant();
			subStateMachine.Start_Ext();
		}

		public static void Exit_Ext<T>(this T subStateMachine)
		where T : ISubStateMachine
		{
			subStateMachine.CurrentState.Exit();
			subStateMachine.AllTransitionsCeaseRelevant();
		}
	}
}