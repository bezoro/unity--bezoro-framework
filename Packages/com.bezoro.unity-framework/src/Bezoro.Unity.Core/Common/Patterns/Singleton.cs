using System;
using System.Threading;

namespace Bezoro.Unity.Core.Common.Patterns
{
	/// <summary>
	///     Generic, thread-safe base class for non-MonoBehaviour singletons.
	/// </summary>
	/// <typeparam name="T">Concrete singleton type.</typeparam>
	public abstract class Singleton<T> : IDisposable where T : Singleton<T>
	{
		private static volatile Lazy<T> _lazy = new(
			CreateInstance,
			LazyThreadSafetyMode.ExecutionAndPublication
		);

		private readonly object _initLock = new();

		private int _isDisposed;
		private int _isInitialized;

		/// <summary>Event raised when <see cref="Dispose" /> finishes.</summary>
		public static event Action? Disposed;

		/// <summary>Event raised after <see cref="OnInitialize" /> completed.</summary>
		public static event Action? Initialized;

		~Singleton()
		{
			Dispose(
				false
			);
		}

		/// <summary>True once <see cref="Instance" /> has been fully initialised.</summary>
		public static bool IsInitialized =>
			_lazy.IsValueCreated && _lazy.Value._isInitialized == 1;

		/// <summary>Returns the singleton instance, creating it if necessary.</summary>
		public static T Instance => _lazy.Value;

		/// <summary>Returns <c>true</c> after <see cref="Dispose" /> has completed.</summary>
		public bool IsDisposed => Interlocked.CompareExchange(
									  ref _isDisposed,
									  0,
									  0
								  ) ==
								  1;

		/// <summary>Destroys the current instance (if any) and starts over.</summary>
		public static void Reset()
		{
			if (_lazy.IsValueCreated)
			{
				_lazy.Value.OnReset();
				_lazy.Value.Dispose();
			}

			_lazy = new(
				CreateInstance,
				LazyThreadSafetyMode.ExecutionAndPublication
			);
		}

		public void Dispose()
		{
			Dispose(
				true
			);

			GC.SuppressFinalize(
				this
			);
		}

		/// <summary>Ensures that the derived class gets its chance to initialise exactly once.</summary>
		public void EnsureInitialized()
		{
			if (Interlocked.CompareExchange(
					ref _isInitialized,
					0,
					0
				) ==
				1) return;

			lock (_initLock)
			{
				if (_isInitialized == 1) return;

				try
				{
					OnInitialize();
					Interlocked.Exchange(
						ref _isInitialized,
						1
					);
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException(
						$"Initialization of singleton {typeof(T)} failed.",
						ex
					);
				}
			}

			Initialized?.Invoke();
		}

		/// <summary>Dispose of managed resources here (null-out references!).</summary>
		protected abstract void DisposeManagedResources();

		/// <summary>Allocate or query expensive resources here.</summary>
		protected abstract void OnInitialize();

		/// <summary>
		///     Called from <see cref="Reset" /> just before the old instance is disposed.
		///     Use it to put the object back into its pristine state.
		/// </summary>
		protected abstract void OnReset();

		protected virtual void Dispose(bool disposing)
		{
			if (Interlocked.Exchange(
					ref _isDisposed,
					1
				) ==
				1) return;

			if (disposing)
				try
				{
					DisposeManagedResources();
				}
				finally
				{
					// clear the init flag so a new instance can be created after Reset()
					Interlocked.Exchange(
						ref _isInitialized,
						0
					);
				}

			Disposed?.Invoke();
		}

		private static T CreateInstance()
		{
			try
			{
				// Call the non-public parameter-less constructor
				var instance = (T)Activator.CreateInstance(
					typeof(T),
					true
				)!;

				instance.EnsureInitialized();
				return instance;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(
					$"Could not create instance of singleton {typeof(T)}.",
					ex
				);
			}
		}
	}
}
