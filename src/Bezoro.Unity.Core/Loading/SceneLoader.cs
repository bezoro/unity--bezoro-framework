using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bezoro.Core.Extensions;
using Bezoro.Logging.Types;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Bezoro.Logging.Logger;
using SceneManager = Bezoro.Unity.Core.Scenes.SceneManager;

namespace Bezoro.Unity.Core.Loading;

/// <summary>
///     High-level gateway for scene loading / unloading operations.
///     Everything is <b>fire-and-forget</b> – callbacks and returned handles
///     are used for progress/complete notifications.
/// </summary>
public static class SceneLoader
{
	private static readonly ConcurrentDictionary<string, AsyncOperation> Loading           = new();
	private static readonly ConcurrentDictionary<string, AsyncOperation> Unloading         = new();
	private static readonly ConcurrentDictionary<string, byte>           InternallyStarted = new();

	public static event Action<Scene>?  SceneLoadCompleted;
	public static event Action<Scene>?  SceneLoadStarted;
	public static event Action<string>? SceneUnloadCompleted;

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
	/// <param name="allowSceneActivation">If false, scenes will load but not activate until set to true.</param>
	/// <param name="ct">Optional cancellation token to cancel the loading operation.</param>
	/// <returns>An AsyncLoadHandle to track the loading progress and completion status.</returns>
	public static SceneLoadHandle Load(
		IEnumerable<string> scenePaths,
		LoadSceneMode       mode                 = LoadSceneMode.Single,
		bool                allowSceneActivation = true,
		CancellationToken   ct                   = default)
	{
		string[] paths      = scenePaths.Where(path => !string.IsNullOrEmpty(path)).ToArray();
		var      loadingOps = new List<AsyncOperation>(paths.Length);

		foreach (string path in paths)
		{
			var op = LoadSceneInternal(
				path,
				mode,
				allowSceneActivation,
				ct
			);

			if (op == null) continue;

			loadingOps.Add(op);
		}

		if (loadingOps.Count == 0)
		{
			var nullHandle = new SceneLoadHandle(null);
			return nullHandle;
		}

		var handle = new SceneLoadHandle(loadingOps);
		return handle;
	}


	/// <summary>
	///     Loads a single scene with the specified parameters.
	/// </summary>
	/// <param name="path">The path to the scene that should be loaded.</param>
	/// <param name="mode">Specifies whether to load the scene additively or with single mode (unloading current scenes).</param>
	/// <param name="allowSceneActivation">If false, the scene will load but not activate until set to true.</param>
	/// <param name="ct">Optional cancellation token to cancel the loading operation.</param>
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
		string[] paths     = scenePaths?.Where(path => !string.IsNullOrEmpty(path)).ToArray() ?? Array.Empty<string>();
		var      unloadOps = new List<AsyncOperation>();

		foreach (string path in paths)
		{
			var op = UnloadSceneInternal(
				path,
				ct
			);

			if (op is null) continue;

			unloadOps.Add(
				op
			);
		}


		if (unloadOps.Count == 0)
		{
			var nullHandle = new SceneLoadHandle(
				null
			);

			return nullHandle;
		}

		SceneLoadHandle handle = new(
			unloadOps
		);

