using System;
using System.Collections.Concurrent;

namespace Bezoro.Unity.Core.EventSystem;

public interface ITypeSafeEventPool
{
	void Return(IEvent item);
}

/// <summary>
///     Reuses event instances of a single event type up to a configured pool size.
/// </summary>
public class TypeSafeEventPool<T>
	: ITypeSafeEventPool
	where T : IEvent
{
	private readonly ConcurrentBag<T> _pool = new();

	private readonly Func<T> _factory;
	private readonly int     _maxPoolSize;

	public TypeSafeEventPool(int maxPoolSize, Func<T> factory)
	{
		_maxPoolSize = maxPoolSize;
		_factory     = factory;
	}

	public T Get() =>
		_pool.TryTake(
			out var item
		)
			? item
			: _factory();

	public void Return(T item)
	{
		if (_pool.Count < _maxPoolSize)
			_pool.Add(
				item
			);
	}

	void ITypeSafeEventPool.Return(IEvent item)
	{
		if (item is T typedItem)
			Return(
				typedItem
			);
	}
}
