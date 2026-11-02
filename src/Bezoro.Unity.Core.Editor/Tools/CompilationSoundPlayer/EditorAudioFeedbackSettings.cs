using System.Collections.Generic;
using Bezoro.Unity.Core.Editor.Utilities;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using FilePathAttribute = UnityEditor.FilePathAttribute;

namespace Bezoro.Unity.Core.Editor.Tools.CompilationSoundPlayer
{
	/// <summary>
	/// Stores audio clips played for compilation, build, test, and editor-ready events.
	/// </summary>
	[FilePath("ProjectSettings/EditorAudioFeedbackSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	public class EditorAudioFeedbackSettings : ScriptableSingleton<EditorAudioFeedbackSettings>
	{
		[Tooltip("Audio clip to play at the start of compilation")]
		public AudioClip compilationStartClip = null!;

		[Tooltip("Volume used when compilation starts")]
		[SerializeField, Range(0f, 1f)] internal float compilationStartVolume = 1f;

		[Tooltip("Audio clip to play at the end of compilation")]
		public AudioClip compilationEndClip = null!;

		[Tooltip("Volume used when compilation finishes")]
		[SerializeField, Range(0f, 1f)] internal float compilationEndVolume = 1f;

		[Tooltip("Audio clip to play when Unity is fully initialized")]
		public AudioClip unityFullyInitializedClip = null!;

		[Tooltip("Volume used when Unity is fully initialized")]
		[SerializeField, Range(0f, 1f)] internal float unityFullyInitializedVolume = 1f;

		[Tooltip("Audio clip to play when a build starts")]
		public AudioClip buildStartedClip = null!;

		[Tooltip("Volume used when a build starts")]
		[SerializeField, Range(0f, 1f)] internal float buildStartedVolume = 1f;

		[Tooltip("Audio clip to play when a build is completed")]
		public AudioClip buildCompletedSuccessfullyClip = null!;

		[Tooltip("Volume used when a build completes successfully")]
		[SerializeField, Range(0f, 1f)] internal float buildCompletedSuccessfullyVolume = 1f;

		[Tooltip("Audio clip to play when a build fails")]
		public AudioClip buildFailedClip = null!;

		[Tooltip("Volume used when a build fails")]
		[SerializeField, Range(0f, 1f)] internal float buildFailedVolume = 1f;

		[Tooltip("Audio clip to play at the start of tests")]
		public AudioClip testStartClip = null!;

		[Tooltip("Volume used when tests start")]
		[SerializeField, Range(0f, 1f)] internal float testStartVolume = 1f;

		[Tooltip("Audio clip to play at the end of tests")]
		public AudioClip testEndClip = null!;

		[Tooltip("Volume used when tests finish")]
		[SerializeField, Range(0f, 1f)] internal float testEndVolume = 1f;

		[Tooltip("Enable or disable audio feedback during editor events")]
		[SerializeField] internal bool play = true;

		[Tooltip("Master volume applied to every editor audio feedback sound")]
		[SerializeField, Range(0f, 1f)] internal float masterVolume = 1f;

		[System.NonSerialized] private bool _missingClipWarningLogged;

		#region Unity Lifecycle

		private void OnEnable()
		{
			NormalizeVolumes();

			if (!IsAssetOnDisk())
				Save(
					true
				);

			WarnIfAudioClipReferencesMissing();
			if (!play) return;

			SubscribeToEditorEvents();
		}

		private void OnDisable()
		{
			UnsubscribeFromEditorEvents();
		}

		private void OnValidate()
		{
			if (UnityEngine.Application.isPlaying) return;

			NormalizeVolumes();
			WarnIfAudioClipReferencesMissing();

			if (!IsAssetOnDisk()) return;

			EditorUtility.SetDirty(
				this
			);
		}

		#endregion

		[InitializeOnLoadMethod]
		internal static void InitializeAudioFeedback()
		{
			if (instance == null) return;

			instance.WarnIfAudioClipReferencesMissing();
			instance.UnsubscribeFromEditorEvents();

			if (!instance.play) return;

			instance.SubscribeToEditorEvents();
		}

		private static bool IsAssetOnDisk() =>
			AssetDatabase.Contains(
				instance
			);

		/// <summary>
		///     Plays the specified audio clip if not null.
		/// </summary>
		/// <param name="clip">The audio clip to play.</param>
		private void PlayAudioClip(AudioClip clip, float eventVolume)
		{
			if (!play) return;
			if (clip == null) return;

			var effectiveVolume = GetEffectiveVolume(
				eventVolume
			);
			if (effectiveVolume <= 0f) return;

			EditorAudioManager.PlayClip(
				clip,
				effectiveVolume
			);
		}

		private void OnBuildFailed() =>
			PlayAudioClip(
				buildFailedClip,
				buildFailedVolume
			);

		private void OnBuildStarted() =>
			PlayAudioClip(
				buildStartedClip,
				buildStartedVolume
			);

		/// <summary>
		///     Handles the event when a build finishes.
		/// </summary>
		private void OnBuildFinishedSuccessfully(BuildResult buildResult)
		{
			if (buildResult != BuildResult.Succeeded) return;

			PlayAudioClip(
				buildCompletedSuccessfullyClip,
				buildCompletedSuccessfullyVolume
			);
		}

		/// <summary>
		///     Handles the event when compilation finishes.
		/// </summary>
		private void OnCompilationFinished() =>
			PlayAudioClip(
				compilationEndClip,
				compilationEndVolume
			);

		/// <summary>
		///     Handles the event when compilation starts.
		/// </summary>
		private void OnCompilationStarted() =>
			PlayAudioClip(
				compilationStartClip,
				compilationStartVolume
			);

		private void OnEditorReady() =>
			PlayAudioClip(
				unityFullyInitializedClip,
				unityFullyInitializedVolume
			);

		/// <summary>
		///     Handles the event when a test run finishes.
		/// </summary>
		private void OnTestRunFinished() =>
			PlayAudioClip(
				testEndClip,
				testEndVolume
			);

		/// <summary>
		///     Handles the event when a test run starts.
		/// </summary>
		private void OnTestRunStarted() =>
			PlayAudioClip(
				testStartClip,
				testStartVolume
			);

		private void Save()
		{
			SaveSettings();
		}

		internal void SaveSettings()
		{
			NormalizeVolumes();
			Save(
				true
			);
		}

		internal void NormalizeVolumes()
		{
			masterVolume = NormalizeVolume(
				masterVolume
			);
			compilationStartVolume = NormalizeVolume(
				compilationStartVolume
			);
			compilationEndVolume = NormalizeVolume(
				compilationEndVolume
			);
			unityFullyInitializedVolume = NormalizeVolume(
				unityFullyInitializedVolume
			);
			buildStartedVolume = NormalizeVolume(
				buildStartedVolume
			);
			buildCompletedSuccessfullyVolume = NormalizeVolume(
				buildCompletedSuccessfullyVolume
			);
			buildFailedVolume = NormalizeVolume(
				buildFailedVolume
			);
			testStartVolume = NormalizeVolume(
				testStartVolume
			);
			testEndVolume = NormalizeVolume(
				testEndVolume
			);
		}

		internal float GetEffectiveVolume(float eventVolume) =>
			NormalizeVolume(
				masterVolume
			)
			* NormalizeVolume(
				eventVolume
			);

		private static float NormalizeVolume(float volume) =>
			Mathf.Clamp01(
				volume
			);

		private IReadOnlyList<string> GetMissingAudioClipReferenceNames()
		{
			var missingClipNames = new List<string>();

			if (compilationStartClip == null)
				missingClipNames.Add(
					nameof(compilationStartClip)
				);

			if (compilationEndClip == null)
				missingClipNames.Add(
					nameof(compilationEndClip)
				);

			if (unityFullyInitializedClip == null)
				missingClipNames.Add(
					nameof(unityFullyInitializedClip)
				);

			if (buildStartedClip == null)
				missingClipNames.Add(
					nameof(buildStartedClip)
				);

			if (buildCompletedSuccessfullyClip == null)
				missingClipNames.Add(
					nameof(buildCompletedSuccessfullyClip)
				);

			if (buildFailedClip == null)
				missingClipNames.Add(
					nameof(buildFailedClip)
				);

			if (testStartClip == null)
				missingClipNames.Add(
					nameof(testStartClip)
				);

			if (testEndClip == null)
				missingClipNames.Add(
					nameof(testEndClip)
				);

			return missingClipNames;
		}

		private void WarnIfAudioClipReferencesMissing()
		{
			if (!play) return;

			var missingClipNames = GetMissingAudioClipReferenceNames();
			if (missingClipNames.Count == 0)
			{
				_missingClipWarningLogged = false;
				return;
			}

			if (_missingClipWarningLogged) return;

			Debug.LogWarning(
				$"Editor audio feedback settings has missing clip references: {string.Join(", ", missingClipNames)}. The matching editor event sounds will not play until clips are assigned.",
				this
			);

			_missingClipWarningLogged = true;
		}

		/// <summary>
		///     Subscribes to the necessary editor events.
		/// </summary>
		private void SubscribeToEditorEvents()
		{
			CompilationEventHandler.CompilationStarted  += OnCompilationStarted;
			CompilationEventHandler.CompilationFinished += OnCompilationFinished;

			BuildEventHandler.BuildStarted      += OnBuildStarted;
			EditorBuildMonitor.BuildCompleted   += OnBuildFinishedSuccessfully;
			BuildEventHandler.BuildFailed       += OnBuildFailed;

			TestRunEventHandler.TestRunStarted  += OnTestRunStarted;
			TestRunEventHandler.TestRunFinished += OnTestRunFinished;

			EditorStatusWatcher.EditorReady += OnEditorReady;
		}

		/// <summary>
		///     Unsubscribes from the editor events.
		/// </summary>
		private void UnsubscribeFromEditorEvents()
		{
			CompilationEventHandler.CompilationStarted  -= OnCompilationStarted;
			CompilationEventHandler.CompilationFinished -= OnCompilationFinished;

			BuildEventHandler.BuildStarted      -= OnBuildStarted;
			EditorBuildMonitor.BuildCompleted   -= OnBuildFinishedSuccessfully;
			BuildEventHandler.BuildFailed       -= OnBuildFailed;

			TestRunEventHandler.TestRunStarted  -= OnTestRunStarted;
			TestRunEventHandler.TestRunFinished -= OnTestRunFinished;

			EditorStatusWatcher.EditorReady -= OnEditorReady;
		}
	}
}
