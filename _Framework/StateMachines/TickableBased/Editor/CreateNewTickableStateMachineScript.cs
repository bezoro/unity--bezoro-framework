using UnityEditor;
using Bezoro.Core.Extensions;

namespace Bezoro.StateMachines.Editor
{
	public static class CreateNewTickableStateMachineScript
	{
		[MenuItem("Assets/Create/" + StateMachineEditorUtils.TickableMenuName + "/New C# Script")]
		private static void CreateScript()
		{
			var guids = AssetDatabase.FindAssets(StateMachineEditorUtils
													 .TickableStateMachineScriptTemplateName);

			if (guids.IsNullOrEmpty())
				return;

			var path = AssetDatabase.GUIDToAssetPath(guids[0]);
			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "newTickableStateMachine.cs");
		}
	}
}