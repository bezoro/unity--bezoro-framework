using System;
using System.Collections.Generic;
using Bezoro.Framework.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Behaviour_Extensions
	{
		/// <summary>
		///     Toggles the enabled state of a collection of specified Behaviour components.
		/// </summary>
		/// <param name="behaviours">The collection of Behaviour components to toggle.</param>
		/// <returns>A UniTask representing the asynchronous operation of toggling the behaviours.</returns>
		public static async UniTask Toggle_Behaviours(this ICollection<Behaviour> behaviours)
		{
			Assert.Is_Not_Null_Or_Empty(behaviours);
			await Process_Behaviours(behaviours, b => b.Toggle_Behaviour());
		}

		/// <summary>
		///     Toggles the enabled state of the specified behaviour.
		///     If the behaviour is currently enabled, it will be disabled, and vice versa.
		/// </summary>
		/// <param name="behaviour">The behaviour whose state is to be toggled.</param>
		public static void Toggle_Behaviour(this Behaviour behaviour)
		{
			Assert.Is_Not_Null(behaviour);
			behaviour.Set_Behaviour_State(!behaviour.enabled);
		}

		/// <summary>
		///     Sets the enabled state of all behaviours in the given collection.
		/// </summary>
		/// <param name="behaviours">The collection of behaviours to update their state.</param>
		/// <param name="value">The desired enabled state to apply to each behaviour (true to enable, false to disable).</param>
		/// <returns>A UniTask representing the asynchronous operation.</returns>
		public static async UniTask Set_Behaviours_State(this ICollection<Behaviour> behaviours, bool value)
		{
			Assert.Is_Not_Null_Or_Empty(behaviours);
			await Process_Behaviours(behaviours, b => b.Set_Behaviour_State(value));
		}

		/// <summary>
		///     Enables all behaviours in the provided collection.
		/// </summary>
		/// <param name="behaviours">The collection of behaviours to enable.</param>
		/// <returns>A UniTask that completes once all behaviours are enabled.</returns>
		public static async UniTask Enable_Behaviours(this ICollection<Behaviour> behaviours)
		{
			Assert.Is_Not_Null_Or_Empty(behaviours);
			await behaviours.Set_Behaviours_State(true);
		}

		/// <summary>
		///     Enables the specified behaviour by setting its enabled state to true.
		/// </summary>
		/// <param name="behaviour">The behaviour to be enabled. Must not be null.</param>
		public static void Enable_Behaviour(this Behaviour behaviour)
		{
			Assert.Is_Not_Null(behaviour);
			behaviour.Set_Behaviour_State(true);
		}

		/// <summary>
		///     Disables a collection of Unity <see cref="Behaviour" /> objects asynchronously by setting their enabled state to
		///     false.
		/// </summary>
		/// <param name="behaviours">The collection of <see cref="Behaviour" /> objects to be disabled. Must not be null or empty.</param>
		/// <returns>A <see cref="UniTaskVoid" /> representing the asynchronous operation.</returns>
		public static async UniTask Disable_Behaviours(this ICollection<Behaviour> behaviours)
		{
			Assert.Is_Not_Null_Or_Empty(behaviours);
			await behaviours.Set_Behaviours_State(false);
		}

		/// <summary>
		///     Disables the specified behaviour by setting its state to disabled.
		/// </summary>
		/// <param name="behaviour">The behaviour to disable. Cannot be null.</param>
		public static void Disable_Behaviour(this Behaviour behaviour)
		{
			Assert.Is_Not_Null(behaviour);
			behaviour.Set_Behaviour_State(false);
		}

		/// Sets the behavior's enabled state to the specified value. Logs the new state of the behavior.
		/// <param name="behaviour">The behavior whose state will be changed. Must not be null.</param>
		/// <param name="value">
		///     A boolean value indicating the desired state.
		///     Pass true to enable the behavior or false to disable it.
		/// </param>
		public static void Set_Behaviour_State(this Behaviour behaviour, bool value)
		{
			Assert.Is_Not_Null(behaviour);
			behaviour.enabled = value;
			var state = value ? "enabled" : "disabled";
			BDebug.Log($"{state.Capitalize()} behaviour: {behaviour.GetType().Name}", behaviour.gameObject);
		}

		/// <summary>
		///     Retrieves the enabled state of the specified Behaviour object.
		/// </summary>
		/// <param name="behaviour">The Behaviour instance to query the enabled state of.</param>
		/// <returns>True if the Behaviour is enabled; otherwise, false.</returns>
		public static bool Get_Behaviour_State(this Behaviour behaviour)
		{
			Assert.Is_Not_Null(behaviour);
			return behaviour.enabled;
		}

		/// <summary>
		///     Processes a collection of behaviours by applying a specified action to each behaviour in the collection.
		/// </summary>
		/// <param name="behaviours">The collection of behaviours to be processed. Must not be null or empty.</param>
		/// <param name="action">The action to be applied to each behaviour in the collection. Must not be null.</param>
		/// <returns>An asynchronous task representing the operation.</returns>
		private static async UniTask Process_Behaviours(
			ICollection<Behaviour> behaviours,
			Action<Behaviour> action
		)
		{
			Assert.Is_Not_Null_Or_Empty(behaviours);

			foreach ( var behaviour in behaviours )
			{
				Assert.Is_Not_Null(behaviour);
				action(behaviour);
				await UniTask.Yield(PlayerLoopTiming.Update);
			}
		}
	}
}
