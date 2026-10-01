using System;
using System.Diagnostics;
using System.Threading;
using Bezoro.Core.Extensions;
using Bezoro.Logging;
using Cysharp.Threading.Tasks;

namespace Bezoro.Unity.Core.Common.Timing;

/// <summary>
///     Resolves and modifies Unity player loop systems.
/// </summary>
public static class PlayerLoopHelpers
{
	private static SynchronizationContext? _mainThreadContext;

	/// <summary>
	///     Checks if the current thread is the game's main thread.
	/// </summary>
	/// <returns>True if the current thread is the main thread; otherwise, false.</returns>
	public static bool IsOnMainThread() => PlayerLoopHelper.IsMainThread;

	/// <summary>
	///     Returns the current managed thread ID and logs it.
	///     This can be useful for debugging and verifying which thread is executing the code.
	/// </summary>
	/// <returns>The managed thread ID of the current thread.</returns>
	public static int Get_Current_Managed_Thread_Id()
	{
		int threadID = Thread.CurrentThread.ManagedThreadId;
		Logger.Log($"Current Managed Thread ID: {threadID}");
		return threadID;
	}

	/// <summary>
	///     Retrieves the synchronization context of the main thread.
	/// </summary>
	/// <returns>The synchronization context for the main thread.</returns>
	public static SynchronizationContext Get_Main_Thread_Context()
	{
		if (_mainThreadContext == null)
			Initialize_Main_Thread_Context();

		return _mainThreadContext!;
	}

	/// <summary>
	///     Runs an async task on a background thread.
	/// </summary>
	/// <param name="action">The action to run on a background thread.</param>
	/// <returns>An awaitable task.</returns>
	public static async UniTask Run_On_Background_Thread(Action action) =>
		await UniTask.RunOnThreadPool(action);

	/// <summary>
	///     Schedules an action to run on the main thread.
	/// </summary>
	/// <param name="action">The action to execute.</param>
	/// <returns>An awaitable task.</returns>
	public static async UniTask Run_On_Main_Thread(Action action)
	{
		if (action == null) throw new ArgumentNullException(nameof(action));

		if (!IsOnMainThread())
			await UniTask.SwitchToMainThread();

		action();
		await UniTask.Yield();
	}

	/// <summary>
	///     Waits for a specified number of frames to pass.
	/// </summary>
	/// <param name="frameCount">The number of frames to wait.</param>
	/// <returns>An awaitable task.</returns>
	public static async UniTask Wait_For_Frames(int frameCount)
	{
		for (var i = 0; i < frameCount; i++)
			await UniTask.WaitForEndOfFrame();
	}

	/// <summary>
	///     Waits for the specified amount of time to elapse.
	/// </summary>
	/// <param name="timeInSeconds">The time to wait, in seconds.</param>
	/// <returns>An awaitable task.</returns>
	public static async UniTask Wait_For_Time(float timeInSeconds) =>
		await UniTask.Delay(TimeSpan.FromSeconds(timeInSeconds));

	/// <summary>
	///     Runs a function on a thread pool thread and returns its result.
	/// </summary>
	/// <typeparam name="T">The type of the result.</typeparam>
	/// <param name="func">The function to run on a thread pool thread.</param>
	/// <returns>An awaitable task containing the result of the function.</returns>
	public static async UniTask<T> Run_On_Background_Thread<T>(Func<T> func)
	{
		T? result = default;
		await UniTask.RunOnThreadPool(() => result = func());
		return result!;
	}

	/// <summary>
	///     Executes a function on the main thread and retrieves the result.
	/// </summary>
	/// <typeparam name="T">The type of the result.</typeparam>
	/// <param name="func">The function to execute.</param>
	/// <returns>The result of the function.</returns>
	public static async UniTask<T> Run_On_Main_Thread<T>(Func<T> func)
	{
		if (func == null) throw new ArgumentNullException(nameof(func));

		if (!IsOnMainThread())
			await UniTask.SwitchToMainThread();

		var result = func();
		await UniTask.Yield();
		return result;
	}

	/// <summary>
	///     Ensures that an operation is executed only on the main thread.
	///     Throws an exception if called from a non-main thread.
	/// </summary>
	[Conditional("UNITY_EDITOR")]
	public static void Ensure_On_Main_Thread()
	{
		if (!IsOnMainThread())
			Logger.LogException(new InvalidOperationException("This operation must be performed on the main thread."));
	}

	/// <summary>
	///     Initializes the main thread context.
	/// </summary>
	/// <remarks>
	///     This method should be called in the Awake method of a MonoBehaviour.
	/// </remarks>
	public static void Initialize_Main_Thread_Context()
	{
		_mainThreadContext = SynchronizationContext.Current;

		if (_mainThreadContext == null)
			Logger.LogException(new InvalidOperationException("SynchronizationContext is null."));
	}

	/// <summary>
	///     Logs the current thread's information including its ID and name.
	/// </summary>
	public static void Log_Current_Thread_Info()
	{
		int     threadID   = Thread.CurrentThread.ManagedThreadId;
		string? threadName = Thread.CurrentThread.Name;
		Logger.Log($"Thread ID: {threadID}, Thread Name: {threadName ?? "Unnamed"}");
	}

	/// <summary>
	///     Ensures that the current thread is the main thread.
	///     If not, switches execution back to the main thread.
	/// </summary>
	public static void ReturnToMainThread()
	{
		UniTask.ReturnToMainThread();
		Logger.Log("Returned to main thread");
	}

	/// <summary>
	///     Posts an action to be executed on the main thread.
	/// </summary>
	/// <param name="action">The action to execute.</param>
	public static void ReturnToMainThread(Action action)
	{
		if (action.IsNull()) throw new ArgumentNullException(nameof(action));

		var context = Get_Main_Thread_Context();
		context.Post(_ => action.Invoke(), null);
	}
}
