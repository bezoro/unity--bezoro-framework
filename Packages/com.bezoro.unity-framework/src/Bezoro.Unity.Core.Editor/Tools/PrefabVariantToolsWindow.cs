using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Tools
{
	public class PrefabVariantToolsWindow : EditorWindow
	{
		public const string TITLE = "Prefab Variant Tools";
		public GameObject originalPrefab = null!;
		public GameObject basePrefab = null!;
		public string outputFolder = string.Empty;
		public string variantName = string.Empty;
		private string _error = string.Empty;

		[MenuItem("Bezoro/Tools/" + TITLE)]
		private static void OpenWindow() => GetWindow<PrefabVariantToolsWindow>(TITLE);

		public void ConvertOriginalToBaseVariant() => Save(true);
		public void CreatePrefabVariant() => Save(false);

		private void Save(bool inPlace)
		{
			try
			{
				string original = AssetDatabase.GetAssetPath(originalPrefab);
				string folder = string.IsNullOrWhiteSpace(outputFolder) ? Path.GetDirectoryName(original)! : outputFolder;
				string name = string.IsNullOrWhiteSpace(variantName) ? originalPrefab.name + "_Variant" : variantName;
				if (name != Path.GetFileName(name)) throw new ArgumentException("Enter a file name without a directory.");
				string path = inPlace ? original : Path.Combine(folder, name + ".prefab").Replace('\\', '/');
				var result = PrefabVariantWriter.Save(originalPrefab, basePrefab, path, inPlace, false);
				Selection.activeObject = result;
				EditorGUIUtility.PingObject(result);
				_error = string.Empty;
			}
			catch (Exception ex) { _error = ex.Message; }
		}

		private void OnGUI()
		{
			originalPrefab = (GameObject)EditorGUILayout.ObjectField("Original Prefab", originalPrefab, typeof(GameObject), false);
			basePrefab = (GameObject)EditorGUILayout.ObjectField("Base Prefab", basePrefab, typeof(GameObject), false);
			outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
			variantName = EditorGUILayout.TextField("Variant Name", variantName);
			using (new EditorGUI.DisabledScope(!originalPrefab || !basePrefab))
			{
				if (GUILayout.Button("Create Prefab Variant")) CreatePrefabVariant();
				if (GUILayout.Button("Convert to Base Variant")) ConvertOriginalToBaseVariant();
			}
			if (_error.Length != 0) EditorGUILayout.HelpBox(_error, MessageType.Error);
		}
	}
}
