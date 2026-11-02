using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Bezoro.Core.Extensions;
using Bezoro.Core.Helpers;
using Bezoro.Unity.Core.Common.Helpers;
using Bezoro.Unity.Core.DI;
using Cysharp.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;
using Array = System.Array;
using Logger = Bezoro.Logging.Logger;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Common.Extensions;

/// <summary>
///     Contains extension methods for Game Object.
/// </summary>
public static class GameObjectExtensions
{
	/// <summary>
	///     Checks if the GameObject has a component of the specified type.
	/// </summary>
	/// <typeparam name="T">The type of Component to check for.</typeparam>
	/// <param name="go">The GameObject to check.</param>
	/// <returns>True if the GameObject has the component, false otherwise.</returns>
	/// <exception cref="System.ArgumentNullException">Thrown if the GameObject is null.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasComponent<T>(this GameObject go)
		where T : Component
	{
		if (!go) return false;

		return go.GetComponent<T>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInAnyScene(this GameObject go) =>
		go.scene != default;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInScene(this GameObject go, string scenePath) =>
		string.Equals(go.scene.path, scenePath, StringComparison.Ordinal);

	/// <summary>
	///     Tries to get the component of type <typeparamref name="T" /> in the specified
	///     <paramref name="go" />.
	/// </summary>
	/// <typeparam name="T">The type of component to retrieve.</typeparam>
	/// <param name="go">The GameObject of which to retrieve the component.</param>
	/// <param name="component">The component of type <typeparamref name="T" /> if found, otherwise null.</param>
	/// <returns>true if the component was found, false otherwise.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetComponentInChildren<T>(this GameObject go, out T? component)
		where T : Component
	{
		component = null;

		if (!go)
		{
			Logger.LogError($"GameObject is null. Aborting {nameof(TryGetComponentInChildren)}", contextObject: go);
			return false;
		}

		if (go.transform.childCount == 0) return false;

		return component = go.GetComponentInChildren<T>();
	}

	/// <summary>
	///     Tries to get the component of type <typeparamref name="T" /> in the specified
	///     <paramref name="go" /> hierarchy.
	/// </summary>
	/// <typeparam name="T">The type of component to retrieve.</typeparam>
	/// <param name="go">The GameObject of which to retrieve the component.</param>
	/// <param name="component">The component of type <typeparamref name="T" /> if found, otherwise null.</param>
	/// <returns>true if the component was found, false otherwise.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetComponentInGameObjectHierarchy<T>(this GameObject go, out T? component)
		where T : Component
	{
		component = null;
		if (!go) return false;

		component = go.GetComponent<T>();
		if (component != null) return true;

		component = go.GetComponentInChildren<T>();
		if (component != null) return true;

		component = go.GetComponentInParent<T>();
		return component != null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetComponentInGameObjectHierarchy(
		this GameObject go,
		Type            componentType,
		out Component?  component)
	{
		component = null;
		if (!go || componentType == null) return false;

		component = go.GetComponent(componentType);
		if (component) return true;

		component = go.GetComponentInChildren(componentType);
		if (component) return true;

		component = go.GetComponentInParents(componentType);
		return component;
	}

	/// <summary>
	///     Tries to get the component of type <typeparamref name="T" /> in the parents of the specified
	///     <paramref name="go" />.
	/// </summary>
	/// <typeparam name="T">The type of component to retrieve.</typeparam>
	/// <param name="go">The GameObject of which to retrieve the component.</param>
	/// <param name="component">The component of type <typeparamref name="T" /> if found, otherwise null.</param>
	/// <returns>true if the component was found, false otherwise.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetComponentInParents<T>(this GameObject go, out T? component)
		where T : Component
	{
		component = null;
		if (!go) return false;

		component = go.GetComponentInParents(typeof(T)) as T;
		return component != null;
	}

	/// <summary>
	///     Tries to get a component of type Type from the parent hierarchy of the GameObject.
	/// </summary>
	/// <param name="go">The GameObject whose parent hierarchy to search.</param>
	/// <param name="component">When this method returns, contains the component if found; otherwise, null.</param>
	/// <returns>True if the component was found, false otherwise.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetComponentInParents(
		this GameObject go,
		Type            componentType,
		out Component?  component)
	{
		component = null;

		if (!go)
		{
			Logger.LogError($"GameObject is null. Aborting {nameof(TryGetComponentInParents)}", contextObject: go);
			return false;
		}

		var parent = go.transform.parent;
		while (parent)
		{
			component = parent.GetComponent(componentType);
			if (component) return true;

			parent = parent.parent;
		}

		return false;
	}

	/// <summary>
	///     Gets a component of the specified type from the parent hierarchy of the GameObject.
	/// </summary>
	/// <param name="go">The GameObject whose parent hierarchy to search.</param>
	/// <param name="component">The type of component to find.</param>
	/// <returns>The component if found, otherwise null.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Component? GetComponentInParents(this GameObject go, Type component)
	{
		if (!go || component == null) return null;

		Component? result = null;
		var        parent = go.transform.parent;
		while (parent)
		{
			result = parent.GetComponent(component);
			if (result) break;

			parent = parent.parent;
		}

		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Component[] GetAllComponentsInHierarchy(this GameObject go, bool includeInactive = true) =>
		go ? go.transform.root.GetComponentsInChildren<Component>(includeInactive) : Array.Empty<Component>();

	/// <summary>
	///     Finds the first valid GameObject in a collection of components.
	/// </summary>
	/// <typeparam name="T">The type of components in the collection.</typeparam>
	/// <param name="componentCollection">The collection of components to search through.</param>
	/// <returns>The first valid GameObject found, or null if none is found.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GameObject? FindFirstIn<T>(ICollection<T> componentCollection)
		where T : Component
	{
		if (componentCollection == null) return null;

		foreach (var behaviour in componentCollection)
		{
			if (!behaviour) continue;

			if (!behaviour.gameObject) continue;

			Logger.LogSuccess($"Found GameObject: {behaviour.gameObject.name}");
			return behaviour.gameObject;
		}

		Logger.LogWarning("No GameObject found in the provided collection.");
		return null;
	}

	/// <summary>
	///     Adds multiple components of the specified types to the GameObject.
	/// </summary>
	/// <param name="go">The GameObject to add components to.</param>
	/// <param name="componentTypes">An array of component types to add.</param>
	/// <returns>An array of the added components.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ICollection<Component> AddComponents(this GameObject go, Type[] componentTypes) =>
		go.AddComponentsInternal(componentTypes);

	/// <summary>
	///     Gets all transforms and their components in the GameObject's hierarchy, filtering for components that are
	///     Behaviours
	///     or have an 'enabled' property.
	/// </summary>
	/// <param name="root">The root GameObject whose hierarchy to analyze.</param>
	/// <param name="initialCapacity">The initial capacity for the internal collections.</param>
	/// <returns>An enumerable of tuples containing each transform and its filtered components.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<(Transform transform, Component[] components)> GetHierarchyComponents(
		this GameObject root,
		int             initialCapacity = 16
	)
	{
		var childTransforms      = root.GetComponentsInChildren<Transform>(true);
		int transformsCount      = childTransforms.Length;
		var result               = new (Transform transform, Component[] components)[transformsCount];
		var componentList        = new List<Component>(initialCapacity);
		var enabledPropertyCache = new Dictionary<Type, bool>(initialCapacity);
		var filteredArray        = new List<Component>(initialCapacity);

		for (var i = 0; i < transformsCount; i++)
		{
			var transform = childTransforms[i];
			componentList.Clear();
			filteredArray.Clear();
			transform.GetComponents(componentList);

			foreach (var component in componentList)
			{
				if (component is Behaviour || CheckIsComponentEnabled(component, enabledPropertyCache))
					filteredArray.Add(component);
			}

			result[i] = (transform, filteredArray.ToArray());
		}

		return result;
	}

	/// <summary>
	///     Gets all components in the GameObject hierarchy
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<(Transform transform, Component[] components)> GetHierarchyComponents(
		this GameObject gameObject)
	{
		if (gameObject == null) yield break;

		const bool INCLUDE_INACTIVE = true;
		var        transforms       = gameObject.GetComponentsInChildren<Transform>(INCLUDE_INACTIVE);

		foreach (var t in transforms)
		{
			var c = t.GetComponents<Component>();
			yield return (t, c);
		}
	}

	/// <summary>
	///     Adds a component of type <typeparamref name="T" /> to the specified <paramref name="go" />
	///     and injects the provided <paramref name="dependencies" />.
	/// </summary>
	/// <typeparam name="T">The type of component to add.</typeparam>
	/// <typeparam name="TDependencies">The type of dependencies to inject.</typeparam>
	/// <param name="go">The GameObject to add the component to.</param>
	/// <param name="dependencies">The dependencies to inject into the component.</param>
	/// <returns>The added component with injected dependencies.</returns>
	/// <exception cref="ArgumentNullException">
	///     Thrown if <paramref name="go" /> or <paramref name="dependencies" /> is
	///     null.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T AddComponentWithDependencies<T, TDependencies>(
		this GameObject go,
		TDependencies   dependencies
	)
		where T : MonoBehaviour, IInjectable<TDependencies> =>
		AddComponentWithDependenciesInternal(
			go, dependencies, (gameObject, deps) => (gameObject.AddComponent<T>(), deps)
		);

