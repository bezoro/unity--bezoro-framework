using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bezoro.Core.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneManager = Bezoro.Unity.Core.Scenes.SceneManager;

namespace Bezoro.Unity.Core.Loading;

/// <summary>
///     High-level gateway for scene loading / unloading operations.
///     Everything is <b>fire-and-forget</b> – callbacks and returned handles
///     are used for progress/complete notifications.
/// </summary>
public static class SceneLoader
{
	private static readonly Dictionary<string, AsyncOperation> Loading           = new();
	private static readonly Dictionary<string, AsyncOperation> Unloading         = new();

	public static event Action<Scene>?  SceneLoadCompleted;
	public static event Action<Scene>?  SceneLoadStarted;
	public static event Action<string>? SceneUnloadCompleted;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetTrackedOperations()
	{
		Loading.Clear();
		Unloading.Clear();
	}

	public static IEnumerable<string> LoadedScenePaths =>
		Enumerable.Range(
					  0,
					  UnityEngine.SceneManagement.SceneManager.sceneCount
				  )
				  .Select(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(
							  i
						  ).path
				  );

	/// <summary>
	///     Loads the given scenes asynchronously.
	/// </summary>
	/// <param name="scenePaths">Collection of scene paths to load.</param>
	/// <param name="mode">LoadSceneMode determining if scenes should be loaded additively or by replacing existing scenes.</param>
	/// <param name="allowSceneActivation">If false, call SceneLoadHandle.Activate on the returned handle to release the native activation barrier.</param>
	/// <param name="ct">Cancels this handle’s wait and releases its deferred activations. Native scene operations finish normally.</param>
	/// <returns>An AsyncLoadHandle to track the loading progress and completion status.</returns>
	public static SceneLoadHandle Load(
		IEnumerable<string> scenePaths,
		LoadSceneMode       mode                 = LoadSceneMode.Single,
		bool                allowSceneActivation = true,
		CancellationToken   ct                   = default)
	{
		if (ct.IsCancellationRequested) return SceneLoadHandle.Cancelled();
		var paths = scenePaths?.Where(path => !string.IsNullOrEmpty(path)).Distinct().ToArray();
		if (paths == null || paths.Length == 0) return new SceneLoadHandle(null);

		var operations = new List<AsyncOperation>(paths.Length);
		var deferred = new List<AsyncOperation>();
		bool replacingCurrentScenes = false;
		try
		{
			foreach (string path in paths)
			{
				var op = LoadSceneInternal(path, mode, allowSceneActivation, replacingCurrentScenes, out bool started);
				if (mode == LoadSceneMode.Single && op != null) replacingCurrentScenes = true;
				if (started && !allowSceneActivation && op != null) deferred.Add(op);
				if (op != null) operations.Add(op);
				mode = LoadSceneMode.Additive;
			}
		}
		catch
		{
			// No handle will reach the caller to release earlier deferred loads.
			foreach (var op in deferred) op.allowSceneActivation = true;
			throw;
		}

		return operations.Count == 0 ? SceneLoadHandle.Completed() : new SceneLoadHandle(operations, ct, deferred);
	}


	/// <summary>
	///     Loads a single scene with the specified parameters.
	/// </summary>
	/// <param name="path">The path to the scene that should be loaded.</param>
	/// <param name="mode">Specifies whether to load the scene additively or with single mode (unloading current scenes).</param>
	/// <param name="allowSceneActivation">If false, the scene will load but not activate until set to true.</param>
	/// <param name="ct">Cancels this handle’s wait and releases its deferred activations. Native scene operations finish normally.</param>
	/// <returns>A handle to track the loading progress and status.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the loading operation could not be started.</exception>
	public static SceneLoadHandle Load(
		string            path,
		LoadSceneMode     mode                 = LoadSceneMode.Single,
		bool              allowSceneActivation = true,
		CancellationToken ct                   = default)
	{
		return Load(
			new[] { path },
			mode,
			allowSceneActivation,
			ct
		);
	}

