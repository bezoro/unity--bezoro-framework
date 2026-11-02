using System;

namespace Bezoro.Unity.Core.DI;

/// <summary>
///     Mutable service registry that can create, cache, retrieve, and unregister
///     services by interface type.
/// </summary>
public interface IServiceLocator : IServiceProvider<IService>, IService, IDisposable
{
	/// <summary>
	///     Registers a factory for an interface using the requested lifetime.
	///     Existing registrations are preserved unless <paramref name="replace" /> is
	///     set.
	/// </summary>
	public IServiceLocator Register<TInterface, TImplementation>(
		Func<TImplementation> factory,
		ServiceLifespan       lifespan,
		bool                  replace = false
	)
		where TInterface : IService
		where TImplementation : class, TInterface;

	/// <summary>
	///     Removes any registration for the service interface.
	/// </summary>
	public void Unregister<T>()
		where T : IService;
}
