using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Bezoro.Logging;

namespace Bezoro.Unity.Core.DI;

/// <summary>
///     Thread-safe implementation of the Bezoro service locator. It supports
///     singleton, lazy singleton, scoped, and transient lifetimes and tracks
///     disposable resolved instances for cleanup.
/// </summary>
public sealed class ServiceLocator : IServiceLocator
{
	private readonly ConcurrentDictionary<Type, Func<IService>> _transientServices;
	private readonly ConcurrentDictionary<Type, IService>       _singletonServices;
	private readonly ConcurrentDictionary<Type, Lazy<IService>> _lazySingletonServices;
	private readonly ConcurrentDictionary<Type, object>         _scopedServices;

	private readonly HashSet<IDisposable> _disposableServices;

	/// <summary>
	///     Creates an empty locator with separate stores for each supported lifetime.
	/// </summary>
	public ServiceLocator()
	{
		_singletonServices     = new();
		_lazySingletonServices = new();
		_transientServices     = new();
		_scopedServices        = new();
		_disposableServices    = new();
	}

	~ServiceLocator()
	{
		Dispose();
	}

	/// <summary>
	///     Disposes tracked service instances and clears all registrations.
	/// </summary>
	public void Dispose()
	{
		IDisposable[] disposableServices;

		lock (_disposableServices)
		{
			disposableServices = new IDisposable[_disposableServices.Count];
			_disposableServices.CopyTo(
				disposableServices
			);

			_disposableServices.Clear();
		}

		foreach (var service in disposableServices) service.Dispose();

		_singletonServices.Clear();
		_lazySingletonServices.Clear();
		_transientServices.Clear();
		_scopedServices.Clear();
	}

	private static void ValidateLifespan(ServiceLifespan lifespan)
	{
		if (lifespan == ServiceLifespan.None)
			throw new ArgumentException(
				"ServiceLifespan cannot be None",
				nameof(lifespan)
			);
	}

	private bool IsServiceRegistered(Type serviceType) =>
		_singletonServices.ContainsKey
		(
			serviceType
		) ||
		_lazySingletonServices.ContainsKey(
			serviceType
		) ||
		_transientServices.ContainsKey(
			serviceType
		) ||
		_scopedServices.ContainsKey(
			serviceType
		);

	private void AddDisposableService(object service)
	{
		if (service is IDisposable disposable)
			lock (_disposableServices)
			{
				_disposableServices.Add(
					disposable
				);
			}
	}

	private void DisposeRegisteredLazyService(Lazy<IService> lazy)
	{
		if (!lazy.IsValueCreated)
			return;

		DisposeRegisteredService(
			lazy.Value
		);
	}

	private void DisposeRegisteredService(object service)
	{
		if (service is IDisposable disposable)
		{
			var shouldDispose = false;

			lock (_disposableServices)
			{
				shouldDispose = _disposableServices.Remove(
					disposable
				);
			}

			if (shouldDispose)
				disposable.Dispose();
		}
	}

	private void RegisterLazySingletonService<TInterface, TImplementation>(Func<TImplementation> factory)
		where TInterface : IService
		where TImplementation : class, TInterface
	{
		var lazy = new Lazy<IService>
		(() =>
			{
				var instance = factory();
				AddDisposableService(
					instance
				);

				return instance;
			}
		);

		_lazySingletonServices[typeof(TInterface)] = lazy;
	}

	private void RegisterScopedService<TInterface, TImplementation>(Func<TImplementation> factory)
		where TInterface : IService
		where TImplementation : class, TInterface
	{
		var instance = factory();
		_scopedServices[typeof(TInterface)] = instance;
		AddDisposableService(
			instance
		);
	}

	private void RegisterSingletonService<TInterface, TImplementation>(Func<TImplementation> factory)
		where TInterface : IService
		where TImplementation : class, TInterface
	{
		var instance = factory();
		_singletonServices[typeof(TInterface)] = instance;
		AddDisposableService(
			instance
		);
	}

