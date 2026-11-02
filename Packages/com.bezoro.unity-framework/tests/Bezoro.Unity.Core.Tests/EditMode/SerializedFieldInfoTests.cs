using System.Reflection;
using Bezoro.Unity.Core.Common.Serialization;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies serialized FieldInfo references can round-trip through callbacks.
	/// </summary>
	[TestFixture]
	public sealed class SerializedFieldInfoTests
	{
		[Test]
		public void Constructor_StoresRuntimeField()
		{
			var field = typeof(TestTarget).GetField(
				nameof(TestTarget.PublicField),
				BindingFlags.Instance | BindingFlags.Public
			);

			var serializedFieldInfo = new SerializedFieldInfo(field);

			Assert.AreSame(field, serializedFieldInfo.RuntimeField);
			Assert.AreSame(field, (FieldInfo)serializedFieldInfo);
		}

		[Test]
		public void OnAfterDeserialize_WithMissingData_ClearsRuntimeField()
		{
			var serializedFieldInfo = new SerializedFieldInfo();

			serializedFieldInfo.OnAfterDeserialize();

			Assert.IsNull(serializedFieldInfo.RuntimeField);
		}

		[Test]
		public void Set_ReplacesRuntimeField()
		{
			var serializedFieldInfo = new SerializedFieldInfo();
			var field = typeof(TestTarget).GetField(
				"PrivateField",
				BindingFlags.Instance | BindingFlags.NonPublic
			);

			serializedFieldInfo.Set(field);

			Assert.AreSame(field, serializedFieldInfo.RuntimeField);
		}

		private sealed class TestTarget
		{
			public int PublicField;
#pragma warning disable CS0169
			private string PrivateField;
#pragma warning restore CS0169
		}
	}
}
