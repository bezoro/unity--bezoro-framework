using System;
using System.Collections.Generic;
using System.Threading;
using Bezoro.Framework.Unity.Utilities;
using Cysharp.Threading.Tasks;

namespace Bezoro.Framework.Unity
{
	public static class Factories
	{
		public static class GameObject
		{
			public const float  DEFAULT_CREATION_TIMEOUT = 5f;
			public const string DEFAULT_OBJECT_NAME      = "[Factory Created] GameObject";

			public static UnityEngine.GameObject Create(
				string object_name = DEFAULT_OBJECT_NAME,
				Transform_Data transform_data = default,
				Func<bool> activation_condition = null,
				params Type[] component_types
			)
			{
				return Create_Async(object_name, transform_data, activation_condition, component_types)
					.GetAwaiter()
					.GetResult();
			}

			public static async UniTask<UnityEngine.GameObject> Create_Async(
				string object_name = DEFAULT_OBJECT_NAME,
				Transform_Data transform_data = default,
				Func<bool> enable_condition = null,
				params Type[] component_types
			)
			{
				Validation_Helpers.ValidateString(object_name);

				var game_object = Create_Internal(object_name, transform_data);
				game_object.SetActive(false);

				if (component_types.Length > 0)
					await game_object.Add_Components_Async(component_types);

				if (enable_condition != null)
				{
					using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DEFAULT_CREATION_TIMEOUT));
					await Monitor_Enable_Condition(game_object, enable_condition, cts.Token);
				}

				await game_object.Enable_Behaviours();

				return game_object;
			}

			public static async UniTask<IReadOnlyList<UnityEngine.GameObject>> Create_Batch(
				IEnumerable<(string name, Type[] components)> object_specs,
				Transform_Data transform_data = default,
				Func<bool> enable_condition = null,
				float timeoutDuration = DEFAULT_CREATION_TIMEOUT
			)
			{
				var created_objects = new List<UnityEngine.GameObject>();

				foreach ( var (name, components) in object_specs )
				{
					var go = await Create_Async(name, transform_data, enable_condition, components);
					created_objects.Add(go);
					await UniTask.Yield(PlayerLoopTiming.Update);
				}

				return created_objects;
			}

			private static UnityEngine.GameObject Create_Internal(
				string name = DEFAULT_OBJECT_NAME,
				Transform_Data transformData = default
			)
			{
				Player_Loop_Helpers.Return_To_Main_Thread_If_Necessary();

				var go = new UnityEngine.GameObject(name)
				{
					transform =
					{
						position   = transformData.Position,
						rotation   = transformData.Rotation,
						localScale = transformData.Scale
					}
				};

				BDebug.Log($"Created GameObject: {name.Bold()}", go);
				return go;
			}

			private static async UniTask Monitor_Enable_Condition(
				UnityEngine.GameObject game_object,
				Func<bool> enable_condition,
				CancellationToken ct
			)
			{
				try
				{
					var is_enabled       = !enable_condition();
					var is_not_cancelled = !ct.IsCancellationRequested;

					while ( is_enabled && game_object.Not_Equals_Null() && is_not_cancelled )
						await UniTask.WaitForEndOfFrame(ct);

					if (game_object.Not_Equals_Null() && is_not_cancelled)
					{
						BDebug.Log($"Enabled GameObject: {game_object.name.Bold()}");
						game_object.SetActive(true);
					}
				}
				catch ( Exception exception )
				{
					BDebug.Log_Exception(exception.Message);
					throw;
				}
			}
		}
	}
}
