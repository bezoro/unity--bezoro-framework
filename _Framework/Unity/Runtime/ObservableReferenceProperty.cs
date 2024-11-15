using System;
using System.Collections.Generic;

namespace Bezoro.Framework.Unity
{
	public abstract class ObservableReferenceProperty<T> where T : class
	{
		private T _currentValue;
		private readonly object _lock = new();
		public event Action<T> OnValueChanged;

		public T Value
		{
			get
			{
				lock (_lock)
				{
					return _currentValue;
				}
			}
			set
			{
				lock (_lock)
				{
					if ( EqualityComparer<T>.Default.Equals( _currentValue, value ) )
					{
						return;
					}

					_currentValue = value;
					OnValueChanged?.Invoke( value );
				}
			}
		}

		public void Subscribe( Action<T> handler )
		{
			lock (_lock)
			{
				OnValueChanged += handler;
			}
		}

		public void Unsubscribe( Action<T> handler )
		{
			lock (_lock)
			{
				OnValueChanged -= handler;
			}
		}
	}
}
