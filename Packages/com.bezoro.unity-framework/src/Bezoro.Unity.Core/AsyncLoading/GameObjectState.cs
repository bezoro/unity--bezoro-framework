using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bezoro.Core.Extensions;
using Bezoro.Logging.Types;
using Bezoro.Unity.Core.Common.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.AsyncLoading;

[Serializable]
/// <summary>
/// Captures a component reference and whether it was initially enabled.
/// </summary>
public class ComponentState : IEquatable<ComponentState>
{
	[SerializeField] private bool      wasInitiallyEnabled;
	[SerializeField] private Component target;

	public ComponentState(Component target)
	{
		if (target == null) throw new ArgumentNullException(nameof(target));

		this.target         = target;
		wasInitiallyEnabled = target.IsEnabled();
	}

	public bool       WasInitiallyEnabled => wasInitiallyEnabled;
	public Component? Target              => target ? target : null;

	public static implicit operator Component?(ComponentState state) => state.Target;

	#region Equality

	public bool Equals(ComponentState? other)
	{
		if (other is null) return false;
		if (ReferenceEquals(
				this,
				other
			)) return true;

		return wasInitiallyEnabled == other.wasInitiallyEnabled && target == other.target;
	}

	public override bool Equals(object? obj)
	{
		if (obj is null) return false;
		if (ReferenceEquals(
				this,
				obj
			)) return true;

		if (obj.GetType() != GetType()) return false;

		return Equals(
			(ComponentState)obj
		);
	}

	public override int GetHashCode() => !Target.IsUnityNull() ? Target.GetHashCode() : 0;

	#endregion
}

/// <summary>
///     Converts captured component states back to their component references.
/// </summary>
public static class ComponentStateExtensions
{
	public static Component[] ToComponents(this ComponentState[] states)
	{
		if (states.IsNull())
			return Array.Empty<Component>();

		var components = new List<Component>(states.Length);

		for (var i = 0; i < states.Length; ++i)
		{
			var component = states[i]?.Target;
			if (!component.IsUnityNull())
				components.Add(component);
		}

		return components.ToArray();
	}
}

[Serializable]
/// <summary>
/// Captures a GameObject's active state and enabled state for each component.
/// </summary>
public class GameObjectState : IEquatable<GameObjectState>
{
	[SerializeField] private bool             wasInitiallyActive;
	[SerializeField] private GameObject?      target;
	[SerializeField] private ComponentState[] componentStates;

	public GameObjectState(GameObject target) : this(target, HideFlags.None) { }

	// The serialized hierarchy cache excludes editor-only helpers. Public snapshots retain every component.
	internal GameObjectState(GameObject target, HideFlags excludedComponentFlags)
	{
		this.target        = target;
		wasInitiallyActive = target.activeSelf;

		var raw   = target.GetComponents<Component>();
		var count = 0;

		for (var i = 0; i < raw.Length; i++)
		{
			if (raw[i] != null && (raw[i].hideFlags & excludedComponentFlags) == 0)
				count++;
		}

		componentStates = new ComponentState[count];
		var idx = 0;

		for (var i = 0; i < raw.Length; i++)
		{
			if (raw[i] != null && (raw[i].hideFlags & excludedComponentFlags) == 0)
				componentStates[idx++] = new(raw[i]);
		}
	}

	public bool             IsActiveSelf       => Target != null && Target.activeSelf;
	public bool             WasInitiallyActive => wasInitiallyActive;
	public ComponentState[] ComponentStates    => componentStates;

	public GameObject? Target => target ? target : null;

	public string? Name => target ? target.name : null;

	public static implicit operator GameObject?(GameObjectState state) => state.Target;

	#region Equality

	public bool Equals(GameObjectState? other)
	{
		if (other is null) return false;
		if (ReferenceEquals(
				this,
				other
			)) return true;

		return other.Target == Target &&
			   other.ComponentStates.SequenceEqual(
				   ComponentStates
			   ) &&
			   WasInitiallyActive == other.WasInitiallyActive;
	}

	public override bool Equals(object? obj) => obj is GameObjectState state &&
												Equals(
													state
												);

	public override int GetHashCode() => Target != null ? Target.GetHashCode() : 0;

	#endregion

	public async UniTask ActivateAsync(
		bool                     respectInitiallyDisabled,
		GameObject?[]?           skipGameObjects = null,
		Component[]?             skipComponents  = null,
		AsyncLoadingFrameBudget? frameBudget     = null,
		CancellationToken        ct              = default)
	{
		var skipGoSet   = skipGameObjects != null ? new HashSet<GameObject>(skipGameObjects!) : null;
		var skipCompSet = skipComponents != null ? new HashSet<Component>(skipComponents!) : null;
		frameBudget ??= new();

		bool activatedGameObject = ActivateGameObject(respectInitiallyDisabled, skipGoSet);
		if (activatedGameObject)
			await frameBudget.WaitIfNeededAsync(ct);

		await ActivateComponentsAsync(
			respectInitiallyDisabled,
			skipCompSet,
			frameBudget,
			ct
		);
	}

	public void DeactivateAll(Component[]? componentsToSkip = null)
	{
		Target?.SetActive(
			false
		);

		foreach (var component in componentStates)
		{
			if (component.Target.IsUnityNull())
				continue;

			if (!componentsToSkip.IsNull() && componentsToSkip.Contains(component.Target))
				continue;

			component.Target.Disable();
		}
	}

	public void SetActive(bool value) =>
		Target?.SetActive(
			value
		);

	private bool ActivateGameObject(bool respectPreDisabled, HashSet<GameObject>? skip)
	{
		if (Target == null) return false;
		if (Target.activeSelf) return false;

		if (respectPreDisabled)
		{
			if (!WasInitiallyActive) return false;
			if (skip != null && skip.Contains(Target)) return false;
		}

		Target.SetActive(true);
		Logger.Log($"Activated: {Target.name}", contextObject: nameof(GameObjectState), category: LogCategory.Loading);
		return true;
	}

	private async UniTask ActivateComponentsAsync(
		bool                    respectPreDisabled,
		HashSet<Component>?     skipComponents,
		AsyncLoadingFrameBudget frameBudget,
		CancellationToken       ct)
	{
		foreach (var state in componentStates)
		{
			ct.ThrowIfCancellationRequested();

			if (state.Target == null) continue;
			if (state.Target.IsEnabled()) continue;
			if (skipComponents != null && skipComponents.Contains(state.Target)) continue;

			if (respectPreDisabled)
				if (!state.WasInitiallyEnabled)
					continue;

			state.Target.Enable();
			Logger.Log(
				$"Enabled: {state.Target.name}",
				contextObject: nameof(GameObjectState),
				category: LogCategory.Loading
			);

			await frameBudget.WaitIfNeededAsync(ct);
		}
	}
}
