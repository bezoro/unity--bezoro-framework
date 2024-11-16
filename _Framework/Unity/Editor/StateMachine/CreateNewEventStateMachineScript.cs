using Bezoro.Framework.Unity.Utilities;
using UnityEditor;

namespace Bezoro.Framework.Unity.Editor.StateMachine
{
	internal static class CreateNewEventStateMachineScript
	{
		[MenuItem(
			"Assets/Create/" + StateMachineEditorUtils.EventMenuName + "/New C# Script",
			false,
			0
		)]
		private static void CreateScript()
		{
			var guids = AssetDatabase.FindAssets(
				StateMachineEditorUtils
					.EventStateMachineScriptTemplateName
			);

			if ( guids.IsNullOrEmpty() )
			{
				return;
			}

			var templatePath = AssetDatabase.GUIDToAssetPath( guids[0] );
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
				templatePath,
				"newEventStateMachine.cs"
			);
		}
	}
}
