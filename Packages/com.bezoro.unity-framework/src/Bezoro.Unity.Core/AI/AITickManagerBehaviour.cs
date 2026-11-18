using System;
using System.Collections.Generic;
using Bezoro.Unity.Core.Common.Timing;
using UnityEngine;

namespace Bezoro.Unity.Core.AI
{
	/// <summary>
	///     Ticks registered AI agents at a fixed interval from Update.
	/// </summary>
	public sealed class AITickManagerBehaviour : MonoBehaviour
	{
		private static readonly List<AITickManagerBehaviour> Managers = new();
		internal static event Action<AITickManagerBehaviour?>? OwnerChanged;
		private readonly List<AIAgentBehaviour> _snapshot = new(128);

		private readonly AITickManagerModel<AIAgentBehaviour> _model = new(128);

		private CustomFixedTick? _customFixedTick;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Init()
		{
			Managers.Clear();
			OwnerChanged = null;
			Instance = null;
		}

		public static AITickManagerBehaviour? Instance { get; private set; }

		public double? Time          => _customFixedTick?.FixedTime;
		public float   TickFrequency => 0.1f;

		public float? DeltaTime => _customFixedTick?.CustomFixedDeltaTime;

		#region Unity Lifecycle

		private void OnEnable()
		{
			_customFixedTick = new(TickFrequency, Tick);
			Managers.Add(this);
			if (Instance != null) return;
			Instance = this;
			OwnerChanged?.Invoke(this);
		}

		private void OnDisable()
		{
			Managers.Remove(this);
			if (Instance != this) return;
			Instance = Managers.Count == 0 ? null : Managers[0];
			OwnerChanged?.Invoke(Instance);
		}

		private void Update()
		{
			if (Instance == this) _customFixedTick?.Tick(UnityEngine.Time.deltaTime);
		}

		#endregion

		public void AddUnique(AIAgentBehaviour aiAgentBehaviour)
		{
			_model.AddUnique(aiAgentBehaviour);
		}

		public void Remove(AIAgentBehaviour aiAgentBehaviour) => _model.Remove(aiAgentBehaviour);

		private void Tick()
		{
			_snapshot.Clear();
			_snapshot.AddRange(_model.Agents);
			foreach (var agent in _snapshot)
			{
				if (Instance != this) break;
				if (agent != null && agent.isActiveAndEnabled) agent.Tick();
			}
			_snapshot.Clear();
		}
	}

	public sealed class AITickManagerModel<TAgent>
	{
		private readonly List<TAgent> _agents;

		public AITickManagerModel(int capacity)
		{
			_agents = new(capacity);
		}

		public IReadOnlyList<TAgent> Agents => _agents;

		public bool AddUnique(TAgent agent)
		{
			if (_agents.Contains(agent)) return false;

			_agents.Add(agent);
			return true;
		}

		public bool Remove(TAgent agent)
		{
			return _agents.Remove(agent);
		}
	}
}
