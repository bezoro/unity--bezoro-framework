using Bezoro.Framework.Unity.Utilities.Extensions;
using UnityEditor;

namespace Bezoro.Framework.Unity.Editor.StateMachine
{
	internal static class CreateNewKCCStateMachineScript
	{
		[MenuItem(
			"Assets/Create/" + StateMachineEditorUtils.KCCMenuName + "/New C# Script",
			false,
			0
		)]
		private static void CreateScript()
		{
			var guids = AssetDatabase.FindAssets(
				StateMachineEditorUtils
					.KCCStateMachineScriptTemplateName
			);

			if (guids.IsNullOrEmpty())
			{
				return;
			}

			var templatePath = AssetDatabase.GUIDToAssetPath(guids[0]);
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
				templatePath,
				"newKCCStateMachine.cs"
			);
		}
	}
}
