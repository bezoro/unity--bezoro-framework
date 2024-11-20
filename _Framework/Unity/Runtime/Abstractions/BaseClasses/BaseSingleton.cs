using System;
using System.Reflection;
using System.Threading;

namespace Bezoro.Framework.Unity
{
	/// <summary>
	///     Base class for creating a singleton instance with thread-safe lazy initialization, proper disposal, and lifecycle
	///     management.
	/// </summary>
	/// <typeparam name="T">Type of the singleton class.</typeparam>
	public abstract class BaseSingleton<T> :
		IDisposable
	where T : BaseSingleton<T>
	{
		private static Lazy<T> _lazyInstance = new(CreateInstance, LazyThreadSafetyMode.ExecutionAndPublication);
		private volatile bool _isDisposed;
		private volatile bool _isInitialized;

		public static T Instance => _lazyInstance.Value;
		public static bool IsInitialized => _lazyInstance.IsValueCreated && _lazyInstance.Value._isInitialized;
		public bool IsDisposed => _isDisposed;

		public static event Action Initialized;
		public static event Action Disposed;

		/// <summary>
		///     Protected constructor to prevent direct instantiation.
		/// </summary>
		protected BaseSingleton() {}

		/// <summary>
		///     Creates an instance of the singleton.
		/// </summary>
		private static T CreateInstance()
		{
			var type = typeof(T);
			var constructors = type.GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic );

			var instance = (T)constructors[0].Invoke( null );
			instance.EnsureInitialized();
			return instance;
		}

		/// <summary>
		///     Ensures the singleton is initialized.
		/// </summary>
		public void EnsureInitialized()
		{
			lock (_lazyInstance)
			{
				if ( _isInitialized ) { return; }
				OnInitialize();
				_isInitialized = true;
				Initialized?.Invoke();
			}
		}

		/// <summary>
		///     Resets the singleton instance.
		/// </summary>
		public static void ResetInstance()
		{
			if ( _lazyInstance.IsValueCreated )
			{
				_lazyInstance.Value.OnReset();
				_lazyInstance.Value.Dispose();
			}

			_lazyInstance = new(CreateInstance, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		protected abstract void OnReset();

		/// <summary>
		///     Disposes the singleton instance, releasing managed resources.
		/// </summary>
		public void Dispose()
		{
			if ( _isDisposed ) { return; }
			if ( !_lazyInstance.IsValueCreated ) { return; }
			lock (this)
			{
				if ( _isDisposed ) { return; }
				DisposeManagedResources();
				_isDisposed    = true;
				_isInitialized = false;
				Disposed?.Invoke();
				GC.SuppressFinalize( this );
			}
		}

		~BaseSingleton()
		{
			Dispose();
		}

		/// <summary>
		///     Abstract method to dispose managed resources, to be implemented by derived classes.
		/// </summary>
		protected abstract void DisposeManagedResources();

		/// <summary>
		///     Abstract method to initialize resources, to be implemented by derived classes.
		/// </summary>
		protected abstract void OnInitialize();
	}
}
