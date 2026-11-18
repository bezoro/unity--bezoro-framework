using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Editor.Tools
{
	internal static class PrefabVariantWriter
	{
		internal static GameObject Save(GameObject source, GameObject target, string path, bool inPlace, bool retarget)
		{
			string sourcePath = AssetDatabase.GetAssetPath(source);
			string targetPath = AssetDatabase.GetAssetPath(target);
			if (!source || !target || source == target || source != AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath) ||
			    target != AssetDatabase.LoadAssetAtPath<GameObject>(targetPath) ||
			    PrefabUtility.GetPrefabAssetType(source) != (retarget ? PrefabAssetType.Variant : PrefabAssetType.Regular) ||
			    PrefabUtility.GetPrefabAssetType(target) != PrefabAssetType.Regular)
				throw new ArgumentException("Select distinct prefab asset roots of the required types.");

			if (!path.StartsWith("Assets/", StringComparison.Ordinal) || path.Split('/').Any(p => p == ".." || p == ".") ||
			    !AssetDatabase.IsValidFolder(Path.GetDirectoryName(path)!.Replace('\\', '/')) ||
			    (inPlace ? path != sourcePath : File.Exists(path) || File.Exists(path + ".meta")))
				throw new IOException("Choose a writable project asset path. New variants require an unused file name.");
			if (inPlace && (AssetDatabase.GetDependencies(targetPath).Contains(sourcePath) || !AssetDatabase.IsOpenForEdit(path)))
				throw new IOException("The destination is read-only or the target depends on it.");

			if (inPlace && PrefabStageUtility.GetCurrentPrefabStage()?.assetPath == path)
				throw new IOException("Close the destination prefab stage before changing its base.");
			if (AssetDatabase.LoadAllAssetsAtPath(sourcePath).Any(obj => obj != null && !(obj is GameObject) && !(obj is Component)))
				throw new IOException("Prefabs with embedded sub-assets require a manual conversion.");

			var expected = DescribeHierarchy(source);
			Dictionary<string, (string value, string reference)>? overrides = null;
			if (retarget)
			{
				var authored = PrefabUtility.LoadPrefabContents(sourcePath);
				try { overrides = ReadOverrides(authored); }
				finally { PrefabUtility.UnloadPrefabContents(authored); }
			}
			string folder = "Assets/BezoroPrefabStage_" + Guid.NewGuid().ToString("N");
			string staged = folder + "/" + Path.GetFileName(path);
			AssetDatabase.CreateFolder("Assets", Path.GetFileName(folder));
			try
			{
				if (!AssetDatabase.CopyAsset(sourcePath, staged)) throw new IOException("Could not stage the source prefab.");
				var workspace = PrefabUtility.LoadPrefabContents(targetPath);
				try
				{
					var root = (GameObject)PrefabUtility.InstantiatePrefab(source, workspace.transform);
					PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
					if (retarget)
					{
						PrefabUtility.ReplacePrefabAssetOfPrefabInstance(root, target, new PrefabReplacingSettings
						{
							objectMatchMode = ObjectMatchMode.ByHierarchy,
							changeRootNameToAssetName = false
						}, InteractionMode.AutomatedAction);
					}
					else
					{
						PrefabUtility.ConvertToPrefabInstance(root, target, new ConvertToPrefabInstanceSettings
						{
							objectMatchMode = ObjectMatchMode.ByHierarchy,
							gameObjectsNotMatchedBecomesOverride = true,
							componentsNotMatchedBecomesOverride = true,
							recordPropertyOverridesOfMatches = true,
							changeRootNameToAssetName = false
						}, InteractionMode.AutomatedAction);
					}
					PrefabUtility.SaveAsPrefabAsset(root, staged, out bool saved);
					if (!saved) throw new IOException("Saving the staged variant failed.");
				}
				finally { PrefabUtility.UnloadPrefabContents(workspace); }

				Verify(staged, target, expected, inPlace);
				if (overrides != null)
				{
					var stagedContents = PrefabUtility.LoadPrefabContents(staged);
					try
					{
						var persisted = ReadOverrides(stagedContents);
						foreach (var entry in overrides)
							if (!persisted.TryGetValue(entry.Key, out var value) || value != entry.Value)
								throw new IOException($"The target cannot preserve override {entry.Key}.");
					}
					finally { PrefabUtility.UnloadPrefabContents(stagedContents); }
				}
				if (inPlace)
					return InstallVerifiedPrefab(staged, path, () => Verify(path, target, expected, true));
				string error = AssetDatabase.MoveAsset(staged, path);
				if (!string.IsNullOrEmpty(error)) throw new IOException(error);
				return AssetDatabase.LoadAssetAtPath<GameObject>(path);
			}
			finally { AssetDatabase.DeleteAsset(folder); }
		}

		internal static GameObject InstallVerifiedPrefab(string staged, string path, Func<GameObject> verify)
		{
			string sibling = path + "." + Guid.NewGuid().ToString("N");
			string pending = sibling + ".pending~";
			string backup = sibling + ".rollback~";
			try
			{
				File.Copy(staged, pending);
				// Both names reside on the destination volume. Unity ignores the tilde-suffixed files.
				File.Replace(pending, path, backup);
				try
				{
					var result = verify();
					File.Delete(backup);
					return result;
				}
				catch
				{
					File.Replace(backup, path, null);
					AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
					throw;
				}
			}
			finally
			{
				File.Delete(pending);
				if (File.Exists(backup)) Debug.LogError($"Prefab recovery copy retained at {backup}.");
			}
		}

		private static Dictionary<string, (string, string)> ReadOverrides(GameObject root)
		{
			if (PrefabUtility.GetRemovedComponents(root).Count != 0 || PrefabUtility.GetRemovedGameObjects(root).Count != 0)
				throw new InvalidOperationException("Retargeting removed-object overrides requires a manual mapping.");
			var result = new Dictionary<string, (string, string)>();
			var parent = PrefabUtility.GetCorrespondingObjectFromSource(root);
			foreach (var mod in PrefabUtility.GetPropertyModifications(root) ?? Array.Empty<PropertyModification>())
			{
				if (!mod.target) throw new IOException("An override has no source object.");
				if (PrefabUtility.IsDefaultOverride(mod)) continue;
				GameObject owner = mod.target is Component component ? component.gameObject : (GameObject)mod.target;
				if (owner != parent && !owner.transform.IsChildOf(parent.transform))
					throw new IOException("An override points outside the direct base prefab.");
				string relative = AnimationUtility.CalculateTransformPath(owner.transform, parent.transform);
				string key = relative + "|" + mod.target.GetType().AssemblyQualifiedName + "|" + mod.propertyPath;
				if (result.ContainsKey(key)) throw new IOException($"Ambiguous override {key}.");
				string reference = string.Empty;
				if (mod.objectReference)
				{
					var obj = mod.objectReference;
					if (EditorUtility.IsPersistent(obj))
						reference = GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
					else
					{
						var node = obj is GameObject go ? go.transform : (obj as Component)?.transform;
						if (node == null || (node != root.transform && !node.IsChildOf(root.transform)))
							throw new IOException($"Cannot identify the object reference of {key}.");
						reference = AnimationUtility.CalculateTransformPath(node, root.transform) + "|" + obj.GetType().AssemblyQualifiedName;
					}
				}
				result.Add(key, (mod.value, reference));
			}
			return result;
		}

		private static GameObject Verify(string path, GameObject target, Dictionary<string, (ulong id, ulong prefab, string nested)> expected, bool requireIds)
		{
			AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
			var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (!asset || PrefabUtility.GetCorrespondingObjectFromSource(asset) != target)
				throw new IOException("The saved prefab does not inherit from the selected target.");
			var actual = DescribeHierarchy(asset);
			foreach (var entry in expected)
				if (!actual.TryGetValue(entry.Key, out var value) || value.nested != entry.Value.nested ||
				    (requireIds && (value.id != entry.Value.id || value.prefab != entry.Value.prefab)))
					throw new IOException($"Cannot preserve the hierarchy or persistent identity of {entry.Key}.");
			return asset;
		}

		private static Dictionary<string, (ulong id, ulong prefab, string nested)> DescribeHierarchy(GameObject root)
		{
			var result = new Dictionary<string, (ulong id, ulong prefab, string nested)>();
			void Add(string key, Object obj, string nested)
			{
				var id = GlobalObjectId.GetGlobalObjectIdSlow(obj);
				if (result.ContainsKey(key) || id.targetObjectId == 0)
					throw new IOException($"Ambiguous or missing persistent object at {key}.");
				result.Add(key, (id.targetObjectId, id.targetPrefabId, nested));
			}
			void Visit(Transform node, string key)
			{
				string nested = node != root.transform && PrefabUtility.IsAnyPrefabInstanceRoot(node.gameObject)
					? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(node.gameObject) : string.Empty;
				Add(key, node.gameObject, nested);
				foreach (var component in node.GetComponents<Component>())
				{
					if (!component) throw new IOException($"Missing script at {key}.");
					Add(key + "|" + component.GetType().AssemblyQualifiedName, component, nested);
				}
				foreach (Transform child in node) Visit(child, key + "/" + child.name.Length + ":" + child.name);
			}
			Visit(root.transform, "root");
			return result;
		}
	}
}
