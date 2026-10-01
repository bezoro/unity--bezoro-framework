using System;
using System.Collections.Generic;
using System.Linq;
using Bezoro.Core.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Common.Extensions;

/// <summary>
///     Contains extension methods for Behaviour.
/// </summary>
public static class BehaviourExtensions
{
	/// <summary>
	///     Retrieves the enabled state of the specified Behaviour object.
	/// </summary>
	/// <param name="behaviour">The Behaviour instance to query the enabled state of.</param>
	/// <returns>True if the Behaviour is enabled; otherwise, false.</returns>
	public static bool GetBehaviourState(this Behaviour behaviour)
	{
		// Assert.Is_Not_Null(behaviour);
		if (behaviour != null) return behaviour.enabled;

		Logger.LogWarning(
			$"Behaviour is null. Aborting {nameof(GetBehaviourState)}"
		);

		return false;
	}

	/// <summary>
	///     Disables a collection of Unity <see cref="Behaviour" /> objects asynchronously by setting their enabled state to
	///     false.
	/// </summary>
	/// <param name="behaviours">The collection of <see cref="Behaviour" /> objects to be disabled. Must not be null or empty.</param>
	/// <returns>A <see cref="UniTaskVoid" /> representing the asynchronous operation.</returns>
	public static async UniTask DisableBehaviours(this ICollection<Behaviour> behaviours)
	{
		// Assert.Is_Not_Null_Or_Empty(behaviours);
		if (behaviours == null || behaviours.Count == 0)
		{
			Logger.LogWarning(
				$"Behaviours is null or empty. Aborting {nameof(DisableBehaviours)}"
			);

			return;
		}

		await behaviours.SetBehavioursState(
			false
		);
	}

	/// <summary>
	///     Enables all behaviours in the provided collection.
	/// </summary>
	/// <param name="behaviours">The collection of behaviours to enable.</param>
	/// <returns>A UniTask that completes once all behaviours are enabled.</returns>
	public static async UniTask EnableBehaviours(this ICollection<Behaviour> behaviours)
	{
		// Assert.Is_Not_Null_Or_Empty(behaviours);
		if (behaviours == null || behaviours.Count == 0)
		{
			Logger.LogWarning(
				$"Behaviours is null or empty. Aborting {nameof(EnableBehaviours)}"
			);

			return;
		}

		await behaviours.SetBehavioursState(
			true
		);
	}

	/// <summary>
	///     Sets the enabled state of all behaviours in the given collection.
	/// </summary>
	/// <param name="behaviours">The collection of behaviours to update their state.</param>
	/// <param name="value">The desired enabled state to apply to each behaviour (true to enable, false to disable).</param>
	/// <returns>A UniTask representing the asynchronous operation.</returns>
	public static async UniTask SetBehavioursState(this ICollection<Behaviour> behaviours, bool value)
	{
		// Assert.Is_Not_Null_Or_Empty(behaviours);
		if (behaviours == null || behaviours.Count == 0)
		{
			Logger.LogWarning(
				$"Behaviours is null or empty. Aborting {nameof(SetBehavioursState)}"
			);

			return;
		}

		await behaviours.ProcessBehaviours(b => b.SetBehaviourState(
											   value
										   )
		);
	}

	/// <summary>
	///     Toggles the enabled state of a collection of specified Behaviour components.
	/// </summary>
	/// <param name="behaviours">The collection of Behaviour components to toggle.</param>
	/// <returns>A UniTask representing the asynchronous operation of toggling the behaviours.</returns>
	public static async UniTask ToggleBehaviours<T>(this ICollection<T> behaviours)
		where T : Behaviour
	{
		if (behaviours == null || behaviours.Count == 0)
		{
			Logger.LogWarning(
				$"Behaviours is null or empty. Aborting {nameof(ToggleBehaviours)}"
			);

			return;
		}

		await behaviours.ProcessBehaviours(b => b.ToggleBehaviour()
		);
	}

	/// <summary>
	///     Disables the specified behaviour by setting its state to disabled.
	/// </summary>
	/// <param name="behaviour">The behaviour to disable. Cannot be null.</param>
	public static void DisableBehaviour(this Behaviour behaviour)
	{
		// Assert.Is_Not_Null(behaviour);
		if (behaviour == null)
		{
			Logger.LogWarning(
				$"Behaviour is null. Aborting {nameof(DisableBehaviour)}"
			);

			return;
		}

		behaviour.SetBehaviourState(
			false
		);
	}

	/// <summary>
	///     Enables the specified behaviour by setting its enabled state to true.
	/// </summary>
	/// <param name="behaviour">The behaviour to be enabled. Must not be null.</param>
	public static void EnableBehaviour(this Behaviour behaviour)
	{
		// Assert.Is_Not_Null(behaviour);
		if (behaviour == null)
		{
			Logger.LogWarning(
				$"Behaviour is null. Aborting {nameof(EnableBehaviour)}"
			);

			return;
		}

		behaviour.SetBehaviourState(
			true
		);
	}

	/// Sets the behavior's enabled state to the specified value. Logs the new state of the behavior.
	/// <param name="behaviour">The behavior whose state will be changed. Must not be null.</param>
	/// <param name="value">
	///     A boolean value indicating the desired state.
	///     Pass true to enable the behavior or false to disable it.
	/// </param>
	public static void SetBehaviourState(this Behaviour behaviour, bool value)
	{
		// Assert.Is_Not_Null(behaviour);
		if (behaviour == null)
		{
			Logger.LogWarning(
				$"Behaviour is null. Aborting {nameof(SetBehaviourState)}"
			);

			return;
		}

		behaviour.enabled = value;
		string state = value ? "enabled" : "disabled";
		Logger.Log(
			$"{state.Capitalize()} behaviour: {behaviour.GetType().Name}",
			contextObject: behaviour.gameObject
		);
	}

	/// <summary>
	///     Toggles the enabled state of the specified behaviour.
	///     If the behaviour is currently enabled, it will be disabled, and vice versa.
	/// </summary>
	/// <param name="behaviour">The behaviour whose state is to be toggled.</param>
	public static void ToggleBehaviour(this Behaviour behaviour)
	{
		if (!behaviour)
		{
			Logger.LogWarning(
				$"Behaviour is null. Aborting {nameof(ToggleBehaviour)}"
			);

			return;
		}

		behaviour.SetBehaviourState(
			!behaviour.enabled
		);
	}

	/// <summary>
	///     Processes a collection of behaviours by applying a specified action to each behaviour in the collection
	///     asynchronously.
	/// </summary>
	/// <param name="behaviours">The collection of behaviours to process.</param>
	/// <param name="action">The action to be applied to each behaviour. Must not be null.</param>
	/// <param name="yieldTiming">
	///     The player loop timing to use for yielding between processing behaviours. Defaults to
	///     PlayerLoopTiming.Update.
	/// </param>
	/// <returns>An asynchronous task representing the processing operation.</returns>
	private static async UniTask ProcessBehaviours<T>(
		this ICollection<T>? behaviours,
		Action<T>            action,
		PlayerLoopTiming     yieldTiming = PlayerLoopTiming.Update
	)
		where T : Behaviour
	{
		if (behaviours == null || behaviours.Count == 0)
		{
			Logger.LogWarning(
				$"Behaviours Collection is null or empty. Aborting {nameof(ProcessBehaviours)}"
			);

			return;
		}

		if (action == null)
		{
			Logger.LogWarning(
				$"Action is null. Aborting {nameof(ProcessBehaviours)}"
			);

			return;
		}

		foreach (var behaviour in behaviours.ToArray())
		{
			if (behaviour == null)
			{
				Logger.LogWarning(
					"Behaviour is null. Skipping."
				);

				continue;
			}

			Logger.Log(
				$"Processing behaviour: {behaviour.GetType().Name}",
				contextObject: behaviour.gameObject
			);

			action.Invoke(
				behaviour
			);

			await UniTask.Yield(
				yieldTiming
			);
		}

		Logger.Log(
			"Finished Processing Behaviours.",
			contextObject: GameObjectExtensions.FindFirstIn(
				behaviours
			)
		);
	}
}
