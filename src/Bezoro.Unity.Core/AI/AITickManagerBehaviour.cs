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
		private static AITickManagerBehaviour? _instance;

		private readonly AITickManagerModel<AIAgentBehaviour> _model = new(128);

		private CustomFixedTick? _customFixedTick;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Init() => Instance = null;

		public static AITickManagerBehaviour? Instance { get; private set; }

		public double? Time          => _customFixedTick?.FixedTime;
		public float   TickFrequency => 0.1f;

		public float? DeltaTime => _customFixedTick?.CustomFixedDeltaTime;

		#region Unity Lifecycle

		private void Awake()
		{
			Instance         = this;
			_customFixedTick = new(TickFrequency, Tick);
		}

		private void Update() => _customFixedTick?.Tick(UnityEngine.Time.deltaTime);

		#endregion

		public void AddUnique(AIAgentBehaviour aiAgentBehaviour)
		{
			_model.AddUnique(aiAgentBehaviour);
		}

		public void Remove(AIAgentBehaviour aiAgentBehaviour) => _model.Remove(aiAgentBehaviour);

		private void Tick()
		{
			foreach (var t in _model.Agents)
				t.Tick();
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
