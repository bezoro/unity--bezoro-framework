using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	public static class SerializedPropertyUtils
	{
		public static object GetValue( SerializedProperty property )
		{
			object obj = property.serializedObject.targetObject;
			var path = property.propertyPath.Replace(
				".Array.data[",
				"[" );

			var elements = path.Split( '.' );

			foreach ( var element in elements )
			{
				if ( element.Contains( "[" ) )
				{
					var elementName = element.Substring(
						0,
						element.IndexOf(
							"[",
							StringComparison.Ordinal ) );

					var index = Convert.ToInt32(
						element.Substring(
								   element.IndexOf(
									   "[",
									   StringComparison.Ordinal ) )
							   .Replace(
								   "[",
								   "" )
							   .Replace(
								   "]",
								   "" ) );

					obj = GetValue_Imp(
						obj,
						elementName,
						index );
				}
				else
				{
					obj = GetValue_Imp(
						obj,
						element );
				}
			}

			return obj;
		}

		private static object GetValue_Imp( object source, string name, int index = -1 )
		{
			if ( source == null )
			{
				return null;
			}

			var type = source.GetType();
			var field = type.GetField(
				name,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );

			if ( field == null )
			{
				return null;
			}

			var val = field.GetValue( source );

			if ( index >= 0 )
			{
				if ( val is IEnumerable enumerable )
				{
					var enumerator = enumerable.GetEnumerator();

					while ( index-- >= 0 )
					{
						if ( !enumerator.MoveNext() )
						{
							return null;
						}
					}

					return enumerator.Current;
				}
			}

			return val;
		}
	}
}
