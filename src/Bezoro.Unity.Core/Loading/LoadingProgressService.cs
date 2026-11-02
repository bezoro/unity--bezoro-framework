using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using Bezoro.Core.Helpers;
using Bezoro.Logging.Types;
using Bezoro.Unity.Core.Common.Enums;
using Bezoro.Unity.Core.MonoBehaviours;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Loading
{
	/// <summary>
	///     A singleton service to coordinate and aggregate progress from multiple asynchronous loading tasks.
	///     It manages loading operations, creates progress trackers for individual tasks, and reports the overall progress.
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(SingletonScope), typeof(GlobalReference))]
	[DefaultExecutionOrder(ExecutionOrder.Manager)]
	public class LoadingProgressService : MonoBehaviour
	{
		private const    float              PROGRESS_SMOOTH_TIME = 0.2f;
		private readonly LoadingProgressServiceModel _model = new();
		private readonly List<LoadingStage> _stages              = new();

		public float completeEventDelayS = 1f;

		[Tooltip(
			"Seconds to wait for at least one tracker before completing automatically."
		)]
		[SerializeField] private float noTrackerTimeoutS = .2f;

		private bool                     isLoading;
		private                              bool                    _isSingletonInstance;
		private                              CancellationTokenSource? _concludeOperationCts;

		private CancellationTokenSource? _waitForTrackerCts;

		private float _smoothProgress;
		private float _smoothVelocity;

		/// <summary>Fires when the entire loading operation is complete (progress reaches 100%).</summary>
		public event Action? Completed;

		/// <summary>Fires every time the overall progress value changes.</summary>
		public event Action<float>? ProgressChanged;

		public static bool IsAvailable => TryGetInstance(out _);

		public static LoadingProgressService Instance => GlobalReferenceAccess<LoadingProgressService>.Instance;

		/// <summary>Indicates whether a loading operation is currently in progress.</summary>
		public bool IsLoading
		{
			get => isLoading;
			private set => isLoading = value;
		}

		/// <summary>The overall progress of the current loading operation, in the range [0, 1].</summary>
		public float OverallProgress { get; private set; }

		/// <summary>The name of the current loading operation.</summary>
		public string OperationName { get; private set; } = "Default Operation";

		public static bool TryGetInstance([NotNullWhen(true)] out LoadingProgressService? instance) =>
			GlobalReferenceAccess<LoadingProgressService>.TryGetInstance(out instance);

		#region Unity Lifecycle

		private void Awake()
		{
			_isSingletonInstance = GlobalReferenceAccess<LoadingProgressService>.IsCurrent(this);

			if (_isSingletonInstance)
				SceneLoader.SceneLoadCompleted += OnSceneLoadCompleted;
		}

		private void Update()
		{
			if (!_model.ShouldUpdate(IsLoading)) return;

			// Smoothly interpolate to the actual progress
			float targetProgress = CalculateRawProgress();
			_smoothProgress = Mathf.SmoothDamp(
				_smoothProgress,
				targetProgress,
				ref _smoothVelocity,
				PROGRESS_SMOOTH_TIME
			);

			_smoothProgress = _model.GetMonotonicProgress(
				_smoothProgress,
				OverallProgress
			);

			if (!_model.ShouldPublishProgress(
					OverallProgress,
					_smoothProgress
				)) return;

			OverallProgress = _smoothProgress;
			ProgressChanged?.Invoke(
				OverallProgress
			);

			if (_model.ShouldConclude(OverallProgress))
				ConcludeOperation().Forget();

			LogProgress();
		}

		private void OnDestroy()
		{
			if (!_isSingletonInstance)
				return;

			SceneLoader.SceneLoadCompleted -= OnSceneLoadCompleted;

			ResetState();

			Completed       = null;
			ProgressChanged = null;

			_waitForTrackerCts?.Cancel();
			_waitForTrackerCts?.Dispose();
			_waitForTrackerCts = null;

			_concludeOperationCts?.Cancel();
			_concludeOperationCts?.Dispose();
			_concludeOperationCts = null;

		}

		#endregion

		[ContextMenu("Log Loading Progress")]
		private void LogLoadingProgress() =>
			Debug.Log($"{OperationName}: loading={isLoading}, progress={OverallProgress:P2}, stages={_stages.Count}.", this);

		/// <summary>
		///     Begins a new stage in the loading process.
		/// </summary>
		/// <param name="stageName">Name of this loading stage</param>
		/// <param name="weight">Weight of this stage in the overall process</param>
		/// <returns>A LoadingStage object to manage trackers for this stage</returns>
		public LoadingStage BeginStage(string stageName, float weight = 1f)
		{
			if (_model.ShouldAutoBeginOperation(IsLoading))
			{
				Debug.LogWarning(
					"BeginStage called while no loading operation was active. Starting a new operation automatically.",
					this
				);

				BeginOperation();
			}

			// A tracker/stage is about to be registered — cancel the 'no-tracker' timeout
			_waitForTrackerCts?.Cancel();

			var stage = new LoadingStage(
				this,
				stageName,
				weight,
				RecalculateProgress
			);

			_stages.Add(
				stage
			);

			return stage;
		}

		/// <summary>
		///     Creates and registers a new progress tracker in the default stage.
		///     Each tracker represents a task that contributes to the overall progress.
		/// </summary>
		/// <param name="trackerName">A descriptive name for the tracker (e.g., "Loading Audio").</param>
		/// <param name="weight">The weight of this tracker in the overall calculation. A higher weight gives it more influence.</param>
		/// <returns>A new <see cref="ProgressTracker" /> instance.</returns>
		public ProgressTracker? CreateTracker(string trackerName, float weight = 1f)
		{
			if (!IsLoading)
			{
				Debug.LogWarning(
					"CreateTracker called while no loading operation was active. Starting a new operation automatically.",
					this
				);

				BeginOperation();
			}

			// A tracker is being registered — cancel the 'no-tracker' timeout
			_waitForTrackerCts?.Cancel();

			// Create a default stage if none exists
			if (_model.ShouldCreateDefaultStage(_stages.Count))
				BeginStage(
					"Default Loading Stage"
				);

			return _stages[0].CreateTracker(
				trackerName,
				weight
			);
		}

		/// <summary>
		///     Begins a new loading operation, clearing any previous state.
		/// </summary>
		/// <param name="operationName">A descriptive name for the loading operation for logging purposes.</param>
		public void BeginOperation(string operationName = "Loading")
		{
			if (IsLoading)
				Debug.LogWarning(
					$"New loading operation '{operationName}' started while '{OperationName}' was in progress. The previous operation will be cleared."
				);

			ResetState();
			IsLoading       = true;
			OperationName   = operationName;
			_smoothProgress = 0f;
			_smoothVelocity = 0f;
			Debug.Log(
				$"Loading operation '{OperationName}' started."
			);

			ProgressChanged?.Invoke(
				0f
			);

			// Start a grace period; if no trackers arrive in time we will auto-complete
			WaitForFirstTracker().Forget();
		}

		/// <summary>
		///     Ends the specified loading stage, smoothly transitioning its progress contribution
		/// </summary>
		public void EndStage(LoadingStage stage)
		{
			if (!_model.ShouldEndStage(_stages.Contains(stage))) return;

			stage.Complete();
			_stages.Remove(
				stage
			);

			RecalculateProgress();
		}

		public void ForceComplete()
		{
			IsLoading = true;
			ConcludeOperation().Forget();
		}

		internal void RegisterStageIfNeeded(LoadingStage stage)
		{
			if (_model.ShouldRegisterStage(_stages.Contains(stage)))
				_stages.Add(
					stage
				);
		}

		private float CalculateRawProgress()
		{
			var totalWeight   = 0f;
			var totalProgress = 0f;

			foreach (var stage in _stages)
			{
				totalWeight   += stage.Weight;
				totalProgress += stage.Progress * stage.Weight;
			}

			return _model.GetWeightedProgress(totalProgress, totalWeight, _stages.Count);
		}

		private async UniTaskVoid ConcludeOperation()
		{
			if (!IsLoading) return;

			_concludeOperationCts?.Cancel();
			_concludeOperationCts?.Dispose();
			_concludeOperationCts = new();
			var ct = _concludeOperationCts.Token;

			IsLoading       = false;
			OverallProgress = 1f;
			ProgressChanged?.Invoke(
				OverallProgress
			);

			Logger.LogSuccess(
				$"Loading operation '{OperationName}' completed. Waiting a bit before raising the event.",
				contextObject: this,
				category:
				LogCategory.Loading
			);

			try
			{
				await UniTask.WaitForSeconds(
					completeEventDelayS,
					cancellationToken: ct
				);
			}
			catch (OperationCanceledException) when (ct.IsCancellationRequested)
			{
				return;
			}

			if (this == null) return;

			Completed?.Invoke();
			ResetState();
		}

		private async UniTaskVoid WaitForFirstTracker()
		{
			// Cancel any previous wait and start a new one
			_waitForTrackerCts?.Cancel();
			_waitForTrackerCts?.Dispose();
			_waitForTrackerCts = new();

			try
			{
				await UniTask.Delay(
					TimeSpan.FromSeconds(
						noTrackerTimeoutS
					),
					cancellationToken: _waitForTrackerCts.Token
				);

				// If an operation is not in progress anymore, do nothing
				if (!IsLoading) return;

				// If we get here the token wasn't cancelled – no tracker was created in time
				if (_model.ShouldForceCompleteForNoTrackers(IsLoading, _stages.Count))
				{
					Logger.LogWarning(
						$"Loading operation '{OperationName}' contains no progress trackers. Completing automatically.",
						contextObject: this,
						category:
						LogCategory.Loading
					);

					ForceComplete();
				}
			}
			catch (OperationCanceledException)
			{
				// Expected when at least one tracker/stage is registered within the grace period
			}
		}

		private void LogProgress()
		{
			var progressDetails = new StringBuilder();
			progressDetails.Append(
				$"[{OperationName}] Overall: {OverallProgress:P2}"
			);

			foreach (var stage in _stages)
			{
				progressDetails.Append(
					$"\nS: {stage.Name}: {stage.Progress:P2}"
				);

				foreach (var tracker in stage.Trackers)
				{
					progressDetails.Append(
						$"\n\tT: {tracker.Name}: {tracker.Progress:P2} |  "
					);
				}
			}

			Logger.Log(
				progressDetails.ToString(),
				contextObject: this,
				category: LogCategory.Loading
			);
		}

		private void OnSceneLoadCompleted(Scene obj)
		{
			WaitForFirstTracker().Forget();
		}

		private void RecalculateProgress()
		{
			if (!_model.ShouldUpdate(IsLoading)) return;

			float targetProgress = CalculateRawProgress();

			// Only invoke events if the progress has meaningfully changed
			if (!_model.ShouldPublishProgress(
					OverallProgress,
					targetProgress
				)) return;

			OverallProgress = targetProgress;
			ProgressChanged?.Invoke(
				OverallProgress
			);

			LogProgress();

			// When progress reaches 100%, automatically conclude the operation
			if (_model.ShouldConclude(OverallProgress))
				ConcludeOperation().Forget();
		}

		private void ResetState()
		{
			_concludeOperationCts?.Cancel();
			_concludeOperationCts?.Dispose();
			_concludeOperationCts = null;

			IsLoading     = false;
			OperationName = string.Empty;
			_stages.Clear();
			OverallProgress = 0f;
			_smoothProgress = 0f;
			_smoothVelocity = 0f;
		}
	}

	public readonly struct LoadingProgressContribution
	{
		public LoadingProgressContribution(float progress, float weight)
		{
			Progress = progress;
			Weight   = weight;
		}

		public float Progress { get; }
		public float Weight   { get; }
	}

	public sealed class LoadingProgressServiceModel
	{
		private const float CompletionThreshold = 0.99f;
		private const float AutoCompleteTrackerThreshold = 0.91f;
		private const float MinimumWeight = 0.01f;

		public bool ShouldUpdate(bool isLoading)
		{
			return isLoading;
		}

		public bool ShouldAutoBeginOperation(bool isLoading)
		{
			return !isLoading;
		}

		public bool ShouldCreateDefaultStage(int stageCount)
		{
			return stageCount == 0;
		}

		public bool ShouldEndStage(bool stageIsRegistered)
		{
			return stageIsRegistered;
		}

		public bool ShouldRegisterStage(bool stageIsRegistered)
		{
			return !stageIsRegistered;
		}

		public bool ShouldForceCompleteForNoTrackers(bool isLoading, int stageCount)
		{
			return isLoading && stageCount == 0;
		}

		public bool ShouldPublishProgress(float currentProgress, float nextProgress)
		{
			return !Mathf.Approximately(currentProgress, nextProgress);
		}

		public bool ShouldConclude(float progress)
		{
			return FloatComparer.IsGreaterThanOrEqual(progress, CompletionThreshold);
		}

		public float GetPositiveWeight(float weight)
		{
			return Mathf.Max(MinimumWeight, weight);
		}

		public float GetMonotonicProgress(float nextProgress, float currentProgress)
		{
			return Mathf.Max(nextProgress, currentProgress);
		}

		public float GetTrackerProgress(float rawProgress)
		{
			float clampedProgress = Mathf.Clamp01(rawProgress);

			return clampedProgress > AutoCompleteTrackerThreshold
					   ? 1f
					   : clampedProgress;
		}

		public float GetClampedProgress(float rawProgress)
		{
			return Mathf.Clamp01(rawProgress);
		}

		public float GetWeightedProgress(float totalProgress, float totalWeight, int itemCount)
		{
			if (itemCount == 0) return 0f;

			return totalWeight > 0 ? totalProgress / totalWeight : 0f;
		}

		public float GetWeightedProgress(IReadOnlyList<LoadingProgressContribution> contributions)
		{
			if (contributions.Count == 0) return 0f;

			var totalWeight   = 0f;
			var totalProgress = 0f;

			foreach (var contribution in contributions)
			{
				totalWeight   += contribution.Weight;
				totalProgress += contribution.Progress * contribution.Weight;
			}

			return GetWeightedProgress(totalProgress, totalWeight, contributions.Count);
		}
	}

	/// <summary>
	///     Represents a stage in the loading process, containing its own set of progress trackers
	/// </summary>
	public class LoadingStage
	{
		private readonly Action                 _onProgressChanged;
		private readonly LoadingProgressServiceModel _model = new();
		private readonly List<ProgressTracker>  _trackers = new();
		private readonly LoadingProgressService _service;
		private          float                  _totalWeight;

		public LoadingStage(LoadingProgressService service, string name, float weight, Action onProgressChanged)
		{
			_service = service;
			Name     = name;
			Weight = _model.GetPositiveWeight(weight);

			_onProgressChanged = onProgressChanged;
		}

		public float Progress
		{
			get
			{
				var totalProgress = 0f;

				foreach (var tracker in _trackers)
					totalProgress += tracker.Progress * tracker.Weight;

				return _model.GetWeightedProgress(totalProgress, _totalWeight, _trackers.Count);
			}
		}

		public float                          Weight   { get; }
		public IReadOnlyList<ProgressTracker> Trackers => _trackers;

		public string Name { get; }

		public ProgressTracker CreateTracker(string name, float weight = 1f)
		{
			// If an operation was completed, revive/start it and ensure this stage is registered with the service.
			if (_model.ShouldAutoBeginOperation(_service.IsLoading))
				_service.BeginOperation(
					string.IsNullOrEmpty(
						_service.OperationName
					)
						? "Loading"
						: _service.OperationName
				);

			_service.RegisterStageIfNeeded(
				this
			);

			var tracker = new ProgressTracker(
				name,
				weight,
				_onProgressChanged
			);

			_trackers.Add(
				tracker
			);

			_totalWeight += tracker.Weight;
			return tracker;
		}

		public void Complete()
		{
			foreach (var tracker in _trackers)
				tracker.Complete();
		}
	}

	/// <summary>
	///     Represents a single trackable task within a larger loading operation.
	///     Its progress contributes to the overall progress reported by the <see cref="LoadingProgressService" />.
	/// </summary>
	public class ProgressTracker
	{
		private readonly Action       _onProgressChanged;
		private readonly LoadingProgressServiceModel _model = new();
		private          float        _progress;
		private          Func<float>? _boundVariable;

		internal ProgressTracker(string name, float weight, Action onProgressChanged)
		{
			Name = name;
			Weight = _model.GetPositiveWeight(weight);

			_onProgressChanged = onProgressChanged;
		}

		/// <summary>The weight of this tracker in the overall progress calculation.</summary>
		public float Weight { get; }

		/// <summary>The name of this tracker for identification and logging.</summary>
		public string Name { get; }

		/// <summary>
		///     The current progress of this tracker, in the range [0, 1].
		///     If bound to a variable, returns the bound variable's value.
		///     Setting this value notifies the LoadingProgressService to recalculate the overall progress.
		/// </summary>
		public float Progress
		{
			get
			{
				float value = _boundVariable != null
								  ? _model.GetTrackerProgress(
									  _boundVariable()
								  )
								  : _model.GetTrackerProgress(_progress);

				return value;
			}
			set
			{
				if (_boundVariable != null)
				{
					Debug.LogWarning(
						$"Tracker '{Name}' is bound to a variable. Setting progress directly will have no effect."
					);

					return;
				}

				float clampedValue = _model.GetClampedProgress(
					value
				);

				if (!_model.ShouldPublishProgress(
						_progress,
						clampedValue
					)) return;

				_progress = clampedValue;
				_onProgressChanged?.Invoke();
			}
		}

		/// <summary>
		///     Binds this tracker's progress to a variable through a getter function.
		///     The tracker will use this value as its progress.
		/// </summary>
		/// <param name="getter">A function that returns the current progress value (should be in range [0,1])</param>
		public void BindToVariable(Func<float> getter)
		{
			_boundVariable = getter;
		}

		/// <summary>
		///     Marks this tracker as complete by setting its progress to 1.0.
		/// </summary>
		public void Complete()
		{
			Progress = 1.0f;
		}
	}
}
