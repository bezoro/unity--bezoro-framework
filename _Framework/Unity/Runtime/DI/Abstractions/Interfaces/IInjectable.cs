using System;

namespace Bezoro.Framework.Unity.DI
{
	public interface IInjectable<in T>
	{
		void Inject( T dependencies );

		bool ValidateDependencies( T dependencies, object context = null );
	}

	public static class InjectableExtensions
	{
		public static bool ValidateDependencies<T>
			( this IInjectable<T> injectable, T dependencies, object context = null )
		{
			if ( dependencies == null )
			{
				throw new ArgumentNullException( nameof(dependencies), "Dependencies cannot be null." );
			}

			return true;
		}
	}

	public class DependencyValidationException : Exception
	{
		public DependencyValidationException( string message ) : base( message ) {}
	}
}
