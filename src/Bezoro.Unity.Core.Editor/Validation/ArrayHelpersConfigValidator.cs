using System.Collections.Generic;
using Bezoro.Unity.Core.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Validation
{
	/// <summary>
	///     Checks runtime asset placement and thresholds without a third-party validator.
	/// </summary>
	public static class ArrayHelpersConfigValidator
	{
		public static bool IsAssetInEditorFolder(ArrayHelpersConfig targetObject) =>
			AssetDatabase.GetAssetPath(targetObject).Contains("/Editor/");

		public static bool IsAssetInResourcesFolder(ArrayHelpersConfig targetObject) =>
			AssetDatabase.GetAssetPath(targetObject).Contains("/Resources/");

		public static IEnumerable<string> GetValidationErrors(ArrayHelpersConfig targetObject)
		{
			if (!IsAssetInResourcesFolder(targetObject))
				yield return "This object must be placed in a Resources folder.";
			if (IsAssetInEditorFolder(targetObject))
				yield return "This object must not be placed in an Editor folder.";
			if (targetObject.ParallelThreshold < 1 || targetObject.ParallelThreshold > 1000)
				yield return "Parallel Threshold must be between 1 and 1000.";
		}

		[MenuItem("Bezoro/Validation/Validate Array Helpers Config")]
		public static void ValidateProject()
		{
			var errorCount = 0;
			foreach (var guid in AssetDatabase.FindAssets("t:ArrayHelpersConfig"))
			{
				var config = AssetDatabase.LoadAssetAtPath<ArrayHelpersConfig>(AssetDatabase.GUIDToAssetPath(guid));
				if (config == null) continue;
				foreach (var error in GetValidationErrors(config))
				{
					Debug.LogError(error, config);
					errorCount++;
				}
			}
			if (errorCount == 0) Debug.Log("Array Helpers Config validation passed.");
		}
	}
}
