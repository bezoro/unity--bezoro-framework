using System;
using System.Diagnostics;

namespace Bezoro.Framework.Core
{
	public static class Validator
	{
		[Conditional( "UNITY_EDITOR" )]
		public static void Object( object obj )
		{
			if ( obj == null ) throw new ArgumentNullException( nameof( obj ), "Object is null" );
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void Is_Subclass_Of<T_Subclass>( object caller, Type type )
		{
			if ( !type.IsSubclassOf( typeof( T_Subclass ) ) )
			{
				throw new ArgumentException(
					$"Type {type.Name} is not subclass of {typeof( T_Subclass ).Name}", nameof( type ) );
			}
		}
	}
}
