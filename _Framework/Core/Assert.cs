using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bezoro.Framework.Core
{
	public class AssertionException : Exception
	{
		public string AssertionMessage { get; }

		public AssertionException( string message ) : base( message )
		{
			AssertionMessage = message;
		}

		public AssertionException( string message, Exception innerException )
			: base( message, innerException )
		{
			AssertionMessage = message;
		}

		public override string ToString() => $"AssertionException: {AssertionMessage}\n{base.ToString()}";
	}

	public static class Assert
	{
		[Conditional( "UNITY_EDITOR" )]
		public static void IsTrue( bool condition, string message = null )
		{
			if ( !condition )
			{
				throw new AssertionException( message ?? "Assertion failed: Expected condition to be true." );
			}
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void IsFalse( bool condition, string message = null )
		{
			if ( condition )
			{
				throw new AssertionException( message ?? "Assertion failed: Expected condition to be false." );
			}
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void IsNull( object obj, string message = null )
		{
			if ( obj != null )
			{
				throw new AssertionException( message ?? "Assertion failed: Expected object to be null." );
			}
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void IsNotNull( object obj, string message = null )
		{
			if ( obj == null )
			{
				throw new AssertionException( message ?? "Assertion failed: Expected object to not be null." );
			}
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void AreEqual<T>( T expected, T actual, string message = null )
		{
			if ( !EqualityComparer<T>.Default.Equals( expected, actual ) )
			{
				throw new AssertionException(
					message ?? $"Assertion failed: Expected '{expected}', but got '{actual}'."
				);
			}
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void AreNotEqual<T>( T notExpected, T actual, string message = null )
		{
			if ( EqualityComparer<T>.Default.Equals( notExpected, actual ) )
			{
				throw new AssertionException(
					message ?? $"Assertion failed: Did not expect '{notExpected}', but got '{actual}'."
				);
			}
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void IsGreaterThan<T>( T left, T right, string message = null ) where T : IComparable<T>
		{
			if ( left.CompareTo( right ) <= 0 )
			{
				throw new AssertionException(
					message ?? $"Assertion failed: Expected '{left}' to be greater than '{right}'."
				);
			}
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void IsLessThan<T>( T left, T right, string message = null ) where T : IComparable<T>
		{
			if ( left.CompareTo( right ) >= 0 )
			{
				throw new AssertionException(
					message ?? $"Assertion failed: Expected '{left}' to be less than '{right}'."
				);
			}
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void IsGreaterThanOrEqual<T>( T left, T right, string message = null ) where T : IComparable<T>
		{
			if ( left.CompareTo( right ) < 0 )
			{
				throw new AssertionException(
					message ?? $"Assertion failed: Expected '{left}' to be greater than or equal to '{right}'."
				);
			}
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void IsLessThanOrEqual<T>( T left, T right, string message = null ) where T : IComparable<T>
		{
			if ( left.CompareTo( right ) > 0 )
			{
				throw new AssertionException(
					message ?? $"Assertion failed: Expected '{left}' to be less than or equal to '{right}'."
				);
			}
		}
	}
}
