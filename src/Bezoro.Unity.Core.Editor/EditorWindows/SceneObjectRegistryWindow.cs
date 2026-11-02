using System.Collections.Generic;
using System.Reflection;
using Bezoro.Unity.Core.Scenes;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.EditorWindows
{
	/// <summary>
	///     Debug window that snapshots the private scene-object GUID registry so
	///     editor users can inspect which scene objects are currently addressable by
	///     GUID.
	/// </summary>
	public class SceneObjectRegistryWindow : EditorWindow
	{
		private const string MAP_FIELD    = "_GUID_TO_GAME_OBJECT_MAP";
		private const string MENU_PATH    = "Bezoro/Tools/Scene Object Registry Debugger";
		private const string WINDOW_TITLE = "Scene Object Registry Debugger";

		private static readonly FieldInfo? GuidToObjectMapField = typeof(SceneObjectRegistry).GetField(
			MAP_FIELD,
			BindingFlags.NonPublic | BindingFlags.Static
		);

		private Dictionary<string, GameObject> GuidToObjectMap => GetGuidToObjectMapSnapshot();

		private Vector2 _scrollPosition;

		private void OnInspectorUpdate() => Repaint();

		private void OnGUI()
		{
			var snapshot = GuidToObjectMap;
			EditorGUILayout.LabelField("Registered Object Count", snapshot.Count.ToString());
			EditorGUILayout.LabelField("GUID to Object Map", EditorStyles.boldLabel);
			if (GuidToObjectMapField == null)
			{
				EditorGUILayout.HelpBox("The scene object registry could not be inspected.", MessageType.Warning);
				return;
			}

			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
			foreach (var entry in snapshot)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.SelectableLabel(entry.Key, GUILayout.Height(EditorGUIUtility.singleLineHeight));
				using (new EditorGUI.DisabledScope(true))
					EditorGUILayout.ObjectField(entry.Value, typeof(GameObject), true);
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
		}

		private static Dictionary<string, GameObject> GetGuidToObjectMapSnapshot()
		{
			if (GuidToObjectMapField == null)
				return new();

			if (GuidToObjectMapField.GetValue(null) is not Dictionary<string, GameObject> registry)
				return new();

			return new(registry);
		}

		[MenuItem(MENU_PATH)]
		private static void OpenWindow() =>
			GetWindow<SceneObjectRegistryWindow>(
				WINDOW_TITLE
			);
	}
}
