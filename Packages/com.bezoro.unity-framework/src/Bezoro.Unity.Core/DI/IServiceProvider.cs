namespace Bezoro.Unity.Core.DI;

/// <summary>
///     Read-only service lookup contract for a constrained family of service
///     interfaces.
/// </summary>
public interface IServiceProvider<TService>
	where TService : IService
{
	/// <summary>
	///     Determines whether a service interface has been registered.
	/// </summary>
	public bool IsServiceRegistered<T>()
		where T : TService;

	/// <summary>
	///     Resolves the service for an interface or lets the implementation throw its
	///     standard not-found exception.
	/// </summary>
	public T GetService<T>()
		where T : TService;
}
