using Bezoro.Framework.Unity.Utilities;
using UnityEditor;

namespace Bezoro.Framework.Unity.Editor.StateMachine
{
	internal static class CreateNewTickableStateMachineScript
	{
		[MenuItem(
			"Assets/Create/" + StateMachineEditorUtils.TickableMenuName + "/New C# Script",
			false,
			0
		)]
		private static void CreateScript()
		{
			var guids = AssetDatabase.FindAssets(
				StateMachineEditorUtils
					.TickableStateMachineScriptTemplateName
			);

			if ( guids.Is_Null_Or_Empty() )
			{
				return;
			}

			var templatePath = AssetDatabase.GUIDToAssetPath( guids[ 0 ] );
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
				templatePath,
				"newTickableStateMachine.cs"
			);
		}
	}
}
