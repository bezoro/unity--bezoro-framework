using System;
using System.Collections.Generic;
using System.Linq;

namespace Bezoro.Framework.Unity.Utilities
{
	/// <summary>
	///     Provides methods for performing topological sorting of a collection of items.
	/// </summary>
	public static class TypeSorter
	{
		/// <summary>
		///     Performs a topological sort on a collection of items based on their dependencies.
		/// </summary>
		/// <typeparam name="T">The type of the items.</typeparam>
		/// <param name="items">The collection of items to be sorted.</param>
		/// <param name="getDependencies">A function to get the dependencies of an item.</param>
		/// <returns>A list of items sorted in topological order.</returns>
		/// <exception cref="InvalidOperationException">Thrown when a circular dependency is detected.</exception>
		public static List<T> TopologicalSort<T>( List<T> items, Func<T, IEnumerable<T>> getDependencies )
		{
			var graph = new Dictionary<T, HashSet<T>>();
			var inDegree = new Dictionary<T, int>();

			// Initialize graph and in-degree
			foreach ( var item in items )
			{
				graph[item]    = new();
				inDegree[item] = 0;
			}

			// Build the graph
			foreach ( var item in items )
			{
				foreach ( var dependency in getDependencies( item ) )
				{
					if ( items.Contains( dependency ) )
					{
						graph[dependency].Add( item );
						inDegree[item]++;
					}
				}
			}

			// Perform topological sort
			var queue = new Queue<T>( items.Where( item => inDegree[item] == 0 ) );
			var result = new List<T>();

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
			if ( result.Count != items.Count )
			{
				throw new InvalidOperationException( "Circular dependency detected in initialization order." );
			}

			return result;
		}
	}
}
