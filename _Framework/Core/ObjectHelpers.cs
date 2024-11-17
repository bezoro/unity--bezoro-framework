using System;
using System.Diagnostics;

namespace Bezoro.Framework.Core
{
	public static class ObjectHelpers
	{
		[Conditional( "UNITY_EDITOR" )]
		public static void ValidateObject( object obj )
		{
			if ( obj == null )
			{
				throw new ArgumentNullException( nameof(obj), "Object is null" );
			}
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void ValidateIsSubclassOf<T>( object caller, Type type )
		{
			if ( !type.IsSubclassOf( typeof(T) ) )
			{
				throw new ArgumentException( $"Type {type.Name} is not subclass of {typeof(T).Name}", nameof(type) );
			}
		}
	}
}
