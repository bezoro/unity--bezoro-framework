using System;
using System.Diagnostics;
using System.Linq;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Exception_Helpers
	{
		public static void Throw_Exception<T>(
			object objectInstance,
			string methodName,
			string message
		)
		where T : Exception
		{
			var exceptionType = typeof( T ).Name;
			var exceptionMessage = Format_Exception_Message(
				exceptionType,
				objectInstance,
				methodName,
				message );

			throw (Exception)Activator.CreateInstance(
				typeof( T ),
				exceptionMessage );
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void Throw_Exception_Editor_Only<T>( string exceptionMessage )
		where T : Exception
		{
			Throw_Exception_Editor_Only<T>(
				null,
				string.Empty,
				exceptionMessage );
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void Throw_Exception_Editor_Only<T>(
			object instance,
			string callingMethodName,
			string exceptionMessage
		)
		where T : Exception
		{
			Throw_Exception_Editor_Only<T>(
				instance.GetType(),
				callingMethodName,
				exceptionMessage );
		}

		[Conditional( "UNITY_EDITOR" )]
		public static void Throw_Exception_Editor_Only<T>(
			Type instanceType,
			string callingMethodName,
			string exceptionMessage
		)
		where T : Exception
		{
			Throw_Exception<T>(
				instanceType,
				callingMethodName,
				exceptionMessage );
		}

		private static string Format_Exception_Message(
			string exceptionType,
			object objectInstance,
			string methodName,
			string message,
			params object[] paramNames
		)
		{
			var objectType          = objectInstance?.GetType().Name ?? "Unknown";
			var paramNamesFormatted = FormatParamNames( paramNames );

			return
				$"{exceptionType} occurred in {objectType}.{methodName} for parameters [{paramNamesFormatted}]: {message}";
		}

		private static string FormatParamNames( params object[] paramNames )
		{
			return string.Join(
				", ",
				paramNames.Select( p => p?.GetType().Name ?? "Unknown" ) );
		}
	}
}
