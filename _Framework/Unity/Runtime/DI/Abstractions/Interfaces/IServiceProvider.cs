namespace Bezoro.Framework.Unity.DI
{
	public interface IServiceProvider<TService>
	where TService : I_Service
	{
		public T GetService<T>()
		where T : TService;

		public bool IsServiceRegistered<T>()
		where T : TService;
	}
}
