using UnityEngine;
using Bezoro.Framework.Core.Singletons;

namespace Bezoro.Framework.Core
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