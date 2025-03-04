using System;

namespace Bezoro.Framework.Core
{
	public class Null_Logger : I_Logger
	{
		public void Log(Logger.Log_Entry log) { }
		public void Log_Success(Logger.Log_Entry log) { }
		public void Log_Warning(Logger.Log_Entry log) { }
		public void Log_Error(Logger.Log_Entry log) { }

		public void Log_Exception(
			Exception exception,
			bool throw_exception = true,
			object context = null,
			string category = "Uncategorized"
		) { }
	}
}
