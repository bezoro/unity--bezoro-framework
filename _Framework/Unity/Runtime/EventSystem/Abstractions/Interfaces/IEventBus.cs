using System;
using Bezoro.Framework.Unity.DI;
using Cysharp.Threading.Tasks;

namespace Bezoro.Framework.Unity
{
	public interface IEventBus : IService, IDisposable
	{
		void    SubscribeUnique<T>(Action<T> handler) where T : IEvent;
		void    Unsubscribe<T>(Action<T> handler) where T : IEvent;
		void    UnsubscribeAll<T>() where T : IEvent;
		UniTask Publish<T>(T eventData) where T : IEvent;
		UniTask PublishAsync<T>(T eventData) where T : IEvent;
		bool    HasSubscribersFor<T>() where T : IEvent;
		void    Clear();
		void    CancelEventProcessing();
		TEvent  CreateEvent<TEvent>() where TEvent : IEvent, new();
		TEvent  CreateEvent<TEvent, TPayload>(TPayload payload) where TEvent : IEvent<TPayload>, new();
	}
}
