using System;
using Bezoro.Logging.Types;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Filters pointer raycast hits and exposes pointer interaction events.
	/// </summary>
	public class PointerEventReceiver : MonoBehaviour,
		IPointerPressDataReceiver,
		IPointerOverDataReceiver,
		IPointerDragDataReceiver
	{
		private readonly PointerEventReceiverModel _model = new();

		[SerializeField] private bool enableLogs = true;

		private bool _isRegistered;

		public event Action? PointerClick;
		public event Action? PointerDown;
		public event Action? PointerDrag;
		public event Action? PointerEnter;
		public event Action? PointerExit;
		public event Action? PointerHold;
		public event Action? PointerStay;
		public event Action? PointerUp;

		public bool EnableLogs
		{
			get => enableLogs;
			set => enableLogs = value;
		}

		#region Unity Lifecycle

		private void OnEnable() =>
			Register();

		private void OnDisable() =>
			Unregister();

		#endregion

		public void OnPointerClick(RaycastHit[] hits)
		{
			if (!IsPrimaryReceiverHit(hits))
				return;

			LogPointer("Click");
			PointerClicked();
		}

		public void OnPointerDown(RaycastHit[] hits)
		{
			if (!IsPrimaryReceiverHit(hits))
				return;

			LogPointer("Down");
			PointerDowned();
		}

		public void OnPointerDrag(RaycastHit[] hits)
		{
			if (!IsPrimaryReceiverHit(hits))
				return;

			LogPointer("Drag");
			PointerDragging();
		}

		public void OnPointerEnter(RaycastHit[] hits)
		{
			if (!IsPrimaryReceiverHit(hits))
				return;

			LogPointer("Enter");
			PointerEntered();
		}

		public void OnPointerExit(RaycastHit[] hits)
		{
			if (!IsPrimaryReceiverHit(hits))
				return;

			LogPointer("Exit");
			PointerExited();
		}

		public void OnPointerHold(RaycastHit[] hits)
		{
			if (!IsPrimaryReceiverHit(hits))
				return;

			LogPointer("Hold");
			PointerHolding();
		}

		public void OnPointerStay(RaycastHit[] hits)
		{
			if (!IsPrimaryReceiverHit(hits))
				return;

			LogPointer("Stay");
			PointerStaying();
		}

		public void OnPointerUp(RaycastHit[] hits)
		{
			if (!IsPrimaryReceiverHit(hits))
				return;

			LogPointer("Up");
			PointerUpped();
		}

		public void PointerClicked() =>
			PointerClick?.Invoke();

		public void PointerDowned() =>
			PointerDown?.Invoke();

		public void PointerDragging() =>
			PointerDrag?.Invoke();

		public void PointerEntered() =>
			PointerEnter?.Invoke();

		public void PointerExited() =>
			PointerExit?.Invoke();

		public void PointerHolding() =>
			PointerHold?.Invoke();

		public void PointerStaying() =>
			PointerStay?.Invoke();

		public void PointerUpped() =>
			PointerUp?.Invoke();

		private bool IsPrimaryReceiverHit(RaycastHit[] hits)
		{
			for (var i = 0; i < hits.Length; i++)
			{
				var collider = hits[i].collider;
				if (!_model.ShouldInspectCollider(collider != null) ||
					collider == null)
					continue;

				var receiver = collider.GetComponentInParent<PointerEventReceiver>();
				var decision = _model.DecideReceiverHit(receiver != null, receiver == this);
				if (decision.HasReceiver)
					return decision.IsPrimaryReceiver;
			}

			return false;
		}

		private void LogPointer(string message)
		{
			if (!_model.ShouldLog(enableLogs))
				return;

			Logger.Log(
				$"[{nameof(PointerEventReceiver)}] {message}: {name}",
				contextObject: this,
				category: LogCategory.Utilities
			);
		}

		private void Register()
		{
			if (!_model.ShouldRegister(_isRegistered))
				return;

			if (!PointerManager.TryGetInstance(out var pointerManager))
				return;

			pointerManager.RegisterPointerDataReceiver(this);
			_isRegistered = true;
			LogPointer("Registered with PointerManager");
		}

		private void Unregister()
		{
			if (!_model.ShouldUnregister(_isRegistered))
				return;

			if (PointerManager.TryGetInstance(out var pointerManager))
				pointerManager.UnregisterPointerDataReceiver(this);

			_isRegistered = false;
		}
	}

	public readonly struct PointerReceiverHitDecision
	{
		public PointerReceiverHitDecision(bool hasReceiver, bool isPrimaryReceiver)
		{
			HasReceiver       = hasReceiver;
			IsPrimaryReceiver = isPrimaryReceiver;
		}

		public bool HasReceiver { get; }
		public bool IsPrimaryReceiver { get; }
	}

	public sealed class PointerEventReceiverModel
	{
		public bool ShouldInspectCollider(bool hasCollider) =>
			hasCollider;

		public PointerReceiverHitDecision DecideReceiverHit(bool hasReceiver, bool isSelf)
		{
			if (!hasReceiver)
				return new(false, false);

			return new(true, isSelf);
		}

		public bool ShouldLog(bool enableLogs) =>
			enableLogs;

		public bool ShouldRegister(bool isRegistered) =>
			!isRegistered;

		public bool ShouldUnregister(bool isRegistered) =>
			isRegistered;
	}
}
