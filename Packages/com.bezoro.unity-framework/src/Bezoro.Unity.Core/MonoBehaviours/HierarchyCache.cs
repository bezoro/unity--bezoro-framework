#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bezoro.Core.Extensions;
using Bezoro.Logging.Types;
using Bezoro.Unity.Core.Attributes;
using Bezoro.Unity.Core.AsyncLoading;
using Bezoro.Unity.Core.Loading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Caches hierarchy activation state and restores disabled groups during loading.
	/// </summary>
	[DisallowMultipleComponent]
	internal sealed class HierarchyCache : MonoBehaviour
	{
		private const float  WEIGHT             = .5f;
		private const string LOADING_STAGE_NAME = "Hierarchy Cache";

		[Header("Debug")]

		[SerializeField] [ReadOnly]
		private GameObjectState?[] gameObjectStatesCache = { };
		[SerializeField] [ReadOnly]
		private GameObject?[] startDisabledObjects = { };

		private bool _inProgress;
		private int _processed;

		private CancellationTokenSource? _destroyCts;

		#region Unity Lifecycle

		private void OnEnable()
		{
			_destroyCts = new();
		}

		private void OnDisable()
		{
			_destroyCts?.Cancel();
			_destroyCts?.Dispose();
			_destroyCts = null;
		}

		#endregion

		public async UniTask<int> EnableGroupsCacheAsync(CancellationToken ct = default)
		{
			ct.ThrowIfCancellationRequested();
			while (_inProgress)
				await UniTask.Yield(ct);

			if (gameObjectStatesCache.IsEmpty()) return _processed;

			_inProgress = true;

			using var linkedCts = _destroyCts != null
									  ? CancellationTokenSource.CreateLinkedTokenSource(ct, _destroyCts.Token)
									  : CancellationTokenSource.CreateLinkedTokenSource(ct);

			var token = linkedCts.Token;
			LoadingStage? stage = null;
			LoadingProgressService.TryGetInstance(out var service);

			try
			{
				gameObjectStatesCache.ThrowIfNull();
				_processed = 0;
				var frameBudget = new AsyncLoadingFrameBudget();

				stage = service?.BeginStage(LOADING_STAGE_NAME, WEIGHT);
				var tracker = stage?.CreateTracker(gameObject.name);

				foreach (var state in gameObjectStatesCache)
				{
					token.ThrowIfCancellationRequested();
					await ActivateGameObjectGroupAsync(state, frameBudget, token);

					_processed++;
					if (tracker != null) tracker.Progress = _processed / (float)gameObjectStatesCache.Length;

					Logger.Log(
						$"Processed {_processed}/{gameObjectStatesCache.Length}",
						contextObject: this,
						category: LogCategory.Loading
					);
				}

				return _processed;
			}
			catch
			{
				if (stage != null && service != null) service.DiscardStage(stage);
				throw;
			}
			finally
			{
				_inProgress = false;
			}
		}

		public void DisableGroupsCache()
		{
			var skip = new Component[] { this };

			foreach (var group in gameObjectStatesCache) group?.DeactivateAll(skip);
		}

		[ContextMenu("Log Cache Status")]
		private void LogCacheStatus() =>
			Debug.Log($"Hierarchy cache: processing={_inProgress}, processed={_processed}, cached={gameObjectStatesCache.Length}.", this);

		[ContextMenu("Rebuild Cache")]
		public void RebuildCache()
		{
			bool changed = PopulateGroupsCache();
			changed |= CollectStartDisabledObjects();

			if (changed)
				MarkCacheDirty();
		}

		private bool CacheMatchesList(List<GameObjectState> list)
		{
			if (gameObjectStatesCache.Length != list.Count)
				return false;

			for (var i = 0; i < list.Count; i++)
			{
				if (!list[i].Equals(gameObjectStatesCache[i]))
					return false;
			}

			return true;
		}

		private bool CollectStartDisabledObjects()
		{
			var disabled = new HashSet<GameObject?>();

			foreach (var group in gameObjectStatesCache)
			{
				if (group?.ComponentStates == null) continue;

				var components = group.ComponentStates.ToComponents();

				foreach (var component in components)
				{
					if (component is StartDisabled sd && sd != null)
					{
						foreach (var target in sd.GameObjects)
						{
							if (target != null && CanCacheHierarchy(target.transform))
								disabled.Add(target);
						}
					}
				}
			}

			// Activation uses this array as a set. Preserve its serialized order when membership is unchanged.
			if (startDisabledObjects.Length == disabled.Count && disabled.SetEquals(startDisabledObjects))
				return false;

			startDisabledObjects = disabled.ToArray();
			return true;
		}

		private bool PopulateGroupsCache()
		{
			var list = new List<GameObjectState>();

			Collect(transform);

			if (CacheMatchesList(list))
				return false;

			gameObjectStatesCache = list.ToArray();
			Logger.LogSuccess(
				$"Cached: {gameObjectStatesCache.Length}",
				contextObject: this,
				category: LogCategory.Editor
			);

			return true;

			void Collect(Transform node)
			{
				if (node == null || !CanCacheHierarchy(node)) return;

				list.Add(new(node.gameObject, ExcludedAuthoringFlags));

				for (var i = 0; i < node.childCount; i++) Collect(node.GetChild(i));
			}
		}

		private static HideFlags ExcludedAuthoringFlags
		{
			get
			{
#if UNITY_EDITOR
				if (!UnityEngine.Application.isPlaying)
					return HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
#endif
				return HideFlags.None;
			}
		}

		private static bool CanCacheHierarchy(Transform node)
		{
			var excludedFlags = ExcludedAuthoringFlags;
			if (excludedFlags == HideFlags.None) return true;

			// External StartDisabled targets can also belong to an editor-only subtree.
			for (var ancestor = node; ancestor != null; ancestor = ancestor.parent)
				if ((ancestor.gameObject.hideFlags & excludedFlags) != 0)
					return false;

			return true;
		}

		private async UniTask ActivateGameObjectGroupAsync(
			GameObjectState?         group,
			AsyncLoadingFrameBudget? frameBudget = null,
			CancellationToken        ct          = default)
		{
			if (group == null) return;
			if (group.Target == null) return;

			Logger.Log($"Processing group {group.Name}...", contextObject: this, category: LogCategory.Loading);

			var        skipGameObjects      = startDisabledObjects;
			const bool RESPECT_PRE_DISABLED = true;
			frameBudget ??= new();
			await group.ActivateAsync(
				RESPECT_PRE_DISABLED,
				skipGameObjects,
				frameBudget: frameBudget,
				ct: ct
			);

			Logger.LogSuccess($"Enabled Group: {group.Name}", contextObject: this, category: LogCategory.Loading);
			await frameBudget.WaitIfNeededAsync(ct);
		}

		/// <summary>
		///     Build <see cref="gameObjectStatesCache" /> in strict top-to-bottom hierarchy order.
		/// </summary>
		private void MarkCacheDirty()
		{
#if UNITY_EDITOR
			if (UnityEngine.Application.isPlaying)
				return;

			EditorUtility.SetDirty(this);
			PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
		}

	}
}
