using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Scene_Manager_Utils
	{
		public static event Action<float>  LoadingProgressChanged;
		public static event Action<string> SceneLoadStarted;
		public static event Action<string> SceneLoadCompleted;
		public static event Action<string> SceneUnloadStarted;
		public static event Action<string> SceneUnloadCompleted;

		private static string _current_load_screen_path;

		public static async UniTask LoadScenes( SceneReference[] scenes, LoadSceneMode mode = LoadSceneMode.Additive )
		{
			var scene_paths = new string[scenes.Length];
			for ( var i = 0 ; i < scenes.Length ; i++ ) { scene_paths[i] = scenes[i].Path; }
			await LoadScenes( scene_paths, mode );
		}

		public static async UniTask LoadScenes( Scene[] scenes, LoadSceneMode mode = LoadSceneMode.Additive )
		{
			var scene_paths = new string[scenes.Length];
			for ( var i = 0 ; i < scenes.Length ; i++ ) { scene_paths[i] = scenes[i].path; }
			await LoadScenes( scene_paths, mode );
		}

		public static async UniTask LoadScenes( string[] paths, LoadSceneMode mode = LoadSceneMode.Additive )
		{
			switch ( mode )
			{
				case LoadSceneMode.Single:
					await ClearLoadedScenes();
					break;
			}
			foreach ( var path in paths ) { await LoadScene( path, LoadSceneMode.Additive ); }
		}

		public static async UniTask LoadScene(
			string        path,
			LoadSceneMode mode = LoadSceneMode.Single )
		{
			Debug.Log( $"Loading scene: {path}" );
			SceneLoadStarted?.Invoke( path );

			var operation = SceneManager.LoadSceneAsync( path, mode );
			if ( operation == null ) { return; }
			operation.allowSceneActivation = true;
			while (!operation.isDone)
			{
				LoadingProgressChanged?.Invoke( operation.progress );
				await UniTask.Yield();
			}
			SceneLoadCompleted?.Invoke( path );
		}

		public static async UniTask UnloadScene( SceneReference sceneReference )
		{
			await UnloadScene( sceneReference.Path );
		}

		public static async UniTask UnloadScene( Scene scene )
		{
			await UnloadScene( scene.path );
		}

		public static async UniTask UnloadScene( string path )
		{
			if ( !IsSceneLoaded( path ) )
			{
				BDebug.Log_Warning( $"Cannot unload scene: {path} is not loaded" );
				return;
			}
			var operation = SceneManager.UnloadSceneAsync( path );
			SceneUnloadStarted?.Invoke( path );
			if ( operation == null ) { return; }
			while (!operation.isDone) { await UniTask.Yield(); }
			SceneUnloadCompleted?.Invoke( path );
		}

		public static async UniTask UnloadScenes( string[] paths )
		{
			foreach ( var path in paths ) { await UnloadScene( path ); }
		}

		public static async UniTask UnloadScenes( Scene[] scenes )
		{
			foreach ( var scene in scenes ) { await UnloadScene( scene.path ); }
		}

		public static bool IsSceneLoaded( string path )
		{
			return SceneManager.GetSceneByPath( path ).isLoaded;
		}

		public static bool IsSceneLoaded( SceneReference sceneReference )
		{
			return SceneManager.GetSceneByPath( sceneReference.Path ).isLoaded;
		}

		public static void SetActiveScene( SceneReference sceneReference )
		{
			var scene = SceneManager.GetSceneByPath( sceneReference.Path );

			if ( scene.isLoaded )
			{
				SceneManager.SetActiveScene( scene );
			}
			else
			{
				BDebug.Log_Warning( $"Cannot set active scene: {sceneReference.Path} is not loaded" );
			}
		}

		public static int GetLoadedSceneCount()
		{
			return SceneManager.sceneCount;
		}

		public static string[] GetAllLoadedScenesPaths()
		{
			var all_loaded_scenes = GetAllLoadedScenes();
			var paths             = new string[all_loaded_scenes.Length];
			for ( var i = 0 ; i < all_loaded_scenes.Length ; i++ )
			{
				paths[i] = all_loaded_scenes[i].path;
			}
			return paths;
		}

		public static Scene[] GetAllLoadedScenes()
		{
			var count_loaded  = SceneManager.sceneCount;
			var loaded_scenes = new Scene[count_loaded];

			for ( var i = 0 ; i < count_loaded ; i++ )
			{
				loaded_scenes[i] = SceneManager.GetSceneAt( i );
			}

			return loaded_scenes;
		}

		public static bool IsSceneReferenceLoaded( SceneReference sceneReference )
		{
			return AreScenesLoaded( new[] { sceneReference } );
		}

		public static bool AreScenesLoaded(
			IEnumerable<SceneReference> sceneReferences,
			LoadCheckMode               mode = LoadCheckMode.Any
		)
		{
			var loaded_scenes = GetAllLoadedScenes();

			switch ( mode )
			{
				case LoadCheckMode.Any:
					return sceneReferences.Any(
						reference => loaded_scenes.Any( scene => scene.path == reference.Path )
					);

				case LoadCheckMode.All:
					return sceneReferences.All(
						reference => loaded_scenes.Any( scene => scene.path == reference.Path )
					);

				case LoadCheckMode.ExceptAny:
					return loaded_scenes.Any(
						scene => sceneReferences.All( reference => scene.path != reference.Path )
					);

				case LoadCheckMode.ExceptAll:
					return loaded_scenes.All(
						scene => sceneReferences.All( reference => scene.path != reference.Path )
					);

				default: return false;
			}
		}

		public enum LoadCheckMode
		{
			Any,
			All,
			ExceptAny,
			ExceptAll
		}

		public static async UniTask LoadSceneWithLoadingScreen(
			Scene          currentScene,
			SceneReference targetScene,
			SceneReference loadingScene )
		{
			// Load loading screen additively
			_current_load_screen_path = loadingScene.Path;
			await LoadScene( loadingScene.Path, LoadSceneMode.Additive );

			// TODO: Unload all previous scenes?
			// Unload previous scene
			await UnloadScene( currentScene.path );

			// Load target scene additively
			await LoadScene( targetScene.Path, LoadSceneMode.Additive );

			// Unload loading screen
			await UnloadScene( loadingScene );
		}

		public static async UniTask LoadScenesWithLoadingScreen(
			Scene[]          current_scenes,
			SceneReference[] scenes_to_load,
			SceneReference   load_screen_scene )
		{
			await LoadScene( load_screen_scene.Path, LoadSceneMode.Additive );
			_current_load_screen_path = load_screen_scene.Path;
			await UnloadScenes( current_scenes );
			await LoadScenes( scenes_to_load );
			await UnloadScene( load_screen_scene );
		}

		public static async UniTask LoadScenesWithLoadingScreen(
			Scene[]        scenes_to_load,
			LoadSceneMode  mode,
			SceneReference load_screen_scene )
		{
			var scene_paths = new string[scenes_to_load.Length];
			for ( var i = 0 ; i < scenes_to_load.Length ; i++ ) { scene_paths[i] = scenes_to_load[i].path; }
			await LoadScenesWithLoadingScreen( scene_paths, mode, load_screen_scene.Path );
		}

		public static async UniTask LoadScenesWithLoadingScreen(
			SceneReference[] scenes_to_load,
			LoadSceneMode    mode,
			SceneReference   load_screen_scene )
		{
			var scene_paths = new string[scenes_to_load.Length];
			for ( var i = 0 ; i < scenes_to_load.Length ; i++ ) { scene_paths[i] = scenes_to_load[i].Path; }
			await LoadScenesWithLoadingScreen( scene_paths, mode, load_screen_scene.Path );
		}

		public static async UniTask LoadScenesWithLoadingScreen(
			string[]      scenes_to_load_paths,
			LoadSceneMode mode,
			string        loading_screen_scene_path )
		{
			_current_load_screen_path = loading_screen_scene_path;
			await LoadScene( loading_screen_scene_path, LoadSceneMode.Additive );
			await LoadScenes( scenes_to_load_paths, mode );
			await UnloadScene( loading_screen_scene_path );
		}

		public static async UniTask ClearLoadedScenes()
		{
			var all_loaded_scenes_paths = GetAllLoadedScenesPaths();

			var filtered_scenes_paths = new string[all_loaded_scenes_paths.Length - 1];
			foreach ( var path in all_loaded_scenes_paths )
			{
				if ( path == _current_load_screen_path ) { continue; }
				filtered_scenes_paths.Add( path );
				Debug.Log( $"Unloading: {path}" );
			}

			await UnloadScenes( filtered_scenes_paths );
		}
	}
}
