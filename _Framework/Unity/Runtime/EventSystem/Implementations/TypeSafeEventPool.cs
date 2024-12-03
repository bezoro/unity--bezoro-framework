using System;
using System.Collections.Concurrent;

namespace Bezoro.Framework.Unity.EventSystem
{
	public class TypeSafeEventPool<T>
	where T : IEvent
	{
		private readonly Func<T>          _factory;
		private readonly int              _maxPoolSize;
		private readonly ConcurrentBag<T> _pool = new();

		public TypeSafeEventPool( int maxPoolSize, Func<T> factory )
		{
			_maxPoolSize = maxPoolSize;
			_factory     = factory;
		}

		public T Get()
		{
			return _pool.TryTake( out var item ) ? item : _factory();
		}

		public void Return( T item )
		{
			if ( _pool.Count < _maxPoolSize )
			{
				_pool.Add( item );
			}
		}
	}
}