	private void RegisterTransientService<TInterface, TImplementation>(Func<TImplementation> factory)
		where TInterface : IService
		where TImplementation : class, TInterface =>
		_transientServices[typeof(TInterface)] = () =>
		{
			var instance = factory();
			AddDisposableService(
				instance
			);

			return instance;
		};

	private void Unregister(Type serviceType)
	{
		if (_singletonServices.TryRemove(
				serviceType,
				out var singleton
			))
			DisposeRegisteredService(
				singleton
			);

		if (_lazySingletonServices.TryRemove(
				serviceType,
				out var lazy
			))
			DisposeRegisteredLazyService(
				lazy
			);

		_transientServices.TryRemove(
			serviceType,
			out _
		);

		if (_scopedServices.TryRemove(
				serviceType,
				out object scoped
			))
			DisposeRegisteredService(
				scoped
			);
	}

	#region Interface Implementations

	/// <inheritdoc />
	public IServiceLocator Register<TInterface, TImplementation>(
		Func<TImplementation> factory,
		ServiceLifespan       lifespan,
		bool                  replace = false
	)
		where TInterface : IService
		where TImplementation : class, TInterface
	{
		if (factory == null)
			throw new ArgumentNullException(nameof(factory));

		ValidateLifespan(lifespan);

		var interfaceType = typeof(TInterface);

		if (IsServiceRegistered(
				interfaceType
			) &&
			!replace)
		{
			Logger.Log(
				$"Service {interfaceType.Name} is already registered."
			);

			return this;
		}

		if (replace)
			Unregister(
				interfaceType
			);

		switch (lifespan)
		{
			case ServiceLifespan.Singleton:
				RegisterSingletonService<TInterface, TImplementation>(
					factory
				);

				break;

			case ServiceLifespan.LazySingleton:
				RegisterLazySingletonService<TInterface, TImplementation>(
					factory
				);

				break;

			case ServiceLifespan.Transient:
				RegisterTransientService<TInterface, TImplementation>(
					factory
				);

				break;

			case ServiceLifespan.Scoped:
				RegisterScopedService<TInterface, TImplementation>(
					factory
				);

				break;
		}

		return this;
	}

	/// <summary>
	///     Resolves a registered service. Transient registrations create a new
	///     instance on each call; lazy singletons are created on first resolution.
	/// </summary>
	/// <exception cref="ServiceNotFoundException">Thrown when no registration exists for <typeparamref name="T" />.</exception>
	public T GetService<T>()
		where T : IService
	{
		var serviceType = typeof(T);

		if (_singletonServices.TryGetValue(
				serviceType,
				out var singleton
			))
			return (T)singleton;

		if (_lazySingletonServices.TryGetValue(
				serviceType,
				out var lazy
			))
			return (T)lazy.Value;

		if (_transientServices.TryGetValue(
				serviceType,
				out var transient
			))
			return (T)transient();

		if (_scopedServices.TryGetValue(
				serviceType,
				out object scoped
			))
			return (T)scoped;

		throw new ServiceNotFoundException(
			serviceType
		);
	}

	/// <inheritdoc />
	public bool IsServiceRegistered<T>()
		where T : IService =>
		IsServiceRegistered(
			typeof(T)
		);

	/// <summary>
	///     Removes the registration for a service interface from every lifetime
	///     store and stops tracking its disposable instance, if one exists.
	/// </summary>
	public void Unregister<T>()
		where T : IService
	{
		Unregister(
			typeof(T)
		);
	}

	#endregion
}

/// <summary>
///     Lifetime policy used when registering a service factory.
/// </summary>
public enum ServiceLifespan
{
	/// <summary>
	///     Sentinel value used to reject uninitialized lifetime input.
	/// </summary>
	None = -1,

	/// <summary>
	///     Create a new service instance every time it is resolved.
	/// </summary>
	Transient = 0,

	/// <summary>
	///     Create the service immediately during registration and reuse it.
	/// </summary>
	Singleton = 1,

	/// <summary>
	///     Defer service creation until the first resolution, then reuse it.
	/// </summary>
	LazySingleton = 2,

	/// <summary>
	///     Create one service instance for this locator scope and reuse it.
	/// </summary>
	Scoped = 3
}
