using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Bezoro.Framework.Core
{
	public class Assertion_Exception : Exception
	{
		public Assertion_Exception(string message) : base(message)
		{
			Assertion_Message = message;
		}

		public Assertion_Exception(string message, Exception innerException) : base(message, innerException)
		{
			Assertion_Message = message;
		}

		public string Assertion_Message { get; }

		public override string ToString()
		{
			return $"AssertionException: {Assertion_Message}\n{base.ToString()}";
		}
	}

	public static class Assert
	{
		[Conditional("UNITY_EDITOR")]
		public static void Is_True(bool condition, string message = null)
		{
			if (!condition)
				throw new Assertion_Exception(message ?? "Assertion failed: Expected condition to be true.");
		}

		[Conditional("UNITY_EDITOR")]
		public static void Is_False(bool condition, string message = null)
		{
			if (condition)
				throw new Assertion_Exception(message ?? "Assertion failed: Expected condition to be false.");
		}

		[Conditional("UNITY_EDITOR")]
		public static void Is_Null(object obj, string message = null)
		{
			if (obj != null)
				throw new Assertion_Exception(message ?? "Assertion failed: Expected object to be null.");
		}

		[Conditional("UNITY_EDITOR")]
		public static void Is_Not_Null(object obj, string message = null)
		{
			if (obj == null)
				throw new Assertion_Exception(message ?? "Assertion failed: Expected object to not be null.");
		}

		[Conditional("UNITY_EDITOR")]
		public static void Are_Equal<T>(T expected, T actual, string message = null)
		{
			if (!EqualityComparer<T>.Default.Equals(expected, actual))
			{
				throw new Assertion_Exception(
					message ?? $"Assertion failed: Expected '{expected}', but got '{actual}'."
				);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public static void Are_Not_Equal<T>(T notExpected, T actual, string message = null)
		{
			if (EqualityComparer<T>.Default.Equals(notExpected, actual))
			{
				throw new Assertion_Exception(
					message ?? $"Assertion failed: Did not expect '{notExpected}', but got '{actual}'."
				);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public static void Is_Greater_Than<T>(T left, T right, string message = null)
		where T : IComparable<T>
		{
			if (left.CompareTo(right) <= 0)
			{
				throw new Assertion_Exception(
					message ?? $"Assertion failed: Expected '{left}' to be greater than '{right}'."
				);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public static void Is_Less_Than<T>(T left, T right, string message = null)
		where T : IComparable<T>
		{
			if (left.CompareTo(right) >= 0)
			{
				throw new Assertion_Exception(
					message ?? $"Assertion failed: Expected '{left}' to be less than '{right}'."
				);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public static void Is_Greater_Than_Or_Equal_To<T>(T left, T right, string message = null)
		where T : IComparable<T>
		{
			if (left.CompareTo(right) < 0)
			{
				throw new Assertion_Exception(
					message ?? $"Assertion failed: Expected '{left}' to be greater than or equal to '{right}'."
				);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public static void Is_Less_Than_Or_Equal_To<T>(T left, T right, string message = null)
		where T : IComparable<T>
		{
			if (left.CompareTo(right) > 0)
			{
				throw new Assertion_Exception(
					message ?? $"Assertion failed: Expected '{left}' to be less than or equal to '{right}'."
				);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public static void Is_Null_Or_Empty(string str, string message = null)
		{
			if (!string.IsNullOrEmpty(str))
				throw new Assertion_Exception(message ?? "Assertion failed: Expected string to be null or empty.");
		}

		[Conditional("UNITY_EDITOR")]
		public static void Is_Not_Null_Or_Empty(string str, string message = null)
		{
			if (string.IsNullOrEmpty(str))
			{
				throw new Assertion_Exception(
					message ?? "Assertion failed: Expected string to not be null or empty."
				);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public static void Is_Null_Or_Empty<T>(ICollection<T> collection, string message = null)
		where T : class
		{
			if (collection != null && collection.Count != 0)
			{
				throw new Assertion_Exception(
					message ?? "Assertion failed: Expected collection to be null or empty."
				);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public static void Is_Not_Null_Or_Empty<T>(ICollection<T> collection, string message = null)
		where T : class
		{
			if (collection == null || collection.Count == 0)
			{
				throw new Assertion_Exception(
					message ?? "Assertion failed: Expected collection to not be null or empty."
				);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public static void Is_Not_Null_Or_Empty<T>(T[,] array2D)
		{
			if (array2D == null || array2D.GetLength(0) == 0 || array2D.GetLength(1) == 0)
				throw new Assertion_Exception("Assertion failed: Expected array2D to not be null or empty.");
		}

		[Conditional("UNITY_EDITOR")]
		public static void Is_Null_Or_Empty<T>(T[,] array2D)
		{
			if (array2D != null && array2D.GetLength(0) != 0 && array2D.GetLength(1) != 0)
				throw new Assertion_Exception("Assertion failed: Expected array2D to be null or empty.");
		}

		[Conditional("UNITY_EDITOR")]
		public static void Is_Not_Null_Or_Empty<T>(IEnumerable<T> enumerable)
		{
			if (enumerable == null || !enumerable.Any())
				throw new Assertion_Exception("Assertion failed: Expected enumerable to not be null or empty.");
		}
	}
}
