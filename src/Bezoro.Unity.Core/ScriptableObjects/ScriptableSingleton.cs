#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
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
					string[] guids = AssetDatabase.FindAssets(
						$"t:{typeof(T).Name}"
					);

					if (guids.Length > 0)
					{
						string path = AssetDatabase.GUIDToAssetPath(
							guids[0]
						);

						_instance = AssetDatabase.LoadAssetAtPath<T>(
							path
						);
					}
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

			// Ensure we don't have duplicate instances in the editor
			if (_instance != null && _instance != this)
			{
				Logger.LogError(
					$"Duplicate singleton instance of {typeof(T).Name} detected!",
					contextObject: this
				);

				EditorApplication.delayCall += () =>
					AssetDatabase.DeleteAsset(
						AssetDatabase.GetAssetPath(
							this
						)
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
