using Bezoro.Unity.Core.ScriptableObjects;
using UnityEditor;

namespace Bezoro.Unity.Core.Editor.Inspectors
{
	/// <summary>
	/// Edits collection fields and applies scene callbacks only after explicit inspector changes.
	/// </summary>
	[CustomEditor(typeof(SceneCollection))]
	[CanEditMultipleObjects]
	public sealed class SceneCollectionInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			using (new EditorGUI.DisabledScope(true))
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

			DrawPropertiesExcluding(serializedObject, "m_Script", "sceneEntries", "useLoadingScreen", "loadScreenOverride", "loadMode");

			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneEntries"), true);
			var sceneEntriesChanged = serializedObject.hasModifiedProperties;
			ApplyCollectionChanges(sceneEntriesChanged, loadModeChanged: false);

			var useLoadingScreen = serializedObject.FindProperty("useLoadingScreen");
			EditorGUILayout.PropertyField(useLoadingScreen);
			if (useLoadingScreen.hasMultipleDifferentValues || useLoadingScreen.boolValue)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("loadScreenOverride"), true);

			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("loadMode"));
			var loadModeChanged = serializedObject.hasModifiedProperties;
			ApplyCollectionChanges(sceneEntriesChanged: false, loadModeChanged);
		}

		private void ApplyCollectionChanges(bool sceneEntriesChanged, bool loadModeChanged)
		{
			if (!sceneEntriesChanged && !loadModeChanged)
				return;

			Undo.RecordObjects(targets, "Edit Scene Collection");
			serializedObject.ApplyModifiedProperties();

			foreach (var inspectedTarget in targets)
			{
				var collection = (SceneCollection)inspectedTarget;
				collection.ApplyInspectorChanges(sceneEntriesChanged, loadModeChanged);
				EditorUtility.SetDirty(collection);
			}

			serializedObject.Update();
		}
	}
}
