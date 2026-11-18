using UnityEngine;

namespace Bezoro.Unity.Core.AI
{
	/// <summary>
	///     Registers an AI agent with the shared AI tick manager while enabled.
	/// </summary>
	public abstract class AIAgentBehaviour : MonoBehaviour
	{
		private AITickManagerBehaviour? _owner;

		#region Unity Lifecycle

		protected virtual void OnEnable()
		{
			AITickManagerBehaviour.OwnerChanged += Bind;
			Bind(AITickManagerBehaviour.Instance);
		}

		protected virtual void OnDisable()
		{
			AITickManagerBehaviour.OwnerChanged -= Bind;
			Bind(null);
		}

		private void Bind(AITickManagerBehaviour? owner)
		{
			if (ReferenceEquals(_owner, owner)) return;
			if (_owner != null) _owner.Remove(this);
			_owner = owner;
			if (_owner != null) _owner.AddUnique(this);
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
