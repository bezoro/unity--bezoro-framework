using System;
using System.Collections.Generic;
using System.Linq;
using Bezoro.Framework.Unity.Utilities;
using Bezoro.Framework.Unity.Utilities.String;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bezoro.Framework.Unity
{
	[DefaultExecutionOrder(int.MinValue)]
	public static class SceneManager
	{
		private static string _Current_Load_Screen_Path = string.Empty;

		public static event Action<float>  Loading_Progress_Changed;
		public static event Action<float>  Unloading_Progress_Changed;
		public static event Action<string> Scene_Load_Started;
		public static event Action<string> Scene_Load_Completed;
		public static event Action<string> Scene_Unload_Started;
		public static event Action<string> Scene_Unload_Completed;

		public static HashSet<string> Currently_Loaded_Scene_Paths { get; } = new();

		public static TReturn[] Get_Values_From_Array<T, TReturn>(T[] source_array, Func<T, TReturn> value_extractor)
		{
			var values = new TReturn[source_array.Length];

			for (var i = 0 ; i < source_array.Length ; i++) { values[i] = value_extractor(source_array[i]); }

			return values;
		}

		public static async UniTask Load_Scenes(SceneReference[] scenes, LoadSceneMode mode = LoadSceneMode.Single)
		{
			var scene_paths = Get_Values_From_Array(
				scenes,
				scene => scene.Path
			);

			await Load_Scenes(
				scene_paths,
				mode
			);
		}

		public static async UniTask Load_Scenes(Scene[] scenes, LoadSceneMode mode = LoadSceneMode.Single)
		{
			var scene_paths = Get_Values_From_Array(
				scenes,
				scene => scene.path
			);

			await Load_Scenes(
				scene_paths,
				mode
			);
		}

		public static async UniTask Load_Scenes(string[] paths, LoadSceneMode mode = LoadSceneMode.Single)
		{
			if (mode == LoadSceneMode.Single)
			{
				await Unload_All_Currently_Loaded_Scenes();
			}

			foreach ( var path in paths )
			{
				await Load_Scene(
					path,
					LoadSceneMode.Additive
				);
			}
		}

		public static async UniTask Load_Scene(
			string path,
			LoadSceneMode mode = LoadSceneMode.Single,
			bool allow_scene_activation = true
		)
		{
			BDebug.Log($"[{nameof(SceneManager).Bold()}] Loading scene: {path.Italic()}");
			Scene_Load_Started?.Invoke(path);
			Currently_Loaded_Scene_Paths.Add(path);

			var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(path, mode);

			if (operation == null) { return; }

			operation.allowSceneActivation = allow_scene_activation;

			while ( !operation.isDone )
			{
				Loading_Progress_Changed?.Invoke(operation.progress);
				await UniTask.Yield();
			}

			Scene_Load_Completed?.Invoke(path);
		}

		public static async UniTask Unload_Scene(SceneReference sceneReference)
		{
			await Unload_Scene(sceneReference.Path);
		}

		public static async UniTask Unload_Scene(Scene scene)
		{
			await Unload_Scene(scene.path);
		}

		public static async UniTask Unload_Scenes(string[] paths)
		{
			foreach ( var path in paths ) { await Unload_Scene(path); }
		}

		public static async UniTask Unload_Scenes(Scene[] scenes)
		{
			foreach ( var scene in scenes ) { await Unload_Scene(scene.path); }
		}

		public static async UniTask Unload_Scene(string path)
		{
			if (!Is_Scene_Loaded(path))
			{
				BDebug.Log_Warning(
					$"[{nameof(SceneManager).Bold()}] Cannot unload scene: {path.Italic()} is not loaded"
				);

				return;
			}

			var operation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(path);
			Currently_Loaded_Scene_Paths.Remove(path);
			Scene_Unload_Started?.Invoke(path);
			BDebug.Log($"[{nameof(SceneManager).Bold()}] Unloading scene: {path.Italic()}");

			if (operation == null) { return; }

			while ( !operation.isDone )
			{
				Unloading_Progress_Changed?.Invoke(operation.progress);
				await UniTask.Yield();
			}

			Scene_Unload_Completed?.Invoke(path);
		}

		public static bool Is_Scene_Loaded(SceneReference scene)
		{
			return Is_Scene_Loaded(scene.Path);
		}

		public static bool Is_Scene_Loaded(string path)
		{
			return Currently_Loaded_Scene_Paths.Contains(path);
		}

		public static void Set_Active_Scene(SceneReference scene_reference)
		{
			Set_Active_Scene(scene_reference.Path);
		}

		public static void Set_Active_Scene(Scene scene)
		{
			Set_Active_Scene(scene.path);
		}

		public static void Set_Active_Scene(string path)
		{
			if (Currently_Loaded_Scene_Paths.TryGetValue(
					path,
					out var found_path
				))
			{
				var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(found_path);
				UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
			}
			else
			{
				BDebug.Log_Warning(
					$"[{nameof(SceneManager).Bold()}] Cannot set active scene: {path.Italic()} is not loaded"
				);
			}
		}

		public static int Get_Loaded_Scene_Count()
		{
			return UnityEngine.SceneManagement.SceneManager.sceneCount;
		}

		public static HashSet<string> Get_All_Loaded_Scene_Paths()
		{
			var loaded_scene_paths = new HashSet<string>();

			for (var i = 0 ; i < UnityEngine.SceneManagement.SceneManager.sceneCount ; i++)
			{
				var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
				loaded_scene_paths.Add(scene.path);
			}

			return loaded_scene_paths;
		}

		public static IEnumerable<Scene> Get_All_Loaded_Scenes()
		{
			var loaded_scenes = new List<Scene>();

			for (var i = 0 ; i < UnityEngine.SceneManagement.SceneManager.sceneCount ; i++)
				loaded_scenes.Add(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));

			return loaded_scenes;
		}

		public static bool Are_Scenes_Loaded(
			IEnumerable<SceneReference> scene_references,
			LoadCheckMode mode = LoadCheckMode.Any
		)
		{
			var loaded_scenes = Get_All_Loaded_Scene_Paths();

			switch ( mode )
			{
				case LoadCheckMode.Any:
					return scene_references.Any(
						reference => loaded_scenes.Any(path => path == reference.Path)
					);

				case LoadCheckMode.All:
					return scene_references.All(
						reference => loaded_scenes.Any(path => path == reference.Path)
					);

				case LoadCheckMode.ExceptAny:
					return loaded_scenes.Any(
						path => scene_references.All(reference => path != reference.Path)
					);

				case LoadCheckMode.ExceptAll:
					return loaded_scenes.All(
						path => scene_references.All(reference => path != reference.Path)
					);

				default: return false;
			}
		}

		public static async UniTask Load_Scene_With_Loading_Screen(
			SceneReference target_scene,
			SceneReference loading_screen_scene,
			LoadSceneMode mode = LoadSceneMode.Single
		)
		{
			_Current_Load_Screen_Path = loading_screen_scene.Path;

			await Load_Scene(
				loading_screen_scene.Path,
				LoadSceneMode.Additive
			);

			switch ( mode )
			{
				case LoadSceneMode.Single:
					await Unload_All_Currently_Loaded_Scenes();
					break;
			}

			await Load_Scene(
				target_scene.Path,
				LoadSceneMode.Additive
			);

			await Unload_Scene(loading_screen_scene);
		}

		public static async UniTask Load_Scenes_With_Load_Screen(
			Scene[] scenes_to_load,
			LoadSceneMode mode,
			SceneReference load_screen_scene
		)
		{
			var paths = Get_Values_From_Array(
				scenes_to_load,
				scene => scene.path
			);

			await Load_Scenes_With_Load_Screen(
				paths,
				mode,
				load_screen_scene.Path
			);
		}

		public static async UniTask Load_Scenes_With_Load_Screen(
			SceneReference[] scenes_to_load,
			LoadSceneMode mode,
			SceneReference load_screen_scene
		)
		{
			var paths = Get_Values_From_Array(
				scenes_to_load,
				scene => scene.Path
			);

			await Load_Scenes_With_Load_Screen(
				paths,
				mode,
				load_screen_scene.Path
			);
		}

		public static async UniTask Load_Scenes_With_Load_Screen(
			string[] scenes_to_load_paths,
			LoadSceneMode mode,
			string load_screen_scene_path
		)
		{
			_Current_Load_Screen_Path = load_screen_scene_path;

			await Load_Scene(
				load_screen_scene_path,
				LoadSceneMode.Additive
			);

			await Load_Scenes(
				scenes_to_load_paths,
				mode
			);

			await Unload_Scene(load_screen_scene_path);
		}

		public static async UniTask Unload_All_Currently_Loaded_Scenes()
		{
			var filtered_scenes_paths = new string[Currently_Loaded_Scene_Paths.Count - 1];

			foreach ( var path in Currently_Loaded_Scene_Paths )
			{
				if (path == _Current_Load_Screen_Path) { continue; }

				filtered_scenes_paths.Add(path);
			}

			await Unload_Scenes(filtered_scenes_paths);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Initialize()
		{
			Refresh_Loaded_Scene_Paths();
		}

		private static void Refresh_Loaded_Scene_Paths()
		{
			Currently_Loaded_Scene_Paths.Clear();

			foreach ( var path in Get_All_Loaded_Scene_Paths() )
			{
				Currently_Loaded_Scene_Paths.Add(path);
				Logger.Log($"Added: {path.Italic()}", nameof(SceneManager));
			}
		}

		public enum LoadCheckMode
		{
			Any,
			All,
			ExceptAny,
			ExceptAll
		}
	}
}
