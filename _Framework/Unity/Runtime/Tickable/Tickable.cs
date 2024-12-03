using System;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	[SelectionBase]
	public abstract class Tickable : MonoBehaviour
	{
		[field: SerializeField] public TickType TickType { get; protected set; } = TickType.Normal;

		protected virtual void OnEnable()
		{
			RegisterWithTickableManager();

			void RegisterWithTickableManager()
			{
				if ( ( TickType & TickType.Early ) != 0 )
				{
					Tickable_Manager.Instance.AddUniqueEarlyTickable( this );
				}

				if ( ( TickType & TickType.Normal ) != 0 )
				{
					Tickable_Manager.Instance.AddUniqueTickable( this );
				}

				if ( ( TickType & TickType.Fixed ) != 0 )
				{
					Tickable_Manager.Instance.AddUniqueFixedTickable( this );
				}

				if ( ( TickType & TickType.Late ) != 0 )
				{
					Tickable_Manager.Instance.AddUniqueLateTickable( this );
				}
			}
		}

		protected virtual void OnDisable()
		{
			UnregisterFromTickableManager();
		}

		public abstract void EarlyTick();

		public abstract void Tick();

		public abstract void FixedTick();

		public abstract void LateTick();

		public void AddToTickPool( TickType flag )
		{
			if ( ( TickType & flag ) != 0 )
			{
				return;
			}

			TickType |= flag;

			switch ( flag )
			{
				case TickType.Early:
					Tickable_Manager.Instance.AddUniqueEarlyTickable( this );
					break;

				case TickType.Normal:
					Tickable_Manager.Instance.AddUniqueTickable( this );
					break;

				case TickType.Fixed:
					Tickable_Manager.Instance.AddUniqueFixedTickable( this );
					break;

				case TickType.Late:
					Tickable_Manager.Instance.AddUniqueLateTickable( this );
					break;
			}
		}

		public void RemoveFromTickPool( TickType flag )
		{
			if ( ( TickType & flag ) == 0 )
			{
				return;
			}

			TickType &= ~flag;

			switch ( flag )
			{
				case TickType.Early:
					Tickable_Manager.Instance.RemoveEarlyTickable( this );
					break;

				case TickType.Normal:
					Tickable_Manager.Instance.RemoveTickable( this );
					break;

				case TickType.Fixed:
					Tickable_Manager.Instance.RemoveFixedTickable( this );
					break;

				case TickType.Late:
					Tickable_Manager.Instance.RemoveLateTickable( this );
					break;
			}
		}

		public void UnregisterFromTickableManager()
		{
			if ( ( TickType & TickType.Early ) != 0 )
			{
				Tickable_Manager.Instance.RemoveEarlyTickable( this );
			}

			if ( ( TickType & TickType.Normal ) != 0 )
			{
				Tickable_Manager.Instance.RemoveTickable( this );
			}

			if ( ( TickType & TickType.Fixed ) != 0 )
			{
				Tickable_Manager.Instance.RemoveFixedTickable( this );
			}

			if ( ( TickType & TickType.Late ) != 0 )
			{
				Tickable_Manager.Instance.RemoveLateTickable( this );
			}
		}
	}

	[Flags]
	[Tooltip( "Which <b>Tick</b> to receive." )]
	public enum TickType
	{
		Early  = 1,
		Normal = 2,
		Fixed  = 4,
		Late   = 8
	}
}
