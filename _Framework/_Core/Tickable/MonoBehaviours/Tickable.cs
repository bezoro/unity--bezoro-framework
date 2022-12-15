using Bezoro.Core.Singletons;
using UnityEngine;

namespace Bezoro.Core
{
	[SelectionBase]
	public abstract class Tickable : MonoBehaviour
	{
		public abstract void Tick();

		public abstract void FixedTick();

		public abstract void LateTick();

		protected virtual void OnEnable() =>
			TickableManager.Instance.AddUnique(this);

		protected virtual void OnDisable() =>
			TickableManager.Instance.Remove(this);
	}
}