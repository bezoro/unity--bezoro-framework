using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Bezoro.Framework.Core;
using Bezoro.Framework.Unity.Utilities.String;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Bezoro.Framework.Unity
{
	public interface I_Console_Logger : I_Logger
	{
		void Log(string message, Object context);
		void Log_Warning(string message, Object context);
		void Log_Error(string message, Object context);
		void Log_Exception(Exception exception, Object context, bool throwException = true);
		void Log_Success(string message, Object context);
	}

	public class Console_Logger : I_Console_Logger
	{
		public void Log(string message)
		{
			Debug.Log(message);
		}

		public void Log_Success(string message)
		{
			Debug.Log(message.Green().Bold());
		}

		public void Log_Warning(string message)
		{
			Debug.LogWarning(message.Bold());
		}

		public void Log_Error(string message)
		{
			Debug.LogError(message.Bold());
		}

		public void Log_Exception(Exception exception)
		{
			Debug.LogException(exception);
		}

		public void Log(string message, Object context)
		{
			Debug.Log(message, context);
		}

		public void Log_Warning(string message, Object context)
		{
			Debug.LogWarning(message, context);
		}

		public void Log_Error(string message, Object context)
		{
			Debug.LogError(message, context);
		}

		public void Log_Exception(Exception exception, Object context, bool throwException = true)
		{
			// Validate the argument to avoid silent failures.
			if (exception == null)
			{
				throw new ArgumentNullException(nameof(exception), "Exception parameter cannot be null.");
			}

			// Log the exception using Unity Debug.
			Debug.LogException(exception, context);

			// If rethrowing is requested, propagate the original exception while preserving the stack trace.
			if (throwException)
			{
				ExceptionDispatchInfo.Capture(exception).Throw();
			}
		}

		public void Log_Success(string message, Object context)
		{
			Debug.Log(message.Green().Bold(), context);
		}
	}

	public static class Logger
	{
		public static I_Console_Logger Logger_Instance
		{
			get
			{
				if (Core.Logger.Logger_Instance is not I_Console_Logger)
				{
					Core.Logger.Set_Logger(new Console_Logger());
				}

				return Core.Logger.Logger_Instance as I_Console_Logger;
			}
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(string message)
		{
			Logger_Instance.Log(message);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Warning(string message)
		{
			Logger_Instance.Log_Warning(message);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Error(string message)
		{
			Logger_Instance.Log_Error(message);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Exception(Exception exception)
		{
			Logger_Instance.Log_Exception(exception);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Success(string message)
		{
			Logger_Instance.Log_Success(message);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(string message, Object context)
		{
			Logger_Instance.Log(message, context);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Warning(string message, Object context)
		{
			Logger_Instance.Log_Warning(message, context);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Error(string message, Object context)
		{
			Logger_Instance?.Log_Error(message, context);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Exception(Exception exception, Object context)
		{
			Logger_Instance?.Log_Exception(exception, context);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Success(string message, Object context)
		{
			Logger_Instance?.Log_Success(message, context);
		}
	}
}
