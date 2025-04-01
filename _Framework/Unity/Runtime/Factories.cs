using System;
using System.Collections.Generic;
using System.Threading;
using Bezoro.Framework.Core;
using Bezoro.Framework.Unity.Utilities;
using Bezoro.Framework.Unity.Utilities.String;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ILogger = Bezoro.Framework.Core.ILogger;

namespace Bezoro.Framework.Unity
{
	public static class Factories
	{
		public static class GameObject
		{
			public const float  DEFAULT_CREATION_TIMEOUT = 5f;
			public const string DEFAULT_OBJECT_NAME      = "[Factory Created] GameObject";

			private static UnityEngine.GameObject Create_Internal(
				string name = DEFAULT_OBJECT_NAME,
				Transform_Data transformData = default
			)
			{
				if (!Player_Loop_Helpers.Is_On_Main_Thread())
					Player_Loop_Helpers.Return_To_Main_Thread();

				var game_obj = new UnityEngine.GameObject(name)
				{
					transform =
					{
						position   = transformData.Position,
						rotation   = transformData.Rotation,
						localScale = transformData.Scale
					}
				};

				BDebug.Log($"Created GameObject: {name.Bold()}", game_obj);
				return game_obj;
			}

			private static async UniTask Monitor_Enable_Condition(
				UnityEngine.GameObject gameObject,
				Func<bool> enableCondition,
				CancellationToken cancellationToken
			)
			{
				try
				{
					var target_not_enabled         = !enableCondition();
					var cancellation_not_requested = !cancellationToken.IsCancellationRequested;

					while ( target_not_enabled && gameObject.Not_Equals_Null() && cancellation_not_requested )
					{
						await UniTask.WaitForEndOfFrame(cancellationToken);

						// Update checks on each frame iteration in case conditions change
						target_not_enabled         = !enableCondition();
						cancellation_not_requested = !cancellationToken.IsCancellationRequested;
					}

					if (gameObject.Not_Equals_Null() && cancellation_not_requested)
					{
						Logger.Log($"Enabled GameObject: {gameObject.name.Bold()}");
						gameObject.SetActive(true);
					}
				}
				catch ( Exception ex )
				{
					BDebug.Log_Exception(ex.Message);
					throw;
				}
			}

			public static UnityEngine.GameObject Create(
				string objectName = DEFAULT_OBJECT_NAME,
				Transform_Data transformData = default,
				Func<bool> activationCondition = null,
				ILogger logger = null,
				params Type[] componentTypes
			)
			{
				if (!Player_Loop_Helpers.Is_On_Main_Thread())
					Player_Loop_Helpers.Return_To_Main_Thread();

				return Create_Async(objectName, transformData, activationCondition, logger, componentTypes)
					.GetAwaiter().GetResult();
			}

			public static async UniTask<UnityEngine.GameObject> Create_Async(
				string objectName = DEFAULT_OBJECT_NAME,
				Transform_Data transformData = default,
				Func<bool> enableCondition = null,
				ILogger logger = null,
				params Type[] componentTypes
			)
			{
				foreach ( var type in componentTypes )
				{
					if (typeof(Component).IsAssignableFrom(type))
						continue;

					Logger.Log_Error($"Type {type} is not a valid Unity Component. Aborting {nameof(Create_Async)}.");
					return null;
				}

				var game_object = Create_Internal(objectName, transformData);
				game_object.SetActive(false);

				if (componentTypes.Length > 0)
				{
					await game_object.Add_Components_Async(componentTypes);
				}

				if (enableCondition != null)
				{
					using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DEFAULT_CREATION_TIMEOUT));
					await Monitor_Enable_Condition(game_object, enableCondition, cts.Token);
				}

				await game_object.Enable_Behaviours();
				return game_object;
			}

			public static async UniTask<IReadOnlyList<UnityEngine.GameObject>> Create_Batch(
				IEnumerable<(string name, Type[] components)> objectSpecs,
				Transform_Data transformData = default,
				Func<bool> enableCondition = null,
				float timeoutDuration = DEFAULT_CREATION_TIMEOUT,
				ILogger logger = null
			)
			{
				var created_objects = new List<UnityEngine.GameObject>();

				foreach ( var (name, components) in objectSpecs )
				{
					var go = await Create_Async(name, transformData, enableCondition, logger, components);
					created_objects.Add(go);
					await UniTask.Yield(PlayerLoopTiming.Update);
				}

				return created_objects;
			}
		}
	}
}
