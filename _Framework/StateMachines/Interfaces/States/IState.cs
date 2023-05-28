namespace Bezoro.Framework.StateMachines
{
	public interface IState
	{
		void Enable();

		void Enter();

		void Exit();

		void Disable();

		void Destroy();
	}
}