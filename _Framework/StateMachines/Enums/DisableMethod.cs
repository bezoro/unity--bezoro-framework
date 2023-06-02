using UnityEngine;

namespace Bezoro.Framework.StateMachines
{
	[Tooltip("What to do when Disabling the State Machine.\n" +
			 "<b>Restart</b> = Restart as if it just started for the first time.\n " +
			 "<b>Continue</b> = Continue from where it was.")]
	public enum DisableMethod
	{
		None = -1,
		Restart,
		Continue
	}
}