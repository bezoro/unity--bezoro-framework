using Bezoro.Unity.Core.Common.Serialization;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies tagged strings normalize null templates and expose processed string conversions.
	/// </summary>
	[TestFixture]
	public sealed class TaggedStringTests
	{
		[Test]
		public void Constructor_WhenTemplateIsNull_UsesEmptyTemplate()
		{
			var taggedString = new TaggedString(null);

			Assert.AreEqual(string.Empty, taggedString.Template);
			Assert.AreEqual(string.Empty, taggedString.Processed);
		}

		[Test]
		public void ImplicitStringConversion_WhenTaggedStringIsNull_ReturnsEmptyString()
		{
			TaggedString taggedString = null;
			string       value        = taggedString;

			Assert.AreEqual(string.Empty, value);
		}

		[Test]
		public void ImplicitTaggedStringConversion_WhenTemplateHasNoTags_PreservesTemplate()
		{
			TaggedString taggedString = "Plain text";

			Assert.AreEqual("Plain text", taggedString.Template);
			Assert.AreEqual("Plain text", taggedString.Processed);
			Assert.AreEqual("Plain text", taggedString.ToString());
		}

		[Test]
		public void TemplateSetter_WhenValueIsNull_UsesEmptyTemplate()
		{
			var taggedString = new TaggedString("value");
			taggedString.Template = null;

			Assert.AreEqual(string.Empty, taggedString.Template);
			Assert.AreEqual(string.Empty, taggedString.ToString());
		}
	}
}
