#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Bezoro.Unity.Core.UI
{
	[DisallowMultipleComponent]
	public sealed class TextBubbleService : MonoBehaviour
	{
		private static TextBubbleService? _instance;

		private readonly Dictionary<int, TextBubbleEntry> _active = new();
		private readonly Stack<TextBubbleView> _pool = new();
		private readonly List<int> _scratchReleaseIds = new();

		[SerializeField] private TextBubbleView? bubblePrefab;
		[SerializeField] private RectTransform? container;
		[SerializeField] private Canvas? canvas;
		[SerializeField] private Camera? defaultCamera;

		private int _nextHandleId = 1;

		public static TextBubbleService? Instance =>
			IsAvailableInstance(_instance) ? _instance : null;

		public int ActiveCount => _active.Count;
		public int PooledCount => _pool.Count;

		public static bool TryGetInstance(out TextBubbleService? service)
		{
			service = Instance;
			return service != null;
		}

		#region Unity Lifecycle

		private void Awake()
		{
			ResolveReferences();
		}

		private void OnEnable()
		{
			ResolveReferences();
			RegisterInstance();
		}

		private void LateUpdate()
		{
			if (!UnityEngine.Application.isPlaying)
				return;

			if (_active.Count == 0)
				return;

			_scratchReleaseIds.Clear();

			foreach (var pair in _active)
			{
				var entry = pair.Value;
				if (entry.Request.LifetimeSeconds > 0f)
				{
					entry.RemainingLifetime -= Time.deltaTime;
					if (entry.RemainingLifetime <= 0f)
					{
						_scratchReleaseIds.Add(entry.Id);
						continue;
					}
				}

				if (entry.IsRequestedVisible)
					ApplyPosition(entry);
			}

			for (var i = 0; i < _scratchReleaseIds.Count; i++)
				ReleaseById(_scratchReleaseIds[i]);
		}

		private void OnDisable()
		{
			if (_instance == this)
				_instance = null;

			ReleaseAllActive();
		}

		private void OnDestroy()
		{
			if (_instance == this)
				_instance = null;

			ReleaseAllActive();
			DestroyPooledViews();
		}

		#endregion

		public void ConfigureForRuntime(
			TextBubbleView? prefab,
			RectTransform? parentContainer,
			Canvas? targetCanvas,
			Camera? targetDefaultCamera)
		{
			bubblePrefab = prefab;
			container = parentContainer;
			canvas = targetCanvas;
			defaultCamera = targetDefaultCamera;
		}

		public TextBubbleHandle Show(TextBubbleRequest request)
		{
			if (_instance != this || !isActiveAndEnabled)
			{
				Debug.LogWarning($"{nameof(TextBubbleService)} cannot show bubbles while inactive, disabled, or unregistered.", this);
				return default;
			}

			var validation = request.Validate();
			if (validation.HasErrors)
			{
				Debug.LogWarning(string.Join("\n", validation.Messages), this);
				return default;
			}

			ResolveReferences();

			if (bubblePrefab == null)
			{
				Debug.LogError($"{nameof(TextBubbleService)} requires a bubble prefab.", this);
				return default;
			}

			if (container == null)
			{
				Debug.LogError($"{nameof(TextBubbleService)} requires a target container.", this);
				return default;
			}

			var view = GetOrCreateView();
			var id = _nextHandleId++;
			var entry = new TextBubbleEntry(id, view, request);
			_active.Add(id, entry);

			view.SetText(request.Text);
			view.SetVisible(false);

			if (entry.IsRequestedVisible)
				ApplyPosition(entry);

			return new(this, id);
		}

		public bool IsHandleValid(TextBubbleHandle handle) =>
			isActiveAndEnabled && handle.Id != 0 && handle.IsOwnedBy(this) && _active.ContainsKey(handle.Id);

		public bool TrySetText(TextBubbleHandle handle, string text)
		{
			if (!TryGetEntry(handle, out var entry))
				return false;

			var request = entry.Request.WithText(text);
			var validation = request.Validate();
			if (validation.HasErrors)
			{
				Debug.LogWarning(string.Join("\n", validation.Messages), this);
				return false;
			}

			entry.Request = request;
			entry.View.SetText(entry.Request.Text);
			return true;
		}

		public bool TrySetAnchor(TextBubbleHandle handle, TextBubbleAnchor anchor)
		{
			if (!anchor.IsValid)
				return false;

			if (!TryGetEntry(handle, out var entry))
				return false;

			entry.Request = entry.Request.WithAnchor(anchor);

			if (entry.IsRequestedVisible)
				ApplyPosition(entry);

			return true;
		}

		public bool TrySetVisible(TextBubbleHandle handle, bool visible)
		{
			if (!TryGetEntry(handle, out var entry))
				return false;

			entry.IsRequestedVisible = visible;
			if (visible)
				ApplyPosition(entry);
			else
				entry.View.SetVisible(false);

			return true;
		}

		/// <summary>
		/// Returns the active bubble view to the pool and invalidates the handle.
		/// </summary>
		public bool TryRelease(TextBubbleHandle handle)
		{
			if (!handle.IsOwnedBy(this))
				return false;

			return ReleaseById(handle.Id);
		}

		/// <summary>
		/// Permanently destroys the active bubble view and invalidates the handle.
		/// </summary>
		public bool TryDestroy(TextBubbleHandle handle)
		{
			if (!TryGetEntry(handle, out var entry))
				return false;

			_active.Remove(handle.Id);
			DestroyView(entry.View);
			return true;
		}

		private void RegisterInstance()
		{
			if (_instance != null && !_instance.isActiveAndEnabled)
				_instance = null;

			if (_instance != null && _instance != this)
			{
				var duplicateAction = UnityEngine.Application.isPlaying
					? "Destroying duplicate"
					: "Leaving duplicate unregistered";

				Debug.LogWarning(
					$"{nameof(TextBubbleService)} already exists on '{_instance.name}'. {duplicateAction} on '{name}'.",
					this
				);

				if (UnityEngine.Application.isPlaying)
					Destroy(this);

				return;
			}

			_instance = this;
		}

		private bool TryGetEntry(TextBubbleHandle handle, [NotNullWhen(true)] out TextBubbleEntry? entry)
		{
			if (!isActiveAndEnabled || handle.Id == 0 || !handle.IsOwnedBy(this))
			{
				entry = null;
				return false;
			}

			return _active.TryGetValue(handle.Id, out entry);
		}

		private TextBubbleView GetOrCreateView()
		{
			var parent = container;
			if (parent == null)
				throw new InvalidOperationException($"{nameof(TextBubbleService)} requires a target container.");

			while (_pool.Count > 0)
			{
				var pooledView = _pool.Pop();
				if (pooledView == null)
					continue;

				EnsureViewParent(pooledView, parent);
				return pooledView;
			}

			var prefab = bubblePrefab;
			if (prefab == null)
				throw new InvalidOperationException($"{nameof(TextBubbleService)} requires a bubble prefab.");

			var view = Instantiate(prefab, parent);
			view.name = $"{prefab.name} (Instance)";
			return view;
		}

		private static void EnsureViewParent(TextBubbleView view, RectTransform parent)
		{
			if (view.transform.parent != parent)
				view.transform.SetParent(parent, false);
		}

		private bool ReleaseById(int id)
		{
			if (!_active.TryGetValue(id, out var entry))
				return false;

			_active.Remove(id);

			if (entry.View != null)
			{
				entry.View.ResetView();
				_pool.Push(entry.View);
			}

			return true;
		}

		private void ReleaseAllActive()
		{
			_scratchReleaseIds.Clear();
			foreach (var pair in _active)
				_scratchReleaseIds.Add(pair.Key);

			for (var i = 0; i < _scratchReleaseIds.Count; i++)
				ReleaseById(_scratchReleaseIds[i]);
		}

		private void DestroyPooledViews()
		{
			while (_pool.Count > 0)
			{
				var view = _pool.Pop();
				if (view != null)
					DestroyView(view);
			}
		}

		private void ApplyPosition(TextBubbleEntry entry)
		{
			if (!TryResolveAnchoredPosition(entry.Request, out var anchoredPosition))
			{
				entry.View.SetVisible(false);
				return;
			}

			entry.View.SetAnchoredPosition(anchoredPosition);
			if (entry.IsRequestedVisible)
				entry.View.SetVisible(true);
		}

		private bool TryResolveAnchoredPosition(TextBubbleRequest request, out Vector2 anchoredPosition)
		{
			anchoredPosition = Vector2.zero;
			ResolveReferences();

			if (container == null || canvas == null)
				return false;

			Vector2 screenPosition;
			var eventCamera = ResolveCanvasEventCamera();

			switch (request.Anchor.Kind)
			{
				case TextBubbleAnchorKind.ScreenPosition:
					screenPosition = request.Anchor.ScreenPosition + request.Anchor.ScreenOffset;
					break;

				case TextBubbleAnchorKind.WorldPosition:
				{
					var camera = ResolveProjectionCamera(request);
					if (camera == null)
						return false;

					var screenPoint = camera.WorldToScreenPoint(request.Anchor.WorldPosition);
					if (screenPoint.z < 0f)
						return false;

					screenPosition = new(screenPoint.x, screenPoint.y);
					screenPosition += request.Anchor.ScreenOffset;
					break;
				}

				case TextBubbleAnchorKind.Transform:
				{
					if (request.Anchor.Target == null)
						return false;

					var camera = ResolveProjectionCamera(request);
					if (camera == null)
						return false;

					var screenPoint = camera.WorldToScreenPoint(request.Anchor.Target.position);
					if (screenPoint.z < 0f)
						return false;

					screenPosition = new(screenPoint.x, screenPoint.y);
					screenPosition += request.Anchor.ScreenOffset;
					break;
				}

				case TextBubbleAnchorKind.None:
				default:
					return false;
			}

			return RectTransformUtility.ScreenPointToLocalPointInRectangle(
				container,
				screenPosition,
				eventCamera,
				out anchoredPosition
			);
		}

		private Camera? ResolveProjectionCamera(TextBubbleRequest request)
		{
			if (request.CameraOverride != null)
				return request.CameraOverride;

			if (defaultCamera != null)
				return defaultCamera;

			if (canvas != null && canvas.worldCamera != null)
				return canvas.worldCamera;

			return Camera.main;
		}

		private Camera? ResolveCanvasEventCamera()
		{
			if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
				return null;

			if (canvas.worldCamera != null)
				return canvas.worldCamera;

			if (defaultCamera != null)
				return defaultCamera;

			return Camera.main;
		}

		private static bool IsAvailableInstance(TextBubbleService? service) =>
			service != null && service.isActiveAndEnabled;

		private void ResolveReferences()
		{
			if (container == null)
				container = GetComponent<RectTransform>();

			if (canvas == null)
				canvas = GetComponentInParent<Canvas>();

			if (container == null && canvas != null)
				container = canvas.GetComponent<RectTransform>();
		}

		private static void DestroyView(TextBubbleView view)
		{
			if (view == null)
				return;

			if (UnityEngine.Application.isPlaying)
				Destroy(view.gameObject);
			else
				DestroyImmediate(view.gameObject);
		}

		private sealed class TextBubbleEntry
		{
			public TextBubbleEntry(int id, TextBubbleView view, TextBubbleRequest request)
			{
				Id = id;
				View = view;
				Request = request;
				IsRequestedVisible = request.VisibleOnCreate;
				RemainingLifetime = request.LifetimeSeconds;
			}

			public int Id { get; }
			public TextBubbleView View { get; }
			public TextBubbleRequest Request { get; set; }
			public bool IsRequestedVisible { get; set; }
			public float RemainingLifetime { get; set; }
		}
	}
}
