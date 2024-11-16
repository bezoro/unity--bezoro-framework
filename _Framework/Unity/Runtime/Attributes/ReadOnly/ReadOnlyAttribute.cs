using System;
using System.Diagnostics;
using UnityEngine;

namespace Bezoro.Framework.Unity.Attributes
{
	[AttributeUsage( AttributeTargets.Field )] [Conditional( "UNITY_EDITOR" )]
	public class ReadOnlyAttribute : PropertyAttribute {}
}
