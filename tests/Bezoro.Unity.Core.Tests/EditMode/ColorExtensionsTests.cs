using System;
using System.Linq;
using System.Reflection;
using Bezoro.Unity.Core.Common.Extensions;
using Bezoro.Unity.Core.Common.Primitives;
using NUnit.Framework;
using UnityColor = UnityEngine.Color;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies the editor/code-theme palette extension methods expose usable colors.
	/// </summary>
	[TestFixture]
	public sealed class ColorExtensionsTests
	{
		[Test]
		public void PaletteMethods_ReturnSupportedColorTypesWithNormalizedChannels()
		{
			var methods = typeof(ColorExtensions)
						  .GetMethods(BindingFlags.Public | BindingFlags.Static)
						  .Where(method => method.GetParameters().Length == 0)
						  .Where(method => method.ReturnType == typeof(UnityColor) ||
										   method.ReturnType == typeof(Colour)
						  )
						  .ToArray();

			Assert.Greater(methods.Length, 0);

			foreach (var method in methods)
			{
				object value = method.Invoke(null, Array.Empty<object>());
				var color = value switch
				{
					UnityColor unityColor => unityColor,
					Colour colour => colour,
					_ => throw new AssertionException($"{method.Name} returned an unsupported color type.")
				};

				AssertChannelIsNormalized(method.Name, nameof(color.r), color.r);
				AssertChannelIsNormalized(method.Name, nameof(color.g), color.g);
				AssertChannelIsNormalized(method.Name, nameof(color.b), color.b);
				AssertChannelIsNormalized(method.Name, nameof(color.a), color.a);
			}
		}

		private static void AssertChannelIsNormalized(string methodName, string channelName, float value)
		{
			Assert.IsFalse(float.IsNaN(value),      $"{methodName}.{channelName} should not be NaN.");
			Assert.IsFalse(float.IsInfinity(value), $"{methodName}.{channelName} should not be infinite.");
			Assert.That(value, Is.InRange(0f, 1f), $"{methodName}.{channelName} should be normalized.");
		}
	}
}