		return handle;
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
		bool              allowSceneActivation = true,
		CancellationToken ct                   = default)
	{
		if (string.IsNullOrEmpty(path)) return null;

		// Already loading / already loaded?
		if (Loading.TryGetValue(
				path,
				out var existingOp
			) ||
			SceneManager.GetAllLoadedScenePaths().Contains(
				path
			))
		{
			Logger.LogWarning(
				$"Skip load – already loading or loaded: {path.Bold()}",
				contextObject: nameof(SceneManager),
				category:
				LogCategory.Loading
			);

			return existingOp;
		}

		var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(
			path
		);

		InternallyStarted.TryAdd(
			path,
			0
		);

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

		op.allowSceneActivation = allowSceneActivation;
		Loading[path]           = op;
		WaitForSceneLoadCompletion(
			path,
			op,
			ct
		).Forget();

		return op;
	}

	private static AsyncOperation? UnloadSceneInternal(string path, CancellationToken ct)
	{
		if (EnumerableExtensions.IsNullOrEmpty(
				path
			)) return null;

		if (Unloading.TryGetValue(
				path,
				out var existingOp
			) ||
			!SceneManager.GetAllLoadedScenePaths().Contains(
				path
			))
		{
			Logger.LogWarning(
				$"Skip unload – already unloading or unloaded: {path.Bold()}",
				contextObject: nameof(SceneManager),
				category:
				LogCategory.Loading
			);

			return existingOp;
		}

		var scene = SceneManager.GetSceneByPath(
			path
		);

		if (!scene.IsValid() || !scene.isLoaded)
			return null;


		var op = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
			path
		);

		if (op == null) return null;

		Unloading[path] = op;
		WaitForSceneUnloadCompletion(
			path,
			ct,
			op
		).Forget();

		InternallyStarted.TryAdd(
			path,
			0
		);

		return op;
	}

	private static bool WasStartedInternally(string scenePath)
	{
		if (InternallyStarted.TryRemove(scenePath, out _))
		{
			Loading.TryRemove(scenePath, out _);

			Unloading.TryRemove(scenePath, out _);

			Logger.Log(
				$"{scenePath.Bold()} was internally started.",
				contextObject: nameof(SceneLoader),
				category: LogCategory.Loading
			);

			return true;
		}

		Logger.LogWarning(
			$"{scenePath.Bold()} was NOT internally started.",
			contextObject: nameof(SceneLoader),
			category: LogCategory.Loading
		);

		return false;
	}

	private static async UniTask WaitForSceneLoadCompletion(
		string            path,
		AsyncOperation    op,
		CancellationToken ct)
	{
		var completed = false;

		try
		{
			// 2. If activation is blocked right now...
			if (!op.allowSceneActivation)
			{
				// 1. Wait (0.0 → 0.9)
				await UniTask.WaitUntil(
					op,
					static o => o.progress >= 0.9f,
					PlayerLoopTiming.Update,
					ct
				);

				Logger.Log(
					$"Scene {path} ready for activation",
					contextObject: nameof(SceneLoader),
					category:
					LogCategory.Loading
				);

				// Wait until the flag becomes true, still honoring cancellation.
				while (!op.allowSceneActivation)
				{
					ct.ThrowIfCancellationRequested();
					await UniTask.NextFrame(
						PlayerLoopTiming.Update,
						ct
					);
				}
			}

			// 3. Final step
			while (!op.isDone)
			{
				ct.ThrowIfCancellationRequested();

				await UniTask.Yield(
					PlayerLoopTiming.Update,
					ct
				);
			}

			completed = true;
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
		finally
		{
			RemoveInternalTracking(
				path
			);

			if (completed)
				InvokeSceneLoadCompleted(
					path
				);
		}
	}

	private static async UniTask WaitForSceneUnloadCompletion(string path, CancellationToken ct, AsyncOperation op)
	{
		var completed = false;

		try
		{
			await UniTask.WaitUntil(
				op,
				static o => o.isDone,
				PlayerLoopTiming.Update,
				ct
			);

			completed = true;
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
		finally
		{
			RemoveInternalTracking(
				path
			);

			if (completed)
				SceneUnloadCompleted?.Invoke(
					path
				);
		}
	}

	private static void InvokeSceneLoadCompleted(string path)
	{
		var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(
			path
		);

		SceneLoadCompleted?.Invoke(
			scene
		);
	}

	private static void RemoveInternalTracking(string path)
	{
		if (Loading.TryGetValue(
				path,
				out var loadingOp
			) &&
			loadingOp.isDone)
			Loading.TryRemove(path, out _);

		if (Unloading.TryGetValue(
				path,
				out var unloadingOp
			) &&
			unloadingOp.isDone)
			Unloading.TryRemove(path, out _);

		if (!Loading.ContainsKey(
				path
			) &&
			!Unloading.ContainsKey(
				path
			))
			InternallyStarted.TryRemove(path, out _);
	}

	/// <summary>
	///     Bridges Unity-internal scene notifications to <see cref="SceneLoader" /> so that
	///     <c>SceneLoadStarted</c> / <c>SceneLoadCompleted</c> are invoked for
	///     • the initial scene that Unity loads automatically in the Editor / Player
	///     • every subsequent load or unload that might bypass <see cref="SceneLoader" />
	///     • active-scene changes (if listeners care about them)
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

				SceneLoadCompleted?.Invoke(
					scene
				);
			}
		}

		private static void OnUnitySceneLoaded(Scene scene, LoadSceneMode _)
		{
			if (WasStartedInternally(
					scene.path
				))
				return;

			InvokeSceneLoadCompleted(
				scene.path
			);
		}

		private static void OnUnitySceneUnloaded(Scene scene)
		{
			if (WasStartedInternally(
					scene.path
				))
				return;

			SceneUnloadCompleted?.Invoke(
				scene.path
			);
		}
	}
}

