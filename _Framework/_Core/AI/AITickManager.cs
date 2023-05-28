using Weaver;
using UnityEngine;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace Bezoro.Framework.Core.Singletons
{
	public sealed class AITickManager : MonoBehaviour
	{
		[AssetInstance] [UsedImplicitly]
		public static AITickManager Instance { get; private set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Init() =>
			Instance = null;

		public float TickFrequency => 0.1f;

		public double Time => _customFixedTick.FixedTime;

		public float DeltaTime => _customFixedTick.CustomFixedDeltaTime;

		public void AddUnique(AIAgent aiAgent)
		{
			if (_aiAgents.Contains(aiAgent))
				return;

			_aiAgents.Add(aiAgent);
		}

		public void Remove(AIAgent aiAgent) =>
			_aiAgents.Remove(aiAgent);

		private void Awake() =>
			_customFixedTick = new CustomFixedTick(TickFrequency, Tick);

		private void Update() =>
			_customFixedTick.Tick(UnityEngine.Time.deltaTime);

		private void Tick()
		{
			for (var i = 0; i < _aiAgents.Count; i++)
				_aiAgents[i].Tick();
		}

		private static AITickManager _instance;

		private CustomFixedTick _customFixedTick;

		private readonly List<AIAgent> _aiAgents = new(128);
	}
}