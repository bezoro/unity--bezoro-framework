namespace Bezoro.StateMachines
{
	public static class KCCTransitionExtensions
	{
		public static T CreateCondition<T>(this IKCCTransition transition, bool inverted)
		where T : class, IKCCCondition, new()
		{
			var newCondition = new T();
			newCondition.Init(transition.StateMachine, inverted);
			return newCondition;
		}
	}
	
	public static class TickTransitionExtensions
	{
		public static void TickConditions(this ITickTransition transition)
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

		public static T CreateCondition<T>(this ITickTransition transition, bool inverted)
		where T : class, ITickCondition, new()
		{
			var newCondition = new T();
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

		public static void AllConditionsBecomeRelevant(this ITransition transition)
		{
			var conditions = transition.Conditions;
			for (var i = 0; i < conditions.Length; i++)
			{
				if (conditions[i] == null)
					continue;

				conditions[i].BecomeRelevant();
			}
		}

		public static void AllConditionsCeaseRelevant(this ITransition transition)
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