using System.Threading;
using Bezoro.Unity.Core.Attributes;
using Bezoro.Unity.Core.Loading;
using UnityEngine;
using UnityEngine.UI;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Handles Level Reloader button actions.
	/// </summary>
	public class LevelReloaderButton : MonoBehaviour
	{
		private readonly CancellationTokenSource _cts = new();
		private readonly LevelReloaderButtonModel _model = new();

		[EditorGetComponent(
			typeof(Button),
			true
		)]
		[SerializeField] private Button? button;

		#region Unity Lifecycle

		private void Awake() =>
			ResolveReferences();

		private void Reset() =>
			ResolveReferences();

		private void OnEnable()
		{
			ResolveReferences();
			button?.onClick.AddListener(
				ReloadSceneAsync
			);
		}

		private void OnDisable()
		{
			button?.onClick.RemoveListener(
				ReloadSceneAsync
			);
		}

		private void OnDestroy()
		{
			_cts.Cancel();
			_cts.Dispose();
		}

		#endregion

		private void ReloadSceneAsync()
		{
			if (!_model.ShouldReload(_cts.IsCancellationRequested))
				return;

			SceneLoader.ReloadScenes(
				_cts.Token
			);
		}

		private void ResolveReferences()
		{
			button ??= GetComponent<Button>();
		}
	}

	public sealed class LevelReloaderButtonModel
	{
		public bool ShouldReload(bool isCancellationRequested) =>
			!isCancellationRequested;
	}
}
