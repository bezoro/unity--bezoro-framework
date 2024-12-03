using UnityEngine;

namespace Bezoro.Framework.Unity.ReflectionProbeManager
{
	public class ReflectionProbeProvider : MonoBehaviour
	{
		[SerializeField] [HideInInspector] private ReflectionProbe reflectionProbe;

		public void OnEnable()
		{
			ReflectionProbeManager.Instance.AddUnique( reflectionProbe );
		}

		public void OnDisable()
		{
			ReflectionProbeManager.Instance.Remove( reflectionProbe );
		}

		private void OnValidate()
		{
			if ( Application.isPlaying )
			{
				return;
			}

			if ( TryGetComponent<ReflectionProbe>( out var rp ) )
			{
				reflectionProbe = rp;
			}
		}
	}
}
