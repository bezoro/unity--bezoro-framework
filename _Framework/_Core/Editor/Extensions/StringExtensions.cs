using UnityEngine;

namespace Bezoro.Core.Editor.Extensions
{
	public static class StringExtensions
	{
		public static string Bold(this string str) =>
			$"<b>{str}</b>";

		public static string Italic(this string str) =>
			$"<i>{str}</i>";

		public static string Color(this string str, Color color) =>
			$"<color={color}>{str}</color>";

		public static string Size(this string str, int size) =>
			$"<size={size}>{str}</size>";
	}
}