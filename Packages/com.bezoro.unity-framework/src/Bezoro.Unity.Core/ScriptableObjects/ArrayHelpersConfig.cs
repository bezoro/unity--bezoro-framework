using UnityEngine;
using UnityEngine.Serialization;

namespace Bezoro.Unity.Core.ScriptableObjects
{
	[CreateAssetMenu(
		fileName = "New_Array_Helpers_Config",
		menuName = "Bezoro/Runtime/Configuration/Array Helpers Config"
	)]
	/// <summary>
	/// Stores thresholds used by array helper algorithms.
	/// </summary>
	public sealed class ArrayHelpersConfig : ScriptableSingleton<ArrayHelpersConfig>
	{
		#region Serialized Fields

		[FormerlySerializedAs(
			"parallel_Threshold"
		)]
		[Range(
			1,
			1000
		)]
		public int ParallelThreshold = 100;

		#endregion

		#region Unity Lifecycle

		protected override void Awake()
		{
			base.Awake();
			// ArrayHelper.SetParallelThreshold(ParallelThreshold);
		}

		// private void OnEnable() =>
		// ArrayHelper.SetParallelThreshold(ParallelThreshold);

		protected override void OnValidate()
		{
			base.OnValidate();
			// ArrayHelper.SetParallelThreshold(ParallelThreshold);
		}

		#endregion
	}
}
