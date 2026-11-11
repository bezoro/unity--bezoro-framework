using System;
using System.Collections.Generic;
using System.Reflection;
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
	/// <summary>Reads the component's native enabled state, or false when it has no enabled member.</summary>
	public static bool IsEnabled(this Component c)
	{
		if (!c) return false;

		switch (c)
		{
			case Behaviour behaviour: return behaviour.enabled;
			case Renderer renderer: return renderer.enabled;
			case Collider collider: return collider.enabled;
		}

		var field = c.GetType().GetField("enabled", BindingFlags.Instance | BindingFlags.Public);
		if (field?.FieldType == typeof(bool))
			return (bool)field.GetValue(c);

		var property = EnabledProperty(c);
		return property?.GetMethod?.IsPublic == true && (bool)property.GetValue(c);
	}

	private static PropertyInfo? EnabledProperty(Component component)
	{
		var property = component.GetType().GetProperty("enabled", BindingFlags.Instance | BindingFlags.Public);
		return property?.PropertyType == typeof(bool) && property.GetIndexParameters().Length == 0 ? property : null;
	}

	private static void SetEnabled(Component component, bool enabled)
	{
		if (!component) return;

		switch (component)
		{
			case Behaviour behaviour: behaviour.enabled = enabled; return;
			case Renderer renderer: renderer.enabled = enabled; return;
			case Collider collider: collider.enabled = enabled; return;
		}

		var field = component.GetType().GetField("enabled", BindingFlags.Instance | BindingFlags.Public);
		if (field?.FieldType == typeof(bool) && !field.IsInitOnly)
		{
			field.SetValue(component, enabled);
			return;
		}

		var property = EnabledProperty(component);
		if (property?.SetMethod?.IsPublic == true)
			property.SetValue(component, enabled);
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

	/// <summary>Disables a component with a writable enabled member; ignores missing or unsupported components.</summary>
	public static void Disable(this Component c) => SetEnabled(c, false);

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

	/// <summary>Enables a component with a writable enabled member; ignores missing or unsupported components.</summary>
	public static void Enable(this Component c) => SetEnabled(c, true);

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
