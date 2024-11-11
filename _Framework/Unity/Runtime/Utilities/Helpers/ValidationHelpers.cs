using System;
using System.Collections.Generic;
using Bezoro.Framework.Unity.Utilities.Extensions;

namespace Bezoro.Framework.Unity.Utilities.Helpers
{
	public static class ValidationHelpers
	{
		public static void ValidateIsSubclassOf<T>(object caller, string methodName, Type type)
		{
			if (!type.IsSubclassOf(typeof(T)))
			{
				ExceptionHelpers.ThrowExceptionEditorOnly<ArgumentNullException>(
					caller,
					methodName,
					$"Type {type} is not a Component"
				);
			}
		}

		public static void ValidatePositiveValue(float value)
		{
			ValidatePositiveValue(null, "", value);
		}

		public static void ValidatePositiveValue
		(
			object caller,
			string methodName,
			float value
		)
		{
			if (value <= 0)
			{
				ExceptionHelpers.ThrowExceptionEditorOnly<ArgumentException>(
					caller,
					methodName,
					$"Value must be positive. Received: {value}"
				);
			}
		}

		public static void ValidateValueNotAboveMax
		(
			object caller,
			string methodName,
			int value,
			int max
		)
		{
			if (value > max)
			{
				ExceptionHelpers.ThrowExceptionEditorOnly<ArgumentException>(
					caller,
					methodName,
					$"{nameof(value)} cannot be greater than {nameof(max)}. Received: {value}, Max: {max}"
				);
			}
		}

		public static void ValidateListNotNullOrEmpty<T>
		(
			object caller,
			string methodName,
			List<T> list
		)
		{
			if (list.IsNullOrEmpty())
			{
				ExceptionHelpers.ThrowExceptionEditorOnly<ArgumentException>(
					caller,
					methodName,
					$"{nameof(list)} is null or empty"
				);
			}
		}

		public static void ValidateConditionIsFalse(bool condition, string errorMessage = "")
		{
			ValidateConditionIsFalse<IncorrectConditionException>(condition, errorMessage);
		}

		public static void ValidateConditionIsFalse<TException>(bool condition, string errorMessage = "")
		where TException : Exception
		{
			if (condition)
			{
				ExceptionHelpers.ThrowExceptionEditorOnly<TException>(errorMessage);
			}
		}

		public static void ValidateObjectNotNull(object objectToValidate)
		{
			ValidateObjectNotNull(objectToValidate, "", "", null);
		}

		public static void ValidateObjectNotNull(object objectToValidate, string errorMessage)
		{
			ValidateObjectNotNull(objectToValidate, errorMessage, "", null);
		}

		public static void ValidateObjectNotNull
		(
			object objectToValidate,
			string errorMessage,
			string callingMethodName,
			object callingObject
		)
		{
			if (objectToValidate != null)
			{
				return;
			}

			errorMessage = $"{nameof(objectToValidate)} is null; {errorMessage}";

			ExceptionHelpers.ThrowExceptionEditorOnly<ArgumentNullException>(
				callingObject,
				callingMethodName,
				errorMessage
			);
		}

		public static void ValidateString(string value)
		{
			ValidateString(null, "", value);
		}

		public static void ValidateString(object caller, string methodName, string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				ExceptionHelpers.ThrowExceptionEditorOnly<ArgumentException>(
					caller,
					methodName,
					$"{nameof(value)} is null or empty"
				);
			}
		}

		public static void ValidateCondition(bool condition, string errorMessage)
		{
			if (!condition)
			{
				ExceptionHelpers.ThrowExceptionEditorOnly<InvalidOperationException>(
					null,
					"",
					errorMessage
				);
			}
		}
	}
}
