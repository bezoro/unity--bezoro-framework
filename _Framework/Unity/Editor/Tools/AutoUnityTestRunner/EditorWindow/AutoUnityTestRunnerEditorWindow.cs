using System;
using Bezoro.Framework.Unity.Editor.Utilities.EditorPrefs;
using UnityEditor;
using UnityEngine;
using EditorHelpers = Bezoro.Framework.Unity.Editor.Utilities.Helpers.EditorHelpers;

namespace Bezoro.Framework.Unity.Editor.Tools
{
	public class AutoUnityTestRunnerEditorWindow : EditorWindow
	{
	#region Private Fields

		private bool _isRunningTests;

		private AutoTestRunnerEditorWindowLogic _logic;
		private bool _reloadingAssembly;

	#endregion

	#region Properties

		public bool IsAutoRunEnabled
		{
			get => _logic.AutoRun;
			set => _logic.SetAutoRun(value);
		}

	#endregion

	#region Public Methods

		[MenuItem("Window/Bezoro/Auto Test Runner")]
		public static void ShowWindow()
		{
			GetWindow<AutoUnityTestRunnerEditorWindow>("Auto Test Runner");
		}

	#endregion

	#region Private Methods

		private void OnAfterAssemblyReload()
		{
			_reloadingAssembly = false;
		}

		private void OnBeforeAssemblyReload()
		{
			_reloadingAssembly = true;
		}

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
			Debug.Log("Running all tests manually...");
			await AutoUnityTestRunner.RunAllTestsAsync();
		}

	#endregion

	#region Unity Event Methods

		private void OnEnable()
		{
			_logic = new AutoTestRunnerEditorWindowLogic(new EditorPrefsSettingsStorage());
			AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

			AutoUnityTestRunner.Callbacks.TestsRunStarted += OnTestsRunStarted;
			AutoUnityTestRunner.Callbacks.TestsRunFinished += OnTestsRunFinished;
		}

		private void OnDisable()
		{
			AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

			AutoUnityTestRunner.Callbacks.TestsRunStarted -= OnTestsRunStarted;
			AutoUnityTestRunner.Callbacks.TestsRunFinished -= OnTestsRunFinished;
		}

		private void OnGUI()
		{
			EditorGUI.BeginChangeCheck();
			var newState = EditorGUILayout.Toggle("Auto Run Tests", IsAutoRunEnabled);

			if (EditorGUI.EndChangeCheck())
			{
				IsAutoRunEnabled = newState;
			}

			EditorGUI.BeginDisabledGroup
			(
				_isRunningTests || _reloadingAssembly || !EditorHelpers.IsEditorIdle()
			);

			if (GUILayout.Button("Run All Tests"))
			{
				RunAllTests();
				_isRunningTests = true; // Disable the button immediately
			}

			EditorGUI.EndDisabledGroup();
		}

	#endregion
	}

	public class AutoTestRunnerEditorWindowLogic
	{
		private const string AutoRunKey = "AutoRunTests_Enabled";
		private readonly ISettingsStorage _settingsStorage;

		public AutoTestRunnerEditorWindowLogic(ISettingsStorage settingsStorage)
		{
			_settingsStorage = settingsStorage ?? throw new ArgumentNullException();
			LoadSettings();
		}

		public bool AutoRun { get; private set; }

		public void SetAutoRun(bool value)
		{
			AutoRun = value;
			_settingsStorage.SetBool(AutoRunKey, value);
		}

		public bool ShouldRunTests()
		{
			return AutoRun;
		}

		private void LoadSettings()
		{
			try
			{
				AutoRun = _settingsStorage.GetBool(AutoRunKey);
			}
			catch ( Exception )
			{
				AutoRun = false; // Default to false if there's an exception
			}
		}
	}
}
