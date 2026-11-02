using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Common.Serialization;

/// <summary>
///     Stores a Unity object reference and exposes it as an interface type.
/// </summary>
public abstract class BaseSerializableInterface<TInterface>
	where TInterface : class
{
	protected CacheState State = CacheState.Unset;

	protected TInterface? CachedValue;

	public TInterface Value
	{
		get => GetCachedValue();
		set => SetValue(
			value
		);
	}

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	protected abstract TInterface? GetValue();

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	protected virtual void SetValue(TInterface value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		if (State != CacheState.Unset)
			throw new InvalidOperationException(
				"Value can only be set once."
			);

		CachedValue = value;
		State       = CacheState.Set;
	}

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	private TInterface GetCachedValue()
	{
		// Inspector edits and Unity deserialization update the serialized references directly.
		// Resolve that reference before using a previously cached value.
		CachedValue = GetValue();
		if (CachedValue == null || CachedValue is Object unityObject && unityObject == null)
			throw new InvalidOperationException("No compatible interface reference is assigned.");

		State = CacheState.Cached;
		return CachedValue;
	}

	/// <summary>
	///     Lists the supported Cache State values.
	/// </summary>
	protected enum CacheState
	{
		Unset,
		Set,
		Cached
	}
}

[Serializable]
/// <summary>
/// Stores a Unity object reference implementing the requested interface.
/// </summary>
public sealed class SerializableInterface<TInterface, TImplementation> : BaseSerializableInterface<TInterface>,
	ISerializableInterface
	where TInterface : class
	where TImplementation : class, TInterface
{
	private static readonly bool IsUnityObjectType =
		typeof(Object).IsAssignableFrom(
			typeof(TImplementation)
		);

	[SerializeField] private Object?          unityReference;
	[SerializeField] private TImplementation? rawReference;

	public SerializableInterface() { }

	public SerializableInterface(TInterface value)
	{
		SetValue(
			value
		);
	}

	public ReferenceMode Mode => IsUnityObjectType ? ReferenceMode.Unity : ReferenceMode.Raw;

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	public bool TryGetObject(out Object? unityObject)
	{
		unityObject = IsUnityObjectType ? unityReference : null;
		return IsUnityObjectType;
	}

	protected override TInterface? GetValue() =>
		IsUnityObjectType ? unityReference as TInterface : rawReference;

	protected override void SetValue(TInterface value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));

		base.SetValue(
			value
		);

		if (IsUnityObjectType && value is Object unityObject)
		{
			unityReference = unityObject;
			rawReference   = null;
		}
		else
		{
			rawReference   = value as TImplementation;
			unityReference = null;
		}
	}

	object? ISerializableInterface.GetRawReference() => rawReference;
}

[Serializable]
/// <summary>
/// Stores a Unity object reference implementing the requested interface and base object type.
/// </summary>
public sealed class SerializableInterface<TInterface> : BaseSerializableInterface<TInterface>,
	ISerializableInterface
	where TInterface : class
{
	[SerializeField]     private Object?     unityReference;
	[SerializeReference] private TInterface? rawReference;

	public SerializableInterface() { }

	public SerializableInterface(TInterface value)
	{
		SetValue(
			value
		);
	}

	public static implicit operator SerializableInterface<TInterface>(TInterface value) => new(
		value
	);

	public static implicit operator TInterface(SerializableInterface<TInterface> serializableInterface) =>
		serializableInterface.Value;

	public bool TryGetObject(out Object? unityObject)
	{
		unityObject = unityReference;
		return unityObject;
	}

	protected override TInterface? GetValue() =>
		unityReference != null ? unityReference as TInterface : rawReference;

	protected override void SetValue(TInterface v)
	{
		if (v == null) throw new ArgumentNullException(nameof(v));

		base.SetValue(
			v
		);

		if (v is Object unityObj)
		{
			unityReference = unityObj;
			rawReference   = null;
		}
		else
		{
			rawReference   = v;
			unityReference = null;
		}
	}

	object? ISerializableInterface.GetRawReference() => rawReference;
}

/// <summary>
///     Lists the supported Reference Mode values.
/// </summary>
public enum ReferenceMode
{
	Raw,
	Unity
}

/// <summary>
///     Defines access to a serialized object implementing an interface.
/// </summary>
public interface ISerializableInterface
{
	public bool    TryGetObject(out Object? unityObject);
	public object? GetRawReference();
}
