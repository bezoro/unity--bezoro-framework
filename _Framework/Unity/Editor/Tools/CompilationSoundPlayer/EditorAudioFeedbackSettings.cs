using Bezoro.Framework.Unity.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.Tools
{
	[CreateAssetMenu( fileName = "EditorAudioFeedbackSettings", menuName = "Bezoro/Editor/Compilation Sound Settings" )]
	public class EditorAudioFeedbackSettings : UnityEditor.ScriptableSingleton<EditorAudioFeedbackSettings>
	{
	#region Serialized Fields

		[SerializeField] [Tooltip( "Enable or disable audio feedback during editor events" )]
		private bool play = true;

		[Tooltip( "Audio clip to play at the start of compilation" )]
		public AudioClip compilationStartClip;

		[Tooltip( "Audio clip to play at the end of compilation" )]
		public AudioClip compilationEndClip;

		[Tooltip( "Audio clip to play when Unity is fully initialized" )]
		public AudioClip unityFullyInitializedClip;

		[Tooltip( "Audio clip to play when a build is completed" )]
		public AudioClip buildCompletedClip;

		[Tooltip( "Audio clip to play at the start of tests" )]
		public AudioClip testStartClip;

		[Tooltip( "Audio clip to play at the end of tests" )]
		public AudioClip testEndClip;

	#endregion

	#region Singleton Check

		/// <summary>
		///     Checks if the current instance is part of the asset database.
		/// </summary>
		public static bool IsAssetInstance() => AssetDatabase.Contains( instance );

	#endregion

	#region Unity Lifecycle

		private void OnEnable()
		{
			if ( !play ) { return; }
			SubscribeToEditorEvents();
		}

		private void OnDestroy()
		{
			UnsubscribeFromEditorEvents();
		}

	#endregion

	#region Event Subscription Management

		/// <summary>
		///     Subscribes to the necessary editor events.
		/// </summary>
		private void SubscribeToEditorEvents()
		{
			CompilationEventHandler.CompilationStarted  += OnCompilationStarted;
			CompilationEventHandler.CompilationFinished += OnCompilationFinished;

			BuildEventHandler.BuildStarted  += OnBuildStarted;
			BuildEventHandler.BuildFinished += OnBuildFinished;

			TestRunEventHandler.TestRunStarted  += OnTestRunStarted;
			TestRunEventHandler.TestRunFinished += OnTestRunFinished;

			EditorStatusWatcher.EditorReady += OnEditorReady;
		}

		private void OnEditorReady()
		{
			PlayAudioClip( unityFullyInitializedClip );
		}

		/// <summary>
		///     Unsubscribes from the editor events.
		/// </summary>
		private void UnsubscribeFromEditorEvents()
		{
			CompilationEventHandler.CompilationStarted  -= OnCompilationStarted;
			CompilationEventHandler.CompilationFinished -= OnCompilationFinished;

			BuildEventHandler.BuildStarted  -= OnBuildStarted;
			BuildEventHandler.BuildFinished -= OnBuildFinished;

			TestRunEventHandler.TestRunStarted  -= OnTestRunStarted;
			TestRunEventHandler.TestRunFinished -= OnTestRunFinished;

			EditorStatusWatcher.EditorReady -= OnEditorReady;
		}

	#endregion

	#region Event Handlers

		/// <summary>
		///     Handles the event when compilation starts.
		/// </summary>
		private void OnCompilationStarted() => PlayAudioClip( compilationStartClip );

		/// <summary>
		///     Handles the event when compilation finishes.
		/// </summary>
		private void OnCompilationFinished() => PlayAudioClip( compilationEndClip );

		/// <summary>
		///     Handles the event when a build starts.
		/// </summary>
		private void OnBuildStarted() => PlayAudioClip( buildCompletedClip );

		/// <summary>
		///     Handles the event when a build finishes.
		/// </summary>
		private void OnBuildFinished() => PlayAudioClip( buildCompletedClip );

		/// <summary>
		///     Handles the event when a test run starts.
		/// </summary>
		private void OnTestRunStarted() => PlayAudioClip( testStartClip );

		/// <summary>
		///     Handles the event when a test run finishes.
		/// </summary>
		private void OnTestRunFinished() => PlayAudioClip( testEndClip );

	#endregion

	#region Utility Methods

		/// <summary>
		///     Plays the specified audio clip if not null.
		/// </summary>
		/// <param name="clip">The audio clip to play.</param>
		private static void PlayAudioClip( AudioClip clip )
		{
			if ( clip != null )
			{
				EditorAudioManager.PlayClip( clip );
			}
		}

	#endregion
	}
}
