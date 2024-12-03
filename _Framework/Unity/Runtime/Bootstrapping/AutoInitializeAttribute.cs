using System;

namespace Bezoro.Framework.Unity.Bootstrapping
{
	[AttributeUsage(
		AttributeTargets.Class,
		Inherited = false )]
	public class AutoInitializeAttribute : Attribute
	{
		public AutoInitializeAttribute( int priority = 0, Type[] before = null, Type[] after = null )
		{
			Priority = priority;
			Before   = before ?? Type.EmptyTypes;
			After    = after ?? Type.EmptyTypes;
		}

		public int    Priority { get; }
		public Type[] Before   { get; }
		public Type[] After    { get; }
	}
}
