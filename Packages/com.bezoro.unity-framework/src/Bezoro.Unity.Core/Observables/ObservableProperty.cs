using System;
using System.Collections.Generic;

namespace Bezoro.Unity.Core.Observables;

/// <summary>
///     Stores a thread-safe value. A setter synchronously notifies its subscriber snapshot
///     after releasing the value lock. Concurrent setters may deliver notifications concurrently.
/// </summary>
public abstract class ObservableProperty<T>
{
	private readonly object _lock = new();
	private          T      _currentValue = default!;
	public event Action<T>? OnValueChanged;

	public T Value
	{
		get => GetLockedValue();
		set => SetLockedValue(
			value
		);
	}

	public void Subscribe(Action<T> handler)
	{
		lock (_lock)
		{
			OnValueChanged += handler;
		}
	}

	public void Unsubscribe(Action<T> handler)
	{
		lock (_lock)
		{
			OnValueChanged -= handler;
		}
	}

	private T GetLockedValue()
	{
		lock (_lock)
		{
			return _currentValue;
		}
	}

	private void SetLockedValue(T value)
	{
		Action<T>? handlers;
		lock (_lock)
		{
			if (EqualityComparer<T>.Default.Equals(
					_currentValue,
					value
				))
				return;

			_currentValue = value;
			handlers = OnValueChanged;
		}
		handlers?.Invoke(value);
	}
}
