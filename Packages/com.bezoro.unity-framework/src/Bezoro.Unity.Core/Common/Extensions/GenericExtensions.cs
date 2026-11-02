using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Common.Extensions;

/// <summary>
///     Contains extension methods for generic types.
/// </summary>
public static class GenericExtensions
{
	/// <summary>
	///     Determines whether the specified object is null.
	/// </summary>
	/// <typeparam name="T">The type of the object.</typeparam>
	/// <param name="unityObject">The object to check.</param>
	/// <returns>true if the object is null; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsUnityNull<T>([NotNullWhen(false)] this T? unityObject) where T : Object =>
		unityObject == null;

	/// <summary>
	///     Checks if the object is a Unity Object.
	/// </summary>
	/// <typeparam name="T">The type of the object.</typeparam>
	/// <param name="obj">The object to check.</param>
	/// <param name="unityObj">Returns the object as a Unity Object if it is one.</param>
	/// <returns>true if the object is a Unity Object; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsUnityObject<T>(this T obj, out Object unityObj) where T : class
	{
		unityObj = (obj as Object).ThrowIfUnityNull();
		return !unityObj.IsUnityNull();
	}

	/// <summary>
	///     Throws <see cref="System.ArgumentNullException" /> when <paramref name="value" /> is <see langword="null" />.
	///     The method returns the non-null value thus enabling fluent usage:
	///     <code>
	/// var nameLength = user.Name.ThrowIfNull().Length;
	/// </code>
	/// </summary>
	[HideInCallstack]
	public static T ThrowIfUnityNull<T>([NotNull] this T? value, string? paramName = null) where T : Object
	{
		if (value.IsUnityNull())
			throw new ArgumentNullException(paramName ?? typeof(T).Name);

		return value;
	}
}
