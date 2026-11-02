using Bezoro.Unity.Core.Common.Extensions;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class MathAndNativeArrayExtensionsTests
	{
		[Test]
		public void Float2Extensions_Magnitude_ReturnsVectorLength()
		{
			var value = new float2(3f, 4f);

			Assert.AreEqual(5f, value.Magnitude(), 0.0001f);
		}

		[Test]
		public void Float2Extensions_SquareMagnitude_ReturnsSquaredVectorLength()
		{
			var value = new float2(3f, 4f);

			Assert.AreEqual(25f, value.Square_Magnitude(), 0.0001f);
		}

		[Test]
		public void NativeArrayExtensions_SafeDispose_WhenArrayIsCreated_DisposesWithoutThrowing()
		{
			var array = new NativeArray<int>(1, Allocator.Temp);

			Assert.DoesNotThrow(() => array.Safe_Dispose());
		}

		[Test]
		public void NativeArrayExtensions_SafeDispose_WhenArrayIsDefault_DoesNotThrow()
		{
			var array = default(NativeArray<int>);

			Assert.DoesNotThrow(() => array.Safe_Dispose());
		}
	}
}
