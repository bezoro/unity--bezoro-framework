using System;
using System.Collections.Generic;

namespace Bezoro.Framework.Unity
{
	public abstract class ObservableValueProperty<T>
	where T : struct
	{
		private readonly object _lock = new();
		private          T      _currentValue;

		public T Value
		{
			get
			{
				lock ( _lock )
				{
					return _currentValue;
				}
			}
			set
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
		}

		public event Action<T> OnValueChanged;

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
