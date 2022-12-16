using UnityEngine;
using System.Collections.Generic;
using Bezoro.Core.Helpers;

namespace Bezoro.Core.Singletons
{
	[AddComponentMenu("")]
	public sealed class TickableManager : MonoBehaviour
	{
		public static TickableManager Instance
		{
			get
			{
				if (_instance != null)
					return _instance;

				_instance = SingletonMonoBehaviourHelpers.CreateSingleton<TickableManager>();
				return _instance;
			}
		}

		public void AddUnique(Tickable tickable)
		{
			if (_tickables.Contains(tickable))
				return;

			_tickables.Add(tickable);
		}

		public void Remove(Tickable tickable) =>
			_tickables.Remove(tickable);

		private void Update()
		{
			for (var i = 0; i < _tickables.Count; i++)
				_tickables[i].Tick();
		}

		private void FixedUpdate()
		{
			for (var i = 0; i < _tickables.Count; i++)
				_tickables[i].FixedTick();
		}

		private void LateUpdate()
		{
			for (var i = 0; i < _tickables.Count; i++)
				_tickables[i].LateTick();
		}

		private static TickableManager _instance;

		private readonly List<Tickable> _tickables = new(128);
	}
}