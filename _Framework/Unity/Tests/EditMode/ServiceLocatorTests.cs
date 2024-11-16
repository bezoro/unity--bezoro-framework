using System;
using Bezoro.Framework.Unity.DI;
using NUnit.Framework;

namespace Bezoro.Framework.Unity.Tests.EditMode
{
	[TestFixture]
	[Category( "Service Locator" )]
	[Category( "Unit Tests" )]
	public class ServiceLocatorTests
	{
		private IServiceLocator _serviceLocator;

		[SetUp]
		public void SetUp()
		{
			_serviceLocator = new ServiceLocator();
		}

		[TearDown]
		public void TearDown()
		{
			_serviceLocator.Dispose();
		}

		[Test]
		public void RegisterService_WhenCalledWithSingletonLifespan_RegistersService()
		{
			_serviceLocator.RegisterService<ITestService, TestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			Assert.IsTrue( _serviceLocator.IsServiceRegistered<ITestService>() );
		}

		[Test]
		public void RegisterService_WhenCalledWithTransientLifespan_RegistersService()
		{
			_serviceLocator.RegisterService<ITestService, TestService>(
				() => new(),
				ServiceLifespan.Transient
			);

			Assert.IsTrue( _serviceLocator.IsServiceRegistered<ITestService>() );
		}

		[Test]
		public void GetService_WhenSingletonRegistered_ReturnsSameInstance()
		{
			_serviceLocator.RegisterService<ITestService, TestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			var service1 = _serviceLocator.GetService<ITestService>();
			var service2 = _serviceLocator.GetService<ITestService>();

			Assert.AreSame( service1, service2 );
		}

		[Test]
		public void GetService_WhenTransientRegistered_ReturnsDifferentInstances()
		{
			_serviceLocator.RegisterService<ITestService, TestService>(
				() => new(),
				ServiceLifespan.Transient
			);

			var service1 = _serviceLocator.GetService<ITestService>();
			var service2 = _serviceLocator.GetService<ITestService>();

			Assert.AreNotSame( service1, service2 );
		}

		[Test]
		public void GetService_WhenLazySingletonRegistered_ReturnsSameInstance()
		{
			_serviceLocator.RegisterService<ITestService, TestService>(
				() => new(),
				ServiceLifespan.LazySingleton
			);

			var service1 = _serviceLocator.GetService<ITestService>();
			var service2 = _serviceLocator.GetService<ITestService>();

			Assert.AreSame( service1, service2 );
		}

		[Test]
		public void GetService_WhenServiceNotRegistered_ThrowsServiceNotFoundException()
		{
			Assert.Throws<ServiceNotFoundException>( () => _serviceLocator.GetService<ITestService>() );
		}

		[Test]
		public void RegisterService_WhenServiceAlreadyRegistered_ThrowsServiceAlreadyRegisteredException()
		{
			_serviceLocator.RegisterService<ITestService, TestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			Assert.Throws<ServiceAlreadyRegisteredException>
			(
				() =>
					_serviceLocator.RegisterService<ITestService, TestService>(
						() => new(),
						ServiceLifespan.Singleton
					)
			);
		}

		[Test]
		public void RegisterService_WithNullFactory_ThrowsException()
		{
			Assert.Throws<ArgumentNullException>
			(
				() =>
					_serviceLocator.RegisterService<ITestService, TestService>(
						null,
						ServiceLifespan.Singleton
					)
			);
		}

		[Test]
		public void Dispose_ClearsAllServices()
		{
			_serviceLocator.RegisterService<ITestService, TestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			_serviceLocator.Dispose();

			Assert.IsFalse( _serviceLocator.IsServiceRegistered<ITestService>() );
		}

		private interface ITestService : IService {}

		private class TestService : ITestService {}
	}
}
