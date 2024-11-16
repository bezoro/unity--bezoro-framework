using System;
using System.Collections.Generic;

namespace Bezoro.Framework.Unity
{
	public class TypeSafeEventQueue
	{
		private readonly Dictionary<Type, object> _queues = new();

		public void Enqueue<T>( T eventData ) where T : IEvent
		{
			lock (_queues)
			{
				if ( !_queues.TryGetValue( typeof(T), out var queue ) )
				{
					queue              = new Queue<T>();
					_queues[typeof(T)] = queue;
				}

				( (Queue<T>)queue ).Enqueue( eventData );
			}
		}

		public bool TryDequeueAny( out Type eventType, out IEvent eventData )
		{
			lock (_queues)
			{
				foreach ( var kvp in _queues )
				{
					var queue = (Queue<IEvent>)kvp.Value;

					if ( queue.Count > 0 )
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
	}
}
