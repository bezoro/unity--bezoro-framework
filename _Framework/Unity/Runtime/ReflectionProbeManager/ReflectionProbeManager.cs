using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Bezoro.Framework.Unity.ReflectionProbeManager
{
	public sealed class ReflectionProbeManager : MonoBehaviour
	{
		private static ReflectionProbeManager _instance;

		private readonly List<ReflectionProbe> _reflectionProbes = new( 128 );

		private                        CustomFixedTick        _customFixedTick;
		[UsedImplicitly] public static ReflectionProbeManager Instance      { get; private set; }
		public                         float                  TickFrequency => 1f;
		public                         double                 Time          => _customFixedTick.FixedTime;
		public                         float                  DeltaTime     => _customFixedTick.CustomFixedDeltaTime;

		private void Awake()
		{
			_customFixedTick = new(
				TickFrequency,
				RenderAllProbes );
		}

		private void Update()
		{
			_customFixedTick.Tick( UnityEngine.Time.deltaTime );
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration )]
		private static void Init()
		{
			Instance = null;
		}

		public void AddUnique( ReflectionProbe reflectionProbe )
		{
			if ( _reflectionProbes.Contains( reflectionProbe ) )
			{
				return;
			}

			_reflectionProbes.Add( reflectionProbe );
		}

		public void Remove( ReflectionProbe reflectionProbe )
		{
			_reflectionProbes.Remove( reflectionProbe );
		}

		private void RenderAllProbes()
		{
			for ( var i = 0 ; i < _reflectionProbes.Count ; i++ )
			{
				_reflectionProbes[ i ].RenderProbe();
			}
		}
	}
}
