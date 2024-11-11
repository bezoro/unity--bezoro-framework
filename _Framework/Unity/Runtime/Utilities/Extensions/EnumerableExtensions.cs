using System;
using System.Collections.Generic;
using System.Linq;

namespace Bezoro.Framework.Unity.Utilities.Extensions
{
	public static class EnumerableExtensions
	{
		public static bool IsEmpty<T>(this IEnumerable<T> source)
		{
			return !source.Any();
		}

		public static bool IsNull<T>(this IEnumerable<T> source)
		{
			return source == null;
		}

		/// <summary>
		///     Determines whether the collection is null or contains no elements.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to check.</param>
		/// <returns>true if the source sequence is null or empty; otherwise, false.</returns>
		public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
		{
			if (source == null)
			{
				return true;
			}

			return !source.Any();
		}

		/// <summary>
		///     Returns the first element of a sequence, or a default value if the sequence contains no elements.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to return the first element of.</param>
		/// <returns>The first element in the sequence, or default(T) if the sequence is empty.</returns>
		public static T FirstOrDefaultSafe<T>(this IEnumerable<T> source)
		{
			if (source.IsNullOrEmpty())
			{
				return default( T );
			}

			return source.First();
		}

		/// <summary>
		///     Returns the last element of a sequence, or a default value if the sequence contains no elements.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to return the last element of.</param>
		/// <returns>The last element in the sequence, or default(T) if the sequence is empty.</returns>
		public static T LastOrDefaultSafe<T>(this IEnumerable<T> source)
		{
			if (source.IsNullOrEmpty())
			{
				return default( T );
			}

			return source.Last();
		}

		/// <summary>
		///     Filters a sequence of values based on a predicate, but only if the predicate is not null.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to filter.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <returns>
		///     An IEnumerable
		///     <T>
		///         that contains elements from the input sequence that satisfy the condition, or the original sequence if the predicate
		///         is null.
		/// </returns>
		public static IEnumerable<T> WhereIfNotNull<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			if (predicate == null)
			{
				return source;
			}

			return source.Where(predicate);
		}

		/// <summary>
		///     Executes an action on each element of the sequence, but only if the action is not null.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to iterate over.</param>
		/// <param name="action">The action to execute on each element.</param>
		public static void ForEachIfNotNull<T>(this IEnumerable<T> source, Action<T> action)
		{
			if (action == null)
			{
				return;
			}

			foreach ( var item in source )
			{
				action(item);
			}
		}

		/// <summary>
		///     Converts the elements of an IEnumerable<T> to a HashSet<T>.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to convert to a HashSet<T>.</param>
		/// <returns>A HashSet<T> that contains elements from the input sequence.</returns>
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
		{
			return new HashSet<T>(source);
		}

		/// <summary>
		///     Returns a random element from the sequence.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to return a random element from.</param>
		/// <returns>A random element from the sequence, or default(T) if the sequence is empty.</returns>
		public static T RandomElement<T>(this IEnumerable<T> source)
		{
			if (source.IsNullOrEmpty())
			{
				return default( T );
			}

			var random = new Random();
			return source.ElementAt(random.Next(source.Count()));
		}

		/// <summary>
		///     Groups the elements of a sequence according to a specified key selector function and projects the elements for each
		///     group by using a specified function.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
		/// <typeparam name="TElement">The type of the elements in the resulting groupings.</typeparam>
		/// <param name="source">The IEnumerable<TSource> whose elements to group.</param>
		/// <param name="keySelector">A function to extract the key for each element.</param>
		/// <param name="elementSelector">A function to map each source element to an element in the resulting grouping.</param>
		/// <returns>
		///     An IEnumerable<IGrouping
		///     <TKey, TElement>
		///         > where each IGrouping
		///         <TKey, TElement> object contains a collection of objects of type TElement and a key.
		/// </returns>
		public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>
		(
			this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector,
			Func<TSource, TElement> elementSelector
		)
		{
			return source.GroupBy(keySelector, elementSelector);
		}

		/// <summary>
		///     Concatenates the members of a collection, using the specified separator between each member.
		/// </summary>
		/// <typeparam name="T">The type of the elements of values.</typeparam>
		/// <param name="values">A collection that contains the objects to concatenate.</param>
		/// <param name="separator">The string to use as a separator.</param>
		/// <returns>
		///     A string that consists of the members of values delimited by the separator string. If values has no members, the
		///     method returns String.Empty.
		/// </returns>
		public static string JoinToString<T>(this IEnumerable<T> values, string separator)
		{
			return string.Join(separator, values);
		}

		/// <summary>
		///     Returns the maximum value in a sequence of values, or a default value if the sequence is empty.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">A sequence of values to determine the maximum value of.</param>
		/// <returns>The maximum value in the sequence, or default(T) if the sequence is empty.</returns>
		public static T MaxOrDefault<T>(this IEnumerable<T> source)
		{
			if (source.IsNullOrEmpty())
			{
				return default( T );
			}

			return source.Max();
		}

