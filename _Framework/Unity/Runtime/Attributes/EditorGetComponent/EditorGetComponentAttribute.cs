using System;
using System.Diagnostics;
using UnityEngine;

namespace Bezoro.Framework.Unity.Attributes
{
	[AttributeUsage(AttributeTargets.Field)] [Conditional("UNITY_EDITOR")]
	public class EditorGetComponentAttribute : PropertyAttribute
	{
		public Type ComponentType { get; private set; }

		public bool IncludeChildren { get; private set; }

		public bool IncludeParents { get; private set; }

		public EditorGetComponentAttribute(Type componentType, bool includeChildren = false, bool includeParents = false)
		{
			if (!typeof(Component).IsAssignableFrom(componentType))
			{
				throw new ArgumentException($"Type {componentType.Name} must inherit from UnityEngine.Component");
			}

			ComponentType = componentType;
			IncludeChildren = includeChildren;
			IncludeParents = includeParents;
		}
	}
}
