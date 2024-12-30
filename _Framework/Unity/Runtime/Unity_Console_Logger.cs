using System;
using System.Diagnostics;
using Bezoro.Framework.Core;
using Bezoro.Framework.Unity.Utilities;
using Object = UnityEngine.Object;

namespace Bezoro.Framework.Unity
{
	public interface I_Unity_Console_Logger : I_Logger
	{
		void Log(string message, Object context);
		void Log_Warning(string message, Object context);
		void Log_Error(string message, Object context);
		void Log_Exception(Exception exception, Object context);
		void Log_Success(string message, Object context);
	}

	/// <summary>
	///     Unity_Console_Logger implements the I_Logger interface and provides logging functionalities specific to Unity
	///     applications.
	///     This class is used to log various messages, such as standard logs, warnings, errors, exceptions, and success
	///     messages, primarily targeting the Unity console.
	///     The logging methods internally use the BDebug utility, which conditions the logs based on the environment (e.g.,
	///     Unity Editor or Development Build).
	/// </summary>
	public class Unity_Console_Logger : I_Logger
	{
		public void Log(string message)
		{
			Log(message, null);
		}

		public void Log_Warning(string message)
		{
			Log_Warning(message, null);
		}

		public void Log_Error(string message)
		{
			Log_Error(message, null);
		}

		public void Log_Exception(Exception exception)
		{
			Log_Exception(exception, null);
		}

		public void Log_Success(string message)
		{
			Log_Success(message, null);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public void Log(string message, Object context)
		{
			BDebug.Log(message, context);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public void Log_Warning(string message, Object context)
		{
			BDebug.Log_Warning(message, context);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public void Log_Error(string message, Object context)
		{
			BDebug.Log_Error(message, context);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public void Log_Exception(Exception exception, Object context)
		{
			BDebug.Log_Exception(exception, context);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public void Log_Success(string message, Object context)
		{
			BDebug.Log_Success(message, context);
		}
	}
}
