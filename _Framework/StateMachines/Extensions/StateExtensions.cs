namespace Bezoro.StateMachines
{
	public static class StateExtensions
	{
		public static bool IsSubStateMachine(this IState state, out IStateMachine outStateMachine)
		{
			if (state is IStateMachine stateMachine)
			{
				outStateMachine = stateMachine;
				return true;
			}

			outStateMachine = null;
			return false;
		}
	}
}