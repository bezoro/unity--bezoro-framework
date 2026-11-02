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
/// Defines the Read Only Attribute.
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute
{
	public ReadOnlyAttribute()
	{
		order = int.MaxValue;
	}
}
