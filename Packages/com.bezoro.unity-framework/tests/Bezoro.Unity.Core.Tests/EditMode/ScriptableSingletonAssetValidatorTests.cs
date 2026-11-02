#nullable enable

using System.IO;
using System.Linq;
using System.Reflection;
using Bezoro.Unity.Core.Editor.Validation;
using Bezoro.Unity.Core.ScriptableObjects;
using NUnit.Framework;
using UnityEngine;
using AssetDatabase = UnityEditor.AssetDatabase;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class ScriptableSingletonAssetValidatorTests
	{
		private static readonly string TemporaryAssetFolder = "Assets/__ScriptableSingletonAssetValidatorTests_" + System.Guid.NewGuid().ToString("N");

		private static readonly FieldInfo? InstanceField =
			typeof(ScriptableSingleton<ScriptableSingletonProbe>)
				.GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);

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
		}

		[Test]
		public void ValidateProject_WithDuplicateAssets_ReturnsOrdinalAssetPaths()
		{
			string firstPath  = $"{TemporaryAssetFolder}/A_Probe.asset";
			string secondPath = $"{TemporaryAssetFolder}/B_Probe.asset";
			CreateProbe(secondPath);
			CreateProbe(firstPath);

			var result = ScriptableSingletonAssetValidator.ValidateProject();
			var duplicate = result.Duplicates.Single(
				item => item.SingletonType == typeof(ScriptableSingletonProbe)
			);

			Assert.That(result.HasDuplicates, Is.True);
			Assert.That(duplicate.AssetPaths, Is.EqualTo(new[] { firstPath, secondPath }));
		}

		[Test]
		public void ValidateProject_WithDuplicateAssets_DoesNotModifyOrDeleteAssets()
		{
			string firstPath  = $"{TemporaryAssetFolder}/A_Probe.asset";
			string secondPath = $"{TemporaryAssetFolder}/B_Probe.asset";
			CreateProbe(firstPath);
			CreateProbe(secondPath);
			AssetDatabase.SaveAssets();
			byte[] firstAssetBefore  = File.ReadAllBytes(Path.GetFullPath(firstPath));
			byte[] secondAssetBefore = File.ReadAllBytes(Path.GetFullPath(secondPath));

			ScriptableSingletonAssetValidator.ValidateProject();

			Assert.That(AssetDatabase.LoadAssetAtPath<ScriptableSingletonProbe>(firstPath), Is.Not.Null);
			Assert.That(AssetDatabase.LoadAssetAtPath<ScriptableSingletonProbe>(secondPath), Is.Not.Null);
			Assert.That(File.ReadAllBytes(Path.GetFullPath(firstPath)), Is.EqualTo(firstAssetBefore));
			Assert.That(File.ReadAllBytes(Path.GetFullPath(secondPath)), Is.EqualTo(secondAssetBefore));
		}

		private static void CreateProbe(string path)
		{
			ResetInstance();
			var instance = ScriptableObject.CreateInstance<ScriptableSingletonProbe>();
			ResetInstance();
			AssetDatabase.CreateAsset(instance, path);
			ResetInstance();
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
	}
}
