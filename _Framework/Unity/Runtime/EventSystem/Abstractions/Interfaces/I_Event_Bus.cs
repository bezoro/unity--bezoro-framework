using System;
using Bezoro.Framework.Unity.DI;
using Cysharp.Threading.Tasks;

namespace Bezoro.Framework.Unity.EventSystem
{
	public interface I_Event_Bus : IService,
		IDisposable
	{
		public void SubscribeUnique<T>( Action<T> handler )
		where T : IEvent;

		public void Unsubscribe<T>( Action<T> handler )
		where T : IEvent;

		public void UnsubscribeAll<T>()
		where T : IEvent;

		public UniTask Publish<T>( T eventData )
		where T : IEvent;

		public UniTask PublishAsync<T>( T eventData )
		where T : IEvent;

		public bool HasSubscribersFor<T>()
		where T : IEvent;

		public void Clear();
		public void CancelEventProcessing();

		public TEvent CreateEvent<TEvent>()
		where TEvent : IEvent, new();

		public TEvent CreateEvent<TEvent, TPayload>( TPayload payload )
		where TEvent : IEvent<TPayload>, new();
	}
}
