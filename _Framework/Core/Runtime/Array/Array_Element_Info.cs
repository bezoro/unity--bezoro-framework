using System;

namespace Bezoro.Framework.Core
{
	/// <summary>
	///     A structure to hold detailed information about an array and its data.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the array.</typeparam>
	public readonly struct Array_Element_Info<T>
	{
		/// <summary>
		///     The index of the specific element in the array.
		/// </summary>
		public int Relevant_Index { get; }

		/// <summary>
		///     The element itself.
		/// </summary>
		public T Element { get; }

		/// <summary>
		///     The total length of the array.
		/// </summary>
		public int Length { get; }

		/// <summary>
		///     The type of the array.
		/// </summary>
		public Type Array_Type => typeof(T);

		/// <summary>
		///     The element's type in the array.
		/// </summary>
		public Type Element_Type => Element?.GetType() ?? typeof(T);

		/// <summary>
		///     Constructor for creating an Array_Element_Info instance.
		/// </summary>
		/// <param name="relevant_index">The index of the element in the array.</param>
		/// <param name="element">The specific array element.</param>
		/// <param name="length">The length of the array.</param>
		public Array_Element_Info(int relevant_index, T element, int length)
		{
			Relevant_Index = relevant_index;
			Element        = element;
			Length         = length;
		}

		/// <summary>
		///     Returns a string representation of the array data.
		/// </summary>
		/// <returns>A string detailing the array data.</returns>
		public override string ToString()
		{
			return
				$"Array Data: [Index: {Relevant_Index}, Element: {Element}, Array Length: {Length}, Array Type: {Array_Type}]";
		}
	}
}
