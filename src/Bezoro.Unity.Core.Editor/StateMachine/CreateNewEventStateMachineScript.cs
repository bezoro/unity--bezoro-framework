using Bezoro.Core.Extensions;
using UnityEditor;

namespace Bezoro.Unity.Core.Editor.StateMachine
{
	/// <summary>
	///     Creates event-based state machine scripts from the project template.
	/// </summary>
	internal static class CreateNewEventStateMachineScript
	{
		[MenuItem(
			"Assets/Create/" + StateMachineEditorUtils.EVENT_MENU_NAME + "/New C# Script",
			false,
			0
		)]
		private static void CreateScript()
		{
			string[] guids = AssetDatabase.FindAssets(
				StateMachineEditorUtils
					.EVENT_STATE_MACHINE_SCRIPT_TEMPLATE_NAME
			);

			if (guids.IsNullOrEmpty()) return;

			string templatePath = AssetDatabase.GUIDToAssetPath(
				guids[0]
			);

			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
				templatePath,
				"newEventStateMachine.cs"
			);
		}
	}
}
