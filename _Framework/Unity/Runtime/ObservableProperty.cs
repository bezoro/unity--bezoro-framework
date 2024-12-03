using System;
using System.Collections.Generic;

namespace Bezoro.Framework.Unity
{
	public abstract class ObservableProperty<T>
	{
		private readonly object _lock = new();
		private          T      _currentValue;

		public T Value
		{
			get => GetLockedValue();
			set => SetLockedValue( value );
		}

		public event Action<T> OnValueChanged;

		private T GetLockedValue()
		{
			lock ( _lock )
			{
				return _currentValue;
			}
		}

		private void SetLockedValue( T value )
		{
			lock ( _lock )
			{
				if ( EqualityComparer<T>.Default.Equals(
						_currentValue,
						value ) )
				{
					return;
				}

				_currentValue = value;
				OnValueChanged?.Invoke( value );
			}
		}

		public void Subscribe( Action<T> handler )
		{
			lock ( _lock )
			{
				OnValueChanged += handler;
			}
		}

		public void Unsubscribe( Action<T> handler )
		{
			lock ( _lock )
			{
				OnValueChanged -= handler;
			}
		}
	}
}