	/// <summary>
	///     Unloads the provided scenes (if they are currently loaded).
	/// </summary>
	public static SceneLoadHandle Unload(IEnumerable<string> scenePaths, CancellationToken ct = default)
	{
		if (ct.IsCancellationRequested) return SceneLoadHandle.Cancelled();
		var paths = scenePaths?.Where(path => !string.IsNullOrEmpty(path)).Distinct().ToArray();
		if (paths == null || paths.Length == 0) return new SceneLoadHandle(null);

		var operations = new List<AsyncOperation>(paths.Length);
		foreach (string path in paths)
		{
			var op = UnloadSceneInternal(path);
			if (op != null) operations.Add(op);
		}
		return operations.Count == 0 ? SceneLoadHandle.Completed() : new SceneLoadHandle(operations, ct);
	}

	/// <summary>
	///     Unloads the provided scene (if it's currently loaded).
	/// </summary>
	public static SceneLoadHandle Unload(string path, CancellationToken ct = default)
	{
		return Unload(
			new[] { path },
			ct
		);
	}

	public static SceneLoadHandle UnloadAll(IEnumerable<string>? pathsToSkip = null, CancellationToken ct = default)
	{
		var       paths  = SceneManager.GetAllLoadedScenePaths();
		string[]? toSkip = pathsToSkip as string[] ?? pathsToSkip?.ToArray();
		if (!toSkip.IsNullOrEmpty())
			paths = paths.Except(
				toSkip
			).ToHashSet();

		if (ct.IsCancellationRequested) return SceneLoadHandle.Cancelled();
		if (paths.Count == 0) return SceneLoadHandle.Completed();

		return Unload(
			paths,
			ct
		);
	}

	public static void ReloadScenes(CancellationToken ct)
	{
		var paths = SceneManager.GetAllLoadedScenePaths();
		LoadingScreenManager.LoadScenesWithLoadingScreen(
			paths,
			LoadSceneMode.Single,
			null
		).AttachExternalCancellation(ct).Forget();
	}

	private static AsyncOperation? LoadSceneInternal(
		string            path,
		LoadSceneMode     mode,
		bool              allowSceneActivation,
		bool              replacingCurrentScenes,
		out bool started)
	{
		started = false;
		if (string.IsNullOrEmpty(path)) return null;

		if (Loading.TryGetValue(path, out var existingOp) && !existingOp.isDone)
			return existingOp;

		var scene = SceneManager.GetSceneByPath(path);
		if (!replacingCurrentScenes && scene.IsValid() && scene.isLoaded &&
			(mode == LoadSceneMode.Additive || SceneManager.GetLoadedSceneCount() == 1))
			return null;

		SceneLoadStarted?.Invoke(
			scene
		);

		var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
			path,
			mode
		);

		if (op == null)
			throw new ArgumentNullException(
				$"Operation is Null for {path}"
			);

		started = true;
		op.allowSceneActivation = allowSceneActivation;
		Track(Loading, path, op);

		return op;
	}

	private static AsyncOperation? UnloadSceneInternal(string path)
	{
		if (EnumerableExtensions.IsNullOrEmpty(
				path
			)) return null;

		if (Unloading.TryGetValue(path, out var existingOp) && !existingOp.isDone)
			return existingOp;

		var scene = SceneManager.GetSceneByPath(
			path
		);

		if (!scene.IsValid() || !scene.isLoaded)
			return null;


		var op = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
			scene
		);

		if (op == null) throw new InvalidOperationException($"Unity could not unload {path}.");

		Track(Unloading, path, op);

		return op;
	}

	private static void Track(Dictionary<string, AsyncOperation> operations, string path, AsyncOperation operation)
	{
		operations[path] = operation;
		operation.completed += completed =>
		{
			if (operations.TryGetValue(path, out var current) && current == completed)
				operations.Remove(path);
		};
	}

	/// <summary>
	///     Bridges Unity-internal scene notifications to <see cref="SceneLoader" /> so that
	///     completion notifications follow native loads and unloads independently of caller waits.
	/// </summary>
	[UsedImplicitly]
	private static class SceneLoaderUnityHooks
	{
		[RuntimeInitializeOnLoadMethod(
			RuntimeInitializeLoadType.BeforeSceneLoad
		)]
		private static void RegisterUnitySceneCallbacks()
		{
			UnityEngine.SceneManagement.SceneManager.sceneLoaded   -= OnUnitySceneLoaded;
			UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnUnitySceneUnloaded;

			UnityEngine.SceneManagement.SceneManager.sceneLoaded   += OnUnitySceneLoaded;
			UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnUnitySceneUnloaded;

			ForwardAlreadyLoadedScenes();
		}

		private static void ForwardAlreadyLoadedScenes()
		{
			for (var i = 0; i < SceneManager.GetLoadedSceneCount(); i++)
			{
				var scene = SceneManager.GetLoadedSceneAt(
					i
				);

				if (scene.isLoaded) SceneLoadCompleted?.Invoke(scene);
			}
		}

		private static void OnUnitySceneLoaded(Scene scene, LoadSceneMode _) =>
			SceneLoadCompleted?.Invoke(scene);

		private static void OnUnitySceneUnloaded(Scene scene) =>
			SceneUnloadCompleted?.Invoke(scene.path);
	}
}

