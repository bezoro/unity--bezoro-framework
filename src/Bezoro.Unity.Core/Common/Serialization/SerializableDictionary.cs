using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Bezoro.Unity.Core.Common.Serialization;

/// <summary>
///     A Unity-friendly dictionary that serialises by keeping a shadow
///     List for keys and one for values.
///     • Allocations are reduced by re-using the two Lists
///     • List capacities are pre-sized to avoid repeated growing
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue> :
	IDictionary<TKey, TValue>,
	ISerializationCallbackReceiver
{
	private const string MismatchedListWarning =
		"[SerializableDictionary<{0},{1}>] Deserialized with mismatched key/value list sizes. " +
		"Keys = {2}, Values = {3}. Extra entries were ignored.";

	[SerializeField] private List<TKey>   keys   = new();
	[SerializeField] private List<TValue> values = new();

	public bool IsReadOnly => false;

	public ICollection<TKey>   Keys   => Dictionary.Keys;
	public ICollection<TValue> Values => Dictionary.Values;
	public int                 Count  => Dictionary.Count;

	public Dictionary<TKey, TValue> Dictionary { get; private set; } = new();

	public TValue this[TKey key]
	{
		[MethodImpl(
			MethodImplOptions.AggressiveInlining
		)]
		get => Dictionary[key];

		[MethodImpl(
			MethodImplOptions.AggressiveInlining
		)]
		set => Dictionary[key] = value;
	}

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	public bool Contains(KeyValuePair<TKey, TValue> item) => Dictionary.ContainsKey(
		item.Key
	);

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	public bool ContainsKey(TKey key) => Dictionary.ContainsKey(
		key
	);

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	public bool Remove(TKey key) => Dictionary.Remove(
		key
	);

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	public bool Remove(KeyValuePair<TKey, TValue> item) => Dictionary.Remove(
		item.Key
	);

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	public bool TryGetValue(TKey key, out TValue value)
		=> Dictionary.TryGetValue(
			key,
			out value
		);

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	public void Add(TKey key, TValue value) => Dictionary.Add(
		key,
		value
	);

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	public void Add(KeyValuePair<TKey, TValue> item) => Dictionary.Add(
		item.Key,
		item.Value
	);

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	public void Clear() => Dictionary.Clear();

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		// Dictionary<TKey,TValue>.Enumerator is a struct – no GC alloc here.
		foreach (var kvp in Dictionary) array[arrayIndex++] = kvp;
	}

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	public void OnAfterDeserialize()
	{
		if (keys.Count == 0 || values.Count == 0)
		{
			Dictionary.Clear();
			return;
		}

		if (Dictionary.Count == 0 || Dictionary.Comparer == null || Dictionary.Count < keys.Count)
			Dictionary = new(
				keys.Count
			);

		Dictionary.Clear();

		for (var i = 0; i < keys.Count && i < values.Count; ++i)
		{
			Dictionary.TryAdd(
				keys[i],
				values[i]
			);
		}

		if (keys.Count != values.Count)
			Debug.LogWarningFormat(
				MismatchedListWarning,
				typeof(TKey).Name,
				typeof(TValue).Name,
				keys.Count,
				values.Count
			);
	}

	[MethodImpl(
		MethodImplOptions.AggressiveInlining
	)]
	public void OnBeforeSerialize()
	{
		int count = Dictionary.Count;

		keys.Clear();
		values.Clear();

		if (keys.Capacity < count) keys.Capacity     = count;
		if (values.Capacity < count) values.Capacity = count;

		keys.AddRange(
			Dictionary.Keys
		);

		values.AddRange(
			Dictionary.Values
		);
	}

	IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();
}
