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
		public static class GameObjectFactory
		{
			public const float DEFAULT_CREATION_TIMEOUT = 5f;

			public static async UniTask<GameObject> Create(
				string        objectName      = "NewGameObject",
				TransformData transformData   = default,
				Func<bool>    enableCondition = null,
				float         timeoutDuration = DEFAULT_CREATION_TIMEOUT,
				params Type[] componentTypes
			)
			{
				ValidationHelpers.ValidateString( objectName );
				ValidationHelpers.ValidatePositiveValue( timeoutDuration );
				var gameObject = CreateInternal( objectName, transformData );
				if ( enableCondition == null )
				{
					gameObject.AddComponents( componentTypes );
					gameObject.EnableBehaviours();
					return gameObject;
				}
				gameObject.SetActive( false );
				await gameObject.AddComponentsAsync( componentTypes );
				using var cts = new CancellationTokenSource( TimeSpan.FromSeconds( timeoutDuration ) );
				await MonitorEnableCondition(
					gameObject,
					enableCondition,
					cts.Token
				);
				await gameObject.EnableBehavioursAsync();
				return gameObject;
			}

			private static GameObject CreateInternal(
				string        name          = "NewGameObject",
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
				BDebug.Log( $"Created GameObject: {name}", go );
				if ( transformData.Parent != null ) { go.transform.SetParent( transformData.Parent ); }
				return go;
			}

			public static async UniTask<IReadOnlyList<GameObject>> CreateBatch(
				IEnumerable<(string name, Type[] components)> objectSpecs,
				TransformData                                 transformData   = default,
				Func<bool>                                    enableCondition = null,
				float                                         timeoutDuration = DEFAULT_CREATION_TIMEOUT
			)
			{
				var createdObjects = new List<GameObject>();
				foreach ( ( var name, var components ) in objectSpecs )
				{
					var go = await Create(
								 name,
								 transformData,
								 enableCondition,
								 timeoutDuration,
								 components
							 );
					createdObjects.Add( go );
					await UniTask.Yield( PlayerLoopTiming.Update );
				}
				return createdObjects;
			}

			private static async UniTask MonitorEnableCondition(
				GameObject        gameObject,
				Func<bool>        enableCondition,
				CancellationToken cancellationToken
			)
			{
				try
				{
					var isEnabled      = !enableCondition();
					var isNotCancelled = !cancellationToken.IsCancellationRequested;
					while (isEnabled && gameObject.NotEqualsNull() && isNotCancelled)
					{
						await UniTask.WaitForEndOfFrame( cancellationToken );
					}

					if ( gameObject.NotEqualsNull() && isNotCancelled )
					{
						BDebug.Log( $"Enabled GameObject: {gameObject.name}" );
						gameObject.SetActive( true );
					}
				}
				catch ( Exception exception )
				{
					BDebug.LogException( exception.Message );
					throw;
				}
			}
		}
	}
}
