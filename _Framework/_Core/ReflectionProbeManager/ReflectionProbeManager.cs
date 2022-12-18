using UnityEngine;
using System.Collections.Generic;
using Bezoro.Core.Helpers;
using JetBrains.Annotations;
using Weaver;

namespace Bezoro.Core.Singletons
{
	public sealed class ReflectionProbeManager : MonoBehaviour
	{
		[AssetInstance] [UsedImplicitly]
		public static readonly ReflectionProbeManager Instance;

		public float TickFrequency => 1f;

		public double Time => _customFixedTick.FixedTime;

		public float DeltaTime => _customFixedTick.CustomFixedDeltaTime;

		public void AddUnique(ReflectionProbe reflectionProbe)
		{
			if (_reflectionProbes.Contains(reflectionProbe))
				return;

			_reflectionProbes.Add(reflectionProbe);
		}

		public void Remove(ReflectionProbe reflectionProbe) =>
			_reflectionProbes.Remove(reflectionProbe);

		private void Awake() =>
			_customFixedTick = new CustomFixedTick(TickFrequency, RenderAllProbes);

		private void Update() =>
			_customFixedTick.Tick(UnityEngine.Time.deltaTime);

		private void RenderAllProbes()
		{
			for (var i = 0; i < _reflectionProbes.Count; i++)
				_reflectionProbes[i].RenderProbe();
		}

		private static ReflectionProbeManager _instance;

		private CustomFixedTick _customFixedTick;

		private readonly List<ReflectionProbe> _reflectionProbes = new(128);
	}
}