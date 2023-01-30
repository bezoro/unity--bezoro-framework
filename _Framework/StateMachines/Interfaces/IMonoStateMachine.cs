namespace Bezoro.StateMachines
{
	public interface IMonoKCCStateMachine : IMonoTickableStateMachine, IKCCStateMachine { }

	public interface IMonoTickableStateMachine : IMonoStateMachine, ITickableStateMachine { }

	public interface IMonoEventStateMachine : IMonoStateMachine, IEventStateMachine { }

	public interface IMonoStateMachine : IStateMachine
	{
		bool Restarted { get; }

		void SetRestarted(bool value);

		public DisableMethod DisableMethod => DisableMethod.Continue;
	}
}