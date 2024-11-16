using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.Framework.Unity.AI
{
	public sealed class AITickManagerBehaviour : MonoBehaviour
	{
		public static AITickManagerBehaviour Instance { get; private set; }
		public float TickFrequency => 0.1f;
		public double Time => _customFixedTick.FixedTime;
		public float DeltaTime => _customFixedTick.CustomFixedDeltaTime;

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration )]
		private static void Init()
		{
			Instance = null;
		}

		private void Awake()
		{
			Instance         = this;
			_customFixedTick = new(TickFrequency, Tick);
		}

		private void Update()
		{
			_customFixedTick.Tick( UnityEngine.Time.deltaTime );
		}

		private void Tick()
		{
			for ( var i = 0 ; i < _aiAgents.Count ; i++ )
			{
				_aiAgents[i].Tick();
			}
		}

		public void AddUnique( AIAgentBehaviour aiAgentBehaviour )
		{
			if ( _aiAgents.Contains( aiAgentBehaviour ) )
			{
				return;
			}

			_aiAgents.Add( aiAgentBehaviour );
		}

		public void Remove( AIAgentBehaviour aiAgentBehaviour )
		{
			_aiAgents.Remove( aiAgentBehaviour );
		}

		private static AITickManagerBehaviour _instance;

		private CustomFixedTick _customFixedTick;

		private readonly List<AIAgentBehaviour> _aiAgents = new(128);
	}
}
