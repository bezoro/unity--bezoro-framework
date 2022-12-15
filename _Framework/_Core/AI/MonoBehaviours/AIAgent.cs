using Bezoro.Core.Singletons;
using UnityEngine;

namespace Bezoro.Core
{
	public abstract class AIAgent : MonoBehaviour
	{
		public abstract void Tick();

		protected virtual void OnEnable() =>
			AITickManager.Instance.AddUnique(this);

		protected virtual void OnDisable() =>
			AITickManager.Instance.Remove(this);
	}
}