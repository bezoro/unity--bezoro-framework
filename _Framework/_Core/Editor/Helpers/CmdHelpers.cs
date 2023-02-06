using System.Diagnostics;

namespace Bezoro.Core.Helpers
{
	public static class CmdHelpers
	{
		public static void ExecuteCmdCommand(string command, bool asAdmin)
		{
			var startInfo = new ProcessStartInfo
							{
								FileName = "CMD.exe",
								Arguments = "/C " + command,
								UseShellExecute = asAdmin,
								RedirectStandardError = !asAdmin,
								CreateNoWindow = true,
							};

			if (asAdmin)
				startInfo.Verb = "runas";

			var proc = new Process { StartInfo = startInfo };

			using (proc)
			{
				proc.Start();
				proc.WaitForExit();

				if (!asAdmin && !proc.StandardError.EndOfStream)
					UnityEngine.Debug.LogError(proc.StandardError.ReadToEnd());
			}
		}
	}
}