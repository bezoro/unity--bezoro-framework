using System;
using System.IO;
using System.Text.RegularExpressions;
using Bezoro.Unity.Core.Editor.Utilities;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Tools
{
	/// <summary>
	///     Retargets a prefab variant YAML file to a different source prefab.
	/// </summary>
	public class PrefabVariantRetargetTool : EditorWindow
	{
		[MenuItem(
			"Bezoro/Tools/Prefab Variant Retargeting"
		)]
		public static void Show_Window()
		{
			var window = GetWindow<PrefabVariantRetargetTool>(
				"Prefab Retargeting"
			);

			window.minSize = new Vector2(500, 200);
		}

		private bool _isRetargeting;

		private bool Can_Retarget()
		{
			if (variant == null || target == null ||
			    !Validate_Prefab_Variant(variant) || !Validate_Target_Prefab(target)) return false;

			string variantPath = AssetDatabase.GetAssetPath(
				variant
			);

			string content = File.ReadAllText(
				variantPath
			);

			string newGuid = AssetDatabase.AssetPathToGUID(
				AssetDatabase.GetAssetPath(
					target
				)
			);

			var currentGuids = Regex.Matches(
				content,
				@"(?:- target:|m_SourcePrefab:|m_CorrespondingSourceObject:|targetCorrespondingSourceObject:).*?guid: ([a-f0-9]{32})"
			);

			foreach (Match match in currentGuids)
			{
				if (match.Groups[1].Value != newGuid)
					return true;
			}

			return false;
		}

		private bool Validate_Prefab_Variant(GameObject prefab) =>
			prefab == null ||
			(EditorUtility.IsPersistent(prefab) && PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Variant);

		private bool Validate_Target_Prefab(GameObject prefab) =>
			prefab == null ||
			(EditorUtility.IsPersistent(prefab) && PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Regular);

		private async UniTaskVoid Update_Prefab_Targets()
		{
			if (EditorStatusWatcher.IsEditorBusy || _isRetargeting || variant == null || target == null ||
			    !Validate_Prefab_Variant(variant) || !Validate_Target_Prefab(target)) return;

			_isRetargeting = true;

			try
			{
				string variantPath = AssetDatabase.GetAssetPath(
					variant
				);

				string prefabVariantName = variant.name;
				string newGuid = AssetDatabase.AssetPathToGUID(
					AssetDatabase.GetAssetPath(
						target
					)
				);

				string targetPrefabName = target.name;

				string content = await File.ReadAllTextAsync(
									 variantPath
								 );

				// Check if any GUID needs updating
				var currentGuids = Regex.Matches(
					content,
					@"(?:- target:|m_SourcePrefab:|m_CorrespondingSourceObject:|targetCorrespondingSourceObject:).*?guid: ([a-f0-9]{32})"
				);

				var needsUpdate = false;

				foreach (Match match in currentGuids)
				{
					if (match.Groups[1].Value == newGuid) continue;

					needsUpdate = true;
					break;
				}

				if (!needsUpdate)
				{
					Debug.Log(
						$"Prefab variant {prefabVariantName} is already targeting {targetPrefabName}"
					);

					return;
				}

				await UniTask.RunOnThreadPool
				(() =>
					{
						// Replace all GUIDs in one pass with a more efficient regex
						content = Regex.Replace(
							content,
							@"(?:- target:|m_SourcePrefab:|m_CorrespondingSourceObject:|targetCorrespondingSourceObject:).*?guid: [a-f0-9]{32}",
							match => match.Value.Substring(
										 0,
										 match.Value.Length - 32
									 ) +
									 newGuid
						);

						File.WriteAllText(
							variantPath,
							content
						);

						return UniTask.CompletedTask;
					}
				);

				await UniTask.SwitchToMainThread();

				AssetDatabase.Refresh();
				AssetDatabase.ImportAsset(
					variantPath,
					ImportAssetOptions.ForceUpdate
				);

				// Update the reference after import
				variant = AssetDatabase.LoadAssetAtPath<GameObject>(
					variantPath
				);

				Debug.Log(
					$"Successfully retargeted [{prefabVariantName}] to [{targetPrefabName}]"
				);
			}
			catch (Exception e)
			{
				Debug.LogError(
					$"Failed to retarget prefab: {e.Message}"
				);
			}
			finally
			{
				await UniTask.SwitchToMainThread();
				_isRetargeting = false;
				if (this != null)
					Repaint();
			}
		}

		private void OnGUI()
		{
			EditorGUILayout.LabelField("Prefab Variant Retargeting", EditorStyles.boldLabel);
			using (new EditorGUI.DisabledScope(_isRetargeting))
			{
				variant = (GameObject)EditorGUILayout.ObjectField("Variant", variant, typeof(GameObject), false);
				target = (GameObject)EditorGUILayout.ObjectField("Target Prefab", target, typeof(GameObject), false);
			}

			var messageType = MessageType.Info;
			var message = "Assign a prefab variant and a regular target prefab.";
			var canRetarget = false;
			if (variant != null && target != null)
			{
				if (!Validate_Prefab_Variant(variant) || !Validate_Target_Prefab(target))
				{
					message = "Variant must be a prefab variant asset, and Target Prefab must be a regular prefab asset.";
					messageType = MessageType.Warning;
				}
				else
				{
					try
					{
						canRetarget = Can_Retarget();
						message = canRetarget
							? "Ready to retarget the variant to the selected prefab."
							: $"[{variant.name}] already points to [{target.name}], or has no retargetable source references.";
					}
					catch (Exception e)
					{
						message = $"Cannot read the prefab variant: {e.Message}";
						messageType = MessageType.Error;
					}
				}
			}

			using (new EditorGUI.DisabledScope(!canRetarget || _isRetargeting || EditorStatusWatcher.IsEditorBusy))
			{
				if (GUILayout.Button("Retarget Variant", GUILayout.Height(30)))
					Update_Prefab_Targets().Forget();
			}
			EditorGUILayout.HelpBox(_isRetargeting ? "Retargeting prefab variant..." : message, messageType);
		}

		#region Serialized Fields

		public GameObject variant = null!;
		public GameObject target = null!;

		#endregion
	}
}
