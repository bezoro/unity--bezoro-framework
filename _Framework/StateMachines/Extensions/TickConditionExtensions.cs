namespace Bezoro.StateMachines
{
	public static class TickConditionExtensions
	{
		public static bool Tick_Ext<T>(this T condition)
		where T : ITickCondition =>
			condition.Inverted ? !condition.OnTick() : condition.OnTick();
	}
}