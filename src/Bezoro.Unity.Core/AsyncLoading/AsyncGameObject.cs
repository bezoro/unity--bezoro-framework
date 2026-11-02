using System.Collections.Generic;
using System.Threading;
using Bezoro.Unity.Core.Attributes;
using Bezoro.Unity.Core.Common.Enums;
using Bezoro.Unity.Core.Common.Extensions;
using Bezoro.Unity.Core.MonoBehaviours;
using Cysharp.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Bezoro.Unity.Core.AsyncLoading
{
	[DefaultExecutionOrder(ExecutionOrder.Early)]
	[SelectionBase] [RequireComponent(typeof(HierarchyCache))]
	/// <summary>
	/// Registers a hierarchy cache with AsyncLoadingManager and restores it during async loading.
	/// </summary>
	public class AsyncGameObject : MonoBehaviour,
		IAsyncLoadable
	{
		private readonly CancellationTokenSource _cts = new();
		private readonly AsyncGameObjectModel _model = new();

		[EditorGetComponent(typeof(HierarchyCache))]
		[SerializeField] private HierarchyCache? hierarchyCacheReference;

		[Header("Loading Settings")]

		[SerializeField] private int priority;

		[Header("Debug")]

		private bool _isLoaded;

		public bool IsLoaded => _isLoaded;
		public int  Priority => priority;

		public string LoadingName => _model.GetLoadingName(
			this != null,
			this != null ? gameObject.name : string.Empty
		);

		#region Unity Lifecycle

		private void Awake()
		{
			if (!_model.ShouldRunAwake(enabled)) return;

#if UNITY_EDITOR
			var hierarchyCache = EditorGetHierarchyCache();
			hierarchyCache?.RebuildCache();
			hierarchyCache?.DisableGroupsCache();
#endif
			if (AsyncLoadingManager.TryGetInstance(out var instance) && _model.ShouldRegisterWithManager(true))
				instance.Register(this);

			_isLoaded = _model.GetInitialLoadedState();
		}

		private void OnDestroy()
		{
			_cts.Cancel();
			_cts.Dispose();

			if (AsyncLoadingManager.TryGetInstance(out var manager) &&
				_model.ShouldRemoveFromManager(true))
				manager.Remove(this);
		}

		#endregion

		[ContextMenu("Log Loading Status")]
		private void LogLoadingStatus() =>
			Debug.Log($"{LoadingName}: loaded={_isLoaded}, priority={priority}.", this);

		public async UniTask LoadAsync(CancellationToken ct)
		{
			await EnableHierarchyCache(ct);
			_isLoaded = true;
		}

		private async UniTask EnableHierarchyCache(CancellationToken ct)
		{
			var hierarchyCache = hierarchyCacheReference;
			if (!_model.ShouldEnableHierarchyCache(ct.IsCancellationRequested, hierarchyCache != null)) return;
			if (hierarchyCache == null) return;

			using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token);
			await hierarchyCache.EnableGroupsCacheAsync(linked.Token);
		}

#if UNITY_EDITOR

		[InitializeOnLoadMethod]
		private static void InitializeOnLoad()
		{
			EditorApplication.hierarchyChanged -= OnHierarchyChanged;
			EditorApplication.hierarchyChanged += OnHierarchyChanged;

			Undo.postprocessModifications -= OnPostprocessModifications;
			Undo.postprocessModifications += OnPostprocessModifications;

			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
		}

		private static void OnHierarchyChanged() => RebuildAllCaches();

		private static void OnUndoRedoPerformed() => RebuildAllCaches();

		private static void RebuildAllCaches()
		{
			var model = new AsyncGameObjectModel();
			if (!model.ShouldRebuildCaches(UnityEngine.Application.isPlaying)) return;

			foreach (var asyncGO in FindObjectsByType<AsyncGameObject>(FindObjectsInactive.Include))
			{
				if (!asyncGO) continue;

				asyncGO.EditorGetHierarchyCache()?.RebuildCache();
			}
		}

		private static UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
		{
			var model = new AsyncGameObjectModel();
			if (!model.ShouldProcessUndoModifications(UnityEngine.Application.isPlaying)) return modifications;

			HashSet<AsyncGameObject>? touched = null;

			foreach (var mod in modifications)
				// target will be Component, GameObject, ScriptableObject, etc.
			{
				if (mod.currentValue.target is not Component component) continue;

				var item = component.GetComponentInParent<AsyncGameObject>();
				if (!model.ShouldTrackUndoModification(item != null)) continue;
				if (item == null) continue;

				touched ??= new();
				touched.Add(item);
			}

			if (touched == null) return modifications;

			foreach (var o in touched)
				o.EditorGetHierarchyCache()?.RebuildCache();

			return modifications;
		}

		public void EditorPrepareForBuild()
		{
			var hierarchyCache = EditorGetHierarchyCache();
			var model = new AsyncGameObjectModel();
			if (model.ShouldUseHierarchyCache(hierarchyCache != null) && hierarchyCache != null)
				hierarchyCache.RebuildCache();

			gameObject.DisableHierarchyImmediately();

			if (model.ShouldUseHierarchyCache(hierarchyCache != null) && hierarchyCache != null)
				hierarchyCache.enabled = true;

			enabled = true;
			gameObject.SetActive(true);
		}

		private HierarchyCache? EditorGetHierarchyCache()
		{
			if (hierarchyCacheReference == null)
				hierarchyCacheReference = GetComponent<HierarchyCache>();

			return hierarchyCacheReference;
		}

#endif
	}

	public sealed class AsyncGameObjectModel
	{
		public bool ShouldRunAwake(bool isEnabled)
		{
			return isEnabled;
		}

		public bool ShouldRegisterWithManager(bool hasManager)
		{
			return hasManager;
		}

		public bool ShouldRemoveFromManager(bool managerIsAvailable)
		{
			return managerIsAvailable;
		}

		public bool ShouldEnableHierarchyCache(bool isCancellationRequested, bool hasHierarchyCache)
		{
			return !isCancellationRequested && hasHierarchyCache;
		}

		public bool ShouldRebuildCaches(bool isPlaying)
		{
			return !isPlaying;
		}

		public bool ShouldProcessUndoModifications(bool isPlaying)
		{
			return !isPlaying;
		}

		public bool ShouldTrackUndoModification(bool hasAsyncGameObject)
		{
			return hasAsyncGameObject;
		}

		public bool ShouldUseHierarchyCache(bool hasHierarchyCache)
		{
			return hasHierarchyCache;
		}

		public bool GetInitialLoadedState()
		{
			return false;
		}

		public string GetLoadingName(bool objectExists, string gameObjectName)
		{
			return objectExists ? gameObjectName : "<Destroyed GameObject>";
		}
	}
}
