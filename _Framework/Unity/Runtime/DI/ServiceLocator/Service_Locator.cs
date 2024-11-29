using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Bezoro.Framework.Core;
using Bezoro.Framework.Unity.Utilities;

namespace Bezoro.Framework.Unity.DI
{
	public sealed class Service_Locator : IServiceLocator
	{
		private readonly ConcurrentDictionary<Type, IService>       _singletonServices;
		private readonly ConcurrentDictionary<Type, Lazy<IService>> _lazySingletonServices;
		private readonly ConcurrentDictionary<Type, Func<IService>> _transientServices;
		private readonly ConcurrentDictionary<Type, object>         _scopedServices;
		private readonly HashSet<IDisposable>                       _disposableServices;

		public Service_Locator()
		{
			_singletonServices     = new();
			_lazySingletonServices = new();
			_transientServices     = new();
			_scopedServices        = new();
			_disposableServices    = new();
		}

		~Service_Locator()
		{
			Dispose();
		}

		public IServiceLocator RegisterService<TInterface, TImplementation>(
			Func<TImplementation> factory,
			ServiceLifespan       lifespan,
			bool                  replace = false
		)
		where TInterface : IService
		where TImplementation : class, TInterface
		{
			Assert.IsNotNull( factory, nameof(factory) );
			Assert.IsFalse( lifespan == ServiceLifespan.None, nameof(lifespan) );

			var interface_type = typeof(TInterface);
			if ( IsServiceRegistered( interface_type ) && !replace )
			{
				BDebug.Log_Warning( $"Service {interface_type.Name} is already registered." );
				return this;
			}

			switch ( lifespan )
			{
				case ServiceLifespan.Singleton: RegisterSingletonService<TInterface, TImplementation>( factory ); break;

				case ServiceLifespan.LazySingleton:
					RegisterLazySingletonService<TInterface, TImplementation>( factory ); break;

				case ServiceLifespan.Transient: RegisterTransientService<TInterface, TImplementation>( factory ); break;

				case ServiceLifespan.Scoped: RegisterScopedService<TInterface, TImplementation>( factory ); break;
			}

			return this;
		}

		public T GetService<T>() where T : IService
		{
			var serviceType = typeof(T);

			if ( _singletonServices.TryGetValue( serviceType, out var singleton ) )
			{
				return (T)singleton;
			}

			if ( _lazySingletonServices.TryGetValue( serviceType, out var lazy ) )
			{
				return (T)lazy.Value;
			}

			if ( _transientServices.TryGetValue( serviceType, out var transient ) )
			{
				return (T)transient();
			}

			if ( _scopedServices.TryGetValue( serviceType, out var scoped ) )
			{
				return (T)scoped;
			}

			throw new ServiceNotFoundException( serviceType );
		}

		public bool IsServiceRegistered<T>() where T : IService
		{
			return IsServiceRegistered( typeof(T) );
		}

		public void UnregisterService<T>() where T : IService
		{
			var serviceType = typeof(T);

			if ( _singletonServices.TryRemove( serviceType, out var singleton ) )
			{
				RemoveDisposableService( singleton );
			}

			if ( _lazySingletonServices.TryRemove( serviceType, out var lazy ) )
			{
				RemoveDisposableService( lazy.Value );
			}

			_transientServices.TryRemove( serviceType, out _ );

			if ( _scopedServices.TryRemove( serviceType, out var scoped ) )
			{
				RemoveDisposableService( scoped );
			}
		}

		public void Dispose()
		{
			foreach ( var service in _singletonServices.Values )
			{
				RemoveDisposableService( service );
			}

			foreach ( var lazy in _lazySingletonServices.Values )
			{
				RemoveDisposableService( lazy.Value );
			}

			foreach ( var service in _scopedServices.Values )
			{
				RemoveDisposableService( service );
			}

			_disposableServices.Clear();
			_singletonServices.Clear();
			_lazySingletonServices.Clear();
			_transientServices.Clear();
			_scopedServices.Clear();
		}

		private void RegisterSingletonService<TInterface, TImplementation>( Func<TImplementation> factory )
		where TInterface : IService
		where TImplementation : class, TInterface
		{
			var instance = factory();
			_singletonServices[typeof(TInterface)] = instance;
			AddDisposableService( instance );
		}

		private void RegisterLazySingletonService<TInterface, TImplementation>( Func<TImplementation> factory )
		where TInterface : IService
		where TImplementation : class, TInterface
		{
			var lazy = new Lazy<IService>
			(
				() =>
				{
					var instance = factory();
					AddDisposableService( instance );

					return instance;
				}
			);

			_lazySingletonServices[typeof(TInterface)] = lazy;
		}

		private void RegisterTransientService<TInterface, TImplementation>( Func<TImplementation> factory )
		where TInterface : IService
		where TImplementation : class, TInterface
		{
			_transientServices[typeof(TInterface)] = () =>
			{
				var instance = factory();
				AddDisposableService( instance );

				return instance;
			};
		}

		private void RegisterScopedService<TInterface, TImplementation>( Func<TImplementation> factory )
		where TInterface : IService
		where TImplementation : class, TInterface
		{
			var instance = factory();
			_scopedServices[typeof(TInterface)] = instance;
			AddDisposableService( instance );
		}

		private bool IsServiceRegistered( Type serviceType )
		{
			return _singletonServices.ContainsKey
						 ( serviceType )
				 || _lazySingletonServices.ContainsKey( serviceType )
				 || _transientServices.ContainsKey( serviceType )
				 || _scopedServices.ContainsKey( serviceType );
		}

		private void AddDisposableService( object service )
		{
			if ( service is IDisposable disposable )
			{
				lock (_disposableServices)
				{
					_disposableServices.Add( disposable );
				}
			}
		}

		private void RemoveDisposableService( object service )
		{
			if ( service is IDisposable disposable )
			{
				lock (_disposableServices)
				{
					_disposableServices.Remove( disposable );
				}
			}
		}

		private static void ValidateLifespan( ServiceLifespan lifespan )
		{
			if ( lifespan == ServiceLifespan.None )
			{
				throw new ArgumentException( "ServiceLifespan cannot be None", nameof(lifespan) );
			}
		}
	}

	public enum ServiceLifespan
	{
		None          = -1,
		Transient     = 0,
		Singleton     = 1,
		LazySingleton = 2,
		Scoped        = 3
	}
}
