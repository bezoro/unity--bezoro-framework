namespace Bezoro.Framework.Core
{
	public interface I_Logger
	{
		public void Log(Logger.Log_Entry log);
		public void Log_Success(Logger.Log_Entry log);
		public void Log_Warning(Logger.Log_Entry log);
		public void Log_Error(Logger.Log_Entry log);
		public void Log_Exception(Logger.Log_Entry log);
	}
}
