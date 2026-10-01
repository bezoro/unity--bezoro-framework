using System.Collections.Generic;
using Bezoro.Unity.Core.Common.Enums;
using UnityEngine;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	[DefaultExecutionOrder(
		ExecutionOrder.Late
	)]
	/// <summary>
	/// Disables configured GameObjects on Start and can toggle them later.
	/// </summary>
	public class StartDisabled : MonoBehaviour
	{
		private readonly StartDisabledModel _model = new();

		[SerializeField] private List<GameObject> gameObjects = new();

		public IReadOnlyCollection<GameObject> GameObjects => gameObjects;

		#region Unity Lifecycle

		private void Start() => DisableAll();

		#endregion

		public void DisableAll()
		{
			SetAllActive(_model.GetTargetActiveState(false));
		}

		public void EnableAll()
		{
			SetAllActive(_model.GetTargetActiveState(true));
		}

		private void SetAllActive(bool isActive)
		{
			foreach (var go in gameObjects)
			{
				if (go == null || !_model.ShouldApplyTarget(true))
					continue;

				go.SetActive(isActive);
			}
		}
	}

	public sealed class StartDisabledModel
	{
		public bool GetTargetActiveState(bool enabled) =>
			enabled;

		public bool ShouldApplyTarget(bool hasTarget) =>
			hasTarget;
	}
}
