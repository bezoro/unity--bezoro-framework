using System;
using System.Collections.Generic;
using System.Linq;
using Bezoro.Framework.Unity.Utilities;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Bezoro.Framework.Unity
{
	public interface I_Game_System
	{
		string            SystemName   { get; }
		IEnumerable<Type> Dependencies { get; }
	}

	public interface I_System_Initializer
	{
		void Initialize();
	}

	public enum Initialization_Priority
	{
		Highest = 0,
		High    = 1,
		Normal  = 2,
		Low     = 3,
		Lowest  = 4
	}

	[Serializable]
	public class System_Registration_Info
	{
		// Runtime properties
		public Type                      System_Type  { get; }
		public RuntimeInitializeLoadType Phase        { get; }
		public Initialization_Priority   Priority     { get; set; }
		public I_Game_System             Instance     { get; }
		public List<Type>                Dependencies { get; }

		// Serialized fields for editor
		[SerializeField] private string       system_type_name;
		[SerializeField] private List<string> dependency_type_names = new();

		public System_Registration_Info(
			Type                      system_type,
			RuntimeInitializeLoadType phase,
			Initialization_Priority   priority,
			I_Game_System             instance )
		{
			System_Type      = system_type;
			Phase            = phase;
			Priority         = priority;
			Instance         = instance;
			Dependencies     = new();
			system_type_name = system_type.AssemblyQualifiedName;
		}

		public System_Registration_Info(
			string                    type_name,
			RuntimeInitializeLoadType phase,
			Initialization_Priority   priority )
		{
			system_type_name = type_name;
			System_Type      = Type.GetType( type_name );
			Phase            = phase;
			Priority         = priority;
			Dependencies     = new();
		}

		public void AddDependency( Type dependency_type )
		{
			Dependencies.Add( dependency_type );
			dependency_type_names.Add( dependency_type.AssemblyQualifiedName );
		}

		public void RemoveDependency( Type dependency_type )
		{
			Dependencies.Remove( dependency_type );
			dependency_type_names.Remove( dependency_type.AssemblyQualifiedName );
		}

		public string GetSerializedTypeName()
		{
			return system_type_name;
		}

		public List<string> GetSerializedDependencyNames()
		{
			return dependency_type_names;
		}

		public void ResolveDependencies()
		{
			Dependencies.Clear();
			foreach ( var dep_name in dependency_type_names )
			{
				var dep_type = Type.GetType( dep_name );
				if ( dep_type != null )
				{
					Dependencies.Add( dep_type );
				}
				else
				{
					Debug.LogError( $"Failed to resolve dependency type: {dep_name}" );
				}
			}
		}
	}

	public class Game_Initialization_Manager
	{
		private static readonly Dictionary<RuntimeInitializeLoadType, List<System_Registration_Info>> Phased_Systems =
			new();

		private static List<System_Registration_Info> Sort_Systems_By_Dependencies(
			List<System_Registration_Info> systems )
		{
			try
			{
				return Dependency_Sorter.Sort_By_Dependencies( systems, Get_Dependencies );
			}
			catch ( InvalidOperationException ex )
			{
				Debug.LogError( "Failed to initialize systems due to circular dependencies!" );
				Debug.LogError( Get_Circular_Dependency_Description( systems ) );
				throw;
			}
		}

		private static IEnumerable<System_Registration_Info> Get_Dependencies( System_Registration_Info system )
		{
			return system.Dependencies
			  .Select( depType => Phased_Systems.Values
							 .SelectMany( x => x )
							 .FirstOrDefault( s => s.System_Type == depType ) )
			  .Where( dep => dep != null );
		}

		private static string Get_Circular_Dependency_Description( List<System_Registration_Info> systems )
		{
			var visited = new HashSet<Type>();
			var path    = new Stack<Type>();
			var adjacency_list = systems.ToDictionary(
				s => s.System_Type,
				s => Get_Dependencies( s ).Select( d => d.System_Type ).ToList()
			);

			foreach ( var system in systems )
			{
				if ( Find_Cycle( system.System_Type, visited, path, adjacency_list, out var cycle ) )
				{
					return
						$"Circular dependency detected: {string.Join( " -> ", cycle.Select( t => t.Name ) )} -> {cycle.First().Name}";
				}
			}

			return "No specific circular dependency found, but topological sort failed";
		}

		private static bool Find_Cycle(
			Type                         current,
			HashSet<Type>                visited,
			Stack<Type>                  path,
			Dictionary<Type, List<Type>> adjacency_list,
			out List<Type>               cycle )
		{
			cycle = null;

			if ( path.Contains( current ) )
			{
				cycle = new();
				var cycle_started = false;
				foreach ( var node in path )
				{
					if ( node == current ) { cycle_started = true; }
					if ( cycle_started ) { cycle.Add( node ); }
				}
				return true;
			}

			if ( !visited.Add( current ) ) { return false; }

			path.Push( current );

			foreach ( var neighbor in adjacency_list[current] )
			{
				if ( Find_Cycle( neighbor, visited, path, adjacency_list, out cycle ) )
				{
					return true;
				}
			}

			path.Pop();
			return false;
		}

		private static void Execute_Initialization_Phase( RuntimeInitializeLoadType phase )
		{
			if ( !Phased_Systems.TryGetValue( phase, out var systems ) )
			{
				return;
			}

			var priorityGroups = systems
			  .GroupBy( s => s.Priority )
			  .OrderBy( g => g.Key )
			  .ToList();

			foreach ( var group in priorityGroups )
			{
				var sorted_systems = Sort_Systems_By_Dependencies( group.ToList() );

				foreach ( var system in sorted_systems )
				{
					if ( system.Instance is I_System_Initializer initializer )
					{
						try
						{
							initializer.Initialize();
							Debug.Log( $"Initialized {system.System_Type.Name}" );
						}
						catch ( Exception ex )
						{
							Debug.LogError( $"Failed to initialize {system.System_Type.Name}: {ex}" );
							throw;
						}
					}
				}
			}
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration )]
		private static void SubsystemRegistrationPhase()
		{
			Execute_Initialization_Phase( RuntimeInitializeLoadType.SubsystemRegistration );
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
		private static void BeforeSceneLoadPhase()
		{
			Execute_Initialization_Phase( RuntimeInitializeLoadType.BeforeSceneLoad );
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterSceneLoad )]
		private static void AfterSceneLoadPhase()
		{
			Execute_Initialization_Phase( RuntimeInitializeLoadType.AfterSceneLoad );
		}

		public class InitializationBuilder
		{
			public InitializationBuilder Register<T>(
				RuntimeInitializeLoadType phase    = RuntimeInitializeLoadType.BeforeSceneLoad,
				Initialization_Priority   priority = Initialization_Priority.Normal ) where T : I_Game_System, new()
			{
				var instance = new T();
				var info     = new System_Registration_Info( typeof(T), phase, priority, instance );

				if ( !Phased_Systems.ContainsKey( phase ) )
				{
					Phased_Systems[phase] = new();
				}

				Phased_Systems[phase].Add( info );
				return this;
			}
		}

		public static InitializationBuilder ConfigureInitialization()
		{
			return new();
		}
	}

	public class TestGameSystem : I_Game_System
	{
		public string            SystemName   { get; }
		public IEnumerable<Type> Dependencies { get; }
	}
}
