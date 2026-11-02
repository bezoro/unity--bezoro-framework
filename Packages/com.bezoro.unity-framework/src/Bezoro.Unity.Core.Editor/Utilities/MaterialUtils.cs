using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Creates and configures Unity material assets.
	/// </summary>
	public class MaterialUtils
	{
		private void DebugMaterialProperties(Renderer renderer)
		{
			if (renderer == null)
			{
				Debug.LogError(
					"Renderer is null in DebugMaterialProperties."
				);

				return;
			}

			var mat = renderer.sharedMaterial;

			if (mat == null)
			{
				Debug.LogError(
					"SharedMaterial is null on the renderer."
				);

				return;
			}

			Debug.Log(
				$"Material name: {mat.name}, Shader: {mat.shader.name}"
			);

			// List all properties in the shader
			for (var i = 0;
				 i <
				 mat.shader.GetPropertyCount();
				 i++)
			{
				string propName = mat.shader.GetPropertyName(
					i
				);

				var propType = mat.shader.GetPropertyType(
					i
				);

				Debug.Log(
					$"Property: {propName}, Type: {propType}"
				);
			}
		}
	}
}
