using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities.Material.Consts
{
	public static class MaterialConstants
	{
		public static int EmissionColor => Shader.PropertyToID( "_EmissionColor" );

		public static int MaterialColorID => Shader.PropertyToID( "_Color" );
	}
}
