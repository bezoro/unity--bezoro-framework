using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bezoro.Framework.Unity.Bootstrapping.Abstractions;
using Bezoro.Framework.Unity.Utilities.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Framework.Unity.Bootstrapping
{
	[DefaultExecutionOrder( int.MinValue )] [UsedImplicitly]
	public sealed class GameInitializer : BaseSingleton<GameInitializer>
	{
		private readonly List<object> _initializables;
		private readonly List<GameObject> _instantiatedGameObjects = new();
		private readonly GameInitializationConfig _gameInitConfigAsset;

		private InitializationState _currentState;

		public static bool IsInitializationComplete { get; private set; }

		private GameInitializer()
		{
			BDebug.LogStarted();

			_gameInitConfigAsset = Resources.LoadAll<GameInitializationConfig>( "" ).FirstOrDefault();
			_initializables      = new();
			_currentState        = InitializationState.NotStarted;

			BDebug.LogCompleted();
		}

		protected override void Initialize() {}

		protected override void DisposeManagedResources()
		{
			BDebug.LogStarted();

			_initializables.Clear();

			BDebug.LogCompleted();
		}

	#region RuntimeInitializeOnLoadMethods

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration )]
		private static void OnSubsystemRegistration()
		{
			ResetGameInitializer();
			Instance._gameInitConfigAsset.SubsystemRegistrationPhase();
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterAssembliesLoaded )]
		private static void OnAfterAssembliesLoaded()
		{
			Instance._gameInitConfigAsset.AfterAssemblyLoadPhase();
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSplashScreen )]
		private static void OnBeforeSplashScreen()
		{
			Instance._gameInitConfigAsset.BeforeSplashScreenPhase();
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
		private static void OnBeforeSceneLoad()
		{
			Instance._gameInitConfigAsset.BeforeSceneLoadPhase();
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterSceneLoad )]
		private static void OnAfterSceneLoad()
		{
			Instance._gameInitConfigAsset.AfterSceneLoadPhase();

			IsInitializationComplete = true;
			Instance.EnableInstantiatedGameObjects();
			Instance.Dispose();
			Instance._currentState = InitializationState.Completed;

			BDebug.LogCompleted();
		}

	#endregion

		public static async UniTask<bool> WaitForInitializationComplete( CancellationToken cancellationToken = default )
		{
			while (!IsInitializationComplete)
			{
				await UniTask.Yield( cancellationToken );
			}

			return true;
		}

		private void AutoRegisterInitializables()
		{
			BDebug.LogStarted();

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			var initializableTypes = assemblies
									 .SelectMany
									 (
										 assembly =>
										 {
											 try
											 {
												 return assembly.GetTypes();
											 }
											 catch ( ReflectionTypeLoadException )
											 {
												 return Enumerable.Empty<Type>();
											 }
										 }
									 ).Where( t => t.GetCustomAttribute<AutoInitializeAttribute>() != null )
									 .ToList();

			BDebug.Log(
				$"Found {initializableTypes.Count.ToString().Color( Color.green ).Bold()} initializable types"
			);

			var sortedTypes = TopologicalSort( initializableTypes );

			foreach ( var type in sortedTypes )
			{
				var instance = CreateInstance( type );
				_initializables.Add( instance );
			}

			BDebug.LogCompleted();
		}

		// TODO: Refactor into a reusable helper method
		private List<Type> TopologicalSort( List<Type> types )
		{
			var graph = new Dictionary<Type, HashSet<Type>>();
			var inDegree = new Dictionary<Type, int>();

			// Initialize graph and in-degree
			foreach ( var type in types )
			{
				graph[type]    = new();
				inDegree[type] = 0;
			}

			// Build the graph
			foreach ( var type in types )
			{
				var attr = type.GetCustomAttribute<AutoInitializeAttribute>();

				foreach ( var beforeType in attr.Before )
				{
					if ( types.Contains( beforeType ) )
					{
						graph[beforeType].Add( type );
						inDegree[type]++;
					}
				}

				foreach ( var afterType in attr.After )
				{
					if ( types.Contains( afterType ) )
					{
						graph[type].Add( afterType );
						inDegree[afterType]++;
					}
				}
			}

			// Perform topological sort
			var queue = new Queue<Type>( types.Where( t => inDegree[t] == 0 ) );
			var result = new List<Type>();

			while (queue.Count > 0)
			{
				var current = queue.Dequeue();
				result.Add( current );

				foreach ( var neighbor in graph[current] )
				{
					inDegree[neighbor]--;

					if ( inDegree[neighbor] == 0 )
					{
						queue.Enqueue( neighbor );
					}
				}
			}

			// Check for cycles
			if ( result.Count != types.Count )
			{
				throw new InvalidOperationException( "Circular dependency detected in initialization order." );
			}

			// Sort by priority within each level
			return result.OrderBy( t => t.GetCustomAttribute<AutoInitializeAttribute>().Priority ).ToList();
		}

		private async Task<object> CreateInstance( Type type )
		{
			if ( !type.IsSubclassOf( typeof(MonoBehaviour) ) )
			{
				return Activator.CreateInstance( type );
			}

			var unityObjectInstance = Object.FindAnyObjectByType( type );

			if ( unityObjectInstance != null )
			{
				return unityObjectInstance;
			}

			var go = await Factories.GameObjectFactory.Create(
				type.Name,
				enableCondition: () => IsInitializationComplete,
				componentTypes: type
			);

			_instantiatedGameObjects.Add( go );

			return unityObjectInstance;
		}

	#region InitializationPhases

		private void InitializePhase( InitializationPhase phase )
		{
			BDebug.LogStarted();

			if ( _currentState == InitializationState.Completed )
			{
				return;
			}

			_currentState = InitializationState.InProgress;

			foreach ( var initializable in _initializables )
			{
				try
				{
					InitializeForPhase( initializable, phase );
				}
				catch ( Exception ex )
				{
					HandleInitializationException( initializable, phase, ex );
				}
			}

			if ( phase != InitializationPhase.AfterSceneLoad )
			{
				return;
			}

			IsInitializationComplete = true;
			EnableInstantiatedGameObjects();
			Dispose();
			_currentState = InitializationState.Completed;

			BDebug.LogCompleted();
		}

		private void InitializeForPhase( object initializable, InitializationPhase phase )
		{
			switch ( phase )
			{
				case InitializationPhase.SubsystemRegistration: InitializeSubsystemRegistration( initializable ); break;
				case InitializationPhase.AfterAssembliesLoaded: InitializeAfterAssembliesLoaded( initializable ); break;
				case InitializationPhase.BeforeSplashScreen:    InitializeBeforeSplashScreen( initializable ); break;
				case InitializationPhase.BeforeSceneLoad:       InitializeBeforeSceneLoad( initializable ); break;
				case InitializationPhase.AfterSceneLoad:        InitializeAfterSceneLoad( initializable ); break;
			}
		}

		private void InitializeSubsystemRegistration( object initializable )
		{
			BDebug.LogStarted();

			if ( initializable is ISubsystemRegistrationInitializable subsystemInit )
			{
				BDebug.LogStarted();

				subsystemInit.InitializeSubsystemRegistration();

				BDebug.LogCompleted();
			}

			BDebug.LogCompleted();
		}

		private void InitializeAfterAssembliesLoaded( object initializable )
		{
			BDebug.LogStarted();

			if ( initializable is IAfterAssembliesLoadedInitializable assembliesInit )
			{
				BDebug.LogStarted();

				assembliesInit.InitializeAfterAssembliesLoaded();

				BDebug.LogCompleted();
			}

			BDebug.LogCompleted();
		}

		private void InitializeBeforeSplashScreen( object initializable )
		{
			BDebug.LogStarted();

			if ( initializable is IBeforeSplashScreenInitializable splashInit )
			{
				BDebug.LogStarted();

				splashInit.InitializeBeforeSplashScreen();

				BDebug.LogCompleted();
			}

			BDebug.LogCompleted();
		}

		private void InitializeBeforeSceneLoad( object initializable )
		{
			BDebug.LogStarted();

			if ( initializable is IBeforeSceneLoadInitializable beforeSceneInit )
			{
				BDebug.LogStarted();

				beforeSceneInit.InitializeBeforeSceneLoad();

				BDebug.LogCompleted();
			}

			BDebug.LogCompleted();
		}

		private void InitializeAfterSceneLoad( object initializable )
		{
			BDebug.LogStarted();

			if ( initializable is IAfterSceneLoadInitializable afterSceneInit )
			{
				BDebug.LogStarted();

				afterSceneInit.InitializeAfterSceneLoad();

				BDebug.LogCompleted();
			}

			BDebug.LogCompleted();
		}

	#endregion

		private void HandleInitializationException( object initializable, InitializationPhase phase, Exception ex )
		{
			BDebug.LogException( ex, initializable as Object );

			_currentState = InitializationState.Failed;

			BDebug.LogError( "Initialization failed" );
		}

		private static void ResetGameInitializer()
		{
			if ( IsInitialized )
			{
				Instance.Reset();
			}
		}

		private void Reset()
		{
			BDebug.LogStarted();

			_instantiatedGameObjects.Clear();
			_initializables.Clear();
			_currentState = InitializationState.NotStarted;

			ResetInstance();
		}

		private void EnableInstantiatedGameObjects()
		{
			BDebug.LogStarted();

			foreach ( var gameObject in _instantiatedGameObjects )
			{
				BDebug.LogStarted();

				gameObject.SetActive( true );
			}

			BDebug.LogCompleted();
		}
	}
}
