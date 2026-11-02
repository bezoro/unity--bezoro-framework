using Bezoro.Unity.Core.Common.Extensions;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies Unity colors are converted to rich-text color tags.
	/// </summary>
	[TestFixture]
	public sealed class StringColourExtensionsTests
	{
		[Test]
		public void Color_ConvertsUnityColorToRgbaHexTag()
		{
			string result = "Check".Color(new(1f, 0.5f, 0f, 0.25f));

			Assert.AreEqual("<color=#FF800040>Check</color>", result);
		}
	}
}
