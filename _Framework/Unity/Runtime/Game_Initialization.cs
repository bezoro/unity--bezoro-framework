using System;
using System.Collections.Generic;
using System.Linq;
using Bezoro.Framework.Unity.Utilities;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	/// <summary>
	///     Defines core functionality for game systems that can be automatically initialized
	/// </summary>
	public interface I_Game_System
	{
		public string            System_Name  { get; }
		public IEnumerable<Type> Dependencies { get; }
	}

	/// <summary>
	///     Defines initialization behavior for game systems
	/// </summary>
	public interface I_System_Initializer
	{
		public void Initialize();
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
		[SerializeField] private string       system_Type_Name;
		[SerializeField] private List<string> dependency_Type_Names = new();

		public System_Registration_Info(
			Type system_type,
			RuntimeInitializeLoadType phase,
			Initialization_Priority priority,
			I_Game_System instance
		)
		{
			System_Type      = system_type ?? throw new ArgumentNullException( nameof( system_type ) );
			Phase            = phase;
			Priority         = priority;
			Instance         = instance ?? throw new ArgumentNullException( nameof( instance ) );
			Dependencies     = new();
			system_Type_Name = system_type.AssemblyQualifiedName;
		}

		public System_Registration_Info(
			string type_name,
			RuntimeInitializeLoadType phase,
			Initialization_Priority priority
		)
		{
			if ( string.IsNullOrEmpty( type_name ) )
				throw new ArgumentException( "Type name cannot be null or empty", nameof( type_name ) );

			system_Type_Name = type_name;
			System_Type = Type.GetType( type_name ) ??
						  throw new ArgumentException( $"Could not resolve type: {type_name}" );

			Phase        = phase;
			Priority     = priority;
			Dependencies = new();
		}

		public Type                      System_Type  { get; }
		public RuntimeInitializeLoadType Phase        { get; }
		public Initialization_Priority   Priority     { get; set; }
		public I_Game_System             Instance     { get; }
		public List<Type>                Dependencies { get; }

		public void Add_Dependency( Type dependency_type )
		{
			if ( dependency_type == null )
				throw new ArgumentNullException( nameof( dependency_type ) );

			Dependencies.Add( dependency_type );
			dependency_Type_Names.Add( dependency_type.AssemblyQualifiedName );
		}

		public void Remove_Dependency( Type dependency_type )
		{
			if ( dependency_type == null )
				throw new ArgumentNullException( nameof( dependency_type ) );

			Dependencies.Remove( dependency_type );
			dependency_Type_Names.Remove( dependency_type.AssemblyQualifiedName );
		}

		public string Get_Serialized_Type_Name()
		{
			return system_Type_Name;
		}

		public List<string> Get_Serialized_Dependency_Names()
		{
			return dependency_Type_Names;
		}

		public void Resolve_Dependencies()
		{
			Dependencies.Clear();

			foreach ( var type_name in dependency_Type_Names )
			{
				var dep_type = Type.GetType( type_name );

				if ( dep_type != null )
				{
					Dependencies.Add( dep_type );
					continue;
				}

				BDebug.Log_Error( $"Failed to resolve dependency type: {type_name}" );
			}
		}
	}

	public class Game_Initialization_Manager
	{
		public static readonly Dictionary<RuntimeInitializeLoadType, List<System_Registration_Info>>
			Phased_Systems = new();

		private static List<System_Registration_Info> Sort_Systems_By_Dependencies(
			List<System_Registration_Info> systems
		)
		{
			try
			{
				return Dependency_Sorter.Sort_By_Dependencies( systems, Get_Dependencies );
			} catch ( InvalidOperationException )
			{
				var error_message = Get_Circular_Dependency_Description( systems );
				BDebug.Log_Error( "Failed to initialize systems due to circular dependencies!" );
				BDebug.Log_Error( error_message );
				throw;
			}
		}

		private static IEnumerable<System_Registration_Info> Get_Dependencies( System_Registration_Info system )
		{
			return system.Dependencies
						 .Select(
							 dep_type => Phased_Systems.Values
													   .SelectMany( x => x )
													   .FirstOrDefault( s => s.System_Type == dep_type ) )
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
				if ( Find_Cycle(
						system.System_Type,
						visited,
						path,
						adjacency_list,
						out var cycle ) )
					return Format_Cycle_Error( cycle );
			}

			return "No specific circular dependency found, but topological sort failed";
		}

		private static string Format_Cycle_Error( List<Type> cycle )
		{
			return "Circular dependency detected: " +
				   $"{string.Join( " -> ", cycle.Select( t => t.Name ) )} -> " +
				   $"{cycle.First().Name}";
		}

		private static bool Find_Cycle(
			Type current,
			HashSet<Type> visited,
			Stack<Type> path,
			Dictionary<Type, List<Type>> adjacency_list,
			out List<Type> cycle
		)
		{
			cycle = null;

			if ( path.Contains( current ) )
			{
				cycle = Extract_Cycle( path, current );
				return true;
			}

			if ( !visited.Add( current ) )
				return false;

			path.Push( current );

			foreach ( var neighbor in adjacency_list[ current ] )
			{
				if ( Find_Cycle( neighbor, visited, path, adjacency_list, out cycle ) )
					return true;
			}

			path.Pop();
			return false;
		}

		private static List<Type> Extract_Cycle( Stack<Type> path, Type current )
		{
			var cycle         = new List<Type>();
			var cycle_started = false;

			foreach ( var node in path )
			{
				if ( node == current )
					cycle_started = true;

				if ( cycle_started )
					cycle.Add( node );
			}

			return cycle;
		}

		private static void Execute_Initialization_Phase( RuntimeInitializeLoadType phase )
		{
			if ( !Phased_Systems.TryGetValue( phase, out var systems ) )
				return;

			var priority_groups = systems
								  .GroupBy( s => s.Priority )
								  .OrderBy( g => g.Key );

			foreach ( var group in priority_groups ) Initialize_System_Group( group.ToList() );
		}

		private static void Initialize_System_Group( List<System_Registration_Info> systems )
		{
			var sorted_systems = Sort_Systems_By_Dependencies( systems );

			foreach ( var system in sorted_systems )
			{
				if ( system.Instance is I_System_Initializer initializer )
					Initialize_System( system, initializer );
			}
		}

		private static void Initialize_System(
			System_Registration_Info system,
			I_System_Initializer initializer
		)
		{
			try
			{
				initializer.Initialize();
				Debug.Log( $"Initialized {system.System_Type.Name}" );
			} catch ( Exception ex )
			{
				Debug.LogError( $"Failed to initialize {system.System_Type.Name}: {ex}" );
				throw;
			}
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration )]
		private static void Subsystem_Registration_Phase()
		{
			Execute_Initialization_Phase( RuntimeInitializeLoadType.SubsystemRegistration );
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
		private static void Before_Scene_Load_Phase()
		{
			Execute_Initialization_Phase( RuntimeInitializeLoadType.BeforeSceneLoad );
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterSceneLoad )]
		private static void After_Scene_Load_Phase()
		{
			Execute_Initialization_Phase( RuntimeInitializeLoadType.AfterSceneLoad );
		}

		public static Initialization_Builder Configure_Initialization()
		{
			return new();
		}
	}

	public class Initialization_Builder
	{
		public Initialization_Builder Register<T_Game_System>(
			RuntimeInitializeLoadType phase = RuntimeInitializeLoadType.BeforeSceneLoad,
			Initialization_Priority priority = Initialization_Priority.Normal
		)
		where T_Game_System : I_Game_System, new()
		{
			var instance = new T_Game_System();
			var info     = new System_Registration_Info( typeof( T_Game_System ), phase, priority, instance );

			if ( !Game_Initialization_Manager.Phased_Systems.ContainsKey( phase ) )
				Game_Initialization_Manager.Phased_Systems[ phase ] = new();

			Game_Initialization_Manager.Phased_Systems[ phase ].Add( info );
			return this;
		}
	}
}
