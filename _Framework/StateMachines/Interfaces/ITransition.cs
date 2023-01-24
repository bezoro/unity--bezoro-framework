namespace Bezoro.StateMachines
{
	public interface IKCCTransition : ITickTransition { }

	public interface ITickTransition : ITransition
	{
		public void Tick();
	}

	public interface IEventTransition : ITransition { }

	public interface ITransition
	{
		public void BecomeRelevant();

		public void CeaseRelevant();
	}
}