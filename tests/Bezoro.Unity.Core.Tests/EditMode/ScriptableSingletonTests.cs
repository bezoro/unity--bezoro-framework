using System.Collections.Generic;
using System.Reflection;
using Bezoro.Unity.Core.ScriptableObjects;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies ScriptableSingleton resource lookup behavior.
	/// </summary>
	[TestFixture]
	public sealed class ScriptableSingletonTests
	{
		private static readonly FieldInfo? InstanceField =
			typeof(ScriptableSingleton<ScriptableSingletonProbe>)
				.GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);

		private readonly List<ScriptableSingletonProbe> _createdInstances = new();

		[SetUp]
		public void SetUp() =>
			ResetSingleton();

		[TearDown]
		public void TearDown()
		{
			foreach (var instance in _createdInstances)
			{
				if (instance != null)
					Object.DestroyImmediate(instance);
			}

			_createdInstances.Clear();
			ResetSingleton();
		}

		[Test]
		public void SelectResourceInstance_WhenExactNameResourceExists_ReturnsExactNameResource()
		{
			var exactNameInstance = CreateProbe();

			var result = SelectResourceInstance(
				exactNameInstance,
				System.Array.Empty<ScriptableSingletonProbe>()
			);

			Assert.AreSame(exactNameInstance, result);
		}

		[Test]
		public void SelectResourceInstance_WhenExactNameMissingAndSingleResourceExists_ReturnsSingleResource()
		{
			var resourceInstance = CreateProbe();

			var result = SelectResourceInstance(
				null,
				new[] { resourceInstance }
			);

			Assert.AreSame(resourceInstance, result);
		}

		[Test]
		public void SelectResourceInstance_WhenExactNameMissingAndMultipleResourcesExist_ReturnsNull()
		{
			var resourceInstance = CreateProbe();

			var result = SelectResourceInstance(
				null,
				new[] { resourceInstance, resourceInstance }
			);

			Assert.IsNull(result);
		}

		private ScriptableSingletonProbe CreateProbe()
		{
			var instance = ScriptableObject.CreateInstance<ScriptableSingletonProbe>();
			_createdInstances.Add(instance);

			return instance;
		}

		private static void ResetSingleton()
		{
			var instanceField = InstanceField;
			if (instanceField == null)
				throw new AssertionException(
					"ScriptableSingleton instance field was not found."
				);

			instanceField.SetValue(null, null);
		}

		private static ScriptableSingletonProbe? SelectResourceInstance(
			ScriptableSingletonProbe?   exactNameInstance,
			ScriptableSingletonProbe[] resourceInstances)
		{
			var method = typeof(ScriptableSingleton<ScriptableSingletonProbe>)
				.GetMethod("SelectResourceInstance", BindingFlags.NonPublic | BindingFlags.Static);

			if (method == null)
				throw new AssertionException(
					"ScriptableSingleton resource selection method was not found."
				);

			return method.Invoke(
				null,
				new object?[] { exactNameInstance, resourceInstances }
			) as ScriptableSingletonProbe;
		}

		private sealed class ScriptableSingletonProbe : ScriptableSingleton<ScriptableSingletonProbe> { }
	}
}