/// <summary>
///     Simple handle returned from SceneLoader to poll progress.
///     Resolves itself automatically when all supplied AsyncOperations are done.
/// </summary>
public sealed class SceneLoadHandle
{
	private readonly IReadOnlyList<AsyncOperation>? _operations;
	private readonly UniTaskCompletionSource<bool>  _tcs = new();

	public event Action<float>? ProgressChanged;

	public SceneLoadHandle(IEnumerable<AsyncOperation>? operations)
	{
		_operations = operations?.ToArray();

		if (_operations == null || _operations.Count == 0)
		{
			Fail(
				"No async operations supplied."
			);

			return;
		}

		// 1. Progress polling (for UI)
		PollProgressAsync().Forget();

		// 2. Self-complete when every operation finishes
		WaitForAllAsync().Forget();
	}

	public bool IsDone            => Progress >= 1f;
	public bool IsDoneOrCancelled => IsDone || IsCancelled;
	public bool IsDoneOrFailed    => IsDone || IsFailed;
	public bool IsFailed          => Progress < 0f;

	public UniTask Task        => _tcs.Task;
	public bool    IsCancelled { get; private set; }
	public float   Progress    { get; private set; }

	// allow `await handle`
	public UniTask.Awaiter GetAwaiter() => Task.GetAwaiter();

	public void Cancel()
	{
		if (IsDone) return;

		IsCancelled = true;
		_tcs.TrySetCanceled();
	}

	private async UniTaskVoid PollProgressAsync()
	{
		if (_operations == null) return;

		while (!_tcs.Task.Status.IsCompleted())
		{
			Progress = _operations.Average(op => op.progress
			);

			ProgressChanged?.Invoke(
				Progress
			);

			await UniTask.Yield(
				PlayerLoopTiming.Update
			);
		}

		// Final update – Unity reports 0.9 until activation, so record 1.0 at the very end.
		Progress = 1f;
		ProgressChanged?.Invoke(
			Progress
		);
	}

	private async UniTaskVoid WaitForAllAsync()
	{
		try
		{
			// convert every operation into a UniTask that completes
			// when the AsyncOperation isDone flag flips
			var tasks = _operations
				.Select(op => op.ToUniTask()
				);

			await UniTask.WhenAll(
				tasks
			);

			_tcs.TrySetResult(
				true
			);
		}
		catch (Exception ex)
		{
			_tcs.TrySetException(
				ex
			);
		}
	}

	private void Fail(string reason)
	{
		Progress = -1f;
		_tcs.TrySetException(
			new InvalidOperationException(
				reason
			)
		);
	}
}
