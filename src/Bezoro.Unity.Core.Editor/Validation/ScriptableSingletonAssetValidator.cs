#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ScriptableObjects = Bezoro.Unity.Core.ScriptableObjects;

namespace Bezoro.Unity.Core.Editor.Validation
{
	public sealed class ScriptableSingletonAssetDuplicate
	{
		internal ScriptableSingletonAssetDuplicate(
			Type                singletonType,
			IEnumerable<string> assetPaths)
		{
			SingletonType = singletonType;
			AssetPaths    = assetPaths.ToArray();
		}

		public Type SingletonType { get; }
		public IReadOnlyList<string> AssetPaths { get; }
	}

	public sealed class ScriptableSingletonAssetValidationResult
	{
		internal ScriptableSingletonAssetValidationResult(
			IEnumerable<ScriptableSingletonAssetDuplicate> duplicates)
		{
			Duplicates = duplicates.ToArray();
		}

		public IReadOnlyList<ScriptableSingletonAssetDuplicate> Duplicates { get; }
		public bool HasDuplicates => Duplicates.Count > 0;
	}

	public static class ScriptableSingletonAssetValidator
	{
		[MenuItem("Bezoro/Validation/Scriptable Singletons")]
		public static void ValidateFromMenu()
		{
			var result = ValidateProject();
			if (!result.HasDuplicates)
			{
				Debug.Log("No duplicate ScriptableSingleton assets found.");
				return;
			}

			foreach (var duplicate in result.Duplicates)
				Debug.LogError(
					$"Duplicate singleton assets of {duplicate.SingletonType.FullName ?? duplicate.SingletonType.Name} detected at:{Environment.NewLine}{string.Join(Environment.NewLine, duplicate.AssetPaths)}"
				);

			Selection.objects = result.Duplicates
				.SelectMany(duplicate => duplicate.AssetPaths)
				.Select(AssetDatabase.LoadMainAssetAtPath)
				.Where(asset => asset != null)
				.ToArray();
		}

		public static ScriptableSingletonAssetValidationResult ValidateProject()
		{
			var duplicates = new List<ScriptableSingletonAssetDuplicate>();
			foreach (var singletonType in TypeCache.GetTypesDerivedFrom<ScriptableObject>()
											   .Where(IsConcreteScriptableSingleton)
											   .OrderBy(
													type => type.FullName ?? type.Name,
													StringComparer.Ordinal
												))
			{
				string[] assetPaths = FindAssetPaths(singletonType);
				if (assetPaths.Length > 1)
					duplicates.Add(
						new ScriptableSingletonAssetDuplicate(singletonType, assetPaths)
					);
			}

			return new ScriptableSingletonAssetValidationResult(duplicates);
		}

		private static string[] FindAssetPaths(Type singletonType)
		{
			return AssetDatabase.FindAssets($"t:{singletonType.Name}")
				.Select(AssetDatabase.GUIDToAssetPath)
				.Where(path => !string.IsNullOrWhiteSpace(path))
				.Where(
					path => AssetDatabase.LoadAssetAtPath(path, singletonType)?.GetType() == singletonType
				)
				.OrderBy(path => path, StringComparer.Ordinal)
				.ToArray();
		}

		private static bool IsConcreteScriptableSingleton(Type type)
		{
			if (type.IsAbstract || type.IsGenericTypeDefinition)
				return false;

			for (Type? baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
			{
				if (baseType.IsGenericType &&
					baseType.GetGenericTypeDefinition() == typeof(ScriptableObjects.ScriptableSingleton<>))
					return true;
			}

			return false;
		}
	}
}
