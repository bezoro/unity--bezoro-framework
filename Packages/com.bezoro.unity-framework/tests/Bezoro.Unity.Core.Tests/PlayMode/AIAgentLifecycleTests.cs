using System.Reflection;
using Bezoro.Unity.Core.AI;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.PlayMode
{
	public sealed class AIAgentLifecycleTests
	{
		[Test]
		public void LateManagerAndOwnerReplacement_KeepEnabledAgentsRegisteredAcrossTickMutation()
		{
			var root = new GameObject("AI lifecycle test");
			try
			{
				var retiring = AddChild<TestAgent>(root);
				var survivor = AddChild<TestAgent>(root);
				retiring.DisableOnTick = true;
				var first = AddChild<AITickManagerBehaviour>(root);
				var replacement = AddChild<AITickManagerBehaviour>(root);
				var tick = typeof(AITickManagerBehaviour).GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Instance)!;
				tick.Invoke(first, null);
				Assert.AreEqual(1, retiring.Calls);
				Assert.AreEqual(1, survivor.Calls);

				first.enabled = false;
				Assert.AreSame(replacement, AITickManagerBehaviour.Instance);
				tick.Invoke(replacement, null);
				Assert.AreEqual(1, retiring.Calls);
				Assert.AreEqual(2, survivor.Calls);
			}
			finally
			{
				Object.DestroyImmediate(root);
			}
		}

		private static T AddChild<T>(GameObject root) where T : Component
		{
			var child = new GameObject(typeof(T).Name);
			child.transform.SetParent(root.transform);
			return child.AddComponent<T>();
		}

		private sealed class TestAgent : AIAgentBehaviour
		{
			public int Calls;
			public bool DisableOnTick;

			public override void Tick()
			{
				Calls++;
				if (DisableOnTick) enabled = false;
			}
		}
	}
}
