using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bezoro.Logging.Types;
using Bezoro.Unity.Core.Common.Enums;
using Bezoro.Unity.Core.Common.Helpers;
using Bezoro.Unity.Core.Tickable;
using UnityEngine;
using UnityEngine.InputSystem;
using Logger = Bezoro.Logging.Logger;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Manages pointer (mouse/touch) interactions with GameObjects in the scene.
	///     Provides a system for registering objects to receive pointer events and handles pointer enter, exit, and over
	///     events.
	/// </summary>
	[RequireComponent(typeof(SingletonScope), typeof(GlobalReference))]
	[DefaultExecutionOrder(ExecutionOrder.Manager)]
	public class PointerManager : MonoBehaviour, IRegularTickable
	{
		private readonly PointerManagerModel<IPointerPressDataReceiver, IPointerOverDataReceiver, IPointerDragDataReceiver> _model = new(128);
		private readonly List<RaycastHit> _previousHits    = new(128);
		private readonly RaycastHit[]     _singleHitBuffer = new RaycastHit[1];

		[SerializeField] private bool enableLogs;
		[SerializeField] private bool sortRaycastHitsByDistance;

		private bool _isTickRegistered;
		private bool _isSingletonInstance;
		private bool _loggedMissingCamera;
		private bool _loggedMissingPointer;

		private Camera? _mainCamera;

		public event Action? PointerClickMissed;

		public static bool IsAvailable => TryGetInstance(out _);

		public static PointerManager Instance => GlobalReferenceAccess<PointerManager>.Instance;

		public bool EnableLogs
		{
			get => enableLogs;
			set => enableLogs = value;
		}

		public bool SortRaycastHitsByDistance
		{
			get => sortRaycastHitsByDistance;
			set => sortRaycastHitsByDistance = value;
		}

		public static bool TryGetInstance([NotNullWhen(true)] out PointerManager? instance) =>
			GlobalReferenceAccess<PointerManager>.TryGetInstance(out instance);

		#region Unity Lifecycle

		private void Awake()
		{
			_isSingletonInstance = GlobalReferenceAccess<PointerManager>.IsCurrent(this);

			if (!_isSingletonInstance)
				return;

			_mainCamera = Camera.main;
			LogPointer($"Awake. Main camera: {GetObjectName(_mainCamera)}");

			// Pre-size to avoid capacity growth and allocations at runtime
			if (_previousHits.Capacity < 256)
				_previousHits.Capacity = 256;
		}

		private void OnEnable()
		{
			if (_isSingletonInstance)
				RegisterTickable();
		}

		private void OnDisable() =>
			UnregisterTickable();

		#endregion

		#region Tickable

		public void RegularTick() => HandlePointerEvents();

		#endregion

		/// <summary>
		///     Registers a pointer data receiver to handle pointer events.
		/// </summary>
		/// <param name="receiver">The receiver to register. Must not be null.</param>
		/// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
		public void RegisterPointerDataReceiver(IPointerDataReceiver receiver)
		{
			if (receiver == null)
				throw new ArgumentNullException(
					nameof(receiver)
				);

			if (receiver is IPointerPressDataReceiver pressDataReceiver)
				_model.AddUniquePressReceiver(pressDataReceiver);

			if (receiver is IPointerOverDataReceiver overDataReceiver)
				_model.AddUniqueOverReceiver(overDataReceiver);

			if (receiver is IPointerDragDataReceiver dragDataReceiver)
				_model.AddUniqueDragReceiver(dragDataReceiver);

			LogPointer(
				$"Registered {receiver.GetType().Name}. Press: {_model.PressReceivers.Count}, Over: {_model.OverReceivers.Count}, Drag: {_model.DragReceivers.Count}"
			);
		}

		/// <summary>
		///     Unregisters a pointer data receiver from handling pointer events.
		/// </summary>
		/// <param name="receiver">The receiver to unregister. Must not be null.</param>
		/// <exception cref="ArgumentNullException">Thrown when receiver is null.</exception>
		public void UnregisterPointerDataReceiver(IPointerDataReceiver receiver)
		{
			if (receiver == null)
				throw new ArgumentNullException(
					nameof(receiver)
				);

			if (receiver is IPointerDragDataReceiver dragDataReceiver)
				_model.RemoveDragReceiver(dragDataReceiver);

			if (receiver is IPointerOverDataReceiver overDataReceiver)
				_model.RemoveOverReceiver(overDataReceiver);

			if (receiver is IPointerPressDataReceiver pressDataReceiver)
				_model.RemovePressReceiver(pressDataReceiver);

			LogPointer(
				$"Unregistered {receiver.GetType().Name}. Press: {_model.PressReceivers.Count}, Over: {_model.OverReceivers.Count}, Drag: {_model.DragReceivers.Count}"
			);
		}

		private static string GetHitName(RaycastHit hit) =>
			hit.collider != null ? hit.collider.name : "<missing collider>";

		private static string GetObjectName(Object? obj) =>
			obj != null ? obj.name : "<none>";

		private string DescribeHits(RaycastHit[] hits)
		{
			if (hits.Length == 0)
				return "0 hits";

			var hitNames = new string[hits.Length];
			for (var i = 0; i < hits.Length; i++)
				hitNames[i] = GetHitName(hits[i]);

			return $"{hits.Length} hits [{string.Join(", ", hitNames)}]";
		}

		private void HandlePointerEvents()
		{
			var pointer = Pointer.current;
			if (pointer == null)
			{
				if (!_loggedMissingPointer)
				{
					LogPointer("No current pointer device.");
					_loggedMissingPointer = true;
				}

				return;
			}

			_loggedMissingPointer = false;

			if (_mainCamera == null)
				_mainCamera = Camera.main;

			if (_mainCamera == null && !_loggedMissingCamera)
			{
				LogPointer("No main camera available for pointer raycasts.");
				_loggedMissingCamera = true;
			}
			else if (_mainCamera != null)
			{
				_loggedMissingCamera = false;
			}

			var currentHits = RaycastUtils.TryGetAllUnderPointer(
								  _mainCamera,
								  out var hits,
								  sortRaycastHitsByDistance
							  )
								  ? hits
								  : Array.Empty<RaycastHit>();

			ProcessOverEvents(
				currentHits,
				_previousHits
			);

			ProcessPressEvents(
				pointer,
				currentHits
			);

			_previousHits.Clear();
			_previousHits.AddRange(
				currentHits
			);
		}

		private void LogPointer(string message)
		{
			if (!enableLogs)
				return;

			Logger.Log(
				$"[{nameof(PointerManager)}] {message}",
				contextObject: this,
				category: LogCategory.Utilities
			);
		}

		private void ProcessOverEvents(RaycastHit[] currentHits, List<RaycastHit> previousHits)
		{
			if (!_model.ShouldProcessOverEvents(_model.OverReceivers.Count))
				return;

			// ---- Dispatch exit events (in previous but NOT in current) --------------
			for (var i = 0; i < previousHits.Count; i++)
			{
				var prevHit  = previousHits[i];
				var prevColl = prevHit.collider;
				if (prevColl == null) continue;

				var found = false;

				for (var j = 0; j < currentHits.Length; j++)
				{
					var currColl = currentHits[j].collider;

					if (currColl == prevColl)
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					_singleHitBuffer[0] = prevHit;
					LogPointer($"Exit: {GetHitName(prevHit)}");
					foreach (var receiver in _model.OverReceivers.ToArray())
					{
						receiver.OnPointerExit(
							_singleHitBuffer
						);
					}
				}
			}

			// ---- Dispatch enter events (in current but NOT in previous) -------------
			for (var i = 0; i < currentHits.Length; i++)
			{
				var currHit  = currentHits[i];
				var currColl = currHit.collider;
				if (currColl == null) continue;

				var found = false;

				for (var j = 0; j < previousHits.Count; j++)
				{
					var prevColl = previousHits[j].collider;

					if (prevColl == currColl)
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					_singleHitBuffer[0] = currHit;
					LogPointer($"Enter: {GetHitName(currHit)}");
					foreach (var receiver in _model.OverReceivers.ToArray())
					{
						receiver.OnPointerEnter(
							_singleHitBuffer
						);
					}
				}
			}

			// ---- Dispatch stay once with all current hits ---------------------------
			if (_model.ShouldDispatchStay(currentHits.Length))
			{
				LogPointer($"Stay: {DescribeHits(currentHits)}");
				foreach (var receiver in _model.OverReceivers.ToArray())
				{
					receiver.OnPointerStay(
						currentHits
					);
				}
			}
		}

		private void ProcessPressEvents(Pointer pointer, RaycastHit[] hits)
		{
			// Handle pointer down event
			if (pointer.press.wasPressedThisFrame)
			{
				_model.NotifyPointerPressed();
				LogPointer($"Down at {pointer.position.ReadValue()}: {DescribeHits(hits)}");
				foreach (var receiver in _model.PressReceivers.ToArray())
				{
					receiver.OnPointerDown(
						hits
					);
				}
			}

			// Handle pointer hold event
			if (pointer.press.isPressed)
			{
				LogPointer($"Hold at {pointer.position.ReadValue()}: {DescribeHits(hits)}");
				foreach (var receiver in _model.PressReceivers.ToArray())
				{
					receiver.OnPointerHold(
						hits
					);
				}
			}

			// Handle pointer drag event (only while pressed and moved this frame)
			if (_model.ShouldDispatchDrag(pointer.press.isPressed, pointer.delta.magnitude, _model.DragReceivers.Count))
			{
				LogPointer($"Drag delta {pointer.delta.ReadValue()}: {DescribeHits(hits)}");
				foreach (var receiver in _model.DragReceivers.ToArray())
				{
					receiver.OnPointerDrag(
						hits
					);
				}
			}

			// Handle pointer up and click events
			if (pointer.press.wasReleasedThisFrame)
			{
				LogPointer($"Up at {pointer.position.ReadValue()}: {DescribeHits(hits)}");
				foreach (var receiver in _model.PressReceivers.ToArray())
				{
					receiver.OnPointerUp(
						hits
					);
				}

				// A click is a press and a release in sequence.
				// We check _isPointerDown to ensure a press event happened before this release.
				if (_model.ShouldDispatchClick())
				{
					LogPointer($"Click at {pointer.position.ReadValue()}: {DescribeHits(hits)}");

					if (_model.ShouldNotifyClickMissed(hits.Length))
						PointerClickMissed?.Invoke();

					foreach (var receiver in _model.PressReceivers.ToArray())
					{
						receiver.OnPointerClick(
							hits
						);
					}
				}

				// The press sequence is now over, so reset the flag for the next press.
				_model.NotifyPointerReleased();
			}
		}

		private void RegisterTickable()
		{
			if (_isTickRegistered)
				return;

			if (!TickableRegistration.Register(this))
			{
				LogPointer("Could not register tickable because TickableManager is unavailable.");
				return;
			}

			_isTickRegistered = true;
			LogPointer("Registered with TickableManager.");
		}

		private void UnregisterTickable()
		{
			if (!_isTickRegistered)
				return;

			TickableRegistration.Unregister(this);
			_isTickRegistered = false;
		}
	}

	/// <summary>
	///     Base interface for all pointer event receivers.
	/// </summary>
	public interface IPointerDataReceiver { }

	/// <summary>
	///     Interface for handling pointer drag events.
	/// </summary>
	public interface IPointerDragDataReceiver : IPointerDataReceiver
	{
		/// <summary>
		///     Called when the pointer is dragged over objects.
		/// </summary>
		/// <param name="hits">Array of raycast hits under the pointer.</param>
		void OnPointerDrag(RaycastHit[] hits);
	}

	/// <summary>
	///     Interface for handling pointer hover events.
	/// </summary>
	public interface IPointerOverDataReceiver : IPointerDataReceiver
	{
		/// <summary>
		///     Called when the pointer enters an object's area.
		/// </summary>
		/// <param name="hits">Array of raycast hits under the pointer.</param>
		void OnPointerEnter(RaycastHit[] hits);

		/// <summary>
		///     Called when the pointer exits an object's area.
		/// </summary>
		/// <param name="hits">Array of raycast hits under the pointer.</param>
		void OnPointerExit(RaycastHit[] hits);

		/// <summary>
		///     Called while the pointer remains over an object.
		/// </summary>
		/// <param name="hits">Array of raycast hits under the pointer.</param>
		void OnPointerStay(RaycastHit[] hits);
	}

	/// <summary>
	///     Interface for handling pointer press events.
	/// </summary>
	public interface IPointerPressDataReceiver : IPointerDataReceiver
	{
		/// <summary>
		///     Called when a pointer click is completed (press and release).
		/// </summary>
		/// <param name="hits">Array of raycast hits under the pointer.</param>
		void OnPointerClick(RaycastHit[] hits);

		/// <summary>
		///     Called when the pointer is pressed down.
		/// </summary>
		/// <param name="hits">Array of raycast hits under the pointer.</param>
		void OnPointerDown(RaycastHit[] hits);

		/// <summary>
		///     Called while the pointer remains pressed.
		/// </summary>
		/// <param name="hits">Array of raycast hits under the pointer.</param>
		void OnPointerHold(RaycastHit[] hits);

		/// <summary>
		///     Called when the pointer is released.
		/// </summary>
		/// <param name="hits">Array of raycast hits under the pointer.</param>
		void OnPointerUp(RaycastHit[] hits);
	}

	public sealed class PointerManagerModel<TPressReceiver, TOverReceiver, TDragReceiver>
	{
		private readonly List<TPressReceiver> _pressReceivers;
		private readonly List<TOverReceiver>  _overReceivers;
		private readonly List<TDragReceiver>  _dragReceivers;

		private bool _isPointerDown;

		public PointerManagerModel(int capacity)
		{
			_pressReceivers = new(capacity);
			_overReceivers  = new(capacity);
			_dragReceivers  = new(capacity);
		}

		public IReadOnlyList<TPressReceiver> PressReceivers => _pressReceivers;
		public IReadOnlyList<TOverReceiver>  OverReceivers  => _overReceivers;
		public IReadOnlyList<TDragReceiver>  DragReceivers  => _dragReceivers;

		public bool AddUniquePressReceiver(TPressReceiver receiver)
		{
			return AddUnique(_pressReceivers, receiver);
		}

		public bool AddUniqueOverReceiver(TOverReceiver receiver)
		{
			return AddUnique(_overReceivers, receiver);
		}

		public bool AddUniqueDragReceiver(TDragReceiver receiver)
		{
			return AddUnique(_dragReceivers, receiver);
		}

		public int RemovePressReceiver(TPressReceiver receiver)
		{
			return RemoveAll(_pressReceivers, receiver);
		}

		public int RemoveOverReceiver(TOverReceiver receiver)
		{
			return RemoveAll(_overReceivers, receiver);
		}

		public int RemoveDragReceiver(TDragReceiver receiver)
		{
			return RemoveAll(_dragReceivers, receiver);
		}

		public bool ShouldProcessOverEvents(int overReceiverCount)
		{
			return overReceiverCount > 0;
		}

		public bool ShouldDispatchStay(int currentHitCount)
		{
			return currentHitCount > 0;
		}

		public bool ShouldDispatchDrag(bool isPressed, float deltaMagnitude, int dragReceiverCount)
		{
			return isPressed && deltaMagnitude > 0f && dragReceiverCount > 0;
		}

		public void NotifyPointerPressed()
		{
			_isPointerDown = true;
		}

		public bool ShouldDispatchClick()
		{
			return _isPointerDown;
		}

		public bool ShouldNotifyClickMissed(int hitCount)
		{
			return _isPointerDown && hitCount == 0;
		}

		public void NotifyPointerReleased()
		{
			_isPointerDown = false;
		}

		private static bool AddUnique<TReceiver>(List<TReceiver> receivers, TReceiver receiver)
		{
			if (receivers.Contains(receiver)) return false;

			receivers.Add(receiver);
			return true;
		}

		private static int RemoveAll<TReceiver>(List<TReceiver> receivers, TReceiver receiver)
		{
			var removed = 0;

			while (receivers.Remove(receiver))
				removed++;

			return removed;
		}
	}
}
