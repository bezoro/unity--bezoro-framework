using System;
using System.IO;
using System.Collections;
using System.Linq;
using Bezoro.Unity.Core.Editor.EditorWindows;
using Bezoro.Unity.Core.Editor.Tools;
using Bezoro.Unity.Core.Editor.Tools.AudioNormalization;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Bezoro.Unity.Core.Editor.SceneView;
using LinkUtility = Bezoro.Unity.Core.Editor.SymlinkUtility.SymlinkUtility;
using GeneratedWav = Bezoro.Unity.Core.Editor.Tools.AudioNormalization.EditorAudioNormalizerWindow.GeneratedNormalizedWav;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class EditorAssetOperationTests
	{
		private Scene _preview;
		private string _folder = string.Empty;

		[SetUp]
		public void SetUp()
		{
			_preview = EditorSceneManager.NewPreviewScene();
			string name = "OwnedEditorAssetTests_" + Guid.NewGuid().ToString("N");
			AssetDatabase.CreateFolder("Assets", name);
			_folder = "Assets/" + name;
		}

		[TearDown]
		public void TearDown()
		{
			if (_preview.IsValid()) EditorSceneManager.ClosePreviewScene(_preview);
			AssetDatabase.DeleteAsset(_folder);
		}

		private GameObject Create(string name)
		{
			var obj = EditorUtility.CreateGameObjectWithHideFlags(name, HideFlags.HideAndDontSave);
			SceneManager.MoveGameObjectToScene(obj, _preview);
			obj.hideFlags = HideFlags.None;
			return obj;
		}

		[UnityTest]
		public IEnumerator SceneFocus_OnlyRestoresTheUiTakeoverAndPreservesOtherVisibilityBits()
		{
			var focus = EditorWindow.focusedWindow;
			var view = ScriptableObject.CreateInstance<UnityEditor.SceneView>();
			int layers = UnityEditor.Tools.visibleLayers;
			int ui = LayerMask.GetMask("UI");
			try
			{
				view.Show();
				yield return null;
				Assert.That(view.camera, Is.Not.Null);
				view.in2DMode = false;
				var ordinary = Create("Ordinary");
				UnityEditor.Tools.visibleLayers |= ui;
				SceneViewSelectionAutoFocus.ApplySelection(view, new[] { ordinary }, false, true);
				Assert.That(UnityEditor.Tools.visibleLayers & ui, Is.EqualTo(ui));
				var canvas = EditorUtility.CreateGameObjectWithHideFlags("Owned UI", HideFlags.HideAndDontSave, typeof(RectTransform));
				SceneManager.MoveGameObjectToScene(canvas, _preview);
				UnityEditor.Tools.visibleLayers &= ~ui;
				SceneViewSelectionAutoFocus.ApplySelection(view, new[] { canvas }, true, false);
				Assert.That(view.in2DMode, Is.True);
				Assert.That(UnityEditor.Tools.visibleLayers & ui, Is.EqualTo(ui));
				UnityEditor.Tools.visibleLayers ^= 1;
				var otherLayers = UnityEditor.Tools.visibleLayers & ~ui;
				SceneViewSelectionAutoFocus.ApplySelection(view, Array.Empty<GameObject>(), true, false);
				Assert.That(view.in2DMode, Is.False);
				Assert.That(UnityEditor.Tools.visibleLayers, Is.EqualTo(otherLayers));
			}
			finally
			{
				SceneViewSelectionAutoFocus.ApplySelection(view, Array.Empty<GameObject>(), false, false);
				UnityEditor.Tools.visibleLayers = layers;
				view.Close();
				if (focus != null) focus.Focus();
			}
		}

		[Test]
		public void DirectoryLink_RelativePathResolvesFromLinkParent()
		{
			string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
			string source = Path.Combine(folder, "audio clips");
			string parent = Path.Combine(folder, "project folder");
			string link = Path.Combine(parent, "relative link");
			Directory.CreateDirectory(source);
			Directory.CreateDirectory(parent);
			try
			{
				var relative = LinkUtility.GetRelativePath(source, link);
				Assert.That(Path.GetFullPath(Path.Combine(parent, relative)), Is.EqualTo(Path.GetFullPath(source)));
				if (UnityEngine.Application.platform == RuntimePlatform.LinuxEditor ||
				    UnityEngine.Application.platform == RuntimePlatform.OSXEditor)
				{
					File.WriteAllText(Path.Combine(source, "owned.txt"), "linked");
					LinkUtility.CreateUnixDirectoryLink(relative, link);
					Assert.That(File.ReadAllText(Path.Combine(link, "owned.txt")), Is.EqualTo("linked"));
					Assert.Throws<IOException>(() => LinkUtility.CreateUnixDirectoryLink(relative, link));
					Directory.Delete(link);
					LinkUtility.CreateUnixDirectoryLink(source, link);
					Assert.That(File.ReadAllText(Path.Combine(link, "owned.txt")), Is.EqualTo("linked"));
				}
			}
			finally
			{
				if (Directory.Exists(link)) Directory.Delete(link);
				Directory.Delete(folder, true);
			}
		}

		[Test]
		public void CopyComponents_PreservesTwoCollidersAndUndoesBothChanges()
		{
			var source = Create("source");
			source.AddComponent<BoxCollider>().size = Vector3.one * 2;
			source.AddComponent<BoxCollider>().size = Vector3.one * 3;
			var destination = Create("destination");
			destination.AddComponent<BoxCollider>();
			Undo.IncrementCurrentGroup();
			int group = Undo.GetCurrentGroup();
			try
			{
				ComponentCopyToolWindow.CopyComponents(source.GetComponents<BoxCollider>(), destination);
				Undo.FlushUndoRecordObjects();
				Assert.That(destination.GetComponents<BoxCollider>().Select(c => c.size.x), Is.EqualTo(new[] { 2f, 3f }));
			}
			finally { Undo.RevertAllDownToGroup(group); }
			Assert.That(destination.GetComponents<BoxCollider>().Select(c => c.size.x), Is.EqualTo(new[] { 1f }));
		}

		[Test]
		public void AudioCopies_PreserveSubfoldersAndRejectDuplicateReplacementBeforeIO()
		{
			string a = EditorAudioNormalizerWindow.GetOutputCopyAssetPath("Assets/Audio/A/Hit.wav", "Assets/Audio", "Assets/Copies");
			string b = EditorAudioNormalizerWindow.GetOutputCopyAssetPath("Assets/Audio/B/Hit.wav", "Assets/Audio", "Assets/Copies");
			Assert.That(a, Is.EqualTo("Assets/Copies/A/Hit.wav"));
			Assert.That(b, Is.EqualTo("Assets/Copies/B/Hit.wav"));
			string output = _folder + "/Hit.wav";
			var inputs = new[] { new GeneratedWav(_folder + "/Hit.mp3", output, ""), new GeneratedWav(_folder + "/Hit.ogg", output, "") };
			Assert.Throws<IOException>(() => EditorAudioNormalizerWindow.ApplyAudioBatch(inputs, true, null));
			Assert.That(File.Exists(output), Is.False);
		}

		[Test]
		public void AudioBatch_WhenSecondCopyFails_RestoresFirstDestinationAndMeta()
		{
			string first = _folder + "/First.bytes";
			File.WriteAllBytes(first, new byte[] { 1, 2 });
			AssetDatabase.ImportAsset(first);
			byte[] metadata = File.ReadAllBytes(first + ".meta");
			string replacement = Path.GetTempFileName();
			try
			{
				File.WriteAllBytes(replacement, new byte[] { 3, 4 });
				var files = new[] { new GeneratedWav(first, first, replacement),
					new GeneratedWav(_folder + "/Second.bytes", _folder + "/Second.bytes", replacement + ".missing") };
				Assert.Throws<FileNotFoundException>(() => EditorAudioNormalizerWindow.ApplyAudioBatch(files, false, null));
				Assert.That(File.ReadAllBytes(first), Is.EqualTo(new byte[] { 1, 2 }));
				Assert.That(File.ReadAllBytes(first + ".meta"), Is.EqualTo(metadata));
			}
			finally { File.Delete(replacement); }
		}

		[Test]
		public void FailedPrefabInstall_RestoresPersistedIdentityAndSceneInstance()
		{
			string original = _folder + "/Original.prefab";
			var root = Create("Original");
			var child = Create("UniqueChild");
			child.transform.SetParent(root.transform, false);
			child.AddComponent<BoxCollider>();
			var asset = PrefabUtility.SaveAsPrefabAsset(root, original);
			string expectedGuid = AssetDatabase.AssetPathToGUID(original);
			byte[] expectedContent = File.ReadAllBytes(original);
			var childId = GlobalObjectId.GetGlobalObjectIdSlow(asset.GetComponentInChildren<BoxCollider>());
			var replacement = PrefabUtility.SaveAsPrefabAsset(Create("Other"), _folder + "/Other.prefab");

			var active = SceneManager.GetActiveScene();
			string scenePath = _folder + "/Consumer.unity";
			Assert.That(AssetDatabase.CopyAsset(SceneTransitionsTests.First, scenePath), Is.True);
			var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
			try
			{
				var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, scene);
				Assert.That(PrefabNameSyncer.IsInstanceRoot(instance), Is.True);
				Assert.That(PrefabNameSyncer.IsInstanceRoot(instance.transform.GetChild(0).gameObject), Is.False);
				Assert.That(EditorSceneManager.SaveScene(scene, scenePath), Is.True);
				EditorSceneManager.CloseScene(scene, true);
				Assert.Throws<IOException>(() => PrefabVariantWriter.InstallVerifiedPrefab(
					AssetDatabase.GetAssetPath(replacement), original, () => throw new IOException("Staged write failed verification.")));
				Assert.That(File.ReadAllBytes(original), Is.EqualTo(expectedContent));
				Assert.That(AssetDatabase.AssetPathToGUID(original), Is.EqualTo(expectedGuid));
				scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
				var persisted = scene.GetRootGameObjects().Single().GetComponentInChildren<BoxCollider>();
				Assert.That(GlobalObjectId.GetGlobalObjectIdSlow(PrefabUtility.GetCorrespondingObjectFromSource(persisted)), Is.EqualTo(childId));
			}
			finally
			{
				if (scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
				if (active.IsValid()) SceneManager.SetActiveScene(active);
			}
		}

		[Test]
		public void NewVariant_PreservesSourceChildAndItsCollider()
		{
			var original = Create("Original");
			var child = Create("UniqueChild");
			child.transform.SetParent(original.transform, false);
			child.AddComponent<BoxCollider>().size = Vector3.one * 3;
			var source = PrefabUtility.SaveAsPrefabAsset(original, _folder + "/Original.prefab");
			var target = PrefabUtility.SaveAsPrefabAsset(Create("Base"), _folder + "/Base.prefab");
			var result = PrefabVariantWriter.Save(source, target, _folder + "/Copy.prefab", false, false);
			Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(result), Is.EqualTo(target));
			Assert.That(result.transform.Find("UniqueChild").GetComponent<BoxCollider>().size, Is.EqualTo(Vector3.one * 3));
		}

		[Test]
		public void Retarget_WithUnmappableOverride_PreservesNestedPrefabAndSourceBytes()
		{
			var original = Create("Original");
			original.AddComponent<BoxCollider>();
			var source = PrefabUtility.SaveAsPrefabAsset(original, _folder + "/Original.prefab");
			var nested = PrefabUtility.SaveAsPrefabAsset(Create("Nested"), _folder + "/Nested.prefab");
			var target = PrefabUtility.SaveAsPrefabAsset(Create("Target"), _folder + "/Target.prefab");
			var instance = (GameObject)PrefabUtility.InstantiatePrefab(source, _preview);
			PrefabUtility.InstantiatePrefab(nested, instance.transform);
			var collider = instance.GetComponent<BoxCollider>();
			collider.size = Vector3.one * 7;
			PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
			string path = _folder + "/Variant.prefab";
			var variant = PrefabUtility.SaveAsPrefabAsset(instance, path);
			byte[] expected = File.ReadAllBytes(path);
			Assert.Catch<Exception>(() => PrefabVariantWriter.Save(variant, target, path, true, true));
			Assert.That(File.ReadAllBytes(path), Is.EqualTo(expected));
			Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(variant), Is.EqualTo(source));
			Assert.That(variant.GetComponent<BoxCollider>().size, Is.EqualTo(Vector3.one * 7));
			Assert.That(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(variant.transform.GetChild(0).gameObject), Is.EqualTo(AssetDatabase.GetAssetPath(nested)));
		}
	}
}
