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
		private ReflectionProbeManager? _owner;

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
			if (reflectionProbe == null)
			{
				Logger.LogWarning($"{nameof(ReflectionProbeProvider)} requires a ReflectionProbe.", contextObject: this);
				return;
			}

			ReflectionProbeManager.Available -= Bind;
			ReflectionProbeManager.Available += Bind;
			if (ReflectionProbeManager.TryGetInstance(out var manager)) Bind(manager);
		}

		private void Bind(ReflectionProbeManager manager)
		{
			if (reflectionProbe == null || _owner == manager) return;
			if (_owner != null) _owner.Remove(reflectionProbe);
			_owner = manager;
			manager.AddUnique(reflectionProbe);
		}

		public void OnDisable()
		{
			ReflectionProbeManager.Available -= Bind;
			if (_owner != null && reflectionProbe != null) _owner.Remove(reflectionProbe);
			_owner = null;
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
