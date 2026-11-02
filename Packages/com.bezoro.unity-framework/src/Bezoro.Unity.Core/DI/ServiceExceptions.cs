using System;

namespace Bezoro.Unity.Core.DI;

/// <summary>
///     Thrown when callers try to register a service interface that already has a
///     registration and replacement was not requested.
/// </summary>
public class ServiceAlreadyRegisteredException : Exception
{
	/// <summary>
	///     Creates an exception message naming the duplicate service interface.
	/// </summary>
	public ServiceAlreadyRegisteredException(Type serviceType)
		: base(
			$"Service of type {serviceType} is already registered."
		) { }
}

/// <summary>
///     Thrown when a service lookup is requested for an interface that has no
///     registration.
/// </summary>
public class ServiceNotFoundException : Exception
{
	/// <summary>
	///     Creates an exception message naming the missing service interface.
	/// </summary>
	public ServiceNotFoundException(Type serviceType)
		: base(
			$"Service of type {serviceType} is not registered."
		) { }
}

/// <summary>
///     Represents a validation failure while registering services.
/// </summary>
public class ServiceRegistrationException : Exception
{
	/// <summary>
	///     Creates the exception with a caller-provided diagnostic message.
	/// </summary>
	public ServiceRegistrationException(string message) : base(
		message
	) { }
}
