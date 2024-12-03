using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public abstract class Singleton_MonoBehaviour<T> : MonoBehaviour
	where T : MonoBehaviour
	{
		public static T Instance { get; private set; }

		protected virtual void Awake()
		{
			if ( Instance == null )
			{
				Instance = this as T;
				DontDestroyOnLoad( gameObject );

				return;
			}

			Destroy( gameObject );
		}

#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration )]
		private static void Init()
		{
			Instance = null;
		}
#endif
	}
}
