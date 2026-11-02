using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Hosts miscellaneous Unity utility functions that do not fit a narrower helper.
/// </summary>
public static class Utils
{
	public static bool VerifyObjectExists(Object obj, string message, Object? context = null)
	{
		if (obj) return true;

		Logger.LogWarning(message, contextObject: context);
		return false;
	}

	public static bool VerifyObjectExists(object? obj, string message, Object? context = null)
	{
		if (obj != null) return true;

		Logger.LogWarning(
			message,
			contextObject: context
		);

		return false;
	}
}
