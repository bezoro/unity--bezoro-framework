using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bezoro.Framework.Unity.Utilities.Helpers;

namespace Bezoro.Framework.Unity
{
	public abstract class BaseSingleton<T> : IDisposable
	where T : BaseSingleton<T>
	{
		private static Lazy<T> _lazyInstance = new(
			CreateInstance,
			LazyThreadSafetyMode.ExecutionAndPublication
		);

		private volatile bool _isDisposed;
		private volatile bool _isInitialized;

		public static T Instance => _lazyInstance.Value;
		public static bool IsInitialized => _lazyInstance.IsValueCreated && _lazyInstance.Value._isInitialized;
		public bool IsDisposed => _isDisposed;

		protected BaseSingleton()
		{
			ValidationHelpers.ValidateConditionIsFalse(
				_lazyInstance.IsValueCreated && _lazyInstance.Value != this,
				"Instance already created."
			);
		}

		private static T CreateInstance()
		{
			var type = typeof(T);
			var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

			ValidationHelpers.ValidateConditionIsFalse(
				constructors.Length == 0,
				$"No non-public instance constructor found for type {type.Name}. Ensure the class has a non-public constructor."
			);

			return (T)constructors[0].Invoke(null);
		}

		public static void EnsureInitialized()
		{
			if (!Instance._isInitialized)
			{
				lock (Instance)
				{
					if (!Instance._isInitialized)
					{
						Instance.Initialize();
						Instance._isInitialized = true;
					}
				}
			}
		}

		public static async Task EnsureInitializedAsync()
		{
			if (!Instance._isInitialized)
			{
				await Task.Run(
					() =>
					{
						lock (Instance)
						{
							if (!Instance._isInitialized)
							{
								Instance.Initialize();
								Instance._isInitialized = true;
							}
						}
					}
				);
			}
		}

		public static void ResetInstance()
		{
			if (_lazyInstance.IsValueCreated)
			{
				_lazyInstance.Value.OnReset();
				_lazyInstance.Value.Dispose();
			}

			_lazyInstance = new Lazy<T>(
				CreateInstance,
				LazyThreadSafetyMode.ExecutionAndPublication
			);
		}

		protected virtual void OnReset() {}

		public void Dispose()
		{
			if (_isDisposed || !_lazyInstance.IsValueCreated)
			{
				return;
			}

			lock (this)
			{
				if (_isDisposed)
				{
					return;
				}

				DisposeManagedResources();
				_isDisposed = true;
				_isInitialized = false;
				GC.SuppressFinalize(this);
			}
		}

		~BaseSingleton()
		{
			Dispose();
		}

		protected abstract void DisposeManagedResources();
		protected abstract void Initialize();

#if UNITY_EDITOR
		private void ValidateInitializationState()
		{
			ValidationHelpers.ValidateConditionIsFalse(
				_isDisposed,
				$"Attempting to use disposed singleton of type {typeof(T).Name}"
			);
			ValidationHelpers.ValidateCondition(
				_isInitialized,
				$"Singleton of type {typeof(T).Name} not initialized. Call EnsureInitialized first."
			);
		}
#endif
	}
}
