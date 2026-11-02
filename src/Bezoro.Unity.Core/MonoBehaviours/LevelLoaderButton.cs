using Bezoro.Unity.Core.Attributes;
using Bezoro.Unity.Core.Scenes;
using Bezoro.Unity.Core.ScriptableObjects;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Handles Level Loader button actions.
	/// </summary>
	public class LevelLoaderButton : MonoBehaviour
	{
		private readonly LevelLoaderButtonModel _model = new();

		[EditorGetComponent(
			typeof(Button),
			true,
			true
		)]
		[SerializeField] private Button? button;
		[SerializeField] private bool useSceneCollection = true;

		[Tooltip("Required when Use Scene Collection is enabled.")]
		[SerializeField] private SceneCollection? sceneCollection;
		[Tooltip("Used when Use Scene Collection is disabled.")]
		[SerializeField] private SceneReference? sceneReference;

		#region Unity Lifecycle

		private void Awake() =>
			button ??= GetComponent<Button>();

		private void Reset() =>
			button ??= GetComponent<Button>();

		private void OnEnable()
		{
			button ??= GetComponent<Button>();
			if (button == null)
			{
				Debug.LogWarning(
					$"{nameof(LevelLoaderButton)} requires a {nameof(Button)} reference.",
					this
				);

				return;
			}

			button.onClick.AddListener(
				LoadSceneAsync
			);
		}

		private void OnDisable()
		{
			if (button == null)
				return;

			button.onClick.RemoveListener(
				LoadSceneAsync
			);
		}

		#endregion

		private void LoadSceneAsync()
		{
			var decision = _model.DecideLoadAction(
				useSceneCollection,
				sceneCollection != null,
				sceneReference != null
			);

			switch (decision.Action)
			{
				case LevelLoaderButtonLoadAction.LoadSceneCollection:
					sceneCollection!.Load().Forget();
					break;
				case LevelLoaderButtonLoadAction.LoadSceneReference:
					sceneReference!.LoadSceneAsync();
					break;
				case LevelLoaderButtonLoadAction.MissingSceneCollection:
					Debug.LogWarning(
						$"{nameof(LevelLoaderButton)} cannot load because {nameof(sceneCollection)} is not assigned.",
						this
					);
					break;
				case LevelLoaderButtonLoadAction.MissingSceneReference:
					Debug.LogWarning(
						$"{nameof(LevelLoaderButton)} cannot load because {nameof(sceneReference)} is not assigned.",
						this
					);
					break;
			}
		}
	}

	public enum LevelLoaderButtonLoadAction
	{
		LoadSceneCollection,
		LoadSceneReference,
		MissingSceneCollection,
		MissingSceneReference
	}

	public readonly struct LevelLoaderButtonLoadDecision
	{
		public LevelLoaderButtonLoadDecision(LevelLoaderButtonLoadAction action)
		{
			Action = action;
		}

		public LevelLoaderButtonLoadAction Action { get; }
	}

	public sealed class LevelLoaderButtonModel
	{
		public LevelLoaderButtonLoadDecision DecideLoadAction(
			bool useSceneCollection,
			bool hasSceneCollection,
			bool hasSceneReference)
		{
			if (useSceneCollection)
				return new(
					hasSceneCollection
						? LevelLoaderButtonLoadAction.LoadSceneCollection
						: LevelLoaderButtonLoadAction.MissingSceneCollection
				);

			return new(
				hasSceneReference
					? LevelLoaderButtonLoadAction.LoadSceneReference
					: LevelLoaderButtonLoadAction.MissingSceneReference
			);
		}
	}
}
