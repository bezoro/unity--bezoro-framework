using UnityEngine;

namespace Bezoro.Unity.Core.AI
{
	/// <summary>
	///     Registers an AI agent with the shared AI tick manager while enabled.
	/// </summary>
	public abstract class AIAgentBehaviour : MonoBehaviour
	{
		private readonly AIAgentModel _model = new();

		#region Unity Lifecycle

		protected virtual void OnEnable()
		{
			var manager = AITickManagerBehaviour.Instance;
			if (!_model.ShouldRegister(manager != null)) return;

			manager?.AddUnique(
				this
			);
		}

		protected virtual void OnDisable()
		{
			var manager = AITickManagerBehaviour.Instance;
			if (!_model.ShouldRemove(manager != null)) return;

			manager?.Remove(
				this
			);
		}

		#endregion

		#region Abstract Methods

		public abstract void Tick();

		#endregion
	}

	public sealed class AIAgentModel
	{
		public bool ShouldRegister(bool hasManager)
		{
			return hasManager;
		}

		public bool ShouldRemove(bool hasManager)
		{
			return hasManager;
		}
	}
}
