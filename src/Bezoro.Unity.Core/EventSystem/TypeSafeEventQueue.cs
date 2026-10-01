using System;
using System.Collections.Generic;

namespace Bezoro.Unity.Core.EventSystem;

/// <summary>
///     Stores queued events by runtime event type for ordered dispatch.
/// </summary>
public class TypeSafeEventQueue
{
	private readonly Dictionary<Type, object> _queues = new();

	public bool TryDequeueAny(out Type? eventType, out IEvent? eventData)
	{
		lock (_queues)
		{
			foreach (var kvp in _queues)
			{
				var queue = (Queue<IEvent>)kvp.Value;

				if (queue.Count > 0)
				{
					eventType = kvp.Key;
					eventData = queue.Dequeue();

					return true;
				}
			}
		}

		eventType = null;
		eventData = null;

		return false;
	}

	public void Clear()
	{
		lock (_queues)
		{
			_queues.Clear();
		}
	}

	public void Enqueue<T>(T eventData)
		where T : IEvent
	{
		if (eventData == null) throw new ArgumentNullException(nameof(eventData));

		lock (_queues)
		{
			if (!_queues.TryGetValue(
					typeof(T),
					out object queue
				))
			{
				queue              = new Queue<IEvent>();
				_queues[typeof(T)] = queue;
			}

			((Queue<IEvent>)queue).Enqueue(
				eventData
			);
		}
	}
}
