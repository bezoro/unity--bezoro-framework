using Bezoro.Unity.Core.Attributes;
using UnityEngine;
using UnityEngine.UI;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Enables alpha-threshold hit testing on a Button image.
	/// </summary>
	public class AlphaRaycastEnabler : MonoBehaviour
	{
		private readonly AlphaRaycastEnablerModel _model = new();

		[EditorGetComponent(typeof(Button), true)]
		[SerializeField] private Button? button;

		#region Unity Lifecycle

		private void Awake()
		{
			var image = button?.image;
			var texture = image?.sprite?.texture;
			var decision = _model.DecideSetup(image != null, texture != null, texture?.isReadable ?? false);

			if (decision.Action == AlphaRaycastSetupAction.ReportUnreadableTexture)
			{
				string targetName = button != null ? button.name : nameof(AlphaRaycastEnabler);
				Logger.LogException(
					new($"Image texture is not read/write enabled on {targetName}."), contextObject: this
				);

				return;
			}

			if (decision.Action != AlphaRaycastSetupAction.ApplyThreshold) return;

			image!.alphaHitTestMinimumThreshold = decision.Threshold;
		}

		#endregion
	}

	public enum AlphaRaycastSetupAction
	{
		None,
		ApplyThreshold,
		ReportUnreadableTexture
	}

	public readonly struct AlphaRaycastSetupDecision
	{
		public AlphaRaycastSetupDecision(AlphaRaycastSetupAction action, float threshold)
		{
			Action    = action;
			Threshold = threshold;
		}

		public AlphaRaycastSetupAction Action { get; }
		public float Threshold { get; }
	}

	public sealed class AlphaRaycastEnablerModel
	{
		private const float HIT_TEST_MINIMUM_THRESHOLD = 0.1f;

		public AlphaRaycastSetupDecision DecideSetup(
			bool hasImage,
			bool hasTexture,
			bool isTextureReadable)
		{
			if (hasTexture && !isTextureReadable)
				return new(AlphaRaycastSetupAction.ReportUnreadableTexture, 0f);

			return hasImage
					   ? new(AlphaRaycastSetupAction.ApplyThreshold, HIT_TEST_MINIMUM_THRESHOLD)
					   : new(AlphaRaycastSetupAction.None, 0f);
		}
	}
}
