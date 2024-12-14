using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	public static class CMD_Helpers
	{
		public static void Execute_Command(string command, bool as_admin)
		{
			var start_info = new ProcessStartInfo
			{
				FileName              = "CMD.exe",
				Arguments             = "/C " + command,
				UseShellExecute       = as_admin,
				RedirectStandardError = !as_admin,
				CreateNoWindow        = true
			};

			if (as_admin)
				start_info.Verb = "runas";

			var proc = new Process { StartInfo = start_info };

			using ( proc )
			{
				proc.Start();
				proc.WaitForExit();

				if (!as_admin && !proc.StandardError.EndOfStream)
					Debug.LogError(proc.StandardError.ReadToEnd());
			}
		}
	}
}
