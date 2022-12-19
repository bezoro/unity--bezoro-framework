using System;
using Bezoro.Core.Singletons;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Bezoro.Core
{
	[SelectionBase]
	public abstract class Tickable : MonoBehaviour
	{
		[SerializeField]
		private TickType tickType = TickType.Normal;

		public abstract void EarlyTick();

		public abstract void Tick();

		public abstract void FixedTick();

		public abstract void LateTick();

		protected virtual void OnEnable()
		{
			if ((tickType & TickType.Early) != 0)
				TickableManager.Instance.AddUniqueEarlyTickable(this);

			if ((tickType & TickType.Normal) != 0)
				TickableManager.Instance.AddUniqueTickable(this);

			if ((tickType & TickType.Fixed) != 0)
				TickableManager.Instance.AddUniqueFixedTickable(this);

			if ((tickType & TickType.Late) != 0)
				TickableManager.Instance.AddUniqueLateTickable(this);
		}

		protected virtual void OnDisable()
		{
			if ((tickType & TickType.Early) != 0)
				TickableManager.Instance.RemoveEarlyTickable(this);

			if ((tickType & TickType.Normal) != 0)
				TickableManager.Instance.RemoveTickable(this);

			if ((tickType & TickType.Fixed) != 0)
				TickableManager.Instance.RemoveFixedTickable(this);

			if ((tickType & TickType.Late) != 0)
				TickableManager.Instance.RemoveLateTickable(this);
		}

		[Button]
		public void AddTickTypeFlag(TickType value)
		{
			tickType |= value;

			if ((tickType & TickType.Early) != 0)
				TickableManager.Instance.AddUniqueEarlyTickable(this);

			if ((tickType & TickType.Normal) != 0)
				TickableManager.Instance.AddUniqueTickable(this);

			if ((tickType & TickType.Fixed) != 0)
				TickableManager.Instance.AddUniqueFixedTickable(this);

			if ((tickType & TickType.Late) != 0)
				TickableManager.Instance.AddUniqueLateTickable(this);
		}

		[Button]
		public void RemoveTickTypeFlag(TickType value)
		{
			tickType &= ~value;

			if ((value & TickType.Early) != 0)
				TickableManager.Instance.RemoveEarlyTickable(this);

			if ((value & TickType.Normal) != 0)
				TickableManager.Instance.RemoveTickable(this);

			if ((value & TickType.Fixed) != 0)
				TickableManager.Instance.RemoveFixedTickable(this);

			if ((value & TickType.Late) != 0)
				TickableManager.Instance.RemoveLateTickable(this);
		}
	}

	[Flags]
	public enum TickType
	{
		Early = 1,
		Normal = 2,
		Fixed = 4,
		Late = 8,
	}
}