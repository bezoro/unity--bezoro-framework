namespace Bezoro.StateMachines
{
	public static class TransitionExtensions
	{
		public static T0 CreateCondition<T0, T1>(this T1 transition)
		where T0 : class, ICondition, new()
		where T1 : class, ITransition
		{
			var newCondition = new T0();
			newCondition.Init(transition, transition.StateMachine);
			return newCondition;
		}

		public static void AllConditionsBecomeRelevant<T>(this T transition)
		where T : ITransition
		{
			var conditions = transition.Conditions;
			for (var i = 0; i < conditions.Length; i++)
			{
				if (conditions[i] == null)
					continue;

				conditions[i].BecomeRelevant();
			}
		}

		public static void AllConditionsCeaseRelevant<T>(this T transition)
		where T : ITransition
		{
			var conditions = transition.Conditions;
			for (var i = 0; i < conditions.Length; i++)
			{
				if (conditions[i] == null)
					continue;

				conditions[i].CeaseRelevant();
			}
		}
	}
}