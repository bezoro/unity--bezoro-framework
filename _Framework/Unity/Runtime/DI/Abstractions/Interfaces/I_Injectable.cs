namespace Bezoro.Framework.Unity.DI
{
	public interface I_Injectable<in T>
	{
		public void Inject(T dependencies);
		public bool Validate_Dependencies(T dependencies, object context = null);
	}
}