	/// <summary>
	///     Adds a component of type <typeparamref name="T" /> to the specified <paramref name="go" />
	///     and injects dependencies created by the provided <paramref name="dependenciesFactory" />.
	/// </summary>
	/// <typeparam name="T">The type of component to add.</typeparam>
	/// <typeparam name="TDependencies">The type of dependencies to inject.</typeparam>
	/// <param name="go">The GameObject to add the component to.</param>
	/// <param name="dependenciesFactory">A factory function to create dependencies for the component.</param>
	/// <returns>The added component with injected dependencies.</returns>
	/// <exception cref="ArgumentNullException">
	///     Thrown if <paramref name="go" /> or
	///     <paramref name="dependenciesFactory" /> is null.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T AddComponentWithDependencies<T, TDependencies>(
		this GameObject        go,
		Func<T, TDependencies> dependenciesFactory
	)
		where T : MonoBehaviour, IInjectable<TDependencies> =>
		AddComponentWithDependenciesInternal(
			go, dependenciesFactory, (gameObject, factory) =>
			{
				var component = gameObject.AddComponent<T>();
				return (component, factory(component));
			}
		);

	/// <summary>
	///     Gets a component of the specified type from the GameObject, or adds it if it doesn't exist.
	/// </summary>
	/// <typeparam name="T">The type of Component to get or add.</typeparam>
	/// <param name="go">The GameObject to get or add the component to.</param>
	/// <returns>The existing or newly added component.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetOrAddComponent<T>(this GameObject go)
		where T : Component
	{
		var component             = go.GetComponent<T>();
		if (!component) component = go.AddComponent<T>();

		return component;
	}

	/// <summary>
	///     Adds a Behaviour component of type T to the GameObject and sets it to disabled.
	/// </summary>
	/// <typeparam name="T">The type of Behaviour to add.</typeparam>
	/// <param name="go">The GameObject to add the behaviour to.</param>
	/// <param name="behaviourType">An instance of the Behaviour type to add.</param>
	/// <returns>The added Behaviour component, or null if the operation fails.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T? AddDisabledBehaviour<T>(this GameObject go, T behaviourType)
		where T : Behaviour
	{
		if (!go)
		{
			Logger.LogWarning($"GameObject is null. Aborting {nameof(AddDisabledBehaviour)}.", contextObject: go);
			return null;
		}

		if (!behaviourType)
		{
			Logger.LogWarning($"Behaviour type is null. Aborting {nameof(AddDisabledBehaviour)}.", contextObject: go);
			return null;
		}

		var behaviour = go.AddComponent<T>();
		behaviour.enabled = false;
		Logger.LogSuccess($"Added disabled {typeof(T).Name} Behaviour.", contextObject: go);
		return behaviour;
	}

	/// <summary>
	///     Gets the first component of type T in the GameObject's children, with option to include inactive children.
	/// </summary>
	/// <typeparam name="T">The type of Component to find.</typeparam>
	/// <param name="go">The GameObject to search.</param>
	/// <param name="includeInactive">Whether to include inactive children in the search.</param>
	/// <returns>The first component of type T found, or null if none exists.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T? GetComponentInChildren<T>(this GameObject go, bool includeInactive)
		where T : Component
	{
		if (!go) return null;

		T?  component                        = null;
		var components                       = go.GetComponentsInChildren<T>(includeInactive);
		if (components.Length > 0) component = components[0];

		return component;
	}

	/// <summary>
	///     Enables a GameObject and all its MonoBehaviour components sequentially, waiting one frame between each operation.
	/// </summary>
	/// <param name="root">The root GameObject to enable along with its hierarchy.</param>
	/// <returns>A UniTask representing the asynchronous operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async UniTask EnableHierarchySequentially(this GameObject root)
	{
		var components = root.GetComponentsInChildren<MonoBehaviour>(true);
		root.SetActive(true);
		await UniTask.NextFrame();

		foreach (var component in components)
		{
			component.enabled = true;
			await UniTask.NextFrame();
		}
	}

	/// <summary>
	///     Enables all Behaviour components on the GameObject asynchronously.
	/// </summary>
	/// <param name="go">The GameObject whose Behaviours should be enabled.</param>
	/// <returns>An array of the enabled Behaviour components, or null if none are found or the GameObject is null.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async UniTask<Behaviour[]?> EnableBehaviours(this GameObject go)
	{
		if (!go)
		{
			Logger.LogWarning($"GameObject is null. Aborting {nameof(EnableBehaviours)}", contextObject: go);
			return null;
		}

		var behaviours = go.GetComponents<Behaviour>();
		if (behaviours.Length == 0)
		{
			Logger.LogWarning(
				$"No Behaviours found on {go.name}. Aborting {nameof(EnableBehaviours)}", contextObject: go
			);

			return null;
		}

		await behaviours.EnableBehaviours();
		Logger.LogSuccess($"Enabled Behaviours on {go.name}", contextObject: go);
		return behaviours;
	}

	/// <summary>
	///     Asynchronously adds a component of the specified type to the GameObject,
	///     with safe handling of Behaviour components by temporarily disabling them.
	/// </summary>
	/// <param name="go">The GameObject to add the component to.</param>
	/// <param name="componentType">The type of component to add.</param>
	/// <returns>The added component, or null if the operation fails.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async UniTask<Component?> AddComponentAsync(this GameObject go, Type componentType)
	{
		if (!go)
		{
			Logger.LogWarning($"GameObject reference is null. Aborting {nameof(AddComponentAsync)}.");
			return null;
		}

		// Verify that the 'componentType' is actually a Unity Component type
		if (!typeof(Component).IsAssignableFrom(componentType))
		{
			Logger.LogWarning(
				$"Provided type {componentType.FullName} is not a Component. Aborting {nameof(AddComponentAsync)}.",
				contextObject: go
			);

			return null;
		}

		// Ensure the method runs on Unity's main thread
		await UniTask.SwitchToMainThread();

		// Add the component
		var addedComponent = go.AddComponent(componentType);
		// If the added component is not a Behaviour, we can return immediately
		if (addedComponent is not Behaviour behaviour)
			return addedComponent;

		// Handle Behaviour types by toggling their 'enabled' state
		behaviour.enabled = false;
		await UniTask.WaitForEndOfFrame();
		behaviour.enabled = true;
		return addedComponent;
	}

	/// <summary>
	///     Asynchronously adds multiple components of specified types to the GameObject,
	///     with safe handling of Behaviour components by temporarily disabling them.
	/// </summary>
	/// <param name="go">The GameObject to add components to.</param>
	/// <param name="componentTypes">The types of components to add.</param>
	/// <returns>An array of the added components, or null if the GameObject is null.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async UniTask<Component[]?> AddComponentsAsync(
		this   GameObject go,
		params Type[]     componentTypes
	)
	{
		if (!go)
		{
			Logger.LogWarning(
				$"GameObject is null. Aborting {nameof(AddComponentsAsync)}.",
				contextObject: go
			);

			return null;
		}

		if (componentTypes.Length == 0)
		{
			Logger.LogWarning(
				$"No component types provided. Aborting {nameof(AddComponentsAsync)}.",
				contextObject: go
			);

			return null;
		}

		componentTypes = componentTypes.Where(type => type != null &&
													  typeof(Component).IsAssignableFrom(
														  type
													  )
									   )
									   .ToArray();

		if (componentTypes.Length == 0)
		{
			Logger.LogWarning(
				$"Provided component types are either null or invalid. Aborting {nameof(AddComponentsAsync)}.",
				contextObject: go
			);

			return Array.Empty<Component>();
		}

		var addedComponents = new List<Component>(
			componentTypes.Length
		);

		foreach (var type in componentTypes)
		{
			var component = await go.AddComponentAsync(
								type
							);

			if (!component)
			{
				Logger.LogWarning(
					$"Failed to add component of type {type.Name}. Skipping.",
					contextObject: go
				);

				continue;
			}

			addedComponents.Add(
				component
			);

			Logger.Log(
				$"Added component: {type.Name}",
				contextObject: go
			);

			if (component is Behaviour behaviour)
			{
				behaviour.enabled = false;
				Logger.Log(
					$"{type.Name} is a Behaviour. Disabling.",
					contextObject: go
				);
			}

			await UniTask.WaitForEndOfFrame();
		}

		foreach (var component in addedComponents)
		{
			if (component is Behaviour behaviour)
			{
				behaviour.enabled = true;
				Logger.Log(
					$"Enabling: {component.GetType().Name}",
					contextObject: go
				);
			}

			await UniTask.WaitForEndOfFrame();
		}

		return addedComponents.ToArray();
	}

	/// <summary>
	///     Asynchronously adds a component of type T to the GameObject,
	///     with safe handling of Behaviour components by temporarily disabling them.
	/// </summary>
	/// <typeparam name="T">The type of Component to add.</typeparam>
	/// <param name="go">The GameObject to add the component to.</param>
	/// <returns>The added component, or null if the GameObject is null.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async UniTask<T?> AddComponentAsync<T>(this GameObject go) where T : Component
	{
		if (!go)
		{
			Logger.LogWarning($"GameObject is null. Aborting {nameof(AddComponentAsync)}.");
			return null;
		}

		var addedComponent = go.AddComponent<T>();
		Logger.LogSuccess($"Added component: {typeof(T).Name}", contextObject: go);

		if (addedComponent is not Behaviour behaviour)
			return addedComponent;

		behaviour.enabled = false;
		Logger.Log($"Disabling: {typeof(T).Name} Behaviour.", contextObject: go);

		await UniTask.WaitForEndOfFrame();

		behaviour.enabled = true;
		Logger.Log($"Enabling: {typeof(T).Name} Behaviour", contextObject: go);

		return addedComponent;
	}

	/// <summary>
	///     Adds a Behaviour component of type T to the GameObject, sets it to disabled,
	///     and enables it when the specified predicate returns true.
	/// </summary>
	/// <typeparam name="T">The type of Behaviour to add.</typeparam>
	/// <param name="go">The GameObject to add the behaviour to.</param>
	/// <param name="behaviourType">An instance of the Behaviour type to add.</param>
	/// <param name="predicate">A function that determines when the behaviour should be enabled.</param>
	/// <returns>The added Behaviour component, or null if the operation fails.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async UniTask<T?> AddDisabledBehaviour<T>(
		this GameObject go,
		T               behaviourType,
		Func<T, bool>   predicate
	) where T : Behaviour
	{
		if (!Utils.VerifyObjectExists(go, $"GameObject is null. Aborting {nameof(AddDisabledBehaviour)}.", go))
			return null;

		if (!Utils.VerifyObjectExists(
				behaviourType, $"Behaviour type is null. Aborting {nameof(AddDisabledBehaviour)}.", go
			))
			return null;

		if (!Utils.VerifyObjectExists(predicate, $"Predicate is null. Aborting {nameof(AddDisabledBehaviour)}.", go))
			return null;

		var behaviour = go.AddComponent<T>();
		behaviour.enabled = false;
		Logger.LogSuccess($"Added disabled {typeof(T).Name} Behaviour.", contextObject: go);

		if (!predicate(behaviour))
			await UniTask.WaitUntil(() => !behaviour || predicate(behaviour));

		if (!behaviour)
		{
			Logger.LogWarning(
				$"{typeof(T).Name} Behaviour was destroyed before it could be enabled.",
				contextObject: go
			);

			return null;
		}

		behaviour.enabled = true;
		Logger.LogSuccess($"Enabled {typeof(T).Name} Behaviour.", contextObject: go);
		return behaviour;
	}

	/// <summary>
	///     Recursively activates or deactivates the GameObject and all its children.
	/// </summary>
	/// <param name="go">The GameObject to modify.</param>
	/// <param name="value">True to activate, false to deactivate.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ActivateHierarchy(this GameObject go, bool value)
	{
		ValidationHelper.IsNotNull(go);
		// Use iterative approach to avoid stack overflow
		var stack = new Stack<GameObject>();
		stack.Push(go);

		while (stack.Count > 0)
		{
			var current = stack.Pop();
			if (!current) continue;

			current.SetActive(value);
			// Add children to stack
			foreach (Transform child in current.transform)
			{
				if (child && child.gameObject)
					stack.Push(child.gameObject);
			}
		}
	}

	/// <summary>
	///     Recursively assigns the specified layer to the GameObject and all its children.
	/// </summary>
	/// <param name="go">The GameObject to modify.</param>
	/// <param name="layer">The layer to assign.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AssignLayerToHierarchy(this GameObject go, int layer)
	{
		if (!go) return;

		go.layer = layer;

		foreach (Transform child in go.transform) child.gameObject.AssignLayerToHierarchy(layer);
	}

	/// <summary>
	///     Copies all components (except Transform) from the source GameObject to the destination GameObject.
	/// </summary>
	/// <param name="destination">The GameObject to copy components to.</param>
	/// <param name="source">The GameObject to copy components from.</param>
	/// <exception cref="System.ArgumentNullException">Thrown if either GameObject is null.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CopyComponentsFrom(this GameObject destination, GameObject source)
	{
		if (!destination)
			throw new ArgumentNullException(nameof(destination));

		if (!source)
			throw new ArgumentNullException(nameof(source));

		// Instantiate a temporary clone of the source GameObject
		var tempclone = Object.Instantiate(source);

		// Transfer components one by one
		foreach (var component in tempclone.GetComponents<Component>())
		{
			if (component is Transform) continue;

			var addComponent = destination.GetComponent(component.GetType()) ??
							   destination.AddComponent(component.GetType());

			CopyComponentData(component, addComponent);
		}

		// Destroy the temporary clone
		Object.DestroyImmediate(tempclone);
	}

	/// <summary>
	///     Destroys all child GameObjects of the specified GameObject.
	/// </summary>
	/// <param name="go">The GameObject whose children should be destroyed.</param>
	/// <exception cref="System.ArgumentNullException">Thrown if the GameObject is null.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DestroyAllChildren(this GameObject go)
	{
		if (!go) return;

		foreach (Transform child in go.transform) Object.Destroy(child.gameObject);
	}

	/// <summary>
	///     Immediately disables all MonoBehaviour components in the GameObject's hierarchy and then deactivates the
	///     GameObject.
	/// </summary>
	/// <param name="root">The root GameObject to disable along with its hierarchy.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void DisableHierarchyImmediately(this GameObject root)
	{
		if (!root) return;

		var components = root.GetComponentsInChildren<MonoBehaviour>(true);

		foreach (var component in components)
		{
			if (!component) continue;

			component.gameObject.SetActive(false);
			component.enabled = false;
		}
	}

	/// <summary>
	///     Resets the GameObject's transform to default values (position: Vector3.zero, rotation: Quaternion.identity, scale:
	///     Vector3.one).
	/// </summary>
	/// <param name="go">The GameObject whose transform should be reset.</param>
	/// <exception cref="System.ArgumentNullException">Thrown if the GameObject is null.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ResetTransform(this GameObject go)
	{
		if (!go) return;

		var transform = go.transform;
		transform.position   = Vector3.zero;
		transform.rotation   = Quaternion.identity;
		transform.localScale = Vector3.one;
	}

	/// <summary>
	///     Sets the parent of the GameObject with options to adjust the transform.
	/// </summary>
	/// <param name="child">The GameObject to set as a child.</param>
	/// <param name="parent">The GameObject to set as a parent.</param>
	/// <param name="transformAdjustmentMethod">The method to use for adjusting the transform after parenting.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetParent(
		this GameObject           child,
		GameObject                parent,
		TransformAdjustmentMethod transformAdjustmentMethod = TransformAdjustmentMethod.KeepTransformState)
	{
		if (!child || !parent) return;

		switch (transformAdjustmentMethod)
		{
			case TransformAdjustmentMethod.KeepTransformState:
				child.transform.SetParent(parent.transform);
				break;
			case TransformAdjustmentMethod.ResetTransformState:
				child.transform.SetParent(parent.transform);
				child.transform.ResetAll();
				break;
		}
	}

	/// <summary>
	///     Sets the parent of the GameObject to the GameObject of the specified component.
	/// </summary>
	/// <param name="child">The GameObject to set as a child.</param>
	/// <param name="component">The Component whose GameObject will be the parent.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetParent(this GameObject child, Component component)
	{
		if (!child || !component) return;

		child.SetParent(component.gameObject);
	}

	/// <summary>
	///     Checks if a component has an 'enabled' property, caching the result for future checks.
	/// </summary>
	/// <param name="component">The component to check.</param>
	/// <param name="cache">A dictionary to cache results by component type.</param>
	/// <returns>True if the component has an 'enabled' property, false otherwise.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool CheckIsComponentEnabled(Component component, Dictionary<Type, bool>? cache)
	{
		if (!component) return false;
		if (cache == null) return false;

		var componentType = component.GetType();
		if (cache.TryGetValue(componentType, out bool hasEnabled))
			return hasEnabled;

		hasEnabled           = componentType.GetProperty("enabled") != null;
		cache[componentType] = hasEnabled;
		return hasEnabled;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ICollection<Component> AddComponentsInternal(
		this GameObject go,
		Type[]          componentTypes)
	{
		go.ThrowIfUnityNull();
		componentTypes.ThrowIfNull().ThrowIfEmpty();

		var addedComponents = new List<Component>(componentTypes.Length);

		foreach (var componentType in componentTypes)
		{
			if (componentType.IsNull())
			{
				Logger.LogWarning("Component type is null. Skipping...", contextObject: go);
				continue;
			}

			ValidationHelper.IsSubclassOf<Component>(go, nameof(AddComponentsAsync), componentType);
			var component = go.AddComponent(componentType);
			addedComponents.Add(component);
			Logger.Log($"Added component: {componentType.Name}", contextObject: go);

			if (component is not Behaviour behaviour) continue;

			behaviour.enabled = false;
		}

		Logger.Log($"Finished Adding components to GameObject: {go.name}", contextObject: go);
		return addedComponents;
	}

	/// <summary>
	///     Internal method to add a component to a GameObject with dependency injection.
	/// </summary>
	/// <typeparam name="T">The type of component to add.</typeparam>
	/// <typeparam name="TArg">The type of the argument used to create dependencies.</typeparam>
	/// <typeparam name="TDependencies">The type of dependencies to inject.</typeparam>
	/// <param name="gameObject">The GameObject to add the component to.</param>
	/// <param name="arg">The argument used to create dependencies.</param>
	/// <param name="factory">A factory function to create the component and dependencies.</param>
	/// <returns>The added component with injected dependencies.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="gameObject" /> or <paramref name="arg" /> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if the dependencies are invalid.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static T AddComponentWithDependenciesInternal<T, TArg, TDependencies>(
		GameObject                                                        gameObject,
		TArg                                                              arg,
		Func<GameObject, TArg, (T component, TDependencies dependencies)> factory
	)
		where T : MonoBehaviour, IInjectable<TDependencies>
	{
		if (!gameObject) throw new ArgumentNullException(nameof(gameObject));

		if (arg is null || EqualityComparer<TArg>.Default.Equals(arg, default!))
			throw new ArgumentNullException(nameof(arg));

		T? component = null;

		try
		{
			using var _ = new ProfilerMarker($"AddComponentWithDependencies<{typeof(T).Name}>").Auto();

			TDependencies dependencies;
			(component, dependencies) = factory(gameObject, arg);

			if (!component.ValidateDependencies(dependencies))
				throw new ArgumentException($"Invalid dependencies for {typeof(T).Name}");

			component.Inject(dependencies);
			return component;
		}
		catch (Exception ex)
		{
			Logger.Log(ex.ToString().Bold(), contextObject: gameObject);

			if (component)
				Object.DestroyImmediate(component);

			throw;
		}
	}

	/// <summary>
	///     Copies all field and property values from the source component to the destination component.
	/// </summary>
	/// <param name="source">The component to copy data from.</param>
	/// <param name="destination">The component to copy data to.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void CopyComponentData(Component source, Component destination)
	{
		var type = source.GetType();

		foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			field.SetValue(destination, field.GetValue(source));

		foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
		{
			if (prop.CanWrite)
				prop.SetValue(destination, prop.GetValue(source));
		}
	}
}

/// <summary>
///     Lists the supported Transform Adjustment Method values.
/// </summary>
public enum TransformAdjustmentMethod
{
	KeepTransformState = 0,
	ResetTransformState
}
