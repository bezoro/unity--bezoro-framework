using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Type_Extensions
	{
		public static bool Is_Anonymous( this Type type )
		{
			return Attribute.IsDefined( type, typeof( CompilerGeneratedAttribute ), false )
				   && type.IsGenericType
				   && type.Name.Contains( "AnonymousType" )
				   && ( type.Name.StartsWith( "<>" ) || type.Name.StartsWith( "VB$" ) )
				   && type.Attributes.HasFlag( TypeAttributes.NotPublic );
		}

		public static bool Is_Null( this Type componentType )
		{
			return componentType == null;
		}
	}
}
