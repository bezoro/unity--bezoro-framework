using UnityEngine;

namespace Bezoro.StateMachines
{
	[Tooltip("What to do when Disabling the State Machine./n" +
			 "Restart = Restart as if it just started for the first time./n" +
			 "Continue = Continue from where it was")]
	public enum DisableMethod
	{
		None = -1,
		Restart,
		Continue
	}
}