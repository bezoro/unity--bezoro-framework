using System;

namespace Bezoro.Framework.Core
{
	public static class Logger
	{
		public static I_Logger Logger_Instance { get; private set; } = new Null_Logger();

		public static void Set_Logger(I_Logger logger)
		{
			Logger_Instance = logger;
			Logger_Instance?.Log_Success($"Logger set to {Logger_Instance.GetType().Name}.");
		}

		public static void Log(string message)
		{
			Logger_Instance?.Log(message);
		}

		public static void Log_Warning(string message)
		{
			Logger_Instance?.Log_Warning(message);
		}

		public static void Log_Error(string message)
		{
			Logger_Instance?.Log(message);
		}

		public static void Log_Success(string message)
		{
			Logger_Instance?.Log_Success(message);
		}

		public static void Log_Exception(Exception exception)
		{
			Logger_Instance?.Log_Exception(exception);
		}
	}
}
