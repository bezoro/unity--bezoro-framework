using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Bezoro.Unity.Core.Common.Enums;
using Bezoro.Unity.Core.Common.Timing;
using Bezoro.Unity.Core.MonoBehaviours;
using UnityEngine;

namespace Bezoro.Unity.Core.ReflectionProbes
{
	/// <summary>
	///     Renders registered reflection probes on a fixed interval.
	/// </summary>
	[RequireComponent(typeof(SingletonScope), typeof(GlobalReference))]
	[DefaultExecutionOrder(ExecutionOrder.Manager)]
	public sealed class ReflectionProbeManager : MonoBehaviour
	{
		internal static event Action<ReflectionProbeManager>? Available;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetAvailability() => Available = null;
		private readonly ReflectionProbeManagerModel<ReflectionProbe> _model = new(128);

		private CustomFixedTick? _customFixedTick;

		public static bool IsAvailable => TryGetInstance(out _);

		public static ReflectionProbeManager Instance => GlobalReferenceAccess<ReflectionProbeManager>.Instance;

		public float TickFrequency => 1f;

		public static bool TryGetInstance([NotNullWhen(true)] out ReflectionProbeManager? instance) =>
			GlobalReferenceAccess<ReflectionProbeManager>.TryGetInstance(out instance);

		#region Unity Lifecycle

		private void OnEnable()
		{
			if (!GlobalReferenceAccess<ReflectionProbeManager>.IsCurrent(this)) return;
			_customFixedTick = new(TickFrequency, RenderAllProbes);
			Available?.Invoke(this);
		}

		private void Update() => _customFixedTick?.Tick(Time.deltaTime);

		private void OnDisable() => _customFixedTick = null;

		#endregion

		public void AddUnique(ReflectionProbe reflectionProbe)
		{
			_model.AddUnique(reflectionProbe, probe => probe);
		}

		public void Remove(ReflectionProbe reflectionProbe) =>
			_model.Remove(
				reflectionProbe
			);

		private void RenderAllProbes()
		{
			foreach (var reflectionProbe in _model.GetValidItemsAndRemoveInvalid(probe => probe))
				reflectionProbe.RenderProbe();
		}
	}

	public sealed class ReflectionProbeManagerModel<TProbe>
	{
		private readonly List<TProbe> _reflectionProbes;

		public ReflectionProbeManagerModel(int capacity)
		{
			_reflectionProbes = new(capacity);
		}

		public IReadOnlyList<TProbe> ReflectionProbes => _reflectionProbes;

		public bool AddUnique(TProbe reflectionProbe, Func<TProbe, bool> isValid)
		{
			if (!isValid(reflectionProbe)) return false;

			if (_reflectionProbes.Contains(
					reflectionProbe
				)) return false;

			_reflectionProbes.Add(
				reflectionProbe
			);

			return true;
		}

		public bool Remove(TProbe reflectionProbe)
		{
			return _reflectionProbes.Remove(
				reflectionProbe
			);
		}

		public IEnumerable<TProbe> GetValidItemsAndRemoveInvalid(Func<TProbe, bool> isValid)
		{
			for (int i = _reflectionProbes.Count - 1; i >= 0; i--)
			{
				var reflectionProbe = _reflectionProbes[i];
				if (!isValid(reflectionProbe))
				{
					_reflectionProbes.RemoveAt(i);
					continue;
				}

				yield return reflectionProbe;
			}
		}
	}
}
