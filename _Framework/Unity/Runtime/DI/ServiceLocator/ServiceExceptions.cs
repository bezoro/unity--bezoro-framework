using System;

namespace Bezoro.Framework.Unity.DI
{
	public class ServiceRegistrationException : Exception
	{
		public ServiceRegistrationException(string message) : base(message) {}
	}

	public class ServiceAlreadyRegisteredException : Exception
	{
		public ServiceAlreadyRegisteredException(Type serviceType)
			: base($"Service of type {serviceType} is already registered.") {}
	}

	public class ServiceNotFoundException : Exception
	{
		public ServiceNotFoundException(Type serviceType)
			: base($"Service of type {serviceType} is not registered.") {}
	}
}