/// <summary>
///     Simple handle returned from SceneLoader to poll progress.
///     Resolves itself automatically when all supplied AsyncOperations are done.
/// </summary>
public sealed class SceneLoadHandle
{
	private readonly IReadOnlyList<AsyncOperation>? _operations;
	private readonly IReadOnlyList<AsyncOperation>? _deferred;
	private readonly UniTaskCompletionSource<bool>  _tcs = new();

	public event Action<float>? ProgressChanged;

	internal static SceneLoadHandle Completed() => new SceneLoadHandle(false);
	internal static SceneLoadHandle Cancelled() => new SceneLoadHandle(true);

	private SceneLoadHandle(bool cancelled)
	{
		if (cancelled) Cancel();
		else
		{
			Progress = 1f;
			_tcs.TrySetResult(true);
		}
	}

	public SceneLoadHandle(IEnumerable<AsyncOperation>? operations) : this(operations, default) { }

	internal SceneLoadHandle(IEnumerable<AsyncOperation>? operations, CancellationToken ct, IReadOnlyList<AsyncOperation>? deferred = null)
	{
		_operations = operations?.ToArray();
		_deferred = deferred;
		if (_operations == null || _operations.Count == 0)
		{
			Progress = -1f;
			_tcs.TrySetException(new InvalidOperationException("No async operations supplied."));
			return;
		}
		ObserveAsync(ct).Forget();
	}

	/// <summary>True when a deferred scene has reached the activation barrier. Unity queues later scenes behind it.</summary>
	public bool IsReadyForActivation => _deferred != null &&
		_deferred.Any(op => !op.isDone && !op.allowSceneActivation && op.progress >= 0.9f);

	/// <summary>Releases all deferred activations for this batch. Call from the Unity main thread.</summary>
	public void Activate()
	{
		if (_deferred == null) return;
		foreach (var operation in _deferred)
			if (!operation.isDone && !operation.allowSceneActivation) operation.allowSceneActivation = true;
	}

	public bool IsDone => _tcs.Task.Status == UniTaskStatus.Succeeded;
	public bool IsDoneOrCancelled => IsDone || IsCancelled;
	public bool IsDoneOrFailed => IsDone || IsFailed;
	public bool IsFailed => _tcs.Task.Status == UniTaskStatus.Faulted;
	public bool IsCancelled => _tcs.Task.Status == UniTaskStatus.Canceled;
	public UniTask Task => _tcs.Task;
	public float Progress { get; private set; }
	public UniTask.Awaiter GetAwaiter() => Task.GetAwaiter();

	/// <summary>Cancels this wait and releases deferred activations. Native operations finish normally. Call on the Unity main thread.</summary>
	public void Cancel()
	{
		if (_tcs.Task.Status.IsCompleted()) return;
		Activate();
		_tcs.TrySetCanceled();
	}

	private async UniTask ObserveAsync(CancellationToken ct)
	{
		try
		{
			while (!_tcs.Task.Status.IsCompleted())
			{
				if (ct.IsCancellationRequested)
				{
					Cancel();
					return;
				}
				if (_operations!.All(op => op.isDone))
				{
					Progress = 1f;
					_tcs.TrySetResult(true);
					ProgressChanged?.Invoke(Progress);
					return;
				}
				Progress = _operations.Average(op => op.progress);
				ProgressChanged?.Invoke(Progress);
				await UniTask.Yield(PlayerLoopTiming.Update);
			}
		}
		catch (Exception ex)
		{
			Activate();
			_tcs.TrySetException(ex);
		}
	}
}
