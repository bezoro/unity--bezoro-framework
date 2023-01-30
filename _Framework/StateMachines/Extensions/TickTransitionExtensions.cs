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

		public static T0 CreateCondition<T0, T1>(this T1 transition, bool inverted)
		where T0 : class, ITickCondition, new()
		where T1 : class, ITickTransition
		{
			var newCondition = new T0();
			newCondition.Init(transition.StateMachine, inverted);
			return newCondition;
		}
	}
}