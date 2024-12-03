using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.Framework.Unity.AI
{
	public sealed class AITickManagerBehaviour : MonoBehaviour
	{
		private static   AITickManagerBehaviour _instance;
		private readonly List<AIAgentBehaviour> _ai_agents = new( 128 );
		private          CustomFixedTick        _custom_fixed_tick;
		public static    AITickManagerBehaviour Instance      { get; private set; }
		public           float                  TickFrequency => 0.1f;
		public           double                 Time          => _custom_fixed_tick.FixedTime;
		public           float                  DeltaTime     => _custom_fixed_tick.CustomFixedDeltaTime;

		private void Awake()
		{
			Instance           = this;
			_custom_fixed_tick = new( TickFrequency, Tick );
		}

		private void Update()
		{
			_custom_fixed_tick.Tick( UnityEngine.Time.deltaTime );
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration )]
		private static void Init()
		{
			Instance = null;
		}

		private void Tick()
		{
			for ( var i = 0 ; i < _ai_agents.Count ; i++ ) { _ai_agents[ i ].Tick(); }
		}

		public void AddUnique( AIAgentBehaviour ai_agent_behaviour )
		{
			if ( _ai_agents.Contains( ai_agent_behaviour ) ) { return; }

			_ai_agents.Add( ai_agent_behaviour );
		}

		public void Remove( AIAgentBehaviour ai_agent_behaviour )
		{
			_ai_agents.Remove( ai_agent_behaviour );
		}
	}
}
