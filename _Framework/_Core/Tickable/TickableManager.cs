using Weaver;
using UnityEngine;
using JetBrains.Annotations;
using Bezoro.Framework.Core.Helpers;

namespace Bezoro.Framework.Core.Singletons
{
	public sealed class TickableManager : MonoBehaviour
	{
		[AssetInstance] [UsedImplicitly]
		public static TickableManager Instance { get; private set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Init() =>
			Instance = null;

		public void AddUniqueEarlyTickable(Tickable tickable) =>
			ArrayHelpers.AddUnique(ref _earlyTickables, tickable, ref _earlyTickablesCount, out _);

		public void AddUniqueTickable(Tickable tickable) =>
			ArrayHelpers.AddUnique(ref _tickables, tickable, ref _tickablesCount, out _);

		public void AddUniqueFixedTickable(Tickable tickable) =>
			ArrayHelpers.AddUnique(ref _fixedTickables, tickable, ref _fixedTickablesCount, out _);

		public void AddUniqueLateTickable(Tickable tickable) =>
			ArrayHelpers.AddUnique(ref _lateTickables, tickable, ref _lateTickablesCount, out _);

		public void RemoveEarlyTickable(Tickable tickable) =>
			ArrayHelpers.Remove(ref _earlyTickables, tickable, ref _earlyTickablesCount, out _);

		public void RemoveTickable(Tickable tickable) =>
			ArrayHelpers.Remove(ref _tickables, tickable, ref _tickablesCount, out _);

		public void RemoveFixedTickable(Tickable tickable) =>
			ArrayHelpers.Remove(ref _fixedTickables, tickable, ref _fixedTickablesCount, out _);

		public void RemoveLateTickable(Tickable tickable) =>
			ArrayHelpers.Remove(ref _lateTickables, tickable, ref _lateTickablesCount, out _);

		private void Update()
		{
			for (var i = 0; i < _earlyTickablesCount; i++)
				_earlyTickables[i].EarlyTick();

			for (var i = 0; i < _tickablesCount; i++)
				_tickables[i].Tick();
		}

		private void FixedUpdate()
		{
			for (var i = 0; i < _fixedTickablesCount; i++)
				_fixedTickables[i].FixedTick();
		}

		private void LateUpdate()
		{
			for (var i = 0; i < _lateTickablesCount; i++)
				_lateTickables[i].LateTick();
		}

		private Tickable[] _earlyTickables = new Tickable[128];
		private int _earlyTickablesCount;

		private Tickable[] _tickables = new Tickable[128];
		private int _tickablesCount;

		private Tickable[] _fixedTickables = new Tickable[128];
		private int _fixedTickablesCount;

		private Tickable[] _lateTickables = new Tickable[128];
		private int _lateTickablesCount;
	}
}