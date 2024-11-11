using System;
using System.Collections.Concurrent;

namespace Bezoro.Framework.Unity
{
	public class TypeSafeEventPool<T> where T : IEvent
	{
		private readonly ConcurrentBag<T> _pool = new();
		private readonly int _maxPoolSize;
		private readonly Func<T> _factory;

		public TypeSafeEventPool(int maxPoolSize, Func<T> factory)
		{
			_maxPoolSize = maxPoolSize;
			_factory = factory;
		}

		public T Get()
		{
			return _pool.TryTake(out var item) ? item : _factory();
		}

		public void Return(T item)
		{
			if (_pool.Count < _maxPoolSize)
			{
				_pool.Add(item);
			}
		}
	}
}
