using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Search;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Editor.Tools
{
	/// <summary>
	///     Menu command for creating a prefab variant from either the selected prefab
	///     asset, the source prefab of a selected scene object, or a prefab chosen
	///     from Unity Search.
	/// </summary>
	public static class PrefabVariantCreator
	{
		private const string DEFAULT_VARIANT_NAME  = "NewVariant";
		private const string MENU_PATH             = "GameObject/Prefab/Create Variant Of";
		private const string PREFAB_SEARCH_CONTEXT = "asset";
		private const string PREFAB_SEARCH_TEXT    = "t:Prefab";

		/// <summary>
		///     Starts the variant creation flow. When selection already identifies a
		///     prefab, it creates from that source; otherwise it opens a prefab picker.
		/// </summary>
		[MenuItem(MENU_PATH, false, 0)]
		public static void CreatePrefabVariant()
		{
			if (TryGetSelectedPrefab(out var prefabAsset, out var selectedTransform))
			{
				CreateVariant(prefabAsset, selectedTransform);
				return;
			}

			ShowPrefabPicker();
		}

		[MenuItem(MENU_PATH, true)]
		private static bool CanCreatePrefabVariant() =>
			Selection.activeObject == null ||
			Selection.activeObject is GameObject;

		private static bool IsPrefabAsset(GameObject? prefab) =>
			prefab != null &&
			PrefabUtility.IsPartOfPrefabAsset(prefab);

		private static bool TryGetSelectedPrefab(out GameObject prefabAsset, out Transform? selectedTransform)
		{
			prefabAsset       = null!;
			selectedTransform = null;

			if (Selection.activeObject is GameObject selectedAsset && IsPrefabAsset(selectedAsset))
			{
				prefabAsset = selectedAsset;
				return true;
			}

			var selectedGameObject = Selection.activeGameObject;
			if (selectedGameObject == null)
				return false;

			var sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(selectedGameObject);
			if (sourcePrefab == null)
				return false;

			prefabAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(selectedGameObject) ?? sourcePrefab;
			selectedTransform = selectedGameObject.transform;
			return true;
		}

		private static string? PromptForVariantPath(GameObject originalPrefab)
		{
			string defaultName = string.IsNullOrWhiteSpace(originalPrefab.name)
									 ? DEFAULT_VARIANT_NAME
									 : $"{originalPrefab.name}_Variant";

			return EditorUtility.SaveFilePanelInProject(
				"Save Prefab Variant",
				defaultName,
				"prefab",
				"Choose where to save the prefab variant."
			);
		}

		private static void CreateVariant(GameObject originalPrefab, Transform? selectedTransform = null)
		{
			string? selectedPath = PromptForVariantPath(originalPrefab);
			if (selectedPath == null || selectedPath.Length == 0)
				return;

			string variantPath = selectedPath;
			EnsureDirectoryExists(variantPath);

			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage != null)
			{
				CreateVariantInPrefabStage(originalPrefab, variantPath, prefabStage, selectedTransform);
				return;
			}

			CreateVariantInScene(originalPrefab, variantPath, selectedTransform);
		}

		private static void CreateVariantInPrefabStage(
			GameObject  originalPrefab,
			string      variantPath,
			PrefabStage prefabStage,
			Transform?  parent)
		{
			GameObject? tempInstance    = null;
			GameObject? variantInstance = null;

			try
			{
				tempInstance = PrefabUtility.InstantiatePrefab(originalPrefab, prefabStage.scene) as GameObject;
				if (tempInstance == null)
					throw new InvalidOperationException("Failed to instantiate original prefab for variant creation.");

				var variantPrefab = PrefabUtility.SaveAsPrefabAsset(tempInstance, variantPath, out bool success);
				if (!success || variantPrefab == null)
					throw new InvalidOperationException("Failed to create prefab variant.");

				Object.DestroyImmediate(tempInstance);
				tempInstance = null;

				variantInstance = PrefabUtility.InstantiatePrefab(variantPrefab, prefabStage.scene) as GameObject;
				if (variantInstance == null)
					throw new InvalidOperationException("Failed to instantiate variant in prefab stage.");

				var targetParent = parent != null ? parent : prefabStage.prefabContentsRoot.transform;
				Undo.RegisterCreatedObjectUndo(variantInstance, "Create Prefab Variant");
				Undo.SetTransformParent(variantInstance.transform, targetParent, "Create Prefab Variant");

				Selection.activeGameObject = variantInstance;
				EditorSceneManager.MarkSceneDirty(prefabStage.scene);
				AssetDatabase.Refresh();

				Debug.Log($"Created prefab variant at: {variantPath}.", variantInstance);
			}
			catch (Exception ex)
			{
				if (variantInstance != null)
					Object.DestroyImmediate(variantInstance);

				if (tempInstance != null)
					Object.DestroyImmediate(tempInstance);

				Debug.LogError($"Error creating prefab variant in prefab mode: {ex}");
			}
		}

		private static void CreateVariantInScene(
			GameObject originalPrefab,
			string     variantPath,
			Transform? parent)
		{
			GameObject? instance = null;

			try
			{
				instance = PrefabUtility.InstantiatePrefab(originalPrefab) as GameObject;
				if (instance == null)
					throw new InvalidOperationException("Failed to instantiate original prefab for variant creation.");

				Undo.RegisterCreatedObjectUndo(instance, "Create Prefab Variant");

				var variantPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
					instance,
					variantPath,
					InteractionMode.UserAction
				);

				if (variantPrefab == null)
					throw new InvalidOperationException("Failed to create prefab variant.");

				if (parent != null)
				{
					Undo.SetTransformParent(instance.transform, parent, "Create Prefab Variant");
				}
				else
				{
					var sceneView = UnityEditor.SceneView.lastActiveSceneView;
					if (sceneView != null)
						instance.transform.position = sceneView.pivot;
				}

				Selection.activeGameObject = instance;
				UnityEditor.SceneView.lastActiveSceneView?.FrameSelected();
				EditorSceneManager.MarkSceneDirty(instance.scene);
				AssetDatabase.Refresh();

				Debug.Log($"Created prefab variant at: {variantPath} and added to scene.", instance);
			}
			catch (Exception ex)
			{
				if (instance != null)
					Object.DestroyImmediate(instance);

				Debug.LogError($"Error creating prefab variant: {ex}");
			}
		}

		private static void EnsureDirectoryExists(string assetPath)
		{
			string? directory = Path.GetDirectoryName(assetPath);
			if (!string.IsNullOrEmpty(directory))
				Directory.CreateDirectory(directory);
		}

		private static void OnPrefabSelected(SearchItem item, bool cancelled)
		{
			if (cancelled || item == null)
				return;

			var prefab = item.ToObject<GameObject>();
			if (!IsPrefabAsset(prefab))
				return;

			CreateVariant(prefab);
		}

		private static void ShowPrefabPicker()
		{
			var searchContext = SearchService.CreateContext(PREFAB_SEARCH_CONTEXT);
			searchContext.searchText = PREFAB_SEARCH_TEXT;

			SearchService.ShowPicker(
				searchContext,
				OnPrefabSelected,
				null,
				null,
				null,
				"Select Prefab for Variant"
			);
		}
	}
}
