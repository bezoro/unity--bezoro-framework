using System;
using System.Threading.Tasks;

namespace Bezoro.Framework.Core
{
	public static class Validation
	{
		public static void TryCatch(Action action, Exception exception = null)
		{
#if UNITY_EDITOR
			try
			{
#endif
				action();
#if UNITY_EDITOR
			}
			catch ( Exception e )
			{
				throw exception ?? e;
			}
#endif
		}

		public static async Task TryCatch(Func<Task> action, Exception exception = null)
		{
#if UNITY_EDITOR
			try
			{
#endif
				await action();
#if UNITY_EDITOR
			}
			catch ( Exception e )
			{
				throw exception ?? e;
			}
#endif
		}
	}
}
