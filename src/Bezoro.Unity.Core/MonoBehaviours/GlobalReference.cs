using System;
using Bezoro.Unity.Core.Common.Enums;
using UnityEngine;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Registers a component for typed global access without owning singleton lifetime.
	/// </summary>
	[DefaultExecutionOrder(ExecutionOrder.EarlyManager)]
	[DisallowMultipleComponent]
	public sealed class GlobalReference : MonoBehaviour
	{
		[SerializeField] private Component? target;

		private Type?      _registeredType;
		private Component? _registeredTarget;

		public bool IsRegistered { get; private set; }

		public Component? Target => target;

		#region Unity Lifecycle

		private void Awake()
		{
			if (target == null)
			{
				enabled = false;
				return;
			}

			RegisterNow();
		}

		private void OnDestroy()
		{
			UnregisterNow();
		}

		#endregion

		internal void ConfigureTarget(Component targetComponent)
		{
			if (targetComponent == null)
				throw new ArgumentNullException(nameof(targetComponent));

			target  = targetComponent;
			enabled = true;

			RegisterNow();
		}

		internal bool RegisterNow()
		{
			if (!TryGetTarget(out var resolvedTarget))
			{
				enabled = false;
				return false;
			}

			var scope = GetComponentInParent<SingletonScope>(true);
			if (scope != null)
			{
				if (!resolvedTarget.transform.IsChildOf(scope.transform))
				{
					Debug.LogError(
						$"{nameof(GlobalReference)} target must be inside the containing {nameof(SingletonScope)}.",
						this
					);

					enabled = false;
					return false;
				}

				if (!scope.IsAccepted)
					return false;
			}

			var type = resolvedTarget.GetType();
			if (IsRegistered && _registeredType == type && ReferenceEquals(_registeredTarget, resolvedTarget))
				return true;

			UnregisterNow();

			bool registered = GlobalReferenceRegistry.TryRegister(type, resolvedTarget);

			if (!registered)
			{
				Debug.LogError(
					$"{type.Name} is already registered as a global reference.",
					this
				);

				enabled = false;
				return false;
			}

			_registeredType   = type;
			_registeredTarget = resolvedTarget;
			IsRegistered     = true;
			return true;
		}

		internal void UnregisterNow()
		{
			if (!IsRegistered || _registeredType == null || ReferenceEquals(_registeredTarget, null))
				return;

			GlobalReferenceRegistry.Unregister(_registeredType, _registeredTarget);
			IsRegistered      = false;
			_registeredType   = null;
			_registeredTarget = null;
		}

		private bool TryGetTarget(out Component resolvedTarget)
		{
			if (target != null)
			{
				resolvedTarget = target;
				return true;
			}

			Debug.LogError(
				$"{nameof(GlobalReference)} on '{name}' requires a target component.",
				this
			);

			resolvedTarget = null!;
			return false;
		}

#if UNITY_EDITOR
		private void Reset()
		{
			target = FindDefaultTarget();
		}

		private Component? FindDefaultTarget()
		{
			foreach (var component in GetComponents<Component>())
			{
				if (component is Transform || component == this)
					continue;

				return component;
			}

			return null;
		}
#endif
	}
}
