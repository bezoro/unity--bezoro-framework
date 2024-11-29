using System;
using Bezoro.Framework.Unity.DI;
using NUnit.Framework;

namespace Bezoro.Framework.Unity.Tests.EditMode
{
	[TestFixture]
	[Category( "Service Locator" )]
	[Category( "Unit Tests" )]
	public class Service_Locator_Tests
	{
		private IServiceLocator _service_locator;

		[SetUp]
		public void SetUp()
		{
			_service_locator = new Service_Locator();
		}

		[TearDown]
		public void TearDown()
		{
			_service_locator.Dispose();
		}

		[Test]
		public void RegisterService_WhenCalledWithSingletonLifespan_RegistersService()
		{
			_service_locator.RegisterService<ITestService, TestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			Assert.IsTrue( _service_locator.IsServiceRegistered<ITestService>() );
		}

		[Test]
		public void RegisterService_WhenCalledWithTransientLifespan_RegistersService()
		{
			_service_locator.RegisterService<ITestService, TestService>(
				() => new(),
				ServiceLifespan.Transient
			);

			Assert.IsTrue( _service_locator.IsServiceRegistered<ITestService>() );
		}

		[Test]
		public void GetService_WhenSingletonRegistered_ReturnsSameInstance()
		{
			_service_locator.RegisterService<ITestService, TestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			var service1 = _service_locator.GetService<ITestService>();
			var service2 = _service_locator.GetService<ITestService>();

			Assert.AreSame( service1, service2 );
		}

		[Test]
		public void GetService_WhenTransientRegistered_ReturnsDifferentInstances()
		{
			_service_locator.RegisterService<ITestService, TestService>(
				() => new(),
				ServiceLifespan.Transient
			);

			var service1 = _service_locator.GetService<ITestService>();
			var service2 = _service_locator.GetService<ITestService>();

			Assert.AreNotSame( service1, service2 );
		}

		[Test]
		public void GetService_WhenLazySingletonRegistered_ReturnsSameInstance()
		{
			_service_locator.RegisterService<ITestService, TestService>(
				() => new(),
				ServiceLifespan.LazySingleton
			);

			var service1 = _service_locator.GetService<ITestService>();
			var service2 = _service_locator.GetService<ITestService>();

			Assert.AreSame( service1, service2 );
		}

		[Test]
		public void GetService_WhenServiceNotRegistered_ThrowsServiceNotFoundException()
		{
			Assert.Throws<ServiceNotFoundException>( () => _service_locator.GetService<ITestService>() );
		}

		[Test]
		public void RegisterService_WhenServiceAlreadyRegistered_ThrowsServiceAlreadyRegisteredException()
		{
			_service_locator.RegisterService<ITestService, TestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			Assert.Throws<ServiceAlreadyRegisteredException>
			(
				() =>
					_service_locator.RegisterService<ITestService, TestService>(
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
					_service_locator.RegisterService<ITestService, TestService>(
						null,
						ServiceLifespan.Singleton
					)
			);
		}

		[Test]
		public void Dispose_ClearsAllServices()
		{
			_service_locator.RegisterService<ITestService, TestService>(
				() => new(),
				ServiceLifespan.Singleton
			);

			_service_locator.Dispose();

			Assert.IsFalse( _service_locator.IsServiceRegistered<ITestService>() );
		}

		private interface ITestService : IService {}

		private class TestService : ITestService {}
	}
}
