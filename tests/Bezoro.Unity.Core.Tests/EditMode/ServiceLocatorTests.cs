using System;
using Bezoro.Unity.Core.DI;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies service registration, resolution lifetimes, missing-service errors, and disposal.
	/// </summary>
	[TestFixture]
	[Category("Service Locator")]
	[Category("Unit Tests")]
	public class ServiceLocatorTests
	{
		private IServiceLocator _serviceLocator;

		[SetUp]
		public void SetUp() =>
			_serviceLocator = new ServiceLocator();

		[TearDown]
		public void TearDown() =>
			_serviceLocator.Dispose();

		[Test]
		public void Dispose_ClearsAllServices()
		{
			_serviceLocator.Register<I_TestService, TestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			_serviceLocator.Dispose();

			Assert.IsFalse(
				_serviceLocator.IsServiceRegistered<I_TestService>()
			);
		}

		[Test]
		public void Dispose_WhenLazySingletonNotResolved_DoesNotCreateService()
		{
			var factoryCallCount = 0;

			_serviceLocator.Register<I_TestService, TestService>(
				() =>
				{
					factoryCallCount++;

					return new();
				},
				ServiceLifespan.LazySingleton
			);

			_serviceLocator.Dispose();

			Assert.AreEqual(
				0,
				factoryCallCount
			);
		}

		[Test]
		public void Dispose_WhenServiceWasCreated_DisposesService()
		{
			_serviceLocator.Register<I_DisposableTestService, DisposableTestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			var service = _serviceLocator.GetService<I_DisposableTestService>();

			_serviceLocator.Dispose();

			Assert.IsTrue(
				service.WasDisposed
			);
		}

		[Test]
		public void GetService_WhenLazySingletonRegistered_ReturnsSameInstance()
		{
			_serviceLocator.Register<I_TestService, TestService>(
				() => new(),
				ServiceLifespan.LazySingleton
			);

			var service1 = _serviceLocator.GetService<I_TestService>();
			var service2 = _serviceLocator.GetService<I_TestService>();

			Assert.AreSame(
				service1,
				service2
			);
		}

		[Test]
		public void GetService_WhenScopedRegistered_ReturnsSameInstance()
		{
			_serviceLocator.Register<I_TestService, TestService>(
				() => new(),
				ServiceLifespan.Scoped
			);

			var service1 = _serviceLocator.GetService<I_TestService>();
			var service2 = _serviceLocator.GetService<I_TestService>();

			Assert.AreSame(
				service1,
				service2
			);
		}

		[Test]
		public void GetService_WhenServiceNotRegistered_ThrowsServiceNotFoundException() =>
			Assert.That(
				Assert.Throws<ServiceNotFoundException>(() => _serviceLocator.GetService<I_TestService>())!.Message,
				Is.EqualTo(
					"Service of type Bezoro.Unity.Core.Tests.EditMode.ServiceLocatorTests+I_TestService is not registered."
				)
			);

		[Test]
		public void GetService_WhenSingletonRegistered_ReturnsSameInstance()
		{
			_serviceLocator.Register<I_TestService, TestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			var service1 = _serviceLocator.GetService<I_TestService>();
			var service2 = _serviceLocator.GetService<I_TestService>();

			Assert.AreSame(
				service1,
				service2
			);
		}

		[Test]
		public void GetService_WhenTransientRegistered_ReturnsDifferentInstances()
		{
			_serviceLocator.Register<I_TestService, TestService>(
				() => new(),
				ServiceLifespan.Transient
			);

			var service1 = _serviceLocator.GetService<I_TestService>();
			var service2 = _serviceLocator.GetService<I_TestService>();

			Assert.AreNotSame(
				service1,
				service2
			);
		}

		[Test]
		public void RegisterService_WhenCalledWithSingletonLifespan_RegistersService()
		{
			_serviceLocator.Register<I_TestService, TestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			Assert.IsTrue(
				_serviceLocator.IsServiceRegistered<I_TestService>()
			);
		}

		[Test]
		public void RegisterService_WhenCalledWithTransientLifespan_RegistersService()
		{
			_serviceLocator.Register<I_TestService, TestService>(
				() => new(),
				ServiceLifespan.Transient
			);

			Assert.IsTrue(
				_serviceLocator.IsServiceRegistered<I_TestService>()
			);
		}

		[Test]
		public void RegisterService_WhenFactoryIsNull_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _serviceLocator.Register<I_TestService, TestService>(
													 null!,
													 ServiceLifespan.Singleton
												 )
			);
		}

		[Test]
		public void RegisterService_WhenLifespanIsNone_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() => _serviceLocator.Register<I_TestService, TestService>(
												 () => new(),
												 ServiceLifespan.None
											 )
			);
		}

		[Test]
		public void RegisterService_WhenReplaceTrue_DisposesPreviousResolvedService()
		{
			_serviceLocator.Register<I_DisposableTestService, DisposableTestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			var firstService = _serviceLocator.GetService<I_DisposableTestService>();

			_serviceLocator.Register<I_DisposableTestService, DisposableTestService>(
				() => new(),
				ServiceLifespan.Singleton,
				true
			);

			Assert.IsTrue(firstService.WasDisposed);
			Assert.AreNotSame(firstService, _serviceLocator.GetService<I_DisposableTestService>());
		}

		[Test]
		public void RegisterService_WhenReplaceTrue_ReplacesPreviousLifetime()
		{
			_serviceLocator.Register<I_TestService, FirstTestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			_serviceLocator.Register<I_TestService, SecondTestService>(
				() => new(),
				ServiceLifespan.Transient,
				true
			);

			Assert.IsInstanceOf<SecondTestService>(
				_serviceLocator.GetService<I_TestService>()
			);
		}

		[Test]
		public void ServiceExceptionConstructors_FormatDiagnosticMessages()
		{
			Assert.AreEqual(
				"Service of type Bezoro.Unity.Core.Tests.EditMode.ServiceLocatorTests+I_TestService is already registered.",
				new ServiceAlreadyRegisteredException(typeof(I_TestService)).Message
			);

			Assert.AreEqual(
				"registration failed",
				new ServiceRegistrationException("registration failed").Message
			);
		}

		[Test]
		public void Unregister_WhenLazySingletonNotResolved_DoesNotCreateService()
		{
			var factoryCallCount = 0;

			_serviceLocator.Register<I_TestService, TestService>(
				() =>
				{
					factoryCallCount++;

					return new();
				},
				ServiceLifespan.LazySingleton
			);

			_serviceLocator.Unregister<I_TestService>();

			Assert.AreEqual(
				0,
				factoryCallCount
			);
		}

		[Test]
		public void Unregister_WhenServiceWasCreated_DisposesAndRemovesService()
		{
			_serviceLocator.Register<I_DisposableTestService, DisposableTestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			var service = _serviceLocator.GetService<I_DisposableTestService>();

			_serviceLocator.Unregister<I_DisposableTestService>();

			Assert.IsTrue(service.WasDisposed);
			Assert.IsFalse(_serviceLocator.IsServiceRegistered<I_DisposableTestService>());
		}

		private class DisposableTestService : I_DisposableTestService
		{
			public bool WasDisposed { get; private set; }

			public void Dispose() =>
				WasDisposed = true;
		}

		private class FirstTestService : I_TestService { }

		private interface I_DisposableTestService : I_TestService, IDisposable
		{
			bool WasDisposed { get; }
		}

		/// <summary>
		///     Marks services used by the service locator tests.
		/// </summary>
		private interface I_TestService : IService { }

		private class SecondTestService : I_TestService { }

		/// <summary>
		///     Concrete service used to verify service locator registrations.
		/// </summary>
		private class TestService : I_TestService { }
	}
}
