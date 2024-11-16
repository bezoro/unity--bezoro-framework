using System;
using System.Diagnostics;
using System.Linq;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class ExceptionHelpers
	{
		public static void ThrowException<T>
		(
			object objectInstance,
			string methodName,
			string message
		) where T : Exception
		{
			var exceptionType = typeof(T).Name;
			var exceptionMessage = FormatExceptionMessage( exceptionType, objectInstance, methodName, message );

			throw (Exception)Activator.CreateInstance( typeof(T), exceptionMessage );
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void ThrowExceptionEditorOnly<T>( string exceptionMessage ) where T : Exception
		{
			ThrowExceptionEditorOnly<T>( null, string.Empty, exceptionMessage );
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void ThrowExceptionEditorOnly<T>
		(
			object instance,
			string callingMethodName,
			string exceptionMessage
		) where T : Exception
		{
			ThrowExceptionEditorOnly<T>( instance.GetType(), callingMethodName, exceptionMessage );
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void ThrowExceptionEditorOnly<T>
		(
			Type instanceType,
			string callingMethodName,
			string exceptionMessage
		) where T : Exception
		{
			ThrowException<T>( instanceType, callingMethodName, exceptionMessage );
		}

		private static string FormatExceptionMessage
		(
			string exceptionType,
			object objectInstance,
			string methodName,
			string message,
			params object[] paramNames
		)
		{
			var objectType = objectInstance?.GetType().Name ?? "Unknown";
			var paramNamesFormatted = FormatParamNames( paramNames );

			return
				$"{exceptionType} occurred in {objectType}.{methodName} for parameters [{paramNamesFormatted}]: {message}";
		}

		private static string FormatParamNames( params object[] paramNames )
		{
			return string.Join( ", ", paramNames.Select( p => p?.GetType().Name ?? "Unknown" ) );
		}
	}
}
