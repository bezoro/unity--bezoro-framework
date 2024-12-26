using System.Diagnostics;

namespace Bezoro.Framework.Core
{
	public static class Log_Helpers
	{
		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log(I_Logger logger, string message)
		{
			if (string.IsNullOrEmpty(message)) return;
			logger?.Log(message);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Warning(I_Logger logger, string message)
		{
			if (string.IsNullOrEmpty(message)) return;
			logger?.Log_Warning(message);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Error(I_Logger logger, string message)
		{
			if (string.IsNullOrEmpty(message)) return;
			logger?.Log_Error(message);
		}

		[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]
		public static void Log_Success(I_Logger logger, string message)
		{
			if (string.IsNullOrEmpty(message)) return;
			logger?.Log_Success(message);
		}
	}
}
