namespace Bezoro.Framework.Core
{
	public interface ILogger
	{
		public void Log(Logger.LogEntry log);
		public void LogSuccess(Logger.LogEntry log);
		public void LogWarning(Logger.LogEntry log);
		public void LogError(Logger.LogEntry log);
		public void LogException(Logger.LogEntry log);
	}
}
