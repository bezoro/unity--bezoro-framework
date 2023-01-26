namespace Bezoro.StateMachines
{
	public static class TickTransitionExtensions
	{
		public static void TickConditions<T>(this T transition)
		where T : ITickTransition
		{
			var conditions = transition.Conditions;

			if (transition.ConditionsCriteria is ConditionsCriteria.AnyTrue)
				TickAnyTrue();

			else if (transition.ConditionsCriteria is ConditionsCriteria.AllTrue)
				TickAllTrue();

			void TickAnyTrue()
			{
				for (var i = 0; i < conditions.Length; i++)
				{
					if (conditions[i] == null)
						continue;

					if (conditions[i].Tick())
						transition.FromState.OnTransitionTrue(transition.ToState);
				}
			}

			void TickAllTrue()
			{
				for (var i = 0; i < conditions.Length; i++)
				{
					if (conditions[i] == null)
						continue;

					if (!conditions[i].Tick())
						return;
				}

				transition.FromState.OnTransitionTrue(transition.ToState);
			}
		}

		public static T1 CreateCondition<T0, T1>(this T0 transition, bool inverted)
		where T0 : class, ITickTransition
		where T1 : class, ITickCondition, new()
		{
			var newCondition = new T1();
			newCondition.Init(transition.StateMachine, inverted);
			return newCondition;
		}
	}

	public static class TransitionExtensions
	{
		public static T CreateCondition<T>(this ITransition transition)
		where T : class, ICondition, new()
		{
			var newCondition = new T();
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