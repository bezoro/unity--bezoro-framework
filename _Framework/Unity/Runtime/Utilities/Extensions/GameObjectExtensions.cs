using System;
using System.Collections.Generic;
using System.Reflection;
using Bezoro.Framework.Unity.DI;
using Cysharp.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class GameObjectExtensions
	{
		public static void SetLayerRecursively( this GameObject gameObject, int layer )
		{
			gameObject.layer = layer;
			foreach ( Transform child in gameObject.transform )
			{
				child.gameObject.SetLayerRecursively( layer );
			}
		}

		public static void SetActiveChildren( this GameObject gameObject, bool isActive )
		{
			foreach ( Transform child in gameObject.transform )
			{
				child.gameObject.SetActive( isActive );
			}
		}

		public static T GetComponentInChildrenIncludingInactive<T>( this GameObject gameObject ) where T : Component
		{
			foreach ( var childTransform in gameObject.GetComponentsInChildren<Transform>( true ) )
			{
				var component = childTransform.GetComponent<T>();
				if ( component != null )
				{
					return component;
				}
			}

			return null;
		}

		public static void DestroyChildren( this GameObject gameObject )
		{
			foreach ( Transform child in gameObject.transform )
			{
				Object.Destroy( child.gameObject );
			}
		}

		public static void ResetTransform( this GameObject gameObject )
		{
			var transform = gameObject.transform;
			transform.position   = Vector3.zero;
			transform.rotation   = Quaternion.identity;
			transform.localScale = Vector3.one;
		}

		public static bool HasComponent<T>( this GameObject gameObject ) where T : Component =>
			gameObject.GetComponent<T>() != null;

		public static void CopyComponentsFrom( this GameObject destination, GameObject source )
		{
			foreach ( var sourceComponent in source.GetComponents<Component>() )
			{
				if ( sourceComponent is Transform )
				{
					continue;
				}

				var destinationComponent = destination.GetComponent( sourceComponent.GetType() );
				if ( !destinationComponent )
				{
					destinationComponent = destination.AddComponent( sourceComponent.GetType() );
				}

				foreach ( var field in sourceComponent.GetType().GetFields(
							 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
						 ) )
				{
					field.SetValue( destinationComponent, field.GetValue( sourceComponent ) );
				}

				foreach ( var prop in sourceComponent.GetType().GetProperties(
							 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
						 ) )
				{
					if ( prop.CanWrite )
					{
						prop.SetValue( destinationComponent, prop.GetValue( sourceComponent, null ), null );
					}
				}
			}
		}

		public static void SetParent(
			this GameObject           child,
			GameObject                parent,
			TransformAdjustmentMethod transformAdjustmentMethod = TransformAdjustmentMethod.KeepTransformState
		)
		{
			switch ( transformAdjustmentMethod )
			{
				case TransformAdjustmentMethod.KeepTransformState: child.transform.SetParent( parent.transform ); break;
				case TransformAdjustmentMethod.ResetTransformState:
					child.transform.SetParent( parent.transform );
					child.transform.ResetAll();
					break;
			}
		}

		public static Component[] AddComponents( this GameObject go, Type[] componentTypes ) =>
			go.AddComponentsInternal( componentTypes ).GetAwaiter().GetResult();

		public static async UniTask<Component[]> AddComponentsAsync( this GameObject go, Type[] componentTypes ) =>
			await go.AddComponentsInternal( componentTypes, true );

		private static async UniTask<Component[]> AddComponentsInternal(
			this GameObject go,
			Type[]          componentTypes,
			bool            useAsync = false
		)
		{
			if ( !PlayerLoopHelper.IsMainThread )
			{
				UniTask.ReturnToMainThread();
			}

			ValidationHelpers.ValidateObjectNotNull( go );
			ValidationHelpers.ValidateObjectNotNull( componentTypes );

			var addedComponents = new Component[componentTypes.Length];
			var behaviours      = new Behaviour[componentTypes.Length];

			foreach ( var componentType in componentTypes )
			{
				if ( componentType.IsNull() )
				{
					BDebug.LogWarning( "Component type is null. Skipping...", go );

					continue;
				}

				ValidationHelpers.ValidateIsSubclassOf<Component>( go, nameof(AddComponentsAsync), componentType );

				BDebug.Log( go, $"Added component: {componentType.Name}", go );

				var component = go.AddComponent( componentType );

				addedComponents.Add( component );

				if ( component is not Behaviour behaviour )
				{
					continue;
				}

				behaviours.Add( behaviour );
				behaviour.enabled = false;

				if ( useAsync )
				{
					await UniTask.WaitForEndOfFrame();
				}
			}

			BDebug.Log( $"Finished Adding components to GameObject: {go.name}", go );

			return addedComponents;
		}

		public static Behaviour[] EnableBehaviours( this GameObject go )
		{
			var behaviours = go.GetComponents<Behaviour>();
			behaviours.EnableBehavioursInternal().GetAwaiter().GetResult();

			return behaviours;
		}

		public static async UniTask<Behaviour[]> EnableBehavioursAsync( this GameObject go )
		{
			var behaviours = go.GetComponents<Behaviour>();
			await behaviours.EnableBehavioursInternal( true );

			return behaviours;
		}

		private static async UniTask EnableBehavioursInternal( this Behaviour[] behaviours, bool useAsync = false )
		{
			ValidationHelpers.ValidateObjectNotNull( behaviours );

			foreach ( var behaviour in behaviours )
			{
				BDebug.Log( $"Enabled behaviour: {behaviour.GetType().Name}", behaviour.gameObject );

				behaviour.enabled = true;

				if ( useAsync )
				{
					await UniTask.Yield( PlayerLoopTiming.Update );
				}
			}
		}

		/// <summary>
		///     Tries to get the component of type <typeparamref name="T" /> in the specified
		///     <paramref name="gameObject" /> hierarchy.
		/// </summary>
		/// <typeparam name="T">The type of component to retrieve.</typeparam>
		/// <param name="gameObject">The GameObject of which to retrieve the component.</param>
		/// <param name="component">The component of type <typeparamref name="T" /> if found, otherwise null.</param>
		/// <returns>true if the component was found, false otherwise.</returns>
		public static bool TryGetComponentInGameObjectHierarchy<T>( this GameObject gameObject, out T component )
		where T : Component =>
			gameObject.TryGetComponent( out component )
			|| gameObject.TryGetComponentInChildren( out component )
			|| TryGetComponentInParents( gameObject, out component );

		public static bool TryGetComponentInChildren<T>( this GameObject gameObject, out T component )
		where T : Component
		{
			component = gameObject.GetComponentInChildren<T>();

			return component != null;
		}

		/// <summary>
		///     Tries to get the component of type <typeparamref name="T" /> in the parents of the specified
		///     <paramref name="gameObject" />.
		/// </summary>
		/// <typeparam name="T">The type of component to retrieve.</typeparam>
		/// <param name="gameObject">The GameObject of which to retrieve the component.</param>
		/// <param name="component">The component of type <typeparamref name="T" /> if found, otherwise null.</param>
		/// <returns>true if the component was found, false otherwise.</returns>
		public static bool TryGetComponentInParents<T>( this GameObject gameObject, out T component )
		where T : Component
		{
			component = gameObject.GetComponentInParents( typeof(T) ) as T;

			return component != null;
		}

		public static T GetOrAddComponent<T>( this GameObject gameObject ) where T : Component
		{
			var component = gameObject.GetComponent<T>();

			if ( component == null )
			{
				component = gameObject.AddComponent<T>();
			}

			return component;
		}

		public static Component GetComponentInParents( this GameObject gameObject, Type component )
		{
			var parent = gameObject.transform.parent;

			while (parent != null)
			{
				var result = parent.GetComponent( component );

				if ( result != null )
				{
					return result;
				}

				parent = parent.parent;
			}

			return null;
		}

		public static bool TryGetComponentInParents( this GameObject gameObject, out Type component )
		{
			component = null;

			var parent = gameObject.transform.parent;

			while (parent != null)
			{
				component = parent.GetComponent<Type>();

				if ( component != null )
				{
					return true;
				}

				parent = parent.parent;
			}

			return false;
		}

		/// <summary>
		///     Adds a component of type <typeparamref name="T" /> to the specified <paramref name="gameObject" />
		///     and injects the provided <paramref name="dependencies" />.
		/// </summary>
		/// <typeparam name="T">The type of component to add.</typeparam>
		/// <typeparam name="TDependencies">The type of dependencies to inject.</typeparam>
		/// <param name="gameObject">The GameObject to add the component to.</param>
		/// <param name="dependencies">The dependencies to inject into the component.</param>
		/// <returns>The added component with injected dependencies.</returns>
		/// <exception cref="ArgumentNullException">
		///     Thrown if <paramref name="gameObject" /> or <paramref name="dependencies" /> is
		///     null.
		/// </exception>
		public static T AddComponentWithDependencies<T, TDependencies>(
			this GameObject gameObject,
			TDependencies   dependencies
		)
		where T : MonoBehaviour, IInjectable<TDependencies>
		{
			return AddComponentWithDependenciesInternal(
				gameObject,
				dependencies,
				( go, deps ) => ( go.AddComponent<T>(), deps )
			);
		}

		/// <summary>
		///     Adds a component of type <typeparamref name="T" /> to the specified <paramref name="gameObject" />
		///     and injects dependencies created by the provided <paramref name="dependenciesFactory" />.
		/// </summary>
		/// <typeparam name="T">The type of component to add.</typeparam>
		/// <typeparam name="TDependencies">The type of dependencies to inject.</typeparam>
		/// <param name="gameObject">The GameObject to add the component to.</param>
		/// <param name="dependenciesFactory">A factory function to create dependencies for the component.</param>
		/// <returns>The added component with injected dependencies.</returns>
		/// <exception cref="ArgumentNullException">
		///     Thrown if <paramref name="gameObject" /> or
		///     <paramref name="dependenciesFactory" /> is null.
		/// </exception>
		public static T AddComponentWithDependencies<T, TDependencies>(
			this GameObject        gameObject,
			Func<T, TDependencies> dependenciesFactory
		)
		where T : MonoBehaviour, IInjectable<TDependencies>
		{
			return AddComponentWithDependenciesInternal(
				gameObject,
				dependenciesFactory,
				( go, factory ) =>
				{
					var component = go.AddComponent<T>();

					return ( component, factory( component ) );
				}
			);
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
		private static T AddComponentWithDependenciesInternal<T, TArg, TDependencies>(
			GameObject                                                        gameObject,
			TArg                                                              arg,
			Func<GameObject, TArg, (T component, TDependencies dependencies)> factory
		)
		where T : MonoBehaviour, IInjectable<TDependencies>
		{
			if ( gameObject == null )
			{
				throw new ArgumentNullException( nameof(gameObject) );
			}

			if ( arg == null )
			{
				throw new ArgumentNullException( nameof(arg) );
			}

			T component = null;

			try
			{
				using var _ = new ProfilerMarker( $"AddComponentWithDependencies<{typeof(T).Name}>" ).Auto();

				TDependencies dependencies;
				( component, dependencies ) = factory( gameObject, arg );

				if ( !component.ValidateDependencies( dependencies ) )
				{
					throw new ArgumentException( $"Invalid dependencies for {typeof(T).Name}" );
				}

				component.Inject( dependencies );

				return component;
			}
			catch ( Exception ex )
			{
				BDebug.Log( ex.ToString().Bold(), gameObject );

				if ( component != null )
				{
					Object.DestroyImmediate( component );
				}

				throw;
			}
		}

		public static async UniTask EnableHierarchySequentially( this GameObject root )
		{
			var components = root.GetComponentsInChildren<MonoBehaviour>( true );
			root.SetActive( true );
			await UniTask.NextFrame();

			foreach ( var component in components )
			{
				component.enabled = true;
				await UniTask.NextFrame();
			}
		}

		public static void DisableHierarchyImmediately( this GameObject root )
		{
			var components = root.GetComponentsInChildren<MonoBehaviour>( true );

			foreach ( var component in components )
			{
				component.enabled = false;
			}

			root.SetActive( false );
		}

		public static IEnumerable<(Transform transform, Component[] components)> GetHierarchyComponents(
			this GameObject root,
			int             initialCapacity = 16,
			bool            trim            = false
		)
		{
			var transforms = root.GetComponentsInChildren<Transform>( true );
			var length     = transforms.Length;
			var result     = new (Transform transform, Component[] components)[length];

			var componentList        = new List<Component>( initialCapacity );
			var filteredArray        = new Component[initialCapacity];
			var enabledPropertyCache = new Dictionary<Type, bool>( initialCapacity );

			for ( var i = 0 ; i < length ; i++ )
			{
				var transform = transforms[i];
				componentList.Clear();
				filteredArray.Clear();
				transform.GetComponents( componentList );

				foreach ( var component in componentList )
				{
					if ( component is Behaviour )
					{
						filteredArray.Add( component );

						continue;
					}

					var componentType = component.GetType();
					if ( !enabledPropertyCache.TryGetValue( componentType, out var hasEnabled ) )
					{
						hasEnabled                          = componentType.GetProperty( "enabled" ) != null;
						enabledPropertyCache[componentType] = hasEnabled;
					}

					if ( hasEnabled )
					{
						filteredArray.Add( component );
					}
				}

				result[i] = ( transform, trim ? filteredArray.Trim() : filteredArray );
			}

			return result;
		}
	}

	public enum TransformAdjustmentMethod
	{
		KeepTransformState = 0,
		ResetTransformState
	}
}
