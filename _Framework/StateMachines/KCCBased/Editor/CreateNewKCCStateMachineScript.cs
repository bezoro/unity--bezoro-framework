using UnityEditor;
using Bezoro.Core.Extensions;

namespace Bezoro.StateMachines.Editor
{
	public static class CreateNewKCCStateMachineScript
	{
		[MenuItem("Assets/Create/" + StateMachineEditorUtils.KCCMenuName + "/New C# Script")]
		private static void CreateScript()
		{
			var guids = AssetDatabase.FindAssets(StateMachineEditorUtils
													 .KCCStateMachineScriptTemplateName);

			if (guids.IsNullOrEmpty())
				return;

			var path = AssetDatabase.GUIDToAssetPath(guids[0]);
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "newKCCStateMachine.cs");
		}
	}
}