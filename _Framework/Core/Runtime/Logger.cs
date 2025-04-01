using System;
using System.Runtime.ExceptionServices;

namespace Bezoro.Framework.Core
{
	public static class Logger
	{
		public static ILogger Logger_Instance { get; private set; } = new NullLogger();

		public class LogEntry
		{
			public LogEntry(string message, object context_object, Log_Type type, string category)
			{
				Type           = type;
				Category       = category;
				Message        = message;
				Context_Object = context_object;
			}

			public string   Message        { get; }
			public object   Context_Object { get; }
			public Log_Type Type           { get; }
			public string   Category       { get; }

			public enum Log_Type
			{
				Info,
				Warning,
				Error,
				Success,
				Exception
			}
		}

		public static void Set_Logger(ILogger logger)
		{
			Logger_Instance = logger;
			LogSuccess($"Logger set to {Logger_Instance.GetType().Name}.");
		}

		public static void Log_Info(string message, object context_object = null, string category = "Uncategorized")
		{
			var log = new LogEntry(message, context_object, LogEntry.Log_Type.Info, category);

			Logger_Instance?.Log(log);
		}

		public static void Log_Warning(string message, object context_object = null, string category = "Uncategorized")
		{
			var log = new LogEntry(message, context_object, LogEntry.Log_Type.Warning, category);

			Logger_Instance?.LogWarning(log);
		}

		public static void Log_Error(string message, object context_object = null, string category = "Uncategorized")
		{
			var log = new LogEntry(message, context_object, LogEntry.Log_Type.Error, category);

			Logger_Instance?.LogError(log);
		}

		public static void LogSuccess(string message, object context_object = null, string category = "Uncategorized")
		{
			var log = new LogEntry(message, context_object, LogEntry.Log_Type.Success, category);

			Logger_Instance?.LogSuccess(log);
		}

		public static void Log_Exception(Exception exception, bool throw_exception = true, object context_object = null)
		{
			var log = new LogEntry(
				exception.ToString(), context_object, LogEntry.Log_Type.Exception, "Uncategorized");

			Logger_Instance?.LogException(log);

			if (throw_exception)
				ExceptionDispatchInfo.Capture(exception).Throw();
		}
	}
}
