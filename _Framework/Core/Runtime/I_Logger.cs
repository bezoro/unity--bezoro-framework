namespace Bezoro.Framework.Core
{
	public interface I_Logger
	{
		public void Log(string message);
		public void Log_Success(string message);
		public void Log_Warning(string message);
		public void Log_Error(string message);
	}
}
