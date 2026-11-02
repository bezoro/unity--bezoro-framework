using UnityEngine;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Marks its GameObject to persist across scene loads.
	/// </summary>
	public class DontDestroyOnLoad : MonoBehaviour
	{
		#region Unity Lifecycle

		private void Awake()
		{
			DontDestroyOnLoad(
				this
			);
		}

		#endregion
	}
}
