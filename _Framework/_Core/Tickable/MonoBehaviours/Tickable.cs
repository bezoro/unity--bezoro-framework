using System;
using UnityEngine;
using Bezoro.Core.Singletons;

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
			RegisterWithTickableManager();

			void RegisterWithTickableManager()
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
		}

		protected virtual void OnDisable() =>
			UnregisterFromTickableManager();

		public void AddToTickPool(TickType flag)
		{
			if ((tickType & flag) != 0)
				return;

			tickType |= flag;

			switch (flag)
			{
				case TickType.Early:
					TickableManager.Instance.AddUniqueEarlyTickable(this);
					break;
				case TickType.Normal:
					TickableManager.Instance.AddUniqueTickable(this);
					break;
				case TickType.Fixed:
					TickableManager.Instance.AddUniqueFixedTickable(this);
					break;
				case TickType.Late:
					TickableManager.Instance.AddUniqueLateTickable(this);
					break;
			}
		}

		public void RemoveFromTickPool(TickType flag)
		{
			if ((tickType & flag) == 0)
				return;

			tickType &= ~flag;

			switch (flag)
			{
				case TickType.Early:
					TickableManager.Instance.RemoveEarlyTickable(this);
					break;
				case TickType.Normal:
					TickableManager.Instance.RemoveTickable(this);
					break;
				case TickType.Fixed:
					TickableManager.Instance.RemoveFixedTickable(this);
					break;
				case TickType.Late:
					TickableManager.Instance.RemoveLateTickable(this);
					break;
			}
		}

		public void UnregisterFromTickableManager()
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