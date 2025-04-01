using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bezoro.Framework.Unity
{
	public static class Player_Loop_Helpers
	{
		private static SynchronizationContext _Main_Thread_Context;

		/// <summary>
		///     Initializes the main thread context.
		/// </summary>
		/// <remarks>
		///     This method should be called in the Awake method of a MonoBehaviour.
		/// </remarks>
		public static void Initialize_Main_Thread_Context()
		{
			_Main_Thread_Context = SynchronizationContext.Current;

			if (_Main_Thread_Context == null)
			{
				Logger.Log_Exception(new InvalidOperationException("SynchronizationContext is null."));
			}
		}

		/// <summary>
		///     Ensures that the current thread is the main thread.
		///     If not, switches execution back to the main thread.
		/// </summary>
		public static void Return_To_Main_Thread()
		{
			// if (!Is_On_Main_Thread()) return;

			UniTask.ReturnToMainThread();
			Logger.Log("Returned to main thread");
		}

		/// <summary>
		///     Returns the current managed thread ID and logs it.
		///     This can be useful for debugging and verifying which thread is executing the code.
		/// </summary>
		/// <returns>The managed thread ID of the current thread.</returns>
		public static int Get_Current_Managed_Thread_Id()
		{
			var thread_id = Thread.CurrentThread.ManagedThreadId;
			Logger.Log($"Current Managed Thread ID: {thread_id}");
			return thread_id;
		}

		/// <summary>
		///     Schedules an action to run on the main thread.
		/// </summary>
		/// <param name="action">The action to execute.</param>
		/// <returns>An awaitable task.</returns>
		public static async UniTask Run_On_Main_Thread(Action action)
		{
			if (!Is_On_Main_Thread())
				Return_To_Main_Thread();

			action();
			await UniTask.Yield();
		}

		/// <summary>
		/// Posts an action to be executed on the main thread.
		/// </summary>
		/// <param name="action">The action to execute.</param>
		public static void Return_To_Main_Thread(Action action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			var context = Get_Main_Thread_Context();
			context.Post(_ => action.Invoke(), null);
		}

		/// <summary>
		///     Checks if the current thread is the game's main thread.
		/// </summary>
		/// <returns>True if the current thread is the main thread; otherwise, false.</returns>
		public static bool Is_On_Main_Thread()
		{
			return PlayerLoopHelper.IsMainThread;
		}

		/// <summary>
		///     Executes a function on the main thread and retrieves the result.
		/// </summary>
		/// <typeparam name="T">The type of the result.</typeparam>
		/// <param name="func">The function to execute.</param>
		/// <returns>The result of the function.</returns>
		public static async UniTask<T> Run_On_Main_Thread<T>(Func<T> func)
		{
			if (!Is_On_Main_Thread())
				Return_To_Main_Thread();

			var result = func();
			await UniTask.Yield();
			return result;
		}

		/// <summary>
		///     Waits for a specified number of frames to pass.
		/// </summary>
		/// <param name="frameCount">The number of frames to wait.</param>
		/// <returns>An awaitable task.</returns>
		public static async UniTask Wait_For_Frames(int frameCount)
		{
			for (var i = 0 ; i < frameCount ; i++)
				await UniTask.WaitForEndOfFrame();
		}

		/// <summary>
		///     Waits for the specified amount of time to elapse.
		/// </summary>
		/// <param name="timeInSeconds">The time to wait, in seconds.</param>
		/// <returns>An awaitable task.</returns>
		public static async UniTask Wait_For_Time(float timeInSeconds)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(timeInSeconds));
		}

		/// <summary>
		///     Logs the current thread's information including its ID and name.
		/// </summary>
		public static void Log_Current_Thread_Info()
		{
			var thread_id   = Thread.CurrentThread.ManagedThreadId;
			var thread_name = Thread.CurrentThread.Name;
			Logger.Log($"Thread ID: {thread_id}, Thread Name: {thread_name ?? "Unnamed"}");
		}

		/// <summary>
		///     Ensures that an operation is executed only on the main thread.
		///     Throws an exception if called from a non-main thread.
		/// </summary>
		[Conditional("UNITY_EDITOR")]
		public static void Ensure_On_Main_Thread()
		{
			if (!Is_On_Main_Thread())
			{
				Logger.Log_Exception(
					new InvalidOperationException("This operation must be performed on the main thread."));
			}
		}

		/// <summary>
		///     Runs an async task on a background thread.
		/// </summary>
		/// <param name="action">The action to run on a background thread.</param>
		/// <returns>An awaitable task.</returns>
		public static async UniTask Run_On_Background_Thread(Action action)
		{
			await UniTask.RunOnThreadPool(action);
		}

		/// <summary>
		///     Runs a function on a thread pool thread and returns its result.
		/// </summary>
		/// <typeparam name="T">The type of the result.</typeparam>
		/// <param name="func">The function to run on a thread pool thread.</param>
		/// <returns>An awaitable task containing the result of the function.</returns>
		public static async UniTask<T> Run_On_Background_Thread<T>(Func<T> func)
		{
			T result = default;

			await UniTask.RunOnThreadPool(() => result = func());

			return result;
		}

		/// <summary>
		///     Retrieves the synchronization context of the main thread.
		/// </summary>
		/// <returns>The synchronization context for the main thread.</returns>
		public static SynchronizationContext Get_Main_Thread_Context()
		{
			if (_Main_Thread_Context == null)
				Initialize_Main_Thread_Context();

			return _Main_Thread_Context;
		}
	}
}
