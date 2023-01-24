namespace Bezoro.StateMachines
{
	public static class StateExtensions
	{
		public static bool IsSubStateMachine<T1>(this IState state, out T1 outStateMachine)
		where T1: class, IStateMachine
		{
			if (state is IStateMachine stateMachine)
			{
				outStateMachine = stateMachine as T1;
				return true;
			}

			outStateMachine = null;
			return false;
		}
	}
}