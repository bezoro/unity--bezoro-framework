using UnityEngine;

namespace Bezoro.Framework.Unity.AI
{
	public abstract class AIAgentBehaviour : MonoBehaviour
	{
		public abstract void Tick();

		protected virtual void OnEnable()
		{
			AITickManagerBehaviour.Instance.AddUnique( this );
		}

		protected virtual void OnDisable()
		{
			AITickManagerBehaviour.Instance.Remove( this );
		}
	}
}
