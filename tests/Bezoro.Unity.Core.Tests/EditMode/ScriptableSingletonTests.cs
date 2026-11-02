#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Bezoro.Unity.Core.ScriptableObjects;
using NUnit.Framework;
using UnityEngine;
using AssetDatabase = UnityEditor.AssetDatabase;
using EditorApplication = UnityEditor.EditorApplication;
using Object = UnityEngine.Object;
using UnityEngine.TestTools;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies ScriptableSingleton resource lookup behavior.
	/// </summary>
	[TestFixture]
	public sealed class ScriptableSingletonTests
	{
		private static readonly string TemporaryAssetFolder = "Assets/__ScriptableSingletonTests_" + System.Guid.NewGuid().ToString("N");

		private static readonly FieldInfo? InstanceField =
			typeof(ScriptableSingleton<ScriptableSingletonProbe>)
				.GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);

		private readonly List<ScriptableSingletonProbe> _createdInstances = new();

		[SetUp]
		public void SetUp()
		{
			if (AssetDatabase.IsValidFolder(TemporaryAssetFolder))
				AssetDatabase.DeleteAsset(TemporaryAssetFolder);

			AssetDatabase.CreateFolder("Assets", System.IO.Path.GetFileName(TemporaryAssetFolder));
			ResetInstance();
		}

		[TearDown]
		public void TearDown()
		{
			ResetInstance();
			AssetDatabase.DeleteAsset(TemporaryAssetFolder);
			AssetDatabase.Refresh();

			foreach (var instance in _createdInstances)
			{
				if (instance != null)
					Object.DestroyImmediate(instance);
			}

			_createdInstances.Clear();
			ResetInstance();
		}

		[UnityTest]
		public IEnumerator DuplicateAssets_AreReportedButNeverDeleted()
		{
			string firstPath  = $"{TemporaryAssetFolder}/A_Probe.asset";
			string secondPath = $"{TemporaryAssetFolder}/B_Probe.asset";
			var second        = CreateProbe(secondPath);
			var first         = CreateProbe(firstPath);
			Assert.That(AssetDatabase.GetAssetPath(first), Is.EqualTo(firstPath));
			Assert.That(AssetDatabase.GetAssetPath(second), Is.EqualTo(secondPath));

			ResetInstance();
			first.InitializeForTest();
			var delayCallsBefore = GetDelayCallCallbacks();
			LogAssert.Expect(LogType.Error, new Regex(Regex.Escape(
				$"Duplicate singleton instance of {nameof(ScriptableSingletonProbe)} detected at '{secondPath}'.")));
			second.InitializeForTest();
			InvokeNewDelayCalls(delayCallsBefore);

			yield return null;

			ResetInstance();
			LogAssert.Expect(LogType.Error, new Regex(Regex.Escape(
				$"Duplicate singleton assets of {nameof(ScriptableSingletonProbe)} detected at:{System.Environment.NewLine}{firstPath}{System.Environment.NewLine}{secondPath}")));
			var instance = GetInstance();

			Assert.That(AssetDatabase.GetAssetPath(instance), Is.EqualTo(firstPath));
			Assert.That(AssetDatabase.LoadAssetAtPath<ScriptableSingletonProbe>(firstPath), Is.Not.Null);
			Assert.That(AssetDatabase.LoadAssetAtPath<ScriptableSingletonProbe>(secondPath), Is.Not.Null);
			Assert.That(File.Exists(Path.GetFullPath(firstPath)), Is.True);
			Assert.That(File.Exists(Path.GetFullPath(secondPath)), Is.True);
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

		private ScriptableSingletonProbe CreateProbe(string path)
		{
			ResetInstance();
			var instance = CreateProbe();
			ResetInstance();
			AssetDatabase.CreateAsset(instance, path);
			var asset = AssetDatabase.LoadAssetAtPath<ScriptableSingletonProbe>(path);
			if (asset == null)
				throw new AssertionException(
					$"ScriptableSingleton probe asset was not created at '{path}'."
				);

			ResetInstance();
			return asset;
		}

		private static EditorApplication.CallbackFunction[] GetDelayCallCallbacks()
		{
			var delayCallField = typeof(EditorApplication).GetField(
				"delayCall",
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
			);
			if (delayCallField == null)
				throw new AssertionException(
					"EditorApplication delayCall field was not found."
				);

			return (delayCallField.GetValue(null) as EditorApplication.CallbackFunction)
				?.GetInvocationList()
				.Cast<EditorApplication.CallbackFunction>()
				.ToArray() ?? System.Array.Empty<EditorApplication.CallbackFunction>();
		}

		private static void InvokeNewDelayCalls(
			IReadOnlyCollection<EditorApplication.CallbackFunction> callbacksBefore)
		{
			foreach (var callback in GetDelayCallCallbacks().Where(callback => !callbacksBefore.Contains(callback)))
			{
				EditorApplication.delayCall -= callback;
				callback();
			}
		}

		private static ScriptableSingletonProbe GetInstance()
		{
			var instance = ScriptableSingletonProbe.Instance;
			if (instance == null)
				throw new AssertionException(
					"ScriptableSingleton probe instance was not found."
				);

			return instance;
		}

		private static void ResetInstance()
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
	}
}
