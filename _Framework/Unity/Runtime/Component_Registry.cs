using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bezoro.Framework.Unity
{
	/// <summary>
	///     Registry for managing and querying components with additional features for
	///     game development including spatial queries, relationship queries, and automatic cleanup.
	/// </summary>
	public class Component_Registry : IDisposable
	{
		public Component_Registry()
		{
			_componentRegistry = new();
			_componentCache    = new();
			_registryLock      = new();

			SceneManager.sceneUnloaded += On_Scene_Unloaded;
			SceneManager.sceneLoaded   += On_Scene_Loaded;
		}

		public delegate void Component_Deregistered_Handler( GameObject obj, Type componentType );
		public delegate void Component_Registered_Handler( GameObject obj, Type componentType );

		public event Component_Deregistered_Handler Component_Deregistered;
		public event Component_Registered_Handler   Component_Registered;

		private readonly Dictionary<GameObject, Dictionary<Type, Component>> _componentCache;
		private readonly Dictionary<Type, HashSet<GameObject>>               _componentRegistry;
		private readonly object                                              _registryLock;

		private bool _isDisposed;

		/// <summary>
		///     Clears the entire registry.
		/// </summary>
		public void Clear()
		{
			lock ( _registryLock )
			{
				_componentRegistry.Clear();
				_componentCache.Clear();
			}
		}

		/// <summary>
		///     Deregisters a GameObject and its components from the registry.
		/// </summary>
		public void Deregister( GameObject obj )
		{
			if ( obj == null ) return;

			lock ( _registryLock )
			{
				if ( !_componentCache.TryGetValue( obj, out var cached_components ) ) return;

				foreach ( var kvp in cached_components )
				{
					var type = kvp.Key;
					if ( !_componentRegistry.TryGetValue( type, out var obj_set ) ) continue;

					obj_set.Remove( obj );
					Component_Deregistered?.Invoke( obj, type );

					if ( obj_set.Count == 0 ) _componentRegistry.Remove( type );
				}

				_componentCache.Remove( obj );
			}
		}

		public void Dispose()
		{
			if ( _isDisposed ) return;

			SceneManager.sceneUnloaded -= On_Scene_Unloaded;
			SceneManager.sceneLoaded   -= On_Scene_Loaded;

			Clear();
			_isDisposed = true;
		}

		/// <summary>
		///     Queries for all components of type T that match the given predicates.
		/// </summary>
		public IEnumerable<T> Query_All<T>( Func<T, bool> predicate = null, bool includeDisabled = false )
		where T : Component
		{
			var results = new List<T>();

			lock ( _registryLock )
			{
				if ( !_componentRegistry.TryGetValue( typeof(T), out var obj_set ) ) return results;

				foreach ( var obj in obj_set )
				{
					if ( !includeDisabled && !obj.activeInHierarchy ) continue;
					if ( !Try_Get_Cached_Component( obj, out T component ) ) continue;
					if ( predicate == null || predicate( component ) ) results.Add( component );
				}
			}

			return results;
		}

		/// <summary>
		///     Queries for components within a specified range of a position.
		/// </summary>
		public IEnumerable<T> Query_By_Position_Within_Range<T>(
			Vector3 position,
			float range,
			Func<T, bool> predicate = null
		) where T : Component
		{
			var sqr_range = range * range;
			return Query_All<T>(
				component =>
					( component.transform.position - position ).sqrMagnitude <= sqr_range
					&& ( predicate == null || predicate( component ) )
			);
		}

		/// <summary>
		///     Queries for the first component of type T that matches the given predicates.
		/// </summary>
		public T Query_First<T>( Func<T, bool> predicate = null, bool includeDisabled = false )
		where T : Component
		{
			lock ( _registryLock )
			{
				if ( !_componentRegistry.TryGetValue( typeof(T), out var obj_set ) ) return null;

				foreach ( var obj in obj_set )
				{
					if ( !includeDisabled && !obj.activeInHierarchy ) continue;
					if ( !Try_Get_Cached_Component( obj, out T component ) ) continue;
					if ( predicate == null || predicate( component ) ) return component;
				}

				return null;
			}
		}

		/// <summary>
		///     Queries for GameObjects that have all of the specified component types.
		/// </summary>
		public IEnumerable<GameObject> Query_Multiple_Components(
			Type[] requiredComponents,
			bool includeDisabled = false
		)
		{
			if ( requiredComponents == null || requiredComponents.Length == 0 )
				return Enumerable.Empty<GameObject>();

			lock ( _registryLock )
			{
				// Start with objects that have the first required component
				if ( !_componentRegistry.TryGetValue( requiredComponents[0], out var initial_set ) )
					return Enumerable.Empty<GameObject>();

				var results = new HashSet<GameObject>( initial_set );

				// Intersect with objects that have each additional required component
				for ( var i = 1 ; i < requiredComponents.Length ; i++ )
				{
					if ( !_componentRegistry.TryGetValue( requiredComponents[i], out var component_set ) )
						return Enumerable.Empty<GameObject>();

					results.IntersectWith( component_set );
				}

				if ( !includeDisabled )
					results.RemoveWhere( obj => !obj.activeInHierarchy );

				return results;
			}
		}

		/// <summary>
		///     Registers a GameObject and its components in the registry.
		/// </summary>
		/// <param name="obj">The GameObject to register.</param>
		/// <param name="componentTypes">Optional specific component types to register. If null, registers all components.</param>
		public void Register( GameObject obj, Type[] componentTypes = null )
		{
			if ( obj == null ) return;

			lock ( _registryLock )
			{
				// Initialize cache for this object if needed
				if ( !_componentCache.ContainsKey( obj ) )
				{
					_componentCache[obj] = new();
				}

				var components = componentTypes != null
					? componentTypes.Select( obj.GetComponent )
					: obj.GetComponents<Component>();

				foreach ( var component in components )
				{
					if ( component == null ) continue;

					var type = component.GetType();

					// Update registry
					if ( !_componentRegistry.TryGetValue( type, out var obj_set ) )
					{
						obj_set                  = new();
						_componentRegistry[type] = obj_set;
					}

					if ( !obj_set.Add( obj ) ) // Returns true if object was added
						continue;

					// Update cache
					_componentCache[obj][type] = component;
					Component_Registered?.Invoke( obj, type );
				}
			}
		}

		private void Cleanup_Destroyed_Objects()
		{
			lock ( _registryLock )
			{
				var destroyed_objects = _componentCache.Keys.Where( obj => obj == null ).ToList();

				foreach ( var obj in destroyed_objects )
					Deregister( obj );
			}
		}

		private void On_Scene_Loaded( Scene scene, LoadSceneMode mode )
		{
			// auto-register objects from the new scene
			var objects_to_register = scene.GetRootGameObjects().Where( obj => obj != null ).ToList();
			foreach ( var obj in objects_to_register )
				Register( obj );
		}

		private void On_Scene_Unloaded( Scene scene )
		{
			lock ( _registryLock )
			{
				// Remove all objects from the unloaded scene
				var objects_to_remove = _componentCache.Keys.Where( obj => obj.scene == scene ).ToList();

				foreach ( var obj in objects_to_remove )
					Deregister( obj );
			}
		}

		private bool Try_Get_Cached_Component<T>( GameObject obj, out T component )
		where T : Component
		{
			component = null;

			if ( obj == null || !_componentCache.TryGetValue( obj, out var type_cache ) ) return false;
			if ( !type_cache.TryGetValue( typeof(T), out var cached_component ) ) return false;

			// Check if the object has been destroyed
			if ( cached_component == null )
			{
				Deregister( obj );
				return false;
			}

			component = (T)cached_component;
			return true;
		}
	}
}
