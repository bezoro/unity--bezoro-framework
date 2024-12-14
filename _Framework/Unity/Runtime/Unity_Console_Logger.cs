using Bezoro.Framework.Core;
using Bezoro.Framework.Unity.Utilities;
using Object = UnityEngine.Object;

namespace Bezoro.Framework.Unity
{
	public class Unity_Console_Logger : I_Logger
	{
		public void Log(string message)
		{
			BDebug.Log(message);
		}

		public void Log_Success(string message)
		{
			BDebug.Log_Success(message);
		}

		public void Log(string message, Object context)
		{
			BDebug.Log(message, context);
		}

		public void Log_Warning(string message)
		{
			BDebug.Log_Warning(message);
		}

		public void Log_Warning(string message, Object context)
		{
			BDebug.Log_Warning(message, context);
		}

		public void Log_Error(string message)
		{
			BDebug.Log_Error(message);
		}

		public void Log_Error(string message, Object context)
		{
			BDebug.Log_Error(message, context);
		}
	}
}
