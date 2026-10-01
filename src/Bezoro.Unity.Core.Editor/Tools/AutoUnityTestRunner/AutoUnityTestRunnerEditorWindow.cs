using System;
using Bezoro.Unity.Core.Common.Interfaces;
using Bezoro.Unity.Core.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Tools.AutoUnityTestRunner
{
	/// <summary>
	///     Stores and updates the auto-run test preference.
	/// </summary>
	public class AutoTestRunnerEditorWindowLogic
	{
		private const string AUTO_RUN_KEY = "AutoRunTests_Enabled";

		private readonly IEditorPrefsUtilities _editorPrefsSettingsStorage;

		public AutoTestRunnerEditorWindowLogic(IEditorPrefsUtilities editorPrefsSettingsStorage)
		{
			_editorPrefsSettingsStorage = editorPrefsSettingsStorage ?? throw new ArgumentNullException();
			LoadSettings();
		}

		public bool AutoRun { get; private set; }

		public bool ShouldRunTests() =>
			AutoRun;

		public void SetAutoRun(bool value)
		{
			AutoRun = value;

			_editorPrefsSettingsStorage.SetBool(
				AUTO_RUN_KEY,
				value
			);
		}

		private void LoadSettings()
		{
			try
			{
				AutoRun = _editorPrefsSettingsStorage.GetBool(
					AUTO_RUN_KEY
				);
			}
			catch (Exception)
			{
				AutoRun = false; // Default to false if there's an exception
			}
		}
	}

	/// <summary>
	///     Lets users enable automatic Unity test runs after assembly reloads.
	/// </summary>
	public class AutoUnityTestRunnerEditorWindow : EditorWindow
	{
		#region Properties

		public bool IsAutoRunEnabled
		{
			get => _logic.AutoRun;
			set => _logic.SetAutoRun(
				value
			);
		}

		#endregion

		#region Public Methods

		[MenuItem(
			"Window/Bezoro/Auto Test Runner"
		)]
		public static void ShowWindow() =>
			GetWindow<AutoUnityTestRunnerEditorWindow>(
				"Auto Test Runner"
			);

		#endregion

		#region Private Fields

		private bool _isRunningTests;

		private AutoTestRunnerEditorWindowLogic _logic = null!;
		private bool                            _reloadingAssembly;

		#endregion

		#region Private Methods

		private void OnAfterAssemblyReload() =>
			_reloadingAssembly = false;

		private void OnBeforeAssemblyReload() =>
			_reloadingAssembly = true;

		private void OnTestsRunFinished()
		{
			_isRunningTests = false;
			Repaint();
		}

		private void OnTestsRunStarted()
		{
			_isRunningTests = true;
			Repaint();
		}

		private async void RunAllTests()
		{
			Debug.Log(
				"Running all tests manually..."
			);

			try
			{
				await AutoUnityTestRunner.RunAllTestsAsync();
			}
			catch (Exception ex)
			{
				_isRunningTests = false;
				Debug.LogError(
					$"Manual test run failed: {ex}"
				);

				Repaint();
			}
		}

		#endregion

		#region Unity Event Methods

		private void OnEnable()
		{
			_logic = new(
				new EditorPrefsUtilities()
			);

			AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload  += OnAfterAssemblyReload;

			AutoUnityTestRunner.TestsRunStarted  += OnTestsRunStarted;
			AutoUnityTestRunner.TestsRunFinished += OnTestsRunFinished;
		}

		private void OnDisable()
		{
			AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload  -= OnAfterAssemblyReload;

			AutoUnityTestRunner.TestsRunStarted  -= OnTestsRunStarted;
			AutoUnityTestRunner.TestsRunFinished -= OnTestsRunFinished;
		}

		private void OnGUI()
		{
			EditorGUI.BeginChangeCheck();

			bool newState = EditorGUILayout.Toggle(
				"Auto Run Tests",
				IsAutoRunEnabled
			);

			if (EditorGUI.EndChangeCheck()) IsAutoRunEnabled = newState;

			EditorGUI.BeginDisabledGroup
			(
				_isRunningTests || _reloadingAssembly || !EditorHelpers.Is_Editor_Idle()
			);

			if (GUILayout.Button(
					"Run All Tests"
				))
			{
				RunAllTests();
				_isRunningTests = true; // Disable the button immediately
			}

			EditorGUI.EndDisabledGroup();
		}

		#endregion
	}
}
