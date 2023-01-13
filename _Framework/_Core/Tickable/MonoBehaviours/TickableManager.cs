using Weaver;
using UnityEngine;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace Bezoro.Core.Singletons
{
	public sealed class TickableManager : MonoBehaviour
	{
		[AssetInstance] [UsedImplicitly]
		public static readonly TickableManager Instance;

		public void AddUniqueEarlyTickable(Tickable tickable)
		{
			if (_earlyTickables.Contains(tickable))
				return;

			_earlyTickables.Add(tickable);
		}

		public void AddUniqueTickable(Tickable tickable)
		{
			if (_tickables.Contains(tickable))
				return;

			_tickables.Add(tickable);
		}

		public void AddUniqueFixedTickable(Tickable tickable)
		{
			if (_fixedTickables.Contains(tickable))
				return;

			_fixedTickables.Add(tickable);
		}

		public void AddUniqueLateTickable(Tickable tickable)
		{
			if (_lateTickables.Contains(tickable))
				return;

			_lateTickables.Add(tickable);
		}

		public void RemoveEarlyTickable(Tickable tickable) =>
			_earlyTickables.Remove(tickable);

		public void RemoveTickable(Tickable tickable) =>
			_tickables.Remove(tickable);

		public void RemoveFixedTickable(Tickable tickable) =>
			_fixedTickables.Remove(tickable);

		public void RemoveLateTickable(Tickable tickable) =>
			_lateTickables.Remove(tickable);

		private void Update()
		{
			for (var i = 0; i < _earlyTickables.Count; i++)
				_earlyTickables[i].EarlyTick();

			for (var i = 0; i < _tickables.Count; i++)
				_tickables[i].Tick();
		}

		private void FixedUpdate()
		{
			for (var i = 0; i < _fixedTickables.Count; i++)
				_fixedTickables[i].FixedTick();
		}

		private void LateUpdate()
		{
			for (var i = 0; i < _lateTickables.Count; i++)
				_lateTickables[i].LateTick();
		}

		private readonly List<Tickable> _earlyTickables = new(128);
		private readonly List<Tickable> _tickables = new(128);
		private readonly List<Tickable> _fixedTickables = new(128);
		private readonly List<Tickable> _lateTickables = new(128);
	}
}