using System;
using Bezoro.Unity.Core.Common.Serialization;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies serializable interface wrappers for raw and Unity object references.
	/// </summary>
	[TestFixture]
	public sealed class SerializableInterfaceTests
	{
		[Test]
		public void SerializableInterface_WhenConstructedWithRawReference_ReturnsValueAndRawReference()
		{
			var implementation = new RawImplementation();
			var serializable   = new SerializableInterface<ITestInterface>(implementation);

			Assert.AreSame(implementation, serializable.Value);
			Assert.AreSame(implementation, ((ISerializableInterface)serializable).GetRawReference());
			Assert.IsFalse(serializable.TryGetObject(out var unityObject));
			Assert.IsNull(unityObject);
		}

		[Test]
		public void SerializableInterface_WhenConstructedWithUnityObject_ReturnsUnityReference()
		{
			var gameObject     = new GameObject("SerializableInterfaceTestsObject");
			var implementation = gameObject.AddComponent<UnityImplementation>();
			var serializable   = new SerializableInterface<ITestInterface>(implementation);

			Assert.AreSame(implementation, serializable.Value);
			Assert.IsTrue(serializable.TryGetObject(out var unityObject));
			Assert.AreSame(implementation, unityObject);

			Object.DestroyImmediate(gameObject);
		}

		[Test]
		public void SerializableInterfaceOfImplementation_ReportsRawModeForRawTypes()
		{
			var implementation = new RawImplementation();
			var serializable   = new SerializableInterface<ITestInterface, RawImplementation>(implementation);

			Assert.AreEqual(ReferenceMode.Raw, serializable.Mode);
			Assert.AreSame(implementation, serializable.Value);
		}

		[Test]
		public void Value_WhenSetTwice_ThrowsInvalidOperationException()
		{
			var serializable = new SerializableInterface<ITestInterface>(new RawImplementation());

			Assert.Throws<InvalidOperationException>(() => serializable.Value = new RawImplementation());
		}

		private interface ITestInterface { }

		private sealed class RawImplementation : ITestInterface { }

		private sealed class UnityImplementation : MonoBehaviour, ITestInterface { }
	}
}
