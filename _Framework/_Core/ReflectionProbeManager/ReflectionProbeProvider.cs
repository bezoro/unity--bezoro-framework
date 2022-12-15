using Bezoro.Core.Singletons;
using UnityEngine;

namespace Bezoro.Core
{
	public class ReflectionProbeProvider : MonoBehaviour
	{
		public void OnEnable() =>
			ReflectionProbeManager.Instance.AddUnique(reflectionProbe);

		public void OnDisable() =>
			ReflectionProbeManager.Instance.Remove(reflectionProbe);

		private void OnValidate()
		{
			if (Application.isPlaying)
				return;

			if (TryGetComponent<ReflectionProbe>(out var rp))
				reflectionProbe = rp;
		}

		[SerializeField, HideInInspector]
		private ReflectionProbe reflectionProbe;
	}
}