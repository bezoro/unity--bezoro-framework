#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Diagnostics;
using Bezoro.Core.Extensions;
using Bezoro.Logging.Types;
using Bezoro.Unity.Core.Application;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.ScriptableObjects
{
	/// <summary>
	///     Provides guarded singleton access for ScriptableObject assets.
	/// </summary>
	public abstract class ScriptableObjectSingleton<T> : ScriptableObject
		where T : ScriptableObjectSingleton<T>
	{
		private static bool IsInstanceAssigned;

		/// <summary>
		///     Checks if the singleton instance is available without creating it.
		/// </summary>
		/// <returns>True if the singleton instance exists and is valid, false otherwise.</returns>
		public static bool IsAvailable =>
			!ApplicationState.IsApplicationQuitting && IsInstanceAssigned && UnsafeInstance != null;

		/// <summary>
		///     Gets or creates the singleton instance. If the instance doesn't exist, it will prompt for creation in editor.
		/// </summary>
		/// <returns>The singleton instance, or null if not found and not created.</returns>
		public static T LazySingleton
		{
			get
			{
				var currentInstance = UnsafeInstance;
				if (IsInstanceAssigned && currentInstance != null) return currentInstance;

				if (ApplicationState.IsApplicationQuitting)
				{
					Logger.LogWarning(
						$"Attempted to access {typeof(T).Name} singleton during application shutdown. Returning null.",
						contextObject: typeof(T).Name,
						category: LogCategory.System
					);

					return null!;
				}

				var instance                   = FindExistingInstance();
				if (instance == null) instance = HandleMissingInstance();

				if (instance != null)
					SetSingletonInstance(
						instance
					);

				return UnsafeInstance;
			}
		}

		/// <summary>
		///     Gets the singleton instance directly without any safety checks.
		///     Warning: This may return null if the instance is not initialized.
		/// </summary>
		/// <returns>The singleton instance or null if not initialized.</returns>
		public static T UnsafeInstance { get; private set; } = null!;

		#region Unity Lifecycle

		/// <summary>
		///     Call base.OnEnable() to automatically assign 'this' as the singleton instance.
		/// </summary>
		protected virtual void OnEnable()
		{
			Logger.Log(
				"OnEnable Started",
				contextObject: this,
				category: LogCategory.System
			);

			if (IsInstanceAssigned)
			{
				if (UnsafeInstance && UnsafeInstance == this) return;

				Logger.LogWarning(
					$"Duplicate {typeof(T).Name} instance detected. " +
					$"Original instance exists at {GetInstancePath(UnsafeInstance)}. " +
					$"Current instance at {GetInstancePath(this as T)}.",
					contextObject: this,
					category: LogCategory.System
				);

				return;
			}

			if (UnsafeInstance != null) return;

			var typedInstance = this as T;
			if (typedInstance == null)
				throw new InvalidOperationException(
					$"ScriptableObject is not of type {typeof(T).Name}"
				);

			SetSingletonInstance(
				typedInstance
			);
		}

		/// <summary>
		///     To support 'disabled domain reload' we need to null the instance in OnDestroy.
		///     CAUTION: you must call base.OnDestroy() when overriding this method!
		/// </summary>
		protected virtual void OnDestroy()
		{
			Logger.Log(
				"Destroy Started",
				contextObject: this,
				category: LogCategory.System
			);

			if (this != UnsafeInstance) return;

			IsInstanceAssigned = false;
			UnsafeInstance     = null!;
		}

		#endregion

		/// <summary>
		///     Attempts to get the singleton instance without creating it if it doesn't exist.
		/// </summary>
		/// <param name="instance">When this method returns, contains the singleton instance if it exists, or null if it doesn't.</param>
		/// <returns>True if the singleton instance exists, false otherwise.</returns>
		public static bool TryGetInstance(out T instance)
		{
			instance = null!;
			if (!IsInstanceAssigned) return false;

			instance = UnsafeInstance;
			return true;
		}

		/// <summary>
		///     Resets the singleton instance. Use with caution - primarily for testing purposes.
		/// </summary>
		[Conditional(
			"UNITY_EDITOR"
		)]
		protected static void ResetInstance()
		{
			IsInstanceAssigned = false;
			UnsafeInstance     = null!;

			Logger.Log(
				$"Reset {typeof(T).Name.Bold()} singleton instance",
				contextObject: UnsafeInstance,
				category:
				LogCategory.System
			);
		}

		/// <summary>
		///     Assigns the singleton instance.
		///     CAUTION: Only call this if you don't call base.OnEnable()!
		/// </summary>
		/// <param name="instance">The instance to be set as singleton.</param>
		/// <exception cref="ArgumentNullException">
		///     The instance parameter is null. The singleton cannot be set to null. This is by design.
		/// </exception>
		protected static void SetSingletonInstance(T instance)
		{
			if (instance == null)
				throw new ArgumentNullException(
					nameof(instance),
					"instance must not be null"
				);

			UnsafeInstance     = instance;
			IsInstanceAssigned = true;

			Logger.LogSuccess(
				$"Singleton {typeof(T).Name.Bold()} instance assigned",
				contextObject: UnsafeInstance,
				category:
				LogCategory.System
			);
		}

		private static string GetInstancePath(T? instance)
		{
			if (instance == null) return "null";

#if UNITY_EDITOR
			string? assetPath = AssetDatabase.GetAssetPath(
				instance
			);

			return string.IsNullOrEmpty(
					   assetPath
				   )
					   ? "Runtime Instance"
					   : assetPath;
#else
			return "Runtime Instance";
#endif
		}

		private static T FindExistingInstance()
		{
			// First try to find in Resources
			var resourcesInstance = Resources.Load<T>(
				typeof(T).Name
			);

			if (resourcesInstance != null) return resourcesInstance;

#if UNITY_EDITOR
			// In editor, also search in project assets
			string[]? guids = AssetDatabase.FindAssets(
				$"t:{typeof(T).Name}"
			);

			foreach (string guid in guids)
			{
				string? assetPath = AssetDatabase.GUIDToAssetPath(
					guid
				);

				var asset = AssetDatabase.LoadAssetAtPath<T>(
					assetPath
				);

				if (asset != null) return asset;
			}
#endif

			return null!;
		}

		private static T HandleMissingInstance()
		{
#if UNITY_EDITOR
			if (UnityEngine.Application.isEditor && !UnityEngine.Application.isPlaying)
				return ShowCreationDialog();
#endif

			// In play mode or runtime, create silently
			Logger.LogWarning(
				$"No {typeof(T).Name} singleton found. Creating temporary runtime instance.",
				contextObject: typeof(T).Name,
				category: LogCategory.System
			);

			return CreateInstance<T>();
		}

#if UNITY_EDITOR
		private static T ShowCreationDialog()
		{
			string typeName = typeof(T).Name;
			int result = EditorUtility.DisplayDialogComplex(
				"Missing Singleton",
				$"The {typeName} singleton does not exist in your project.\n\n" +
				"Would you like to create it now?",
				"Create in Resources",
				"Create Custom Location",
				"Cancel"
			);

			switch (result)
			{
				case 0: // Create in Resources
					return CreateInResources();
				case 1: // Create Custom Location
					return CreateCustomLocation();
				case 2: // Cancel
				default:
					Logger.Log(
						$"User cancelled creation of {typeName} singleton.",
						contextObject: typeof(T).Name,
						category: LogCategory.System
					);

					return null!;
			}
		}

		private static T CreateInResources()
		{
			string typeName = typeof(T).Name;

			Logger.Log(
				$"Creating {typeName.Bold()} singleton in Resources folder",
				contextObject: typeof(T).Name,
				category: LogCategory.System
			);

			var created = CreateInstance<T>();
			if (created == null)
			{
				EditorUtility.DisplayDialog(
					"Creation Failed",
					$"Failed to create {typeName} instance.",
					"OK"
				);

				return null!;
			}

			// Ensure Resources folder exists
			var resourcesPath = "Assets/Resources";
			if (!AssetDatabase.IsValidFolder(
					resourcesPath
				))
				AssetDatabase.CreateFolder(
					"Assets",
					"Resources"
				);

			var assetPath = $"{resourcesPath}/{typeName}.asset";

			// Check if file already exists
			if (AssetDatabase.LoadAssetAtPath<T>(
					assetPath
				) !=
				null)
			{
				bool overwrite = EditorUtility.DisplayDialog(
					"File Exists",
					$"A file already exists at {assetPath}.\n\nOverwrite it?",
					"Overwrite",
					"Cancel"
				);

				if (!overwrite)
				{
					DestroyImmediate(
						created
					);

					return null!;
				}
			}

			AssetDatabase.CreateAsset(
				created,
				assetPath
			);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			Logger.LogSuccess(
				$"Created {typeName.Bold()} singleton at {assetPath}",
				contextObject: created,
				category: LogCategory.System
			);

			// Ping the asset in project window
			EditorGUIUtility.PingObject(
				created
			);

			return created;
		}

		private static T CreateCustomLocation()
		{
			string typeName    = typeof(T).Name;
			var    defaultName = $"{typeName}.asset";

			string? savePath = EditorUtility.SaveFilePanelInProject(
				$"Save {typeName} Singleton",
				defaultName,
				"asset",
				$"Choose where to save the {typeName} singleton"
			);

			if (string.IsNullOrEmpty(
					savePath
				))
			{
				Logger.Log(
					$"User cancelled save location for {typeName} singleton.",
					contextObject: typeof(T).Name,
					category:
					LogCategory.System
				);

				return null!;
			}

			Logger.Log(
				$"Creating {typeName.Bold()} singleton at custom location: {savePath}",
				contextObject: typeof(T).Name,
				category:
				LogCategory.System
			);

			var created = CreateInstance<T>();
			if (created == null)
			{
				EditorUtility.DisplayDialog(
					"Creation Failed",
					$"Failed to create {typeName} instance.",
					"OK"
				);

				return null!;
			}

			AssetDatabase.CreateAsset(
				created,
				savePath
			);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			Logger.LogSuccess(
				$"Created {typeName.Bold()} singleton at {savePath}",
				contextObject: created,
				category:
				LogCategory.System
			);

			// Ping the asset in project window
			EditorGUIUtility.PingObject(
				created
			);

			return created;
		}
#endif
	}
}
