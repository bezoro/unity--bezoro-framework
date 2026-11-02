using System;
using System.Linq;
using Bezoro.Unity.Core.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.EditorWindows
{
	/// <summary>
	///     Displays and edits the singleton ArrayHelpersConfig asset.
	/// </summary>
	public sealed class ArrayHelpersConfigWindow : EditorWindow
	{
		#region Serialized Fields

		public ArrayHelpersConfig? config;

		private UnityEditor.Editor? _configEditor;

		#endregion

		#region Unity Lifecycle

		private void OnEnable()
		{
			if (config != null)
				return;

			config = AssetDatabase.FindAssets($"t:{nameof(ArrayHelpersConfig)}")
				.Select(AssetDatabase.GUIDToAssetPath)
				.OrderBy(path => path, StringComparer.Ordinal)
				.Select(AssetDatabase.LoadAssetAtPath<ArrayHelpersConfig>)
				.FirstOrDefault(asset => asset != null);
		}

		private void OnDisable()
		{
			if (_configEditor != null)
				DestroyImmediate(_configEditor);
			_configEditor = null;
		}

		private void OnGUI()
		{
			EditorGUILayout.LabelField("Array Helpers Configuration", EditorStyles.boldLabel);
			config = (ArrayHelpersConfig)EditorGUILayout.ObjectField("Configuration", config, typeof(ArrayHelpersConfig), false);
			if (config == null)
			{
				EditorGUILayout.HelpBox("Assign an Array Helpers Config asset to edit its settings.", MessageType.Info);
				return;
			}

			UnityEditor.Editor.CreateCachedEditor(config, null, ref _configEditor);
			if (_configEditor != null)
				_configEditor.OnInspectorGUI();
		}

		#endregion

		[MenuItem(
			"Bezoro/Runtime/Array Helpers Config"
		)]
		private static void OpenWindow() =>
			GetWindow<ArrayHelpersConfigWindow>().Show();
	}
}
