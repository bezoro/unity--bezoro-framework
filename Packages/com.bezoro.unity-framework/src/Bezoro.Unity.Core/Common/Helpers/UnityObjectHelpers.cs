using System.Diagnostics;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Handles UnityEngine.Object null checks and object lookup helpers.
/// </summary>
public static class UnityObjectHelpers
{
	/// <summary>
	///     Validates the given Unity object and throws an exception if it is null.
	/// </summary>
	/// <param name="obj">The Unity object to validate.</param>
	/// <exception cref="System.Exception">Thrown when the object is null.</exception>
	[Conditional(
		"UNITY_EDITOR"
	)]
	public static void ValidateUnityObject(Object obj)
	{
		if (obj == null)
			throw new(
				"Object is null."
			);
	}
}
