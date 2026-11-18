using System;
using System.Collections.Generic;

namespace Bezoro.Unity.Core.EventSystem;

/// <summary>Queues higher priorities first, preserving publication order within each priority.</summary>
public class TypeSafeEventQueue
{
	private readonly StableEventQueue<(Type Type, IEvent Event)> _queue = new();

	public bool TryDequeueAny(out Type? eventType, out IEvent? eventData)
	{
		lock (_queue)
		{
			if (_queue.TryDequeue(out var item))
			{
				(eventType, eventData) = item;
				return true;
			}
			eventType = null;
			eventData = null;
			return false;
		}
	}

	public void Clear()
	{
		lock (_queue) _queue.Clear();
	}

	public void Enqueue<T>(T eventData) where T : IEvent
	{
		if (eventData == null) throw new ArgumentNullException(nameof(eventData));
		lock (_queue) _queue.Enqueue((typeof(T), eventData), eventData.Priority);
	}
}

internal sealed class StableEventQueue<T>
{
	private readonly SortedDictionary<int, Queue<T>> _queues =
		new(Comparer<int>.Create((left, right) => right.CompareTo(left)));

	public void Enqueue(T item, int priority)
	{
		if (!_queues.TryGetValue(priority, out var items))
			_queues.Add(priority, items = new Queue<T>());
		items.Enqueue(item);
	}

	public bool TryDequeue(out T item)
	{
		using var entries = _queues.GetEnumerator();
		if (!entries.MoveNext())
		{
			item = default!;
			return false;
		}

		var first = entries.Current;
		item = first.Value.Dequeue();
		if (first.Value.Count == 0) _queues.Remove(first.Key);
		return true;
	}

	public void Clear() => _queues.Clear();
}
