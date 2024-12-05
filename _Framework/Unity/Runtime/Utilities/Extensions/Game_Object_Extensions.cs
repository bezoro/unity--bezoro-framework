using System;
using System.Collections.Generic;
using System.Reflection;
using Bezoro.Framework.Core;
using Bezoro.Framework.Unity.DI;
using Cysharp.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Game_Object_Extensions
	{
		public static void Set_Layer_Recursively( this GameObject game_object, int layer )
		{
			Assert.Is_Not_Null( game_object );

			game_object.layer = layer;
			foreach ( Transform child in game_object.transform ) child.gameObject.Set_Layer_Recursively( layer );
		}

		public static void Activate_Self_And_Children( this GameObject game_object, bool value )
		{
			Assert.Is_Not_Null( game_object );

			game_object.SetActive( value );
			foreach ( Transform child in game_object.transform ) child.gameObject.SetActive( value );
		}

		public static T Get_Component_In_Children<T>( this GameObject game_object, bool including_inactive )
		where T : Component
		{
			Assert.Is_Not_Null( game_object );

			var component = game_object.GetComponent<T>();

			if ( component.Not_Equals_Null() ) { return component; }

			foreach ( var child_transform in game_object.GetComponentsInChildren<Transform>( including_inactive ) )
			{
				component = child_transform.GetComponent<T>();

				if ( component != null ) { return component; }
			}

			return null;
		}

		public static void Destroy_All_Children( this GameObject game_object )
		{
			Assert.Is_Not_Null( game_object );

			foreach ( Transform child in game_object.transform ) Object.Destroy( child.gameObject );
		}

		public static void Reset_Transform( this GameObject game_object )
		{
			Assert.Is_Not_Null( game_object );

			var transform = game_object.transform;
			transform.position   = Vector3.zero;
			transform.rotation   = Quaternion.identity;
			transform.localScale = Vector3.one;
		}

		public static bool Has_Component<T>( this GameObject gameObject )
		where T : Component
		{
			Assert.Is_Not_Null( gameObject );

			return gameObject.GetComponent<T>() != null;
		}

		public static void Copy_Components_From( this GameObject destination, GameObject source )
		{
			Assert.Is_Not_Null( destination );
			Assert.Is_Not_Null( source );

			// Instantiate a temporary clone of the source GameObject
			var temp_clone = Object.Instantiate( source );

			// Transfer components one by one
			foreach ( var temp_component in temp_clone.GetComponents<Component>() )
			{
				if ( temp_component is Transform ) { continue; }

				var destination_component = destination.GetComponent( temp_component.GetType() )
											?? destination.AddComponent( temp_component.GetType() );

				CopyComponentData( temp_component, destination_component );
			}

			// Destroy the temporary clone
			Object.DestroyImmediate( temp_clone );
		}

		private static void CopyComponentData( Component source, Component destination )
		{
			var type = source.GetType();

			foreach ( var field in type.GetFields(
						 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) )
			{
				field.SetValue( destination, field.GetValue( source ) );
			}

			foreach ( var prop in type.GetProperties(
						 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) )
			{
				if ( prop.CanWrite ) prop.SetValue( destination, prop.GetValue( source ) );
			}
		}

		public static void Set_Parent(
			this GameObject child,
			GameObject parent,
			TransformAdjustmentMethod transform_adjustment_method = TransformAdjustmentMethod.KeepTransformState
		)
		{
			switch ( transform_adjustment_method )
			{
				case TransformAdjustmentMethod.KeepTransformState:
					child.transform.SetParent( parent.transform );
					break;

				case TransformAdjustmentMethod.ResetTransformState:
					child.transform.SetParent( parent.transform );
					child.transform.ResetAll();
					break;
			}
		}

		public static Component[] AddComponents( this GameObject go, Type[] component_types )
		{
			return go.AddComponentsInternal( component_types ).GetAwaiter().GetResult();
		}

		public static async UniTask<Component[]> Add_Components_Async( this GameObject go, Type[] component_types )
		{
			return await go.AddComponentsInternal( component_types, true );
		}

		private static async UniTask<Component[]> AddComponentsInternal(
			this GameObject go,
			Type[] component_types,
			bool use_async = false
		)
		{
			if ( !PlayerLoopHelper.IsMainThread )
			{
				UniTask.ReturnToMainThread();
			}

			Validation_Helpers.Validate_Object_Not_Null( go );
			Validation_Helpers.Validate_Object_Not_Null( component_types );

			var added_components = new Component[component_types.Length];
			var behaviours       = new Behaviour[component_types.Length];

			foreach ( var component_type in component_types )
			{
				if ( component_type.Is_Null() )
				{
					BDebug.Log_Warning( "Component type is null. Skipping...", go );
					continue;
				}

				Validation_Helpers.Validate_Is_Subclass_Of<Component>(
					go, nameof( Add_Components_Async ), component_type );

				BDebug.Log( go, $"Added component: {component_type.Name}", go );

				var component = go.AddComponent( component_type );

				added_components.Add( component );

				if ( component is not Behaviour behaviour )
					continue;

				behaviours.Add( behaviour );
				behaviour.enabled = false;

				if ( use_async )
					await UniTask.WaitForEndOfFrame();
			}

			BDebug.Log( $"Finished Adding components to GameObject: {go.name}", go );

			return added_components;
		}

		public static Behaviour[] Enable_Behaviours( this GameObject go )
		{
			var behaviours = go.GetComponents<Behaviour>();
			behaviours.Enable_Behaviours_Internal().GetAwaiter().GetResult();
			return behaviours;
		}

		public static async UniTask<Behaviour[]> Enable_Behaviours_Async( this GameObject go )
		{
			var behaviours = go.GetComponents<Behaviour>();
			await behaviours.Enable_Behaviours_Internal( true );
			return behaviours;
		}

		private static async UniTask Enable_Behaviours_Internal( this Behaviour[] behaviours, bool use_async = false )
		{
			Validation_Helpers.Validate_Object_Not_Null( behaviours );

			foreach ( var behaviour in behaviours )
			{
				BDebug.Log( $"Enabled behaviour: {behaviour.GetType().Name}", behaviour.gameObject );

				behaviour.enabled = true;

				if ( use_async ) await UniTask.Yield( PlayerLoopTiming.Update );
			}
		}

		/// <summary>
		///     Tries to get the component of type <typeparamref name="T" /> in the specified
		///     <paramref name="game_object" /> hierarchy.
		/// </summary>
		/// <typeparam name="T">The type of component to retrieve.</typeparam>
		/// <param name="game_object">The GameObject of which to retrieve the component.</param>
		/// <param name="component">The component of type <typeparamref name="T" /> if found, otherwise null.</param>
		/// <returns>true if the component was found, false otherwise.</returns>
		public static bool Try_Get_Component_In_Game_Object_Hierarchy<T>( this GameObject game_object, out T component )
		where T : Component
		{
			return game_object.TryGetComponent( out component )
				   || game_object.Try_Get_Component_In_Children( out component )
				   || Try_Get_Component_In_Parents( game_object, out component );
		}

		public static bool Try_Get_Component_In_Children<T>( this GameObject game_object, out T component )
		where T : Component
		{
			component = game_object.GetComponentInChildren<T>();
			return component != null;
		}

		/// <summary>
		///     Tries to get the component of type <typeparamref name="T" /> in the parents of the specified
		///     <paramref name="game_object" />.
		/// </summary>
		/// <typeparam name="T">The type of component to retrieve.</typeparam>
		/// <param name="game_object">The GameObject of which to retrieve the component.</param>
		/// <param name="component">The component of type <typeparamref name="T" /> if found, otherwise null.</param>
		/// <returns>true if the component was found, false otherwise.</returns>
		public static bool Try_Get_Component_In_Parents<T>( this GameObject game_object, out T component )
		where T : Component
		{
			component = game_object.Get_Component_In_Parents( typeof( T ) ) as T;
			return component != null;
		}

		public static T Get_Or_Add_Component<T>( this GameObject game_object )
		where T : Component
		{
			var component                      = game_object.GetComponent<T>();
			if ( component == null ) component = game_object.AddComponent<T>();
			return component;
		}

		public static Component Get_Component_In_Parents( this GameObject game_object, Type component )
		{
			var parent = game_object.transform.parent;

			while ( parent != null )
			{
				var result = parent.GetComponent( component );

				if ( result != null )
					return result;

				parent = parent.parent;
			}

			return null;
		}

		public static bool Try_Get_Component_In_Parents( this GameObject game_object, out Type component )
		{
			component = null;

			var parent = game_object.transform.parent;

			while ( parent != null )
			{
				component = parent.GetComponent<Type>();

				if ( component != null )
					return true;

				parent = parent.parent;
			}

			return false;
		}

		/// <summary>
		///     Adds a component of type <typeparamref name="T" /> to the specified <paramref name="game_object" />
		///     and injects the provided <paramref name="dependencies" />.
		/// </summary>
		/// <typeparam name="T">The type of component to add.</typeparam>
		/// <typeparam name="TDependencies">The type of dependencies to inject.</typeparam>
		/// <param name="game_object">The GameObject to add the component to.</param>
		/// <param name="dependencies">The dependencies to inject into the component.</param>
		/// <returns>The added component with injected dependencies.</returns>
		/// <exception cref="ArgumentNullException">
		///     Thrown if <paramref name="game_object" /> or <paramref name="dependencies" /> is
		///     null.
		/// </exception>
		public static T Add_Component_With_Dependencies<T, TDependencies>(
			this GameObject game_object,
			TDependencies dependencies
		)
		where T : MonoBehaviour, I_Injectable<TDependencies>
		{
			return Add_Component_With_Dependencies_Internal(
				game_object, dependencies, ( go, deps ) => ( go.AddComponent<T>(), deps ) );
		}

		/// <summary>
		///     Adds a component of type <typeparamref name="T" /> to the specified <paramref name="game_object" />
		///     and injects dependencies created by the provided <paramref name="dependencies_factory" />.
		/// </summary>
		/// <typeparam name="T">The type of component to add.</typeparam>
		/// <typeparam name="TDependencies">The type of dependencies to inject.</typeparam>
		/// <param name="game_object">The GameObject to add the component to.</param>
		/// <param name="dependencies_factory">A factory function to create dependencies for the component.</param>
		/// <returns>The added component with injected dependencies.</returns>
		/// <exception cref="ArgumentNullException">
		///     Thrown if <paramref name="game_object" /> or
		///     <paramref name="dependencies_factory" /> is null.
		/// </exception>
		public static T Add_Component_With_Dependencies<T, TDependencies>(
			this GameObject game_object,
			Func<T, TDependencies> dependencies_factory
		)
		where T : MonoBehaviour, I_Injectable<TDependencies>
		{
			return Add_Component_With_Dependencies_Internal(
				game_object, dependencies_factory, ( go, factory ) =>
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
		private static T Add_Component_With_Dependencies_Internal<T, TArg, TDependencies>(
			GameObject gameObject,
			TArg arg,
			Func<GameObject, TArg, (T component, TDependencies dependencies)> factory
		)
		where T : MonoBehaviour, I_Injectable<TDependencies>
		{
			if ( gameObject == null ) throw new ArgumentNullException( nameof( gameObject ) );
			if ( arg == null ) throw new ArgumentNullException( nameof( arg ) );

			T component = null;

			try
			{
				using var _ = new ProfilerMarker( $"AddComponentWithDependencies<{typeof( T ).Name}>" ).Auto();

				TDependencies dependencies;
				( component, dependencies ) = factory( gameObject, arg );

				if ( !component.ValidateDependencies( dependencies ) )
					throw new ArgumentException( $"Invalid dependencies for {typeof( T ).Name}" );

				component.Inject( dependencies );

				return component;
			} catch ( Exception ex )
			{
				BDebug.Log( ex.ToString().Bold(), gameObject );

				if ( component != null ) Object.DestroyImmediate( component );
				throw;
			}
		}

		public static async UniTask Enable_Hierarchy_Sequentially( this GameObject root )
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

		public static void Disable_Hierarchy_Immediately( this GameObject root )
		{
			var components = root.GetComponentsInChildren<MonoBehaviour>( true );
			foreach ( var component in components ) component.enabled = false;
			root.SetActive( false );
		}

		public static IEnumerable<(Transform transform, Component[] components)> Get_Hierarchy_Components(
			this GameObject root,
			int initial_capacity = 16,
			bool trim = false
		)
		{
			var transforms = root.GetComponentsInChildren<Transform>( true );
			var length     = transforms.Length;
			var result     = new (Transform transform, Component[] components)[length];

			var component_list         = new List<Component>( initial_capacity );
			var filtered_array         = new Component[initial_capacity];
			var enabled_property_cache = new Dictionary<Type, bool>( initial_capacity );

			for ( var i = 0 ; i < length ; i++ )
			{
				var transform = transforms[ i ];
				component_list.Clear();
				filtered_array.Clear();
				transform.GetComponents( component_list );

				foreach ( var component in component_list )
				{
					if ( component is Behaviour )
					{
						filtered_array.Add( component );
						continue;
					}

					var component_type = component.GetType();

					if ( !enabled_property_cache.TryGetValue( component_type, out var has_enabled ) )
					{
						has_enabled                              = component_type.GetProperty( "enabled" ) != null;
						enabled_property_cache[ component_type ] = has_enabled;
					}

					if ( has_enabled ) filtered_array.Add( component );
				}

				result[ i ] = ( transform, trim ? filtered_array.Trim() : filtered_array );
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
