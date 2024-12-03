using System;
using System.Collections.Generic;
using System.Linq;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Dependency_Sorter
	{
		public static List<T> Sort_By_Dependencies<T>( Dependency_Item<T>[] dependencyItems )
		{
			// Convert the array of DependencyItem into lists of items and dependencies
			var items = dependencyItems.Select( di => di.Item ).ToList();
			Func<T, IEnumerable<T>> getDependencies = item =>
				dependencyItems.First( di => EqualityComparer<T>.Default.Equals( di.Item, item ) ).Dependencies;

			return Sort_By_Dependencies( items, getDependencies );
		}

		/// <summary>
		///     Performs a topological sort on a collection of items based on their dependencies.
		/// </summary>
		/// <typeparam name="T">The type of the items.</typeparam>
		/// <param name="items">The collection of items to be sorted.</param>
		/// <param name="get_dependencies">A function to get the dependencies of an item.</param>
		/// <returns>A list of items sorted in topological order.</returns>
		/// <exception cref="InvalidOperationException">Thrown when a circular dependency is detected.</exception>
		public static List<T> Sort_By_Dependencies<T>( List<T> items, Func<T, IEnumerable<T>> get_dependencies )
		{
			var graph     = Initialize_Graph( items );
			var in_degree = Initialize_In_Degree( items );
			Build_Graph( items, get_dependencies, graph, in_degree );
			var sorted_items = Perform_Topological_Sort( items, graph, in_degree );
			Validate_Sort_Result( items, sorted_items );
			return sorted_items;
		}

		private static Dictionary<T, HashSet<T>> Initialize_Graph<T>( List<T> items )
		{
			var graph = new Dictionary<T, HashSet<T>>();

			foreach ( var item in items ) graph[ item ] = new();

			return graph;
		}

		private static Dictionary<T, int> Initialize_In_Degree<T>( List<T> items )
		{
			var in_degree = new Dictionary<T, int>();

			foreach ( var item in items ) in_degree[ item ] = 0;

			return in_degree;
		}

		private static void Build_Graph<T>(
			List<T> items,
			Func<T, IEnumerable<T>> get_dependencies,
			Dictionary<T, HashSet<T>> graph,
			Dictionary<T, int> in_degree
		)
		{
			foreach ( var item in items )
			{
				foreach ( var dependency in get_dependencies( item ) )
				{
					if ( !items.Contains( dependency ) ) continue;

					graph[ dependency ].Add( item );
					in_degree[ item ]++;
				}
			}
		}

		private static List<T> Perform_Topological_Sort<T>(
			List<T> items,
			Dictionary<T, HashSet<T>> graph,
			Dictionary<T, int> in_degree
		)
		{
			var queue  = new Queue<T>( items.Where( item => in_degree[ item ] == 0 ) );
			var result = new List<T>();

			while ( queue.Count > 0 )
			{
				var current = queue.Dequeue();
				result.Add( current );

				foreach ( var neighbor in graph[ current ] )
				{
					in_degree[ neighbor ]--;

					if ( in_degree[ neighbor ] == 0 ) queue.Enqueue( neighbor );
				}
			}

			return result;
		}

		private static void Validate_Sort_Result<T>( List<T> items, List<T> sorted_items )
		{
			if ( sorted_items.Count != items.Count )
				throw new InvalidOperationException( "Circular dependency detected in initialization order." );
		}
	}
}
