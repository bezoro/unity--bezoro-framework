using System;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Tools
{
	public class PrefabVariantRetargetTool : EditorWindow
	{
		public GameObject variant = null!;
		public GameObject target = null!;
		private string _error = string.Empty;

		[MenuItem("Bezoro/Tools/Prefab Variant Retargeting")]
		public static void Show_Window() => GetWindow<PrefabVariantRetargetTool>("Prefab Retargeting");

		private void OnGUI()
		{
			variant = (GameObject)EditorGUILayout.ObjectField("Variant", variant, typeof(GameObject), false);
			target = (GameObject)EditorGUILayout.ObjectField("Target Prefab", target, typeof(GameObject), false);
			bool canRetarget = variant && target && PrefabUtility.GetPrefabAssetType(variant) == PrefabAssetType.Variant &&
				PrefabUtility.GetPrefabAssetType(target) == PrefabAssetType.Regular &&
				PrefabUtility.GetCorrespondingObjectFromSource(variant) != target;
			using (new EditorGUI.DisabledScope(!canRetarget))
			{
				if (GUILayout.Button("Retarget Variant"))
				{
					try
					{
						variant = PrefabVariantWriter.Save(variant, target, AssetDatabase.GetAssetPath(variant), true, true);
						_error = string.Empty;
					}
					catch (Exception ex) { _error = ex.Message; }
				}
			}
			if (_error.Length != 0) EditorGUILayout.HelpBox(_error, MessageType.Error);
		}
	}
}
