using System;

namespace Bezoro.Framework.Core
{
	public static class Logger
	{
		public static I_Logger Logger_Instance { get; private set; } = new Null_Logger();

		public class Log_Entry
		{
			public Log_Entry(string message, object context_object, Log_Type type, string category)
			{
				this.Type           = type;
				this.Category       = category;
				this.Message        = message;
				this.Context_Object = context_object;
			}

			public string   Message        { get; }
			public object   Context_Object { get; }
			public Log_Type Type           { get; } = Log_Type.Info;
			public string   Category       { get; } = "Uncategorized";

			public enum Log_Type
			{
				Info,
				Warning,
				Error,
				Success,
				Exception
			}
		}

		public static void Set_Logger(I_Logger logger)
		{
			Logger_Instance = logger;
			Log_Success($"Logger set to {Logger_Instance.GetType().Name}.");
		}

		public static void Log_Info(string message, object context_object = null, string category = "Uncategorized")
		{
			var log = new Log_Entry(message, context_object, Log_Entry.Log_Type.Info, category);

			Logger_Instance?.Log(log);
		}

		public static void Log_Warning(string message, object context_object = null, string category = "Uncategorized")
		{
			var log = new Log_Entry(message, context_object, Log_Entry.Log_Type.Warning, category);

			Logger_Instance?.Log_Warning(log);
		}

		public static void Log_Error(string message, object context_object = null, string category = "Uncategorized")
		{
			var log = new Log_Entry(message, context_object, Log_Entry.Log_Type.Error, category);

			Logger_Instance?.Log_Error(log);
		}

		public static void Log_Success(string message, object context_object = null, string category = "Uncategorized")
		{
			var log = new Log_Entry(message, context_object, Log_Entry.Log_Type.Success, category);

			Logger_Instance?.Log_Success(log);
		}

		public static void Log_Exception(Exception exception, bool throw_exception = true, object context_object = null)
		{
			var log = new Log_Entry(
				exception.ToString(), context_object, Log_Entry.Log_Type.Exception, "Uncategorized");

			Logger_Instance?.Log_Exception(log);

			if (throw_exception)
				throw exception;
		}
	}
}
