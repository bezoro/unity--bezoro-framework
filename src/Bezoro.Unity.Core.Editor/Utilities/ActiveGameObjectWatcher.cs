using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Watches a set of GameObjects and fires a callback as soon as any of them
	///     changes from inactive to active in the hierarchy.
	/// </summary>
	public sealed class ActiveGameObjectWatcher : IDisposable
	{
		private readonly Action<GameObject>           _onBecameActive;
		private readonly Dictionary<GameObject, bool> _lastActiveState = new();
		private readonly GameObject[]                 _targets;

		private bool _isDisposed;

		public ActiveGameObjectWatcher(GameObject[] targets, Action<GameObject> onBecameActive)
		{
			_targets        = targets ?? Array.Empty<GameObject>();
			_onBecameActive = onBecameActive;

			// Cache initial states
			foreach (var t in _targets)
			{
				if (!t) continue;

				_lastActiveState[t] = t.activeInHierarchy;
			}

			EditorApplication.update -= OnEditorUpdate;
			EditorApplication.update += OnEditorUpdate;
		}

		public void Dispose()
		{
			if (_isDisposed) return;

			_isDisposed              =  true;
			EditorApplication.update -= OnEditorUpdate;
			_lastActiveState.Clear();
		}

		private void OnEditorUpdate()
		{
			if (_isDisposed) return;

			foreach (var t in _targets)
			{
				if (!t) continue;

				bool wasActive = _lastActiveState.TryGetValue(
									 t,
									 out bool prev
								 ) &&
								 prev;

				bool isActive = t.activeInHierarchy;

				if (!wasActive && isActive)
					_onBecameActive?.Invoke(
						t
					);

				_lastActiveState[t] = isActive;
			}
		}
	}
}
