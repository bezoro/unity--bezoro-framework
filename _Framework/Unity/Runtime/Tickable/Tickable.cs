using System;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	[SelectionBase]
	public abstract class Tickable : MonoBehaviour
	{
		[field: SerializeField]
		public TickType TickType { get; protected set; } = TickType.Normal;

		public abstract void EarlyTick();

		public abstract void Tick();

		public abstract void FixedTick();

		public abstract void LateTick();

		protected virtual void OnEnable()
		{
			RegisterWithTickableManager();

			void RegisterWithTickableManager()
			{
				if (( TickType & TickType.Early ) != 0)
				{
					TickableManager.Instance.AddUniqueEarlyTickable(this);
				}

				if (( TickType & TickType.Normal ) != 0)
				{
					TickableManager.Instance.AddUniqueTickable(this);
				}

				if (( TickType & TickType.Fixed ) != 0)
				{
					TickableManager.Instance.AddUniqueFixedTickable(this);
				}

				if (( TickType & TickType.Late ) != 0)
				{
					TickableManager.Instance.AddUniqueLateTickable(this);
				}
			}
		}

		protected virtual void OnDisable()
		{
			UnregisterFromTickableManager();
		}

		public void AddToTickPool(TickType flag)
		{
			if (( TickType & flag ) != 0)
			{
				return;
			}

			TickType |= flag;

			switch ( flag )
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
			if (( TickType & flag ) == 0)
			{
				return;
			}

			TickType &= ~flag;

			switch ( flag )
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
			if (( TickType & TickType.Early ) != 0)
			{
				TickableManager.Instance.RemoveEarlyTickable(this);
			}

			if (( TickType & TickType.Normal ) != 0)
			{
				TickableManager.Instance.RemoveTickable(this);
			}

			if (( TickType & TickType.Fixed ) != 0)
			{
				TickableManager.Instance.RemoveFixedTickable(this);
			}

			if (( TickType & TickType.Late ) != 0)
			{
				TickableManager.Instance.RemoveLateTickable(this);
			}
		}
	}

	[Flags]
	[Tooltip("Which <b>Tick</b> to receive.")]
	public enum TickType
	{
		Early = 1,
		Normal = 2,
		Fixed = 4,
		Late = 8
	}
}
