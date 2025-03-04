using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Bezoro.Framework.Core;
using Bezoro.Framework.Unity.Utilities.String;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Bezoro.Framework.Unity
{
	public interface I_Console_Logger : I_Logger { }

	public class Unity_Log_Entry : Core.Logger.Log_Entry
	{
		public Unity_Log_Entry(string message, Object context_object, Log_Type type, string category)
			: base(message, context_object, type, category)
		{
			Context_Object = context_object;
		}

		public new Object Context_Object { get; }
	}

	public class Console_Logger : I_Console_Logger
	{
		public void Log(Core.Logger.Log_Entry log)
		{
			var unityLog = log as Unity_Log_Entry;
			Debug.Log(log.Message, unityLog?.Context_Object);
		}

		public void Log_Success(Core.Logger.Log_Entry log)
		{
			var unityLog = log as Unity_Log_Entry;
			Debug.Log(log.Message.Green().Bold(), unityLog?.Context_Object);
		}

		public void Log_Warning(Core.Logger.Log_Entry log)
		{
			var unityLog = log as Unity_Log_Entry;
			Debug.LogWarning(log.Message.Yellow().Bold(), unityLog?.Context_Object);
		}

		public void Log_Error(Core.Logger.Log_Entry log)
		{
			var unityLog = log as Unity_Log_Entry;
			Debug.LogError(log.Message.Red().Bold(), unityLog?.Context_Object);
		}

		public void Log_Exception(Core.Logger.Log_Entry log)
		{
			var unityLog = log as Unity_Log_Entry;
			Debug.LogError(log.Message.Red().Bold(), unityLog?.Context_Object);
		}

		public void Log_Exception(
			Exception exception,
			bool throw_exception = true,
			object context = null,
			string category = "Uncategorized"
		)
		{
			Debug.LogException(exception);
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
					Core.Logger.Set_Logger(new Console_Logger());

				return Core.Logger.Logger_Instance as I_Console_Logger;
			}
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(string message, Object context = null, string category = "Uncategorized")
		{
			var log = new Unity_Log_Entry(
				message,
				context,
				Core.Logger.Log_Entry.Log_Type.Info,
				category);

			Logger_Instance.Log(log);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Warning(string message, Object context = null, string category = "Uncategorized")
		{
			var log = new Unity_Log_Entry(
				message,
				context,
				Core.Logger.Log_Entry.Log_Type.Warning,
				category);

			Logger_Instance.Log_Warning(log);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Error(string message, Object context = null, string category = "Uncategorized")
		{
			var log = new Unity_Log_Entry(
				message,
				context,
				Core.Logger.Log_Entry.Log_Type.Error,
				category);

			Logger_Instance.Log_Error(log);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Exception(Exception exception, bool throw_exception = true, Object context = null)
		{
			var log = new Unity_Log_Entry(
				exception.Message,
				context,
				Core.Logger.Log_Entry.Log_Type.Exception,
				"Uncategorized");

			Logger_Instance.Log_Exception(log);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Success(string message, Object context = null, string category = "Uncategorized")
		{
			var log = new Unity_Log_Entry(
				message,
				context,
				Core.Logger.Log_Entry.Log_Type.Success,
				category);

			Logger_Instance.Log_Success(log);
		}

		// [Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		// public static void Log(string message, ICollection<Behaviour> context)
		// {
		// 	GameObject go = null;
		//
		// 	foreach ( var behaviour in context )
		// 	{
		// 		if (behaviour.gameObject == null)
		// 			continue;
		//
		// 		go = behaviour.gameObject;
		// 	}
		//
		// 	Log(message, go);
		// }

		// [Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		// public static void Log_Warning(string message, Object context)
		// {
		// 	Logger_Instance.Log_Warning((Log)message);
		// }
		//
		// [Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		// public static void Log_Error(string message, Object context)
		// {
		// 	Logger_Instance?.Log_Error((Log)message);
		// }
		//
		// [Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		// public static void Log_Exception(Exception exception, Object context)
		// {
		// 	Logger_Instance?.Log_Exception(exception, context);
		// }
		//
		// [Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		// public static void Log_Success(string message, Object context)
		// {
		// 	Logger_Instance?.Log_Success((Log)message);
		// }
	}
}
