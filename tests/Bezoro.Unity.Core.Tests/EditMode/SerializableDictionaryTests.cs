using System.Collections.Generic;
using Bezoro.Unity.Core.Common.Serialization;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies SerializableDictionary mirrors dictionary state through serialization callbacks.
	/// </summary>
	[TestFixture]
	public sealed class SerializableDictionaryTests
	{
		[Test]
		public void AddAndIndexer_ExposeDictionaryValues()
		{
			var dictionary = new SerializableDictionary<string, int>();

			dictionary.Add("one", 1);
			dictionary["two"] = 2;

			Assert.AreEqual(2, dictionary.Count);
			Assert.AreEqual(1, dictionary["one"]);
			Assert.AreEqual(2, dictionary["two"]);
		}

		[Test]
		public void CopyTo_CopiesKeyValuePairsFromDictionary()
		{
			var dictionary = new SerializableDictionary<string, int>();
			dictionary.Add("one", 1);
			var target = new KeyValuePair<string, int>[1];

			dictionary.CopyTo(target, 0);

			Assert.AreEqual(new KeyValuePair<string, int>("one", 1), target[0]);
		}

		[Test]
		public void OnBeforeSerializeAndOnAfterDeserialize_PreserveEntries()
		{
			var dictionary = new SerializableDictionary<string, int>();
			dictionary.Add("one", 1);
			dictionary.Add("two", 2);

			dictionary.OnBeforeSerialize();
			dictionary.Clear();
			dictionary.OnAfterDeserialize();

			Assert.AreEqual(2, dictionary.Count);
			Assert.AreEqual(1, dictionary["one"]);
			Assert.AreEqual(2, dictionary["two"]);
		}

		[Test]
		public void Remove_RemovesByKey()
		{
			var dictionary = new SerializableDictionary<string, int>();
			dictionary.Add("one", 1);

			bool removed = dictionary.Remove("one");

			Assert.IsTrue(removed);
			Assert.IsFalse(dictionary.ContainsKey("one"));
		}
	}
}
