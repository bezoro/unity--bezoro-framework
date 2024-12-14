using System;

namespace Bezoro.Framework.Unity.DI
{
	public interface I_ServiceLocator : IServiceProvider<I_Service>, I_Service, IDisposable
	{
		public I_ServiceLocator Register<TInterface, TImplementation>(
			Func<TImplementation> factory,
			ServiceLifespan lifespan,
			bool replace = false
		)
		where TInterface : I_Service
		where TImplementation : class, TInterface;

		public void Unregister<T>()
		where T : I_Service;
	}
}
