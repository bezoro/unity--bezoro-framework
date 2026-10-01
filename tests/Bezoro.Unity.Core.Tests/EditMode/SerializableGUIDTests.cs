using System;
using Bezoro.Unity.Core.Common.Serialization;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies GUID validation and serialized GUID generation behavior.
	/// </summary>
	[TestFixture]
	public sealed class SerializableGUIDTests
	{
		[Test]
		public void CheckGuidValidity_WhenGuidCanBeParsed_ReturnsTrue()
		{
			Assert.IsTrue(GUIDUtils.Check_GUID_Validity(Guid.NewGuid().ToString()));
		}

		[Test]
		public void CheckGuidValidity_WhenGuidIsEmptyOrInvalid_ReturnsFalse()
		{
			Assert.IsFalse(GUIDUtils.Check_GUID_Validity(null));
			Assert.IsFalse(GUIDUtils.Check_GUID_Validity(string.Empty));
			Assert.IsFalse(GUIDUtils.Check_GUID_Validity("not-a-guid"));
		}

		[Test]
		public void GenerateNewValidGuid_WhenExistingGuidIsInvalid_GeneratesParseableGuid()
		{
			var serializableGuid = new SerializableGUID();
			serializableGuid.Set_GUID("invalid");

			bool generated = serializableGuid.Generate_New_Valid_GUID();

			Assert.IsTrue(generated);
			Assert.IsTrue(Guid.TryParse(serializableGuid.Value, out _));
		}

		[Test]
		public void GenerateNewValidGuid_WhenExistingGuidIsValidAndNotForced_KeepsCurrentValue()
		{
			var existingGuid     = Guid.NewGuid().ToString();
			var serializableGuid = new SerializableGUID();
			serializableGuid.Set_GUID(existingGuid);

			bool generated = serializableGuid.Generate_New_Valid_GUID();

			Assert.IsFalse(generated);
			Assert.AreEqual(existingGuid, serializableGuid.Value);
		}

		[Test]
		public void SetGuid_WhenValueIsNull_StoresEmptyString()
		{
			var serializableGuid = new SerializableGUID();

			serializableGuid.Set_GUID(null);

			Assert.AreEqual(string.Empty, serializableGuid.Value);
		}
	}
}
