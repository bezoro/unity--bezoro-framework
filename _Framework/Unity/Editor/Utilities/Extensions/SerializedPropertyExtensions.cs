using UnityEditor;

namespace Bezoro.Framework.Unity.Editor.Utilities.Extensions
{
	public static class SerializedPropertyExtensions
	{
		public static SerializedProperty FindPropertyByAutoPropertyName( this SerializedObject obj, string propName )
		{
			return obj.FindProperty( $"<{propName}>k__BackingField" );
		}
	}
}
