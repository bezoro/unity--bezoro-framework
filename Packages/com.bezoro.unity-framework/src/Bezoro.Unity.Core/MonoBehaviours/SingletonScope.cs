using System;
using Bezoro.Unity.Core.Common.Enums;
using Bezoro.Unity.Core.Loading;
using UnityEngine;
using UnityEngine.Serialization;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Defines the hierarchy boundary for a singleton-owned object tree.
	/// </summary>
	[DefaultExecutionOrder(ExecutionOrder.Fundamental)]
	[DisallowMultipleComponent]
	public sealed class SingletonScope : MonoBehaviour, ISerializationCallbackReceiver
	{
		private const int CURRENT_SERIALIZATION_VERSION = 1;

		[SerializeField] private Component? identity;
		[SerializeField] private SingletonLifetime lifetime;
		[SerializeField] private bool       destroyDuplicateScope = true;

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("isPersistent")]
		private bool legacyIsPersistent;

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("cleanupOnSceneUnload")]
		private bool legacyCleanupOnSceneUnload;

		[SerializeField] [HideInInspector] private int serializationVersion;

		private Type?      _identityType;
		private string?    _originScenePath;
		private bool       _persistenceApplied;
		private bool       _subscribedToUnload;

		public bool IsAccepted { get; private set; }

		public Component? Identity => identity;

		public SingletonLifetime Lifetime => lifetime;

		#region Unity Lifecycle

		private void Awake()
		{
			if (identity == null)
			{
				enabled = false;
				return;
			}

			RegisterNow();
		}

		private void Start()
		{
			if (IsAccepted)
				ApplyPersistence();
		}

		private void OnDestroy()
		{
			UnregisterNow();
		}

		#endregion

		internal void Configure(
			Component identityComponent,
			SingletonLifetime configuredLifetime,
			bool              destroyDuplicate = true)
		{
			if (identityComponent == null)
				throw new ArgumentNullException(nameof(identityComponent));

			identity             = identityComponent;
			lifetime             = configuredLifetime;
			destroyDuplicateScope = destroyDuplicate;
			enabled              = true;

			RegisterNow();
		}

		internal void ConfigureIdentity(Component identityComponent)
		{
			if (identityComponent == null)
				throw new ArgumentNullException(nameof(identityComponent));

			identity = identityComponent;
			enabled  = true;

			RegisterNow();
		}

		internal bool RegisterNow()
		{
			IsAccepted = false;

			if (!TryGetIdentity(out var resolvedIdentity))
			{
				enabled = false;
				return false;
			}

			_originScenePath = gameObject.scene.path;
			_identityType    = resolvedIdentity.GetType();
			var result = SingletonScopeRegistry.TryRegister(_identityType, this);

			switch (result)
			{
				case SingletonScopeRegistry.RegistrationResult.Accepted:
					IsAccepted = true;
					ConfigureAcceptedScope();
					return true;

				case SingletonScopeRegistry.RegistrationResult.RejectedDuplicate:
					HandleDuplicateScope();
					return false;

				default:
					throw new InvalidOperationException($"Unknown {nameof(SingletonScopeRegistry.RegistrationResult)}.");
			}
		}

		internal void UnregisterNow()
		{
			UnsubscribeFromSceneUnload();

			if (_identityType == null)
				return;

			SingletonScopeRegistry.Unregister(_identityType, this);
			_identityType      = null;
			IsAccepted         = false;
			_persistenceApplied = false;
		}

		private bool TryGetIdentity(out Component resolvedIdentity)
		{
			if (identity == null)
			{
				Debug.LogError(
					$"{nameof(SingletonScope)} on '{name}' requires an identity component.",
					this
				);

				resolvedIdentity = null!;
				return false;
			}

			if (!identity.transform.IsChildOf(transform))
			{
				Debug.LogError(
					$"{nameof(SingletonScope)} identity must be on this GameObject or one of its children.",
					this
				);

				resolvedIdentity = null!;
				return false;
			}

			resolvedIdentity = identity;
			return true;
		}

		private void ConfigureAcceptedScope()
		{
			SubscribeToSceneUnload();
			ApplyPersistence();
		}

		private void HandleDuplicateScope()
		{
			Debug.LogWarning(
				$"Duplicate singleton scope for '{_identityType!.Name}' rejected on '{name}'.",
				this
			);

			if (!destroyDuplicateScope)
			{
				enabled = false;
				return;
			}

			gameObject.SetActive(false);
			DestroyScopeObject(gameObject);
		}

		private void ApplyPersistence()
		{
			if (_persistenceApplied)
				return;

			if (lifetime == SingletonLifetime.Scene)
				return;

			if (!UnityEngine.Application.isPlaying)
				return;

			if (transform.parent != null)
				transform.SetParent(null, true);

			if (gameObject.scene.name == "DontDestroyOnLoad")
			{
				_persistenceApplied = true;
				return;
			}

			DontDestroyOnLoad(gameObject);
			_persistenceApplied = true;
		}

		private void SubscribeToSceneUnload()
		{
			if (_subscribedToUnload)
				return;

			if (lifetime != SingletonLifetime.SceneSession)
				return;

			SceneLoader.SceneUnloadCompleted += OnSceneUnloaded;
			_subscribedToUnload              =  true;
		}

		private void UnsubscribeFromSceneUnload()
		{
			if (!_subscribedToUnload)
				return;

			SceneLoader.SceneUnloadCompleted -= OnSceneUnloaded;
			_subscribedToUnload              =  false;
		}

		private void OnSceneUnloaded(string scenePath)
		{
			if (lifetime != SingletonLifetime.SceneSession)
				return;

			if (string.IsNullOrEmpty(_originScenePath))
				return;

			if (scenePath != _originScenePath)
				return;

			DestroyScopeObject(gameObject);
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			serializationVersion = CURRENT_SERIALIZATION_VERSION;
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (serializationVersion >= CURRENT_SERIALIZATION_VERSION)
				return;

			lifetime = !legacyIsPersistent
						   ? SingletonLifetime.Scene
						   : legacyCleanupOnSceneUnload
							   ? SingletonLifetime.SceneSession
							   : SingletonLifetime.Application;
		}

		private static void DestroyScopeObject(UnityEngine.Object obj)
		{
			if (UnityEngine.Application.isPlaying)
				Destroy(obj);
			else
				DestroyImmediate(obj);
		}

#if UNITY_EDITOR
		private void Reset()
		{
			identity = FindDefaultIdentity();
		}

		private Component? FindDefaultIdentity()
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
