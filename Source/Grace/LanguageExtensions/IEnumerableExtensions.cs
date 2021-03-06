﻿using System;
using System.Collections.Generic;

namespace Grace.LanguageExtensions
{
	/// <summary>
	/// Extensions for IEnumerable
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static class IEnumerableExtensions
	{
		/// <summary>
		/// Apply an action to an IEnumerable
		/// </summary>
		/// <typeparam name="T">t type</typeparam>
		/// <param name="enumerable">enumerable</param>
		/// <param name="action">action to apply</param>
		public static void Apply<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach (T t in enumerable)
			{
				action(t);
			}
		}

		/// <summary>
		/// Operates on an IEnumerable and creates a new IEnumerable that is sorted
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <param name="comparison"></param>
		/// <returns></returns>
		public static List<T> SortEnumerable<T>(this IEnumerable<T> enumerable, Comparison<T> comparison)
		{
			List<T> list = new List<T>(enumerable);

			list.Sort(comparison);

			return list;
		}

		/// <summary>
		/// Reverses an IEnumerable
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <returns></returns>
		public static List<T> ReverseEnumerable<T>(this IEnumerable<T> enumerable)
		{
			var returnValue = new List<T>(enumerable);

			returnValue.Reverse();

			return returnValue;
		}
	}
}