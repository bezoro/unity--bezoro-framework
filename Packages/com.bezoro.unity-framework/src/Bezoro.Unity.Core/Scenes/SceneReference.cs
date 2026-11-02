#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using Bezoro.Core.Extensions;
using Bezoro.Unity.Core.Attributes;
using Bezoro.Unity.Core.Loading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Scenes;

/// <summary>
///     A serializable reference to a Scene asset that can be used in MonoBehaviours and ScriptableObjects.
///     Provides a convenient way to assign scenes in the Inspector and load them at runtime.
/// </summary>
[Serializable]
public class SceneReference :
	ISerializationCallbackReceiver
{
#if UNITY_EDITOR
	[SerializeField] private SceneAsset? sceneAsset;
#endif
	[SerializeField]            private LoadSceneMode loadSceneMode = LoadSceneMode.Single;
	[SerializeField] [ReadOnly] private int           buildIndex;
	[SerializeField] [ReadOnly] private string        sceneGuid = string.Empty;
	[SerializeField] [ReadOnly] private string        scenePath = string.Empty;

	public SceneReference(Scene scene)
	{
		scenePath = scene.path;
#if UNITY_EDITOR
		sceneGuid = AssetDatabase.AssetPathToGUID(
			scenePath
		);
#endif
	}

	public SceneReference(string scenePath)
	{
		this.scenePath = scenePath;
#if UNITY_EDITOR
		sceneGuid = AssetDatabase.AssetPathToGUID(
			scenePath
		);
#endif
	}

	public bool HasValue => !string.IsNullOrEmpty(
								scenePath
							);

	public bool IsInBuildSettings => BuildIndex >= 0;
	public int  BuildIndex        => buildIndex;

	public Scene Scene => SceneManager.GetSceneByPath(
		Path
	);

	public string Guid => sceneGuid;
	public string Path => scenePath;

	public static implicit operator string?(SceneReference? sceneReference) =>
		sceneReference?.Path;

	public override string ToString() => Path;

	/// <summary>
	///     Loads the scene asynchronously.
	/// </summary>
	/// <param name="mode">Specifies whether the scene should be loaded additive or single.</param>
	/// <returns>
	///     An AsyncOperation handle for the loading process, or null if the reference is invalid or called outside of
	///     Play mode.
	/// </returns>
	public SceneLoadHandle LoadSceneAsync(LoadSceneMode? mode = null)
	{
		if (!HasValue)
		{
			Logger.LogError(
				"Cannot load scene: SceneReference is not set or invalid."
			);

			return new(null);
		}

		if (!UnityEngine.Application.isPlaying)
		{
			Logger.LogWarning(
				"Scene loading is only available in Play mode."
			);

			return new(null);
		}

		// Warn if trying to load a scene not in build settings, as it won't work in a build.
		// Loading by path works in the Editor even if not in Build Settings.
		if (!IsInBuildSettings &&
			UnityEngine.Application.platform != RuntimePlatform.WindowsEditor &&
			UnityEngine.Application.platform != RuntimePlatform.OSXEditor &&
			UnityEngine.Application.platform != RuntimePlatform.LinuxEditor)
			Logger.LogWarning(
				$"Trying to load scene '{scenePath}' which is not added to Build Settings. This will fail in a build."
			);

		return SceneLoader.Load(
			scenePath,
			mode.HasValue ? mode.Value : loadSceneMode
		);
	}

	public void OnAfterDeserialize() { }

	public void OnBeforeSerialize()
	{
#if UNITY_EDITOR
		if (!sceneAsset) return;

		string? sceneAssetPath = AssetDatabase.GetAssetPath(
			sceneAsset
		);

		sceneGuid = AssetDatabase.AssetPathToGUID(
			sceneAssetPath
		);

		scenePath = string.IsNullOrEmpty(
						sceneGuid
					)
						? string.Empty
						: AssetDatabase.GUIDToAssetPath(
							sceneGuid
						);

		buildIndex = SceneUtility.GetBuildIndexByScenePath(
			scenePath
		);
#endif
	}

	public void SetLoadSceneMode(LoadSceneMode mode) => loadSceneMode = mode;

#if UNITY_EDITOR
	public SceneReference(Scene scene, SceneAsset? sceneAsset) : this(
		scene
	)
	{
		this.sceneAsset = sceneAsset;
	}

	public SceneReference(string scenePath, SceneAsset? sceneAsset) : this(
		scenePath
	)
	{
		this.sceneAsset = sceneAsset;
	}

	public SceneAsset? SceneAsset => sceneAsset;
#endif
}

/// <summary>
///     Contains extension methods for Scene Reference.
/// </summary>
public static class SceneReferenceExtensions
{
	public static ICollection<string> Paths(this ICollection<SceneReference> sceneReference)
	{
		sceneReference.ThrowIfNull();
		sceneReference.ThrowIfEmpty();

		ICollection<string> paths = new List<string>(
			sceneReference.Count
		);

		foreach (var scene in sceneReference)
		{
			if (scene == null || !scene.HasValue) continue;

			paths.Add(
				scene.Path
			);
		}

		return paths;
	}
}
