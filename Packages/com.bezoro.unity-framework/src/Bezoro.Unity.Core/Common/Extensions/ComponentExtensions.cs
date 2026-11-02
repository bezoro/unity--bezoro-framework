using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Common.Extensions;

/// <summary>
///     Contains extension methods for Component.
/// </summary>
public static class ComponentExtensions
{
	public static bool IsEnabled(this Component c)
	{
		if (!c) return false;

		if (c is Behaviour behaviour)
			return behaviour.enabled;

		if (c.GetType().GetField(
				"enabled"
			) !=
			null)
			return (bool)c.GetType().GetField(
				"enabled"
			).GetValue(
				c
			);

		return false;
	}

	public static bool TryGetComponentInChildren<T>(this Component c, out T? component)
		where T : Component
	{
		component = null;

		return c != null && c.gameObject.TryGetComponentInChildren(out component);
	}

	public static async UniTask DisableAsync(
		this ICollection<Component> c,
		PlayerLoopTiming            playerLoopTiming = PlayerLoopTiming.Update,
		CancellationToken           ct               = default)
	{
		if (c == null || c.Count == 0) return;

		foreach (var comp in c)
		{
			if (ct.IsCancellationRequested) break;

			if (!comp) continue;

			comp.Disable();
			await UniTask.Yield(
				playerLoopTiming,
				ct
			);
		}
	}

	public static async UniTask EnableAsync(
		this Component[]  c,
		float             delayMs,
		PlayerLoopTiming  playerLoopTiming = PlayerLoopTiming.Update,
		CancellationToken ct               = default)
	{
		if (c == null || c.Length == 0) return;

		foreach (var comp in c)
		{
			if (ct.IsCancellationRequested) break;

			if (!comp) continue;

			comp.Enable();
			Logger.Log(
				$"(Async) Enabled: {comp.GetType().Name}@{comp.gameObject.name}",
				contextObject: comp
			);

			if (delayMs > 0)
			{
				await UniTask.Delay(
					TimeSpan.FromMilliseconds(
						delayMs
					),
					DelayType.Realtime,
					playerLoopTiming,
					ct
				);

				continue;
			}

			await UniTask.Yield(
				playerLoopTiming,
				ct
			);
		}
	}

	public static void Disable(this Component[] c)
	{
		if (c == null || c.Length == 0) return;

		foreach (var comp in c)
		{
			if (!comp) continue;

			comp.Disable();
		}
	}

	/// <summary>
	///     Disables a component using reflection for components that have an 'enabled' field
	/// </summary>
	public static void Disable(this Component c)
	{
		if (!c) return;

		if (c is Behaviour behaviour)
		{
			behaviour.enabled = false;
		}
		else
		{
			var enabledField = c.GetType().GetField(
				"enabled"
			);

			enabledField?.SetValue(
				c,
				false
			);
		}
	}

	public static void Enable(this Component[] c)
	{
		if (c == null || c.Length == 0) return;

		foreach (var comp in c)
		{
			if (!comp || comp.IsEnabled()) continue;

			comp.Enable();
			Logger.Log(
				$"(Sync) Enabled: {comp.GetType().Name}@{comp.gameObject.name}",
				contextObject: comp
			);
		}
	}

	/// <summary>
	///     Enables a component using reflection for components that have an 'enabled' field
	/// </summary>
	public static void Enable(this Component c)
	{
		if (!c) return;

		if (c is Behaviour behaviour)
		{
			behaviour.enabled = true;
			return;
		}

		var enabledField = c.GetType().GetField(
			"enabled"
		);

		enabledField?.SetValue(
			c,
			true
		);
	}

	public static void SetHideFlags(this Component c, HideFlags hideFlags)
	{
		if (!c) return;

		c.gameObject.hideFlags = hideFlags;
	}

	public static void SetHideFlagsRecursive(this Component c, HideFlags hideFlags)
	{
		if (!c) return;

		c.gameObject.hideFlags = hideFlags;

		foreach (Transform child in c.transform)
		{
			child.gameObject.hideFlags = hideFlags;
			child.SetHideFlagsRecursive(
				hideFlags
			);
		}
	}
}
