using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using Weaver;

namespace Bezoro.Core.Singletons
{
	public sealed class TickableManager : MonoBehaviour
	{
		[AssetInstance] [UsedImplicitly]
		public static readonly TickableManager Instance;

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