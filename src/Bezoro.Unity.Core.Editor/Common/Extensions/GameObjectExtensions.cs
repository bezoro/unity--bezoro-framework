using System.Collections.Generic;
using System.Reflection;
using Bezoro.Unity.Core.Common.Extensions;
using UnityEditor;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Editor.Common.Extensions
{
	/// <summary>
	///     Provides extension methods for Unity's GameObject class to simplify common operations in the Editor environment.
	///     These methods provide utilities for prefab management, layer handling, component manipulation, and transform
	///     operations.
	/// </summary>
	public static class GameObjectExtensions
	{
		public static bool AllHaveComponentOfType<T>(this ICollection<GameObject> gos)
			where T : Component
		{
			foreach (var go in gos)
			{
				if (go.IsUnityNull()) return false;
				if (!go.TryGetComponent<T>(out _)) return false;
			}

			return true;
		}

		public static bool AreEqual(this ICollection<GameObject> gos, ICollection<GameObject> compare)
		{
			if (gos.Count != compare.Count)
				return false;

			foreach (var go in gos)
			{
				if (!compare.Contains(go))
					return false;
			}

			return true;
		}

		public static bool AreValidSceneGOs(this ICollection<GameObject> gos)
		{
			foreach (var go in gos)
				if (!go.IsValidSceneGO())
					return false;

			return true;
		}

		/// <summary>
		///     Checks if the GameObject has a component of the specified type.
		/// </summary>
		/// <typeparam name="T">The type of Component to check for.</typeparam>
		/// <param name="gameObject">The GameObject to check.</param>
		/// <returns>True if the GameObject has the component, false otherwise.</returns>
		public static bool HasComponent<T>(this GameObject gameObject)
			where T : Component =>
			gameObject.GetComponent<T>() != null;

		/// <summary>
		///     Checks if the GameObject is the same instance as another GameObject.
		/// </summary>
		/// <param name="obj">The GameObject to check.</param>
		/// <param name="gameObject">The GameObject to compare against.</param>
		/// <returns>True if both GameObjects are the same instance, false otherwise.</returns>
		public static bool Is(this GameObject obj, GameObject gameObject) =>
			obj == gameObject;

		public static bool IsChild(this GameObject go) => go.transform.parent != null;

		public static bool IsChildOf(this GameObject go, GameObject parent) => go.transform.parent == parent.transform;

		/// <summary>
		///     Determines if the GameObject is not a prefab.
		/// </summary>
		/// <param name="gameObject">The GameObject to check.</param>
		/// <returns>True if the GameObject is not a prefab, false otherwise.</returns>
		public static bool IsNotPrefab(this GameObject gameObject) =>
			PrefabUtility.GetPrefabAssetType(
				gameObject
			) ==
			PrefabAssetType.NotAPrefab;

		/// <summary>
		///     Determines if the GameObject is not a prefab variant.
		/// </summary>
		/// <param name="gameObject">The GameObject to check.</param>
		/// <returns>True if the GameObject is not a prefab variant, false otherwise.</returns>
		public static bool IsNotPrefabVariant(this GameObject gameObject) =>
			PrefabUtility.GetPrefabAssetType(
				gameObject
			) !=
			PrefabAssetType.Variant;

		public static bool IsOfType<T>(this GameObject go)
			where T : Component =>
			go.GetComponent<T>() != null;

		/// <summary>
		///     Determines if the GameObject is any type of prefab.
		/// </summary>
		/// <param name="gameObject">The GameObject to check.</param>
		/// <returns>True if the GameObject is a prefab, false otherwise.</returns>
		public static bool IsPrefab(this GameObject gameObject) =>
			PrefabUtility.GetPrefabAssetType(
				gameObject
			) !=
			PrefabAssetType.NotAPrefab;

		/// <summary>
		///     Determines if the GameObject is a prefab asset in the project.
		/// </summary>
		/// <param name="go">The GameObject to check.</param>
		/// <returns>True if the GameObject is a prefab asset, false otherwise.</returns>
		public static bool IsPrefabAsset(this GameObject go) =>
			!go.gameObject.scene.IsValid() &&
			(PrefabUtility.IsPartOfPrefabAsset(
				 go.gameObject
			 ) ||
			 PrefabUtility.IsPartOfVariantPrefab(
				 go.gameObject
			 ));

		/// <summary>
		///     Determines if the GameObject is a prefab variant.
		/// </summary>
		/// <param name="gameObject">The GameObject to check.</param>
		/// <returns>True if the GameObject is a prefab variant, false otherwise.</returns>
		public static bool IsPrefabVariant(this GameObject gameObject) =>
			PrefabUtility.GetPrefabAssetType(
				gameObject
			) ==
			PrefabAssetType.Variant;

		public static bool IsRoot(this GameObject go) => go.transform.parent == null;

		public static bool IsValidSceneGO(this GameObject? go) => go && go.scene.IsValid();

		/// <summary>
		///     Attempts to get the GUID of a GameObject asset.
		/// </summary>
		/// <param name="gameObject">The GameObject asset to get the GUID for.</param>
		/// <returns>The GUID string of the asset, or null if the GameObject is not an asset.</returns>
		public static string TryGetAssetGuid(this GameObject gameObject)
		{
			string path = AssetDatabase.GetAssetPath(
				gameObject
			);

			if (!string.IsNullOrEmpty(
					path
				))
				return AssetDatabase.AssetPathToGUID(
					path
				);

			Logger.LogError(
				"GameObject is not an asset or is not part of the project."
			);

			return null!;
		}

		/// <summary>
		///     Gets a component of specified type from the GameObject's children, including inactive children.
		/// </summary>
		/// <typeparam name="T">The type of Component to find.</typeparam>
		/// <param name="gameObject">The GameObject to search within.</param>
		/// <returns>The first matching component found, or null if none was found.</returns>
		public static T GetComponentInChildrenIncludingInactive<T>(this GameObject gameObject)
			where T : Component
		{
			foreach (var childTransform in gameObject.GetComponentsInChildren<Transform>(
						 true
					 ))
			{
				var component = childTransform.GetComponent<T>();

				if (component != null) return component;
			}

			return null!;
		}

		/// <summary>
		///     Copies all components (except Transform) from a source GameObject to a destination GameObject.
		///     This includes all fields and properties in each component, including private and protected members.
		/// </summary>
		/// <param name="destination">The GameObject to copy components to.</param>
		/// <param name="source">The GameObject to copy components from.</param>
		public static void CopyComponentsFrom(this GameObject destination, GameObject source)
		{
			foreach (var sourceComponent in source.GetComponents<Component>())
			{
				if (sourceComponent is Transform) continue;

				var destinationComponent = destination.GetComponent(
					sourceComponent.GetType()
				);

				if (!destinationComponent)
					destinationComponent = destination.AddComponent(
						sourceComponent.GetType()
					);

				foreach (var field in sourceComponent.GetType()
													 .GetFields(
														 BindingFlags.Public |
														 BindingFlags.NonPublic |
														 BindingFlags.Instance
													 ))
				{
					field.SetValue(
						destinationComponent,
						field.GetValue(
							sourceComponent
						)
					);
				}

				foreach (var prop in sourceComponent.GetType().GetProperties(
							 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
						 ))
				{
					if (prop.CanWrite)
						prop.SetValue(
							destinationComponent,
							prop.GetValue(
								sourceComponent,
								null
							),
							null
						);
				}
			}
		}

		/// <summary>
		///     Destroys all child GameObjects of the parent GameObject.
		/// </summary>
		/// <param name="gameObject">The parent GameObject whose children will be destroyed.</param>
		public static void DestroyChildren(this GameObject gameObject)
		{
			foreach (Transform child in gameObject.transform)
			{
				if (UnityEngine.Application.isPlaying)
					Object.Destroy(
						child.gameObject
					);
				else
					Object.DestroyImmediate(
						child.gameObject
					);
			}
		}

		/// <summary>
		///     Resets the GameObject's transform to its default values (position to zero, rotation to identity, scale to one).
		/// </summary>
		/// <param name="gameObject">The GameObject whose transform will be reset.</param>
		public static void ResetTransform(this GameObject gameObject)
		{
			var transform = gameObject.transform;
			transform.position   = Vector3.zero;
			transform.rotation   = Quaternion.identity;
			transform.localScale = Vector3.one;
		}

		/// <summary>
		///     Sets the active state of all child GameObjects of the parent GameObject.
		/// </summary>
		/// <param name="gameObject">The parent GameObject.</param>
		/// <param name="isActive">The active state to set for all children.</param>
		public static void SetActiveChildren(this GameObject gameObject, bool isActive)
		{
			foreach (Transform child in gameObject.transform)
			{
				child.gameObject.SetActive(
					isActive
				);
			}
		}

		/// <summary>
		///     Sets the layer of the GameObject and all of its children recursively.
		/// </summary>
		/// <param name="gameObject">The GameObject to modify.</param>
		/// <param name="layer">The layer value to set.</param>
		public static void SetLayerRecursively(this GameObject gameObject, int layer)
		{
			gameObject.layer = layer;

			foreach (Transform child in gameObject.transform)
			{
				child.gameObject.SetLayerRecursively(
					layer
				);
			}
		}

		/// <summary>
		///     Sets the parent of a GameObject and resets its local transform properties to default values.
		/// </summary>
		/// <param name="child">The child GameObject to parent.</param>
		/// <param name="parent">The parent GameObject to set.</param>
		public static void SetParentAndReset(this GameObject child, GameObject parent)
		{
			child.transform.SetParent(
				parent.transform
			);

			child.transform.localPosition = Vector3.zero;
			child.transform.localRotation = Quaternion.identity;
			child.transform.localScale    = Vector3.one;
		}
	}
}
