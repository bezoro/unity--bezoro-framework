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
		protected BaseSingleton()
		{
			// Protected constructor ensures that the derived class can manage its initialization.
		}

		/// <summary>
		///     Finalizer that triggers the disposal.
		/// </summary>
		~BaseSingleton()
		{
			Dispose( false );
		}

		/// <summary>
		///     Creates an instance of the singleton using a non-public constructor.
		///     Ensures initialization of the instance.
		/// </summary>
		/// <returns>A fully initialized instance of the singleton.</returns>
		private static T CreateInstance()
		{
			var type = typeof(T);
			var constructors = type.GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic );
			try
			{
				var instance = (T)constructors[0].Invoke( null );
				instance.EnsureInitialized();
				return instance;
			}
			catch ( Exception e )
			{
				throw new InvalidOperationException( $"Failed to create instance of {type}", e );
			}
		}

		/// <summary>
		///     Ensures the singleton is initialized.
		/// </summary>
		public void EnsureInitialized()
		{
			lock (_lazyInstance)
			{
				if ( _isInitialized ) { return; }
				try
				{
					OnInitialize();
					_isInitialized = true;
					InvokeInitialized();
				}
				catch ( Exception e ) { throw new InvalidOperationException( "Initialization failed.", e ); }
			}
		}

		/// <summary>
		///     Resets the singleton instance.
		/// </summary>
		public static void ResetInstance()
		{
			if ( _lazyInstance.IsValueCreated )
			{
				var instance = _lazyInstance.Value;
				instance.OnReset();
				instance.Dispose();
			}

			_lazyInstance = new(CreateInstance, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		protected abstract void OnReset();

		/// <summary>
		///     Disposes the singleton instance, releasing managed resources.
		/// </summary>
		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		///     Implements the actual disposal logic.
		/// </summary>
		/// <param name="disposing">Indicates whether managed resources should be disposed.</param>
		protected virtual void Dispose( bool disposing )
		{
			if ( _isDisposed ) { return; }

			if ( disposing )
			{
				DisposeManagedResources();
				lock (_lazyInstance) { _isInitialized = false; }
				InvokeDisposed();
			}

			_isDisposed = true;
		}

		/// <summary>
		///     Abstract method to dispose of managed resources, to be implemented by derived classes.
		///     Ensure all references to managed resources are set to null.
		/// </summary>
		protected abstract void DisposeManagedResources();

		/// <summary>
		///     Abstract method to initialize resources, to be implemented by derived classes.
		/// </summary>
		protected abstract void OnInitialize();

		/// <summary>
		///     Safely invoke the Initialized event, handling exceptions.
		/// </summary>
		private void InvokeInitialized()
		{
			var handlers = Initialized;
			if ( handlers == null ) { return; }

			foreach ( var handler in handlers.GetInvocationList() )
			{
				try { ( (Action)handler )?.Invoke(); }
				catch ( Exception e )
				{
					// Optional: Log the exception
				}
			}
		}

		/// <summary>
		///     Safely invoke the Disposed event, handling exceptions.
		/// </summary>
		private void InvokeDisposed()
		{
			var handlers = Disposed;
			if ( handlers == null ) { return; }

			foreach ( var handler in handlers.GetInvocationList() )
			{
				try { ( (Action)handler )?.Invoke(); }
				catch ( Exception e )
				{
					// Optional: Log the exception
				}
			}
		}
	}
}
