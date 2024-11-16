using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
	{
		public static T Instance { get; private set; }

		protected virtual void Awake()
		{
			if ( Instance == null )
			{
				Instance = this as T;
				Debug.Assert( Instance != null, $"SingletonMonoBehaviour<{typeof(T).Name}> is not a MonoBehaviour" );
				DontDestroyOnLoad( gameObject );

				return;
			}

			Destroy( gameObject );
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration )]
		private static void Init()
		{
			Instance = null;
		}
	}
}