		/// <summary>
		///     Returns the minimum value in a sequence of values, or a default value if the sequence is empty.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">A sequence of values to determine the minimum value of.</param>
		/// <returns>The minimum value in the sequence, or default(T) if the sequence is empty.</returns>
		public static T MinOrDefault<T>(this IEnumerable<T> source)
		{
			if (source.IsNullOrEmpty())
			{
				return default( T );
			}

			return source.Min();
		}

		/// <summary>
		///     Determines whether all elements of a sequence satisfy a condition.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">An IEnumerable<T> that contains the elements to apply the predicate to.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <returns>
		///     true if every element of the source sequence passes the test in the specified predicate, or if the sequence is
		///     empty; otherwise, false.
		/// </returns>
		public static bool AllSafe<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			if (source.IsNullOrEmpty())
			{
				return true;
			}

			return source.All(predicate);
		}

		/// <summary>
		///     Determines whether any element of a sequence satisfies a condition.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">An IEnumerable<T> that contains the elements to apply the predicate to.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <returns>true if any elements in the source sequence pass the test in the specified predicate; otherwise, false.</returns>
		public static bool AnySafe<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			if (source.IsNullOrEmpty())
			{
				return false;
			}

			return source.Any(predicate);
		}

		/// <summary>
		///     Returns a specified number of contiguous elements from the start of a sequence.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to return elements from.</param>
		/// <param name="count">The number of elements to return.</param>
		/// <returns>An IEnumerable<T> that contains the specified number of elements from the start of the input sequence.</returns>
		public static IEnumerable<T> TakeSafe<T>(this IEnumerable<T> source, int count)
		{
			if (source.IsNullOrEmpty())
			{
				return Enumerable.Empty<T>();
			}

			return source.Take(count);
		}

		/// <summary>
		///     Bypasses a specified number of elements in a sequence and then returns the remaining elements.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to return elements from.</param>
		/// <param name="count">The number of elements to skip.</param>
		/// <returns>An IEnumerable<T> that contains the elements that occur after the specified index in the input sequence.</returns>
		public static IEnumerable<T> SkipSafe<T>(this IEnumerable<T> source, int count)
		{
			if (source.IsNullOrEmpty())
			{
				return Enumerable.Empty<T>();
			}

			return source.Skip(count);
		}

		/// <summary>
		///     Returns distinct elements from a sequence by using a specified comparer to compare values.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to remove duplicate elements from.</param>
		/// <param name="comparer">An IEqualityComparer<T> to compare values.</param>
		/// <returns>An IEnumerable<T> that contains distinct elements from the source sequence.</returns>
		public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
		{
			if (source.IsNullOrEmpty())
			{
				return Enumerable.Empty<T>();
			}

			return source.Distinct(comparer);
		}

		/// <summary>
		///     Returns the element at a specified index in a sequence or a default value if the index is out of range.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to return the element from.</param>
		/// <param name="index">The zero-based index of the element to retrieve.</param>
		/// <returns>The element at the specified position in the source sequence, or default(T) if the index is out of range.</returns>
		public static T ElementAtOrDefaultSafe<T>(this IEnumerable<T> source, int index)
		{
			if (source.IsNullOrEmpty() || index < 0 || index >= source.Count())
			{
				return default( T );
			}

			return source.ElementAt(index);
		}

		/// <summary>
		///     Sorts the elements of a sequence in ascending order according to a key.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
		/// <param name="source">A sequence of values to order.</param>
		/// <param name="keySelector">A function to extract a key from an element.</param>
		/// <returns>An IOrderedEnumerable<T> whose elements are sorted according to a key.</returns>
		public static IOrderedEnumerable<T> OrderBySafe<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
		{
			if (source.IsNullOrEmpty())
			{
				return Enumerable.Empty<T>().OrderBy(keySelector);
			}

			return source.OrderBy(keySelector);
		}

		/// <summary>
		///     Sorts the elements of a sequence in descending order according to a key.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
		/// <param name="source">A sequence of values to order.</param>
		/// <param name="keySelector">A function to extract a key from an element.</param>
		/// <returns>An IOrderedEnumerable<T> whose elements are sorted in descending order according to a key.</returns>
		public static IOrderedEnumerable<T> OrderByDescendingSafe<T, TKey>
		(
			this IEnumerable<T> source,
			Func<T, TKey> keySelector
		)
		{
			if (source.IsNullOrEmpty())
			{
				return Enumerable.Empty<T>().OrderByDescending(keySelector);
			}

			return source.OrderByDescending(keySelector);
		}

		/// <summary>
		///     Projects each element of a sequence into a new form.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
		/// <param name="source">A sequence of values to invoke a transform function on.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <returns>
		///     An IEnumerable<TResult> whose elements are the result of invoking the transform function on each element of source.
		/// </returns>
		public static IEnumerable<TResult> SelectSafe<TSource, TResult>
		(
			this IEnumerable<TSource> source,
			Func<TSource, TResult> selector
		)
		{
			if (source.IsNullOrEmpty())
			{
				return Enumerable.Empty<TResult>();
			}

			return source.Select(selector);
		}

		/// <summary>
		///     Projects each element of a sequence to an IEnumerable
		///     <T>, flattens the resulting sequences into one sequence, and invokes a result selector function on each element therein.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TCollection">The type of the intermediate elements collected by collectionSelector.</typeparam>
		/// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
		/// <param name="source">A sequence of values to project.</param>
		/// <param name="collectionSelector">A transform function to apply to each element of the input sequence.</param>
		/// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
		/// <returns>
		///     An IEnumerable
		///     <TResult>
		///         whose elements are the result of invoking the one-to-many transform function collectionSelector on each
		///         element of source and then mapping each of those sequence elements and their corresponding source element to a
		///         result element.
		/// </returns>
		public static IEnumerable<TResult> SelectManySafe<TSource, TCollection, TResult>
		(
			this IEnumerable<TSource> source,
			Func<TSource, IEnumerable<TCollection>> collectionSelector,
			Func<TSource, TCollection, TResult> resultSelector
		)
		{
			if (source.IsNullOrEmpty())
			{
				return Enumerable.Empty<TResult>();
			}

			return source.SelectMany(collectionSelector, resultSelector);
		}

		/// <summary>
		///     Computes the sum of the sequence of nullable decimal values that are obtained by invoking a transform function on each
		///     element of the input sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">A sequence of values that are used to calculate a sum.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <returns>The sum of the projected values.</returns>
		public static decimal? SumSafe<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		{
			if (source.IsNullOrEmpty())
			{
				return null;
			}

			return source.Sum(selector);
		}

		/// <summary>
		///     Computes the average of a sequence of nullable decimal values that are obtained by invoking a transform function on each
		///     element of the input sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <returns>
		///     The average of the sequence of values, or null if the source sequence is empty or contains only values that are
		///     null.
		/// </returns>
		public static decimal? AverageSafe<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		{
			if (source.IsNullOrEmpty())
			{
				return null;
			}

			return source.Average(selector);
		}

		/// <summary>
		///     Returns the number of elements in a sequence.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> that contains the elements to be counted.</param>
		/// <returns>The number of elements in the input sequence.</returns>
		public static int CountSafe<T>(this IEnumerable<T> source)
		{
			if (source.IsNullOrEmpty())
			{
				return 0;
			}

			return source.Count();
		}

		/// <summary>
		///     Returns the number of elements in the specified sequence that satisfy a condition.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">An IEnumerable<T> that contains the elements to be counted.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <returns>The number of elements in the sequence that satisfy the condition in the predicate function.</returns>
		public static int CountSafe<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			if (source.IsNullOrEmpty())
			{
				return 0;
			}

			return source.Count(predicate);
		}

		/// <summary>
		///     Returns the elements of the specified sequence or the type parameter's default value in a singleton collection if the
		///     sequence is empty.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to return a default value for if empty.</param>
		/// <returns>An IEnumerable<T> that contains default(T) if source is empty; otherwise, source.</returns>
		public static IEnumerable<T> DefaultIfEmptySafe<T>(this IEnumerable<T> source)
		{
			if (source.IsNullOrEmpty())
			{
				return new List<T>
				{
					default
				};
			}

			return source;
		}

		/// <summary>
		///     Returns the elements of the specified sequence or the specified value in a singleton collection if the sequence is
		///     empty.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to return a default value for if empty.</param>
		/// <param name="defaultValue">The value to return if the sequence is empty.</param>
		/// <returns>An IEnumerable<T> that contains defaultValue if source is empty; otherwise, source.</returns>
		public static IEnumerable<T> DefaultIfEmptySafe<T>(this IEnumerable<T> source, T defaultValue)
		{
			if (source.IsNullOrEmpty())
			{
				return new List<T> { defaultValue };
			}

			return source;
		}

		/// <summary>
		///     Returns the first element of a sequence, or a default value if the sequence contains no elements.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to return the first element of.</param>
		/// <param name="defaultValue">The default value to return if the sequence is empty.</param>
		/// <returns>The first element in the sequence, or defaultValue if the sequence is empty.</returns>
		public static T FirstOrDefaultSafe<T>(this IEnumerable<T> source, T defaultValue)
		{
			if (source.IsNullOrEmpty())
			{
				return defaultValue;
			}

			return source.FirstOrDefault();
		}

		/// <summary>
		///     Returns the last element of a sequence, or a default value if the sequence contains no elements.
		/// </summary>
		/// <typeparam name="T">The type of the elements of source.</typeparam>
		/// <param name="source">The IEnumerable<T> to return the last element of.</param>
		/// <param name="defaultValue">The default value to return if the sequence is empty.</param>
		/// <returns>The last element in the sequence, or defaultValue if the sequence is empty.</returns>
		public static T LastOrDefaultSafe<T>(this IEnumerable<T> source, T defaultValue)
		{
			if (source.IsNullOrEmpty())
			{
				return defaultValue;
			}

			return source.LastOrDefault();
		}
	}
}
