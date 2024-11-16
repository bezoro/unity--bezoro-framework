using System.Diagnostics;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class UnityObjectHelpers
	{
		/// <summary>
		///     Validates the given Unity object and throws an exception if it is null.
		/// </summary>
		/// <param name="obj">The Unity object to validate.</param>
		/// <exception cref="System.Exception">Thrown when the object is null.</exception>
		[Conditional( "UNITY_EDITOR" )]
		public static void ValidateUnityObject( Object obj )
		{
			if ( obj == null )
			{
				throw new("Object is null.");
			}
		}
	}
}
