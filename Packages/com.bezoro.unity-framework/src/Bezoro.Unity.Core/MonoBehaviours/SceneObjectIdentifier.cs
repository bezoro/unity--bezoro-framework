using Bezoro.Core.Extensions;
using Bezoro.Unity.Core.Scenes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Attach this component to any GameObject that needs a persistent, unique identity
	///     within the scene reference system. It manages the GameObject's GUID and handles
	///     registration/unregistration with the SceneObjectRegistry.
	/// </summary>
	[AddComponentMenu(
		"Bezoro/Framework/Scene Object Identifier"
	)]
	[DisallowMultipleComponent]
	public class SceneObjectIdentifier : MonoBehaviour
	{
		// Serialized field holding the reference data (which contains the GUID)
		[SerializeField]
		private SceneObjectReference sceneObjectReference = new();

		/// <summary>
		///     Public access to the underlying SceneObjectReference data, if needed.
		/// </summary>
		public SceneObjectReference ObjectReference => sceneObjectReference;

		/// <summary>
		///     Public read-only access to the GUID string associated with this GameObject.
		/// </summary>
		public string? GUID => sceneObjectReference?.GUID; // Null-conditional access

		#region Unity Lifecycle

		/// <summary>
		///     Ensures a valid GUID exists and registers this GameObject with the registry.
		/// </summary>
		private void Awake()
		{
			EnsureValidGuid(); // Make sure we have a GUID, especially important if OnValidate didn't run or wasn't used

			if (!string.IsNullOrEmpty(
					GUID
				))
				SceneObjectRegistry.Register(
					gameObject,
					GUID!
				);
			else
				Debug.LogError(
					$"[{nameof(SceneObjectIdentifier)}] Failed to register GameObject '{gameObject.name}': GUID is null or empty after initialization attempt!",
					gameObject
				);
		}

		/// <summary>
		///     Unregisters this GameObject from the registry when it's destroyed.
		/// </summary>
		private void OnDestroy()
		{
			// Only try to unregister if we have a valid GUID and reference object
			if (sceneObjectReference != null &&
				!string.IsNullOrEmpty(
					GUID
				))
				SceneObjectRegistry.Unregister(
					GUID!
				);
		}

#if UNITY_EDITOR
		/// <summary>
		///     Editor-only validation. Ensures GUID exists when component is added or scene is loaded in editor.
		///     Also useful for catching GUID duplication issues early (optional).
		/// </summary>
		private void OnValidate()
		{
			// Prevent running validation on prefab assets in the project view,
			// only run on instances in the scene.
			// Check Scene path instead of IsPersistent to allow variant overrides in scene
			if (gameObject.scene.path == null &&
				EditorUtility.IsPersistent(
					this
				))
			{
				// If we are a prefab asset, clear the registry reference just in case
				// This prevents prefab assets showing up in the runtime registry lookup.
				// Note: This doesn't clear the GUID itself, which is correct for prefabs.
				if (sceneObjectReference != null &&
					!string.IsNullOrEmpty(
						GUID
					))
				{
					// No registry in edit mode usually, but good practice if there was
					// SceneObjectRegistry.Unregister(GUID);
				}

				return;
			}

			bool generatedNew = EnsureValidGuid();

			if (generatedNew)
			{
				// If OnValidate generated a new GUID, mark the component/scene dirty
				// so the change gets saved.
				EditorUtility.SetDirty(
					this
				);

				if (!UnityEngine.Application.isPlaying &&
					gameObject.scene.IsValid()) // Avoid dirtying scene during play mode
					EditorSceneManager.MarkSceneDirty(
						gameObject.scene
					);

				Debug.Log(
					$"[{nameof(SceneObjectIdentifier)}] Generated new GUID for '{gameObject.name}' in OnValidate: {(GUID ?? "<missing>").Bold()}",
					gameObject
				);
			}
		}
#endif

		#endregion

		/// <summary>
		///     Initializes the SceneObjectReference and ensures a valid GUID exists.
		///     Returns true if a *new* GUID had to be generated, false otherwise.
		/// </summary>
		private bool EnsureValidGuid()
		{
			// Ensure the reference object itself exists (defensive check)
			if (sceneObjectReference == null)
			{
				sceneObjectReference = new();

				Debug.LogWarning(
					$"[{nameof(SceneObjectIdentifier)}] SceneObjectReference was null on GameObject '{gameObject.name}'. Initializing.",
					gameObject
				);

				// Need to generate a GUID since the reference is new
				if (sceneObjectReference.GUID == null)
					sceneObjectReference.InitializeGUID(); // Ensure Serializable_GUID exists too

				var serializableGuid = sceneObjectReference.SerializableGUID;
				if (serializableGuid == null) return false;

				return serializableGuid.Generate_New_Valid_GUID(
					true
				); // Force generation
			}

			if (sceneObjectReference.SerializableGUID == null)
			{
				sceneObjectReference.InitializeGUID();

				Debug.LogWarning(
					$"[{nameof(SceneObjectIdentifier)}] Serializable_GUID was null within SceneObjectReference on GameObject '{gameObject.name}'. Initializing.",
					gameObject
				);

				var serializableGuid = sceneObjectReference.SerializableGUID;
				if (serializableGuid == null) return false;

				return serializableGuid.Generate_New_Valid_GUID(
					true
				); // Force generation
			}

			// Ensure the existing GUID is valid, generate if not
			return sceneObjectReference.SerializableGUID.Generate_New_Valid_GUID(); // Don't force if already valid
		}
	}
}
