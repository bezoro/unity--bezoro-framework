using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Typed facade for manager-style global access.
	/// </summary>
	/// <typeparam name="T">The globally registered component type.</typeparam>
	public static class GlobalReferenceAccess<T> where T : Component
	{
		public static T Instance => GlobalReferenceRegistry.GetRequired<T>();

		public static bool TryGetInstance([NotNullWhen(true)] out T? instance) =>
			GlobalReferenceRegistry.TryGet(out instance);

		public static bool IsCurrent(T? instance) =>
			instance != null &&
			GlobalReferenceRegistry.TryGet(out T? current) &&
			current == instance;
	}
}
