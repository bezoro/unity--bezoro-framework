using UnityEngine;

namespace Bezoro.Unity.Core.StateMachines;

[Tooltip(
	"What to do when Disabling the State Machine.\n" +
	"<b>Restart</b> = Restart as if it just started for the first time.\n " +
	"<b>Continue</b> = Continue from where it was."
)]
/// <summary>
/// Lists the supported Disable Method values.
/// </summary>
public enum DisableMethod
{
	None = -1,
	Restart,
	Continue
}
