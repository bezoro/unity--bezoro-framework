using UnityEditor;
using Bezoro.Core.Extensions;

namespace Bezoro.StateMachines.Editor
{
	public static class CreateNewEventStateMachineScript
	{
		[MenuItem("Assets/Create/" + StateMachineEditorUtils.EventMenuName + "/New C# Script")]
		private static void CreateScript()
		{
			var guids = AssetDatabase.FindAssets(StateMachineEditorUtils
													 .EventStateMachineScriptTemplateName);

			if (guids.IsNullOrEmpty())
				return;

			var path = AssetDatabase.GUIDToAssetPath(guids[0]);
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "newEventStateMachine.cs");
		}
	}
}