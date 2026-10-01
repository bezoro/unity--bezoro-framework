using System;
using System.Diagnostics;
using UnityEngine;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Validates common Unity references and collection inputs.
/// </summary>
public static class ValidationHelpers
{
	[Conditional(
		"UNITY_EDITOR"
	)]
	public static void RequiredComponent<T>(T component) where T : Component
	{
		if (!component)
			throw new ArgumentNullException(
				$"{typeof(T).Name} requires a {typeof(T).Name} component"
			);
	}
}
