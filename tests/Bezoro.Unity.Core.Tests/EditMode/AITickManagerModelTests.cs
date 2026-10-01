using Bezoro.Unity.Core.AI;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class AITickManagerModelTests
	{
		[Test]
		public void AddUnique_WhenAgentAlreadyExists_DoesNotAddDuplicate()
		{
			var model = new AITickManagerModel<object>(1);
			var agent = new object();
			model.AddUnique(agent);

			bool added = model.AddUnique(agent);

			Assert.IsFalse(added);
			Assert.AreEqual(1, model.Agents.Count);
		}

		[Test]
		public void AddUnique_WhenAgentIsNew_AddsAgent()
		{
			var model = new AITickManagerModel<object>(1);
			var agent = new object();

			bool added = model.AddUnique(agent);

			Assert.IsTrue(added);
			CollectionAssert.AreEqual(new[] { agent }, model.Agents);
		}

		[Test]
		public void Remove_WhenAgentDoesNotExist_ReturnsFalse()
		{
			var model = new AITickManagerModel<object>(1);

			bool removed = model.Remove(new());

			Assert.IsFalse(removed);
		}

		[Test]
		public void Remove_WhenAgentExists_RemovesAgent()
		{
			var model = new AITickManagerModel<object>(1);
			var agent = new object();
			model.AddUnique(agent);

			bool removed = model.Remove(agent);

			Assert.IsTrue(removed);
			Assert.IsEmpty(model.Agents);
		}

		[TestCase(true,  ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool ShouldRegister_ReturnsWhetherManagerExists(bool hasManager)
		{
			var model = new AIAgentModel();

			return model.ShouldRegister(hasManager);
		}

		[TestCase(true,  ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool ShouldRemove_ReturnsWhetherManagerExists(bool hasManager)
		{
			var model = new AIAgentModel();

			return model.ShouldRemove(hasManager);
		}
	}
}
