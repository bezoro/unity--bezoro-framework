using System;
using System.Collections.Generic;

namespace Bezoro.Unity.Core.DI;

/// <summary>Thread-safe service registry. Its owner explicitly disposes resolved services at the end of its lifetime.</summary>
public sealed class ServiceLocator : IServiceLocator
{
	private sealed class Registration
	{
		internal Func<IService> Resolve = null!;
		internal IDisposable? CachedDisposable;
	}

	private readonly object _gate = new();
	private readonly Dictionary<Type, Registration> _registrations = new();
	private readonly HashSet<IDisposable> _disposables = new();
	private bool _disposed;

	public void Dispose()
	{
		IDisposable[] services;
		lock (_gate)
		{
			if (_disposed) return;
			_disposed = true;
			_registrations.Clear();
			services = new IDisposable[_disposables.Count];
			_disposables.CopyTo(services);
			_disposables.Clear();
		}

		List<Exception>? errors = null;
		foreach (var service in services)
		{
			try { service.Dispose(); }
			catch (Exception ex) { (errors ??= new()).Add(ex); }
		}
		if (errors != null) throw new AggregateException(errors);
	}

	public IServiceLocator Register<TInterface, TImplementation>(Func<TImplementation> factory,
		ServiceLifespan lifespan, bool replace = false)
		where TInterface : IService
		where TImplementation : class, TInterface
	{
		if (factory == null) throw new ArgumentNullException(nameof(factory));
		if (!Enum.IsDefined(typeof(ServiceLifespan), lifespan) || lifespan == ServiceLifespan.None)
			throw new ArgumentException("Unsupported service lifespan.", nameof(lifespan));

		var type = typeof(TInterface);
		var registration = new Registration();
		bool cached = lifespan != ServiceLifespan.Transient;
		IService Create() => Track(type, registration, factory(), cached);
		if (cached)
		{
			var lazy = new Lazy<IService>(Create);
			registration.Resolve = () => lazy.Value;
		}
		else registration.Resolve = Create;

		IDisposable? previousService;
		lock (_gate)
		{
			if (_disposed) throw new ObjectDisposedException(nameof(ServiceLocator));
			if (_registrations.ContainsKey(type) && !replace) return this;
			previousService = Remove(type);
			_registrations[type] = registration;
		}

		try
		{
			previousService?.Dispose();
			if (lifespan == ServiceLifespan.Singleton || lifespan == ServiceLifespan.Scoped)
				registration.Resolve();
		}
		catch
		{
			lock (_gate)
				if (_registrations.TryGetValue(type, out var current) && current == registration)
					_registrations.Remove(type);
			throw;
		}

		return this;
	}

	private IService Track(Type type, Registration registration, IService instance, bool cached)
	{
		lock (_gate)
		{
			if (!_disposed && _registrations.TryGetValue(type, out var current) && current == registration)
			{
				if (instance is IDisposable disposable)
				{
					_disposables.Add(disposable);
					if (cached) registration.CachedDisposable = disposable;
				}
				return instance;
			}
		}

		// A factory may finish after the owning registration was removed.
		(instance as IDisposable)?.Dispose();
		throw new InvalidOperationException($"Registration for {type.Name} ended while its factory was running.");
	}

	public T GetService<T>() where T : IService
	{
		Registration registration;
		lock (_gate)
			if (!_registrations.TryGetValue(typeof(T), out registration))
				throw new ServiceNotFoundException(typeof(T));
		return (T)registration.Resolve();
	}

	public bool IsServiceRegistered<T>() where T : IService
	{
		lock (_gate) return _registrations.ContainsKey(typeof(T));
	}

	public void Unregister<T>() where T : IService
	{
		IDisposable? service;
		lock (_gate) service = Remove(typeof(T));
		service?.Dispose();
	}

	private IDisposable? Remove(Type type)
	{
		if (!_registrations.TryGetValue(type, out var registration)) return null;
		_registrations.Remove(type);
		var service = registration.CachedDisposable;
		return service != null && _disposables.Remove(service) ? service : null;
	}
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
