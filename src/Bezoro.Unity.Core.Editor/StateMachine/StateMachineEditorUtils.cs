namespace Bezoro.Unity.Core.Editor.StateMachine
{
	/// <summary>
	///     Draws shared editor UI for state machine transition data.
	/// </summary>
	public static class StateMachineEditorUtils
	{
		public const string EVENT_MENU_NAME = MENU_NAME + "/Event";

		public const string EVENT_STATE_MACHINE_SCRIPT_TEMPLATE_NAME =
			"EventStateMachineScriptTemplate";
		public const string KCC_MENU_NAME = MENU_NAME + "/KCC";

		public const string KCC_STATE_MACHINE_SCRIPT_TEMPLATE_NAME =
			"KCCStateMachineScriptTemplate";
		public const string MENU_NAME          = "Bezoro/StateMachines";
		public const string TICKABLE_MENU_NAME = MENU_NAME + "/Tickable";

		public const string TICKABLE_STATE_MACHINE_SCRIPT_TEMPLATE_NAME =
			"TickableStateMachineScriptTemplate";
	}
}
