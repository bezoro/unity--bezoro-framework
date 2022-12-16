using System.Collections.Generic;
using Bezoro.Core.Helpers;
using UnityEngine;

namespace Bezoro.Core.Singletons
{
	[AddComponentMenu("")]
	public sealed class AITickManager : MonoBehaviour
	{
		public static AITickManager Instance
		{
			get
			{
				if (_instance != null)
					return _instance;

				_instance = SingletonMonoBehaviourHelpers.CreateSingleton<AITickManager>();
				return _instance;
			}
		}

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