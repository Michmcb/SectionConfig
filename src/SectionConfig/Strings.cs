namespace SectionConfig
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;

	/// <summary>
	/// Represents either <see cref="string"/> or <see cref="string"/>[].
	/// </summary>
	public readonly struct Strings : IReadOnlyList<string>
	{
		private readonly bool array;
		private readonly object val;
		/// <summary>
		/// Creates a new instance representing <see cref="string"/>
		/// </summary>
		/// <param name="s">The string.</param>
		public Strings(string s)
		{
			array = false;
			val = s;
		}
		/// <summary>
		/// Cretes a new instance representing <see cref="string"/>[]
		/// </summary>
		/// <param name="s">The array of strings.</param>
		public Strings(string[] s)
		{
			array = true;
			val = s;
		}
		/// <summary>
		/// Indexes into the array. If this is a single string, only 0 is a valid index.
		/// </summary>
		public string this[int index] =>
			array
				? Unsafe.As<string[]>(val)[index]
				: index == 0
					? Unsafe.As<string>(val)
					: throw new IndexOutOfRangeException("This is just 1 string, index must be 0");
		/// <summary>
		/// The number of strings. If an array, the length of the array. If a single string, always 1.
		/// </summary>
		public int Count => array ? Unsafe.As<string[]>(val).Length : 1;
		/// <summary>
		/// If this is a string, returns the string.
		/// If this is an array, returns the 1st element of the array.
		/// If this is an empty array, returns null.
		/// </summary>
		public override string? ToString()
		{
			if (array)
			{
				string[] arr = Unsafe.As<string[]>(val);
				return arr.Length >= 1 ? arr[0] : null;
			}
			else
			{
				return Unsafe.As<string>(val);
			}
		}
		/// <summary>
		/// If this is a string, returns the string.
		/// If this is an array, returns all elements of the array concatenated, separated by <paramref name="separator"/>.
		/// If this is an empty array, returns null.
		/// </summary>
		public string? ToString(char separator)
		{
			if (array)
			{
				string[] arr = Unsafe.As<string[]>(val);
#if NETSTANDARD2_0
				return arr.Length >= 1 ? string.Join(separator.ToString(), arr) : null;
#else
				return arr.Length >= 1 ? string.Join(separator, arr) : null;
#endif
			}
			else
			{
				return Unsafe.As<string>(val);
			}
		}
		/// <summary>
		/// If this is a string, returns the string.
		/// If this is an array, returns all elements of the array concatenated, separated by <paramref name="separator"/>.
		/// If this is an empty array, returns null.
		/// </summary>
		public string? ToString(string separator)
		{
			if (array)
			{
				string[] arr = Unsafe.As<string[]>(val);
				return arr.Length >= 1 ? string.Join(separator, arr) : null;
			}
			else
			{
				return Unsafe.As<string>(val);
			}
		}
		/// <summary>
		/// Returns an enumerator that iterates over all strings in the array if this is an array, or just returns a single string if this is a single string.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<string> GetEnumerator()
		{
			return array ? ((IReadOnlyList<string>)Unsafe.As<string[]>(val)).GetEnumerator() : new SingleEnumerator(Unsafe.As<string>(val));
		}
		/// <summary>
		/// Returns an enumerator that iterates over all strings in the array if this is an array, or just returns a single string if this is a single string.
		/// </summary>
		/// <returns></returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return array ? Unsafe.As<string[]>(val).GetEnumerator() : new SingleEnumerator(Unsafe.As<string>(val));
		}
		/// <summary>
		/// Enumerates over a single string, as if it were a string array with 1 string.
		/// </summary>
		public struct SingleEnumerator : IEnumerator<string>
		{
			private readonly string s;
			private int state;
			/// <summary>
			/// Creates a new instance with the single element <paramref name="s"/>.
			/// </summary>
			/// <param name="s">The element.</param>
			public SingleEnumerator(string s)
			{
				// We have 3 states. Before s, at s, and after s.
				// In other words, 0, 1, and >=2
				state = 0;
				this.s = s;
			}
			/// <summary>
			/// Returns the current element, or null if <see cref="MoveNext"/> returned false.
			/// </summary>
			public string Current => state == 1 ? s : null!;
			object IEnumerator.Current => Current;
			/// <summary>
			/// Does nothing.
			/// </summary>
			public void Dispose() { }
			/// <summary>
			/// Moves to the next element. Only returns true once.
			/// </summary>
			/// <returns>True on successfully advancing to the next element, false otherwise.</returns>
			public bool MoveNext()
			{
				// We return true once; when we move past the first element
				return state++ == 0;
			}
			/// <summary>
			/// Resets enumeration to the initial position (one before the only element).
			/// </summary>
			public void Reset()
			{
				state = 0;
			}
		}
	}
}
