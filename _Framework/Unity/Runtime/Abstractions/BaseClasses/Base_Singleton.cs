using System;
using System.Reflection;
using System.Threading;
using Bezoro.Framework.Unity.Utilities;

namespace Bezoro.Framework.Unity
{
	/// <summary>
	///     Base class for creating a singleton instance with thread-safe lazy initialization, proper disposal, and lifecycle
	///     management.
	/// </summary>
	/// <typeparam name="T">Type of the singleton class.</typeparam>
	public abstract class Base_Singleton<T> :
		IDisposable
	where T : Base_Singleton<T>
	{
		/// <summary>
		///     Protected constructor to prevent direct instantiation.
		/// </summary>
		protected Base_Singleton()
		{
			// Protected constructor ensures that the derived class can manage its initialization.
		}

		/// <summary>
		///     Finalizer that triggers the disposal.
		/// </summary>
		~Base_Singleton()
		{
			Dispose(false);
		}

		private static Lazy<T>     _LazyInstance = new( Create_Instance, LazyThreadSafetyMode.ExecutionAndPublication );
		public static event Action Disposed;

		public static event Action Initialized;

		private volatile bool _isDisposed;
		private volatile bool _isInitialized;

		public static T    Instance      => _LazyInstance.Value;
		public        bool IsDisposed    => _isDisposed;
		public static bool IsInitialized => _LazyInstance.IsValueCreated && _LazyInstance.Value._isInitialized;

		/// <summary>
		///     Disposes the singleton instance, releasing managed resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///     Ensures the singleton is initialized.
		/// </summary>
		public void Ensure_Initialized()
		{
			lock ( _LazyInstance )
			{
				if (_isInitialized) { return; }

				try
				{
					On_Initialize();
					_isInitialized = true;
					Invoke_Initialized();
				}
				catch ( Exception e )
				{
					throw new InvalidOperationException("Initialization failed.", e);
				}
			}
		}

		/// <summary>
		///     Resets the singleton instance.
		/// </summary>
		public static void Reset_Instance()
		{
			if (_LazyInstance.IsValueCreated)
			{
				var instance = _LazyInstance.Value;
				instance.On_Reset();
				instance.Dispose();
			}

			_LazyInstance = new( Create_Instance, LazyThreadSafetyMode.ExecutionAndPublication );
		}

		/// <summary>
		///     Creates an instance of the singleton using a non-public constructor.
		///     Ensures initialization of the instance.
		/// </summary>
		/// <returns>A fully initialized instance of the singleton.</returns>
		private static T Create_Instance()
		{
			var type         = typeof(T);
			var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

			try
			{
				var instance = (T)constructors[0].Invoke(null);
				instance.Ensure_Initialized();
				return instance;
			}
			catch ( Exception e )
			{
				throw new InvalidOperationException($"Failed to create instance of {type}", e);
			}
		}

		/// <summary>
		///     Safely invoke the Disposed event, handling exceptions.
		/// </summary>
		private void Invoke_Disposed()
		{
			var handlers = Disposed;

			if (handlers == null) { return; }

			foreach ( var handler in handlers.GetInvocationList() )
			{
				try { ( (Action)handler )?.Invoke(); }
				catch ( Exception e ) { BDebug.Log_Exception(e); }
			}
		}

		/// <summary>
		///     Safely invoke the Initialized event, handling exceptions.
		/// </summary>
		private void Invoke_Initialized()
		{
			var handlers = Initialized;

			if (handlers == null) { return; }

			foreach ( var handler in handlers.GetInvocationList() )
			{
				try { ( (Action)handler )?.Invoke(); }
				catch ( Exception e ) { BDebug.Log_Exception(e); }
			}
		}

		protected abstract void On_Reset();

		/// <summary>
		///     Implements the actual disposal logic.
		/// </summary>
		/// <param name="disposing">Indicates whether managed resources should be disposed.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed) { return; }

			if (disposing)
			{
				Dispose_Managed_Resources();

				lock ( _LazyInstance )
				{
					_isInitialized = false;
				}

				Invoke_Disposed();
			}

			_isDisposed = true;
		}

		/// <summary>
		///     Abstract method to dispose of managed resources, to be implemented by derived classes.
		///     Ensure all references to managed resources are set to null.
		/// </summary>
		protected abstract void Dispose_Managed_Resources();

		/// <summary>
		///     Abstract method to initialize resources, to be implemented by derived classes.
		/// </summary>
		protected abstract void On_Initialize();
	}
}
