using System;
using Bezoro.Unity.Core.Common.Serialization;
using NUnit.Framework;
using UnityEditor;
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

		[Test]
		public void Value_AfterManagedReferenceDeserialization_ResolvesSerializedImplementation()
		{
			var original = new SerializableInterface<ITestInterface>(new RawImplementation { Number = 17 });
			var restored = JsonUtility.FromJson<SerializableInterface<ITestInterface>>(JsonUtility.ToJson(original));

			Assert.That(restored.Value, Is.TypeOf<RawImplementation>());
			Assert.That(((RawImplementation)restored.Value).Number, Is.EqualTo(17));
		}

		[Test]
		public void Value_AfterInspectorChangesManagedReference_ReturnsEditedImplementation()
		{
			var holder = ScriptableObject.CreateInstance<InterfaceReferenceHolder>();
			try
			{
				holder.Reference = new SerializableInterface<ITestInterface>(new RawImplementation { Number = 1 });
				Assert.That(((RawImplementation)holder.Reference.Value).Number, Is.EqualTo(1));
				using var serializedObject = new SerializedObject(holder);
				serializedObject.FindProperty("Reference.rawReference").managedReferenceValue = new RawImplementation { Number = 2 };
				serializedObject.ApplyModifiedProperties();

				Assert.That(((RawImplementation)holder.Reference.Value).Number, Is.EqualTo(2));
			}
			finally
			{
				Object.DestroyImmediate(holder);
			}
		}

		[Test]
		public void Value_AfterInspectorClearsManagedReference_DoesNotReturnCachedValue()
		{
			var holder = ScriptableObject.CreateInstance<InterfaceReferenceHolder>();
			try
			{
				holder.Reference = new SerializableInterface<ITestInterface>(new RawImplementation());
				Assert.That(holder.Reference.Value, Is.Not.Null);
				using var serializedObject = new SerializedObject(holder);
				serializedObject.FindProperty("Reference.rawReference").managedReferenceValue = null;
				serializedObject.ApplyModifiedProperties();

				Assert.Throws<InvalidOperationException>(() => _ = holder.Reference.Value);
			}
			finally
			{
				Object.DestroyImmediate(holder);
			}
		}

		[Test]
		public void Value_AfterReadingAssignedValue_StillRejectsSecondAssignment()
		{
			var serializable = new SerializableInterface<ITestInterface>(new RawImplementation());
			Assert.That(serializable.Value, Is.Not.Null);
			Assert.Throws<InvalidOperationException>(() => serializable.Value = new RawImplementation());
		}

		private interface ITestInterface { }

		[Serializable]
		private sealed class RawImplementation : ITestInterface
		{
			public int Number;
		}

		private sealed class InterfaceReferenceHolder : ScriptableObject
		{
			public SerializableInterface<ITestInterface> Reference = new();
		}

		private sealed class UnityImplementation : MonoBehaviour, ITestInterface { }
	}
}
