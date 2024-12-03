namespace Bezoro.Framework.Unity.DI
{
	public interface IServiceProvider<TService>
	where TService : IService
	{
		public bool IsServiceRegistered<T>()
		where T : TService;

		public T GetService<T>()
		where T : TService;
	}
}
