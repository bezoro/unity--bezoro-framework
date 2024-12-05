using System;
using System.Collections.Generic;
using System.Threading;
using Bezoro.Framework.Unity.Utilities;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public static class Factories
	{
		public static class Game_Object
		{
			public const float  DEFAULT_CREATION_TIMEOUT = 5f;
			public const string DEFAULT_OBJECT_NAME      = "New_GameObject";

			public static GameObject Create(
				string object_name = DEFAULT_OBJECT_NAME,
				TransformData transform_data = default,
				Func<bool> activation_condition = null,
				params Type[] component_types
			)
			{
				return CreateAsync(
						   object_name,
						   transform_data,
						   activation_condition,
						   component_types )
					   .GetAwaiter()
					   .GetResult();
			}

			public static async UniTask<GameObject> CreateAsync(
				string objectName = DEFAULT_OBJECT_NAME,
				TransformData transformData = default,
				Func<bool> enableCondition = null,
				params Type[] componentTypes
			)
			{
				Validation_Helpers.ValidateString( objectName );
				var gameObject = CreateInternal(
					objectName,
					transformData );

				if ( enableCondition == null )
				{
					gameObject.AddComponents( componentTypes );
					gameObject.Enable_Behaviours();
					return gameObject;
				}

				gameObject.SetActive( false );
				await gameObject.Add_Components_Async( componentTypes );
				using var cts = new CancellationTokenSource( TimeSpan.FromSeconds( DEFAULT_CREATION_TIMEOUT ) );
				await MonitorEnableCondition(
					gameObject,
					enableCondition,
					cts.Token
				);

				await gameObject.Enable_Behaviours_Async();
				return gameObject;
			}

			private static GameObject CreateInternal(
				string name = "NewGameObject",
				TransformData transformData = default
			)
			{
				if ( !PlayerLoopHelper.IsMainThread ) { UniTask.ReturnToMainThread(); }

				var go = new GameObject( name )
				{
					transform =
					{
						position   = transformData.Position,
						rotation   = transformData.Rotation,
						localScale = transformData.Scale
					}
				};

				BDebug.Log(
					$"Created GameObject: {name}",
					go );

				if ( transformData.Parent != null ) { go.transform.SetParent( transformData.Parent ); }

				return go;
			}

			public static async UniTask<IReadOnlyList<GameObject>> CreateBatch(
				IEnumerable<(string name, Type[] components)> objectSpecs,
				TransformData transformData = default,
				Func<bool> enableCondition = null,
				float timeoutDuration = DEFAULT_CREATION_TIMEOUT
			)
			{
				var createdObjects = new List<GameObject>();

				foreach ( var (name, components) in objectSpecs )
				{
					var go = await CreateAsync(
						name,
						transformData,
						enableCondition,
						components
					);

					createdObjects.Add( go );
					await UniTask.Yield( PlayerLoopTiming.Update );
				}

				return createdObjects;
			}

			private static async UniTask MonitorEnableCondition(
				GameObject gameObject,
				Func<bool> enableCondition,
				CancellationToken cancellationToken
			)
			{
				try
				{
					var isEnabled      = !enableCondition();
					var isNotCancelled = !cancellationToken.IsCancellationRequested;

					while ( isEnabled && gameObject.Not_Equals_Null() && isNotCancelled )
					{
						await UniTask.WaitForEndOfFrame( cancellationToken );
					}

					if ( gameObject.Not_Equals_Null() && isNotCancelled )
					{
						BDebug.Log( $"Enabled GameObject: {gameObject.name}" );
						gameObject.SetActive( true );
					}
				} catch ( Exception exception )
				{
					BDebug.Log_Exception( exception.Message );
					throw;
				}
			}
		}
	}
}
