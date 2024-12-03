using UnityEngine;

namespace Bezoro.Framework.Unity.AI
{
	public abstract class AIAgentBehaviour : MonoBehaviour
	{
		protected virtual void OnEnable()
		{
			AITickManagerBehaviour.Instance.AddUnique( this );
		}

		protected virtual void OnDisable()
		{
			AITickManagerBehaviour.Instance.Remove( this );
		}

		public abstract void Tick();
	}
}
