using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.ScriptableObjects
{
	/// <summary>
	///     Loads a singleton ScriptableObject asset from Resources or the editor AssetDatabase.
	/// </summary>
	public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>
	{
		private static T? _instance;

		public static T? Instance
		{
			get
			{
				if (_instance != null) return _instance;

				_instance = LoadFromResources();

#if UNITY_EDITOR

				// In editor, if not found in Resources, look in the entire project
				if (_instance == null && !UnityEngine.Application.isPlaying)
				{
					string[] assetPaths = FindAssetPaths();
					foreach (string path in assetPaths)
					{
						var candidate = AssetDatabase.LoadAssetAtPath<T>(
							path
						);
						if (candidate == null)
							continue;

						_instance = candidate;
						break;
					}

					if (assetPaths.Length > 1)
						Logger.LogError(
							$"Duplicate singleton assets of {typeof(T).Name} detected at:{Environment.NewLine}{string.Join(Environment.NewLine, assetPaths)}",
							contextObject: typeof(T)
						);
				}
#endif

				if (_instance == null)
					Logger.LogWarning($"Failed to load {typeof(T).Name} singleton instance.", contextObject: typeof(T));

				return _instance;
			}
		}

		#region Unity Lifecycle

		protected virtual void Awake()
		{
#if UNITY_EDITOR
			if (_instance != null && _instance != this)
			{
				Logger.LogError(
					$"Duplicate singleton instance of {typeof(T).Name} detected at '{AssetDatabase.GetAssetPath(this)}'.",
					contextObject: this
				);
				return;
			}
#endif
			_instance = (this as T)!;
		}

		[Conditional(
			"UNITY_EDITOR"
		)]
		protected virtual void OnValidate() { }

		#endregion

#if UNITY_EDITOR
		private static string[] FindAssetPaths()
		{
			return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
				.Select(AssetDatabase.GUIDToAssetPath)
				.Where(path => !string.IsNullOrWhiteSpace(path))
				.OrderBy(path => path, StringComparer.Ordinal)
				.ToArray();
		}
#endif

		private static T? LoadFromResources()
		{
			var exactNameInstance = Resources.Load<T>(
				typeof(T).Name
			);

			if (exactNameInstance != null) return exactNameInstance;

			return SelectResourceInstance(
				null,
				Resources.LoadAll<T>(
					string.Empty
				)
			);
		}

		private static T? SelectResourceInstance(
			T?               exactNameInstance,
			IReadOnlyList<T> resourceInstances)
		{
			if (exactNameInstance != null) return exactNameInstance;

			if (resourceInstances.Count == 1) return resourceInstances[0];

			if (resourceInstances.Count > 1)
				Logger.LogWarning(
					$"Found {resourceInstances.Count} {typeof(T).Name} resources, but none matched the exact resource path '{typeof(T).Name}'.",
					contextObject: typeof(T)
				);

			return null;
		}
	}
}
