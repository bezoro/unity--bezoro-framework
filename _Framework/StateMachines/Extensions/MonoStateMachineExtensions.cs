namespace Bezoro.StateMachines
{
	public static class MonoStateMachineExtensions
	{
		public static void Awake_Ext<T>(this T stateMachine)
		where T : IMonoStateMachine
		{
			stateMachine.RegisterStates();
			stateMachine.SetUpTransitions();
		}

		public static void Start_Ext<T>(this T stateMachine)
		where T : IStateMachine
		{
			var startState = stateMachine.ResolveStartState();
			stateMachine.SetCurrentState(startState);
			stateMachine.CurrentState.Enter();
		}

		public static void OnEnable_Ext<T>(this T stateMachine)
		where T : IMonoStateMachine
		{
			if (stateMachine.Restarted)
			{
				stateMachine.SetRestarted(false);
				stateMachine.Start_Ext();
			}
		}

		public static void OnDisable_Ext<T>(this T stateMachine)
		where T : IMonoStateMachine
		{
			if (stateMachine.DisableMethod is DisableMethod.Restart)
				stateMachine.SetRestarted(true);
		}
	}
}