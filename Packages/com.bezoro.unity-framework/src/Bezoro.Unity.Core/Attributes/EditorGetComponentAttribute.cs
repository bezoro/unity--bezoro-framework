using System;
using System.Diagnostics;
using UnityEngine;

namespace Bezoro.Unity.Core.Attributes;

[AttributeUsage(
	AttributeTargets.Field
)]
[Conditional(
	"UNITY_EDITOR"
)]
/// <summary>
/// Defines the Editor Get Component Attribute.
/// </summary>
public class EditorGetComponentAttribute : PropertyAttribute
{
	public EditorGetComponentAttribute(
		Type componentType,
		bool includeChildren = false,
		bool includeParents  = false)
	{
		if (!typeof(Component).IsAssignableFrom(
				componentType
			))
			throw new ArgumentException(
				$"Type {componentType.Name} must inherit from UnityEngine.Component"
			);

		ComponentType   = componentType;
		IncludeChildren = includeChildren;
		IncludeParents  = includeParents;
	}

	public bool IncludeChildren { get; private set; }
	public bool IncludeParents  { get; private set; }
	public bool OverrideDrawer  { get; set; }
	public Type ComponentType   { get; private set; }
}
