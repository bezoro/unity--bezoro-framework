using Bezoro.Unity.Core.Attributes;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.ReflectionProbes
{
	/// <summary>
	///     Registers a ReflectionProbe with the shared probe manager while enabled.
	/// </summary>
	public class ReflectionProbeProvider : MonoBehaviour
	{
		private readonly ReflectionProbeProviderModel _model = new();

		[EditorGetComponent(
			typeof(ReflectionProbe),
			true,
			true
		)]
		[SerializeField] private ReflectionProbe? reflectionProbe;

		#region Unity Lifecycle

		private void Reset() =>
			ResolveReferences();

		public void OnEnable()
		{
			ResolveReferences();
			var decision = _model.DecideEnable(reflectionProbe != null);
			if (decision == ReflectionProbeProviderEnableAction.ReportMissingProbe)
			{
				Logger.LogWarning(
					$"{nameof(ReflectionProbeProvider)} requires a ReflectionProbe.", contextObject: this
				);

				return;
			}

			if (decision == ReflectionProbeProviderEnableAction.Register &&
				reflectionProbe != null &&
				ReflectionProbeManager.TryGetInstance(
					out var instance
				))
				instance.AddUnique(
					reflectionProbe
				);
		}

		public void OnDisable()
		{
			ResolveReferences();
			if (!_model.ShouldUnregister(reflectionProbe != null) ||
				reflectionProbe == null)
				return;

			if (ReflectionProbeManager.TryGetInstance(
					out var instance
				))
				instance.Remove(
					reflectionProbe
				);
		}

		private void OnValidate()
		{
			if (UnityEngine.Application.isPlaying) return;

			ResolveReferences();
		}

		#endregion

		private void ResolveReferences()
		{
			if (reflectionProbe != null) return;

			if (TryGetComponent<ReflectionProbe>(out var rp))
				reflectionProbe = rp;
		}
	}

	public enum ReflectionProbeProviderEnableAction
	{
		Register,
		ReportMissingProbe
	}

	public sealed class ReflectionProbeProviderModel
	{
		public ReflectionProbeProviderEnableAction DecideEnable(bool hasReflectionProbe) =>
			hasReflectionProbe
				? ReflectionProbeProviderEnableAction.Register
				: ReflectionProbeProviderEnableAction.ReportMissingProbe;

		public bool ShouldUnregister(bool hasReflectionProbe) =>
			hasReflectionProbe;
	}
}
