using System;

namespace Bezoro.Framework.Unity.Bootstrapping
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class Auto_Initialize_Attribute : Attribute
	{
		public Auto_Initialize_Attribute(int priority = 0, Type[] before = null, Type[] after = null)
		{
			Priority = priority;
			Before   = before ?? Type.EmptyTypes;
			After    = after ?? Type.EmptyTypes;
		}

		public Type[] After  { get; }
		public Type[] Before { get; }

		public int Priority { get; }
	}
}
