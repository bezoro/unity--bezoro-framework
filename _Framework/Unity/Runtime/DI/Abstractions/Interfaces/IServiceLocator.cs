using System;

namespace Bezoro.Framework.Unity.DI
{
	public interface IServiceLocator : IServiceProvider<IService>, IService, IDisposable
	{
		IServiceLocator RegisterService<TInterface, TImplementation>
		(
			Func<TImplementation> factory,
			ServiceLifespan lifespan,
			bool replace = false
		)
		where TInterface : IService
		where TImplementation : class, TInterface;

		void UnregisterService<T>() where T : IService;
	}
}
