// using System;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
//
// namespace Bezoro.Framework.Unity.Utilities
// {
// 	public static class Enumerable_Extensions
// 	{
// 		public static bool Is_Null<T>(this IEnumerable<T> source)
// 		{
// 			return source == null;
// 		}
//
// 		public static bool Is_Empty<T>(this IEnumerable<T> source)
// 		{
// 			if (source == null)
// 			{
// 				throw new NullReferenceException("The source collection is null.");
// 			}
//
// 			if (source is ICollection<T> collection)
// 				return collection.Count == 0;
//
// 			if (source is IReadOnlyCollection<T> read_only_collection)
// 				return read_only_collection.Count == 0;
//
// 			return !source.Any();
// 		}
//
// 		public static bool Validate<T>(IEnumerable<T> source)
// 		{
// 			if (source is ICollection<T> collection)
// 				return collection.Count > 0;
//
// 			if (source is IReadOnlyCollection<T> read_only_collection)
// 				return read_only_collection.Count > 0;
//
// 			return source.Any();
// 		}
//
// 		/// <summary>
// 		/// Determines whether the given collection is either null or does not contain any elements.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements in the collection.</typeparam>
// 		/// <param name="source">The <see cref="IEnumerable{T}"/> to evaluate.</param>
// 		/// <returns>Returns true if the collection is null or empty; otherwise, returns false.</returns>
// 		public static bool Is_Null_Or_Empty<T>(this IEnumerable<T> source)
// 		{
// 			if (source == null)
// 				return true;
//
// 			// Try to get count efficiently using available properties/interfaces
// 			if (TryGetCollectionCount(source, out var count))
// 				return count == 0;
//
// 			// Fallback to enumeration
// 			return !source.Any();
// 		}
//
// 		private static bool TryGetCollectionCount<T>(IEnumerable<T> source, out int count)
// 		{
// 			count = 0;
//
// 			switch ( source )
// 			{
// 				case ICollection<T> collection:
// 					count = collection.Count;
// 					return true;
//
// 				case IReadOnlyCollection<T> read_only_collection:
// 					count = read_only_collection.Count;
// 					return true;
//
// 				case Array array:
// 					count = array.Length;
// 					return true;
// 			}
//
// 			// Check for Count or Length property using reflection (cached)
// 			var count_property = GetCountProperty(source.GetType());
//
// 			if (count_property != null)
// 			{
// 				count = (int)count_property.GetValue(source);
// 				return true;
// 			}
//
// 			return false;
// 		}
//
// 		// Cache for reflection results
// 		private static readonly ConcurrentDictionary<Type, PropertyInfo> _CountPropertyCache = new();
//
// 		private static PropertyInfo GetCountProperty(Type type)
// 		{
// 			return _CountPropertyCache.GetOrAdd(
// 				type, t =>
// 				{
// 					// Look for Count or Length property
// 					var property = t.GetProperty("Count", BindingFlags.Public | BindingFlags.Instance) ??
// 								   t.GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
//
// 					// Ensure property is readable and returns int
// 					if (property?.CanRead == true && property.PropertyType == typeof(int))
// 						return property;
//
// 					return null;
// 				});
// 		}
//
// 		/// <summary>
// 		///	 Returns the first element of a sequence, or the default value for the type
// 		///	 if the sequence is null or contains no elements.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements of the sequence.</typeparam>
// 		/// <param name="source">The sequence to return the first element of.</param>
// 		/// <returns>The first element in the sequence, or the default value of type T if the sequence is null or empty.</returns>
// 		public static T First_Or_Default_Safe<T>(this IEnumerable<T> source)
// 		{
// 			var array = source as T[] ?? source.ToArray();
// 			return array.Is_Null_Or_Empty() ? default : array.First();
// 		}
//
// 		/// <summary>
// 		/// Returns the last element of a sequence, or the default value for the type of elements
// 		/// if the sequence is null or contains no elements.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements in the sequence.</typeparam>
// 		/// <param name="source">The sequence to return the last element of.</param>
// 		/// <returns>The last element in the sequence, or the default value of type T if the sequence is null or empty.</returns>
// 		public static T LastOrDefaultSafe<T>(this IEnumerable<T> source)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return default;
// 			}
//
// 			return source.Last();
// 		}
//
// 		/// <summary>
// 		///	 Filters a sequence of values based on a predicate, but only if the predicate is not null.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
// 		/// <param name="source">The <see cref="IEnumerable{T}"/> to filter.</param>
// 		/// <param name="predicate">A function to test each element for a condition.</param>
// 		/// <returns>
// 		///	 An <see cref="IEnumerable{T}"/> that contains elements from the input sequence that satisfy the condition,
// 		///	 or the original sequence if the predicate is null.
// 		/// </returns>
// 		public static IEnumerable<T> Where_If_Not_Null<T>(this IEnumerable<T> source, Func<T, bool> predicate)
// 		{
// 			return predicate == null ? source : source.Where(predicate);
// 		}
//
// 		/// <summary>
// 		///	 Executes an action on each element of the sequence if the sequence is not null.
// 		///	 If the action is null, no operation is performed.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements in the sequence.</typeparam>
// 		/// <param name="source">The sequence to iterate over.</param>
// 		/// <param name="action">The action to execute on each element of the sequence.</param>
// 		public static void ForEachIfNotNull<T>(this IEnumerable<T> source, Action<T> action)
// 		{
// 			if (action == null)
// 			{
// 				return;
// 			}
//
// 			foreach ( var item in source )
// 			{
// 				action(item);
// 			}
// 		}
//
// 		/// <summary>
// 		///	 Converts the elements of an <see cref="IEnumerable{T}" /> to a <see cref="HashSet{T}" />.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
// 		/// <param name="source">The <see cref="IEnumerable{T}" /> to convert to a <see cref="HashSet{T}" />.</param>
// 		/// <returns>A <see cref="HashSet{T}" /> that contains elements from the input sequence.</returns>
// 		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
// 		{
// 			return new( source );
// 		}
//
// 		/// <summary>
// 		///	 Returns a random element from the sequence.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
// 		/// <param name="source">The <see cref="IEnumerable{T}"/> to return a random element from.</param>
// 		/// <returns>A random element from the sequence, or the default value of type T if the sequence is null or empty.</returns>
// 		public static T RandomElement<T>(this IEnumerable<T> source)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return default;
// 			}
//
// 			var random = new Random();
//
// 			return source.ElementAt(random.Next(source.Count()));
// 		}
//
// 		/// <summary>
// 		/// Groups the elements of a sequence according to a specified key selector function and projects the elements for each
// 		/// group using a specified function.
// 		/// </summary>
// 		/// <typeparam name="TSource">The type of the elements of the source sequence.</typeparam>
// 		/// <typeparam name="TKey">The type of the key selected by the keySelector function.</typeparam>
// 		/// <typeparam name="TElement">The type of the elements in the resulting groupings.</typeparam>
// 		/// <param name="source">An <see cref="IEnumerable{TSource}"/> whose elements to group.</param>
// 		/// <param name="keySelector">A function to extract the key for each element.</param>
// 		/// <param name="elementSelector">A function to map each source element to an element in the resulting grouping.</param>
// 		/// <returns>
// 		/// An <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/> where each grouping contains
// 		/// a collection of objects of type <typeparamref name="TElement"/> and a key of type <typeparamref name="TKey"/>.
// 		/// </returns>
// 		/// <exception cref="ArgumentNullException">
// 		/// Thrown when <paramref name="source"/>, <paramref name="keySelector"/>, or <paramref name="elementSelector"/> is null.
// 		/// </exception>
// 		public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
// 			this IEnumerable<TSource> source,
// 			Func<TSource, TKey> keySelector,
// 			Func<TSource, TElement> elementSelector
// 		)
// 		{
// 			if (source is null)
// 				throw new ArgumentNullException(nameof(source));
//
// 			if (keySelector is null)
// 				throw new ArgumentNullException(nameof(keySelector));
//
// 			if (elementSelector is null)
// 				throw new ArgumentNullException(nameof(elementSelector));
//
// 			return source.GroupBy(keySelector, elementSelector);
// 		}
//
// 		/// <summary>
// 		///     Concatenates the members of a collection, using the specified separator between each member.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements of values.</typeparam>
// 		/// <param name="values">A collection that contains the objects to concatenate.</param>
// 		/// <param name="separator">The string to use as a separator.</param>
// 		/// <returns>
// 		///     A string that consists of the members of values delimited by the separator string. If values has no members, the
// 		///     method returns String.Empty.
// 		/// </returns>
// 		public static string Join_To_String<T>(this IEnumerable<T> values, string separator)
// 		{
// 			return string.Join(separator, values);
// 		}
//
// 		/// <summary>
// 		///     Returns the maximum value in a sequence of values, or a default value if the sequence is empty.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements of source.</typeparam>
// 		/// <param name="source">A sequence of values to determine the maximum value of.</param>
// 		/// <returns>The maximum value in the sequence, or default(T) if the sequence is empty.</returns>
// 		public static T Max_Or_Default<T>(this IEnumerable<T> source)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return default;
// 			}
//
// 			return source.Max();
// 		}
//
// 		/// <summary>
// 		///     Returns the minimum value in a sequence of values, or a default value if the sequence is empty.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements of source.</typeparam>
// 		/// <param name="source">A sequence of values to determine the minimum value of.</param>
// 		/// <returns>The minimum value in the sequence, or default(T) if the sequence is empty.</returns>
// 		public static T Min_Or_Default<T>(this IEnumerable<T> source)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return default;
// 			}
//
// 			return source.Min();
// 		}
//
// 		/// <summary>
// 		///	 Determines whether all elements of a sequence satisfy a condition, or returns true if the sequence is null or empty.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements in the sequence.</typeparam>
// 		/// <param name="source">The sequence of elements to evaluate.</param>
// 		/// <param name="predicate">A function that tests each element for a condition.</param>
// 		/// <returns>
// 		///	 true if every element of the sequence satisfies the condition in the specified predicate,
// 		///	 or if the sequence is null or empty; otherwise, false.
// 		/// </returns>
// 		public static bool All_Safe<T>(this IEnumerable<T> source, Func<T, bool> predicate)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return true;
// 			}
//
// 			return source.All(predicate);
// 		}
//
// 		/// <summary>
// 		///	 Determines whether any element of a sequence satisfies a condition, or returns false if the sequence is null or empty.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements of the source sequence.</typeparam>
// 		/// <param name="source">The sequence to apply the condition to.</param>
// 		/// <param name="predicate">A function to test each element for a condition.</param>
// 		/// <returns>
// 		///	 true if any elements in the sequence satisfy the condition; otherwise, false.
// 		///	 Returns false if the sequence is null or empty.
// 		/// </returns>
// 		public static bool Any_Safe<T>(this IEnumerable<T> source, Func<T, bool> predicate)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return false;
// 			}
//
// 			return source.Any(predicate);
// 		}
//
// 		/// <summary>
// 		///	 Returns a specified number of contiguous elements from the start of a sequence.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements in the sequence.</typeparam>
// 		/// <param name="source">The sequence to return elements from.</param>
// 		/// <param name="count">The number of elements to return.</param>
// 		/// <returns>
// 		///	 A sequence that contains the specified number of elements from the start of the input sequence.
// 		///	 If the sequence is null or empty, an empty sequence is returned.
// 		/// </returns>
// 		public static IEnumerable<T> Take_Safe<T>(this IEnumerable<T> source, int count)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return Enumerable.Empty<T>();
// 			}
//
// 			return source.Take(count);
// 		}
//
// 		/// <summary>
// 		///	 Bypasses a specified number of elements in a sequence and then returns the remaining elements.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements in the sequence.</typeparam>
// 		/// <param name="source">The sequence to return elements from.</param>
// 		/// <param name="count">The number of elements to skip.</param>
// 		/// <returns>
// 		///	 A sequence that contains the elements that occur after the specified count in the input sequence.
// 		///	 If the sequence is null or empty, an empty sequence is returned.
// 		/// </returns>
// 		public static IEnumerable<T> SkipSafe<T>(this IEnumerable<T> source, int count)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return Enumerable.Empty<T>();
// 			}
//
// 			return source.Skip(count);
// 		}
//
// 		/// <summary>
// 		///	 Returns distinct elements from a sequence by using a specified comparer to compare values.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
// 		/// <param name="source">An <see cref="IEnumerable{T}"/> to remove duplicate elements from.</param>
// 		/// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare values.</param>
// 		/// <returns>An <see cref="IEnumerable{T}"/> that contains distinct elements from the source sequence.</returns>
// 		public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return Enumerable.Empty<T>();
// 			}
//
// 			return source.Distinct(comparer);
// 		}
//
// 		/// <summary>
// 		///	 Returns the element at a specified index in a sequence, or the default value if the index is out of range or the sequence is null or empty.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements in the sequence.</typeparam>
// 		/// <param name="source">The sequence to retrieve the element from.</param>
// 		/// <param name="index">The zero-based index of the element to retrieve.</param>
// 		/// <returns>
// 		///	 The element at the specified index in the sequence, or default(T) if the index is out of range or the sequence is null or empty.
// 		/// </returns>
// 		public static T Element_At_Or_Default_Safe<T>(this IEnumerable<T> source, int index)
// 		{
// 			if (source.Is_Null_Or_Empty() || index < 0 || index >= source.Count())
// 			{
// 				return default;
// 			}
//
// 			return source.ElementAt(index);
// 		}
//
// 		/// <summary>
// 		///	 Sorts the elements of a sequence in ascending order according to a key.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
// 		/// <typeparam name="TKey">The type of the key used for sorting.</typeparam>
// 		/// <param name="source">The sequence of values to be sorted.</param>
// 		/// <param name="keySelector">A function to extract the key for sorting from an element.</param>
// 		/// <returns>An IOrderedEnumerable&lt;T&gt; whose elements are sorted in ascending order based on the specified key.</returns>
// 		public static IOrderedEnumerable<T> Order_By_Safe<T, TKey>(
// 			this IEnumerable<T> source,
// 			Func<T, TKey> keySelector
// 		)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return Enumerable.Empty<T>().OrderBy(keySelector);
// 			}
//
// 			return source.OrderBy(keySelector);
// 		}
//
// 		/// <summary>
// 		///	 Sorts the elements of a sequence in descending order according to a key.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
// 		/// <typeparam name="TKey">The type of the key used for sorting.</typeparam>
// 		/// <param name="source">A sequence of values to sort.</param>
// 		/// <param name="keySelector">A function to extract the key for sorting from each element.</param>
// 		/// <returns>An IOrderedEnumerable&lt;T&gt; whose elements are sorted in descending order based on the specified key.</returns>
// 		public static IOrderedEnumerable<T> Order_By_Descending_Safe<T, TKey>(
// 			this IEnumerable<T> source,
// 			Func<T, TKey> keySelector
// 		)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return Enumerable.Empty<T>().OrderByDescending(keySelector);
// 			}
//
// 			return source.OrderByDescending(keySelector);
// 		}
//
// 		/// <summary>
// 		///     Projects each element of a sequence into a new form.
// 		/// </summary>
// 		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
// 		/// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
// 		/// <param name="source">A sequence of values to invoke a transform function on.</param>
// 		/// <param name="selector">A transform function to apply to each element.</param>
// 		/// <returns>
// 		///     An IEnumerable<TResult> whose elements are the result of invoking the transform function on each element of source.
// 		/// </returns>
// 		public static IEnumerable<TResult> SelectSafe<TSource, TResult>(
// 			this IEnumerable<TSource> source,
// 			Func<TSource, TResult> selector
// 		)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return Enumerable.Empty<TResult>();
// 			}
//
// 			return source.Select(selector);
// 		}
//
// 		/// <summary>
// 		///     Projects each element of a sequence to an IEnumerable
// 		///     <T>
// 		///         , flattens the resulting sequences into one sequence, and invokes a result selector function on each element
// 		///         therein.
// 		/// </summary>
// 		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
// 		/// <typeparam name="TCollection">The type of the intermediate elements collected by collectionSelector.</typeparam>
// 		/// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
// 		/// <param name="source">A sequence of values to project.</param>
// 		/// <param name="collectionSelector">A transform function to apply to each element of the input sequence.</param>
// 		/// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
// 		/// <returns>
// 		///     An IEnumerable
// 		///     <TResult>
// 		///         whose elements are the result of invoking the one-to-many transform function collectionSelector on each
// 		///         element of source and then mapping each of those sequence elements and their corresponding source element to a
// 		///         result element.
// 		/// </returns>
// 		public static IEnumerable<TResult> SelectManySafe<TSource, TCollection, TResult>(
// 			this IEnumerable<TSource> source,
// 			Func<TSource, IEnumerable<TCollection>> collectionSelector,
// 			Func<TSource, TCollection, TResult> resultSelector
// 		)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return Enumerable.Empty<TResult>();
// 			}
//
// 			return source.SelectMany(
// 				collectionSelector,
// 				resultSelector
// 			);
// 		}
//
// 		/// <summary>
// 		///     Computes the sum of the sequence of nullable decimal values that are obtained by invoking a transform function on
// 		///     each
// 		///     element of the input sequence.
// 		/// </summary>
// 		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
// 		/// <param name="source">A sequence of values that are used to calculate a sum.</param>
// 		/// <param name="selector">A transform function to apply to each element.</param>
// 		/// <returns>The sum of the projected values.</returns>
// 		public static decimal? SumSafe<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return null;
// 			}
//
// 			return source.Sum(selector);
// 		}
//
// 		/// <summary>
// 		///     Computes the average of a sequence of nullable decimal values that are obtained by invoking a transform function on
// 		///     each
// 		///     element of the input sequence.
// 		/// </summary>
// 		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
// 		/// <param name="source">A sequence of values to calculate the average of.</param>
// 		/// <param name="selector">A transform function to apply to each element.</param>
// 		/// <returns>
// 		///     The average of the sequence of values, or null if the source sequence is empty or contains only values that are
// 		///     null.
// 		/// </returns>
// 		public static decimal? AverageSafe<TSource>(
// 			this IEnumerable<TSource> source,
// 			Func<TSource, decimal?> selector
// 		)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return null;
// 			}
//
// 			return source.Average(selector);
// 		}
//
// 		/// <summary>
// 		///     Returns the number of elements in a sequence.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements of source.</typeparam>
// 		/// <param name="source">The IEnumerable<T> that contains the elements to be counted.</param>
// 		/// <returns>The number of elements in the input sequence.</returns>
// 		public static int CountSafe<T>(this IEnumerable<T> source)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return 0;
// 			}
//
// 			return source.Count();
// 		}
//
// 		/// <summary>
// 		///     Returns the number of elements in the specified sequence that satisfy a condition.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements of source.</typeparam>
// 		/// <param name="source">An IEnumerable<T> that contains the elements to be counted.</param>
// 		/// <param name="predicate">A function to test each element for a condition.</param>
// 		/// <returns>The number of elements in the sequence that satisfy the condition in the predicate function.</returns>
// 		public static int CountSafe<T>(this IEnumerable<T> source, Func<T, bool> predicate)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return 0;
// 			}
//
// 			return source.Count(predicate);
// 		}
//
// 		/// <summary>
// 		///     Returns the elements of the specified sequence or the type parameter's default value in a singleton collection if
// 		///     the
// 		///     sequence is empty.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements of source.</typeparam>
// 		/// <param name="source">The IEnumerable<T> to return a default value for if empty.</param>
// 		/// <returns>An IEnumerable<T> that contains default(T) if source is empty; otherwise, source.</returns>
// 		public static IEnumerable<T> DefaultIfEmptySafe<T>(this IEnumerable<T> source)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return new List<T>
// 				{
// 					default
// 				};
// 			}
//
// 			return source;
// 		}
//
// 		/// <summary>
// 		///     Returns the elements of the specified sequence or the specified value in a singleton collection if the sequence is
// 		///     empty.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements of source.</typeparam>
// 		/// <param name="source">The IEnumerable<T> to return a default value for if empty.</param>
// 		/// <param name="defaultValue">The value to return if the sequence is empty.</param>
// 		/// <returns>An IEnumerable<T> that contains defaultValue if source is empty; otherwise, source.</returns>
// 		public static IEnumerable<T> DefaultIfEmptySafe<T>(this IEnumerable<T> source, T defaultValue)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return new List<T> { defaultValue };
// 			}
//
// 			return source;
// 		}
//
// 		/// <summary>
// 		///     Returns the first element of a sequence, or a default value if the sequence contains no elements.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements of source.</typeparam>
// 		/// <param name="source">The IEnumerable<T> to return the first element of.</param>
// 		/// <param name="defaultValue">The default value to return if the sequence is empty.</param>
// 		/// <returns>The first element in the sequence, or defaultValue if the sequence is empty.</returns>
// 		public static T First_Or_Default_Safe<T>(this IEnumerable<T> source, T defaultValue)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return defaultValue;
// 			}
//
// 			return source.FirstOrDefault();
// 		}
//
// 		/// <summary>
// 		///     Returns the last element of a sequence, or a default value if the sequence contains no elements.
// 		/// </summary>
// 		/// <typeparam name="T">The type of the elements of source.</typeparam>
// 		/// <param name="source">The IEnumerable<T> to return the last element of.</param>
// 		/// <param name="defaultValue">The default value to return if the sequence is empty.</param>
// 		/// <returns>The last element in the sequence, or defaultValue if the sequence is empty.</returns>
// 		public static T LastOrDefaultSafe<T>(this IEnumerable<T> source, T defaultValue)
// 		{
// 			if (source.Is_Null_Or_Empty())
// 			{
// 				return defaultValue;
// 			}
//
// 			return source.LastOrDefault();
// 		}
// 	}
// }
