#nullable enable
using System;

namespace Lidgren.Core
{
	public static class TokenizerExtensions
	{
		public static Tokenizer<char> Tokenize(this ReadOnlySpan<char> str)
		{
			return new Tokenizer<char>(str);
		}
	}

	/// <summary>
	/// Allocation free alternative to string.Split() working on Span<char> (actually span of anything)
	/// Usage:
	/// var tokenizer = "this is a string".Tokenize();
	/// while(tokenizer.GetNext(' ', out var span)
	///    HandleToken(span);
	/// </summary>
	public ref struct Tokenizer<T> where T : struct, IEquatable<T>
	{
		private ReadOnlySpan<T> m_all;
		private int m_currentIndex;

		public Tokenizer(ReadOnlySpan<T> data)
		{
			m_all = data;
			m_currentIndex = 0;
		}

		/// <summary>
		/// Gets remaining (untokenized) data
		/// </summary>
		public readonly ReadOnlySpan<T> Remaining => m_currentIndex >= m_all.Length ? ReadOnlySpan<T>.Empty : m_all.Slice(m_currentIndex);

		/// <summary>
		/// Follows same rules as string.Split(delimiter)
		/// </summary>
		public ReadOnlySpan<T> Next(T delimiter)
		{
			int idx = m_currentIndex;

			var all = m_all;
			if (idx > all.Length)
				return default(ReadOnlySpan<T>);

			for (; ; )
			{
				if (idx >= all.Length)
					break;
				if (all[idx].Equals(delimiter))
					break;
				idx++;
			}
			var token = all.Slice(m_currentIndex, idx - m_currentIndex);
			m_currentIndex = idx + 1;
			return token;
		}

		/// <summary>
		/// Follows same rules as string.Split(delimiters)
		/// </summary>
		public ReadOnlySpan<T> Next(ReadOnlySpan<T> delimiters)
		{
			int idx = m_currentIndex;

			var all = m_all;
			if (idx > all.Length)
				return default(ReadOnlySpan<T>);

			for (; ; )
			{
				if (idx >= all.Length)
					break;

				bool got = false;
				for (int i = 0; i < delimiters.Length; i++)
					got |= all[idx].Equals(delimiters[i]);
				if (got)
					break;
				idx++;
			}
			var token = all.Slice(m_currentIndex, idx - m_currentIndex);
			m_currentIndex = idx + 1;
			return token;
		}

		/// <summary>
		/// Follows same rules as string.Split(delimiter)
		/// </summary>
		public ReadOnlySpan<T> PeekNext(T delimiter)
		{
			int idx = m_currentIndex;

			var all = m_all;
			if (idx > all.Length)
				return default(ReadOnlySpan<T>);

			for (; ; )
			{
				if (idx >= all.Length)
					break;
				if (all[idx].Equals(delimiter))
					break;
				idx++;
			}
			var token = all.Slice(m_currentIndex, idx - m_currentIndex);
			return token;
		}

		/// <summary>
		/// Follows same rules as string.Split(delimiter)
		/// </summary>
		public bool GetNext(T delimiter, out ReadOnlySpan<T> token)
		{
			int idx = m_currentIndex;

			var all = m_all;
			if (idx > all.Length)
			{
				token = default(ReadOnlySpan<T>);
				return false;
			}

			for (; ; )
			{
				if (idx >= all.Length)
					break;
				if (all[idx].Equals(delimiter))
					break;
				idx++;
			}
			token = all.Slice(m_currentIndex, idx - m_currentIndex);
			m_currentIndex = idx + 1;
			return true;
		}

		/// <summary>
		/// Follows same rules as string.Split(delimiter)
		/// </summary>
		public bool GetNext(ReadOnlySpan<T> delimiters, out ReadOnlySpan<T> token)
		{
			int idx = m_currentIndex;

			var all = m_all;
			if (idx > all.Length)
			{
				token = default(ReadOnlySpan<T>);
				return false;
			}

			for (; ; )
			{
				if (idx >= all.Length)
					break;
				bool got = false;
				for (int i = 0; i < delimiters.Length; i++)
					got |= all[idx].Equals(delimiters[i]);
				if (got)
					break;
				idx++;
			}
			token = all.Slice(m_currentIndex, idx - m_currentIndex);
			m_currentIndex = idx + 1;
			return true;
		}

		public bool GetNextSkipEmpty(T delimiter, out ReadOnlySpan<T> token)
		{
			int idx = m_currentIndex;

			if (idx > m_all.Length)
			{
				token = default(ReadOnlySpan<T>);
				return false;
			}

			// skip delimiters
			for (; ; )
			{
				if (idx >= m_all.Length)
				{
					token = default(ReadOnlySpan<T>);
					return false;
				}
				if (m_all[idx].Equals(delimiter) == false)
					break;
				idx++;
			}
			m_currentIndex = idx;

			for (; ; )
			{
				if (idx >= m_all.Length)
					break;
				if (m_all[idx].Equals(delimiter))
					break;
				idx++;
			}
			token = m_all.Slice(m_currentIndex, idx - m_currentIndex);
			m_currentIndex = idx + 1;
			return token.Length > 0;
		}

		public ReadOnlySpan<T> NextSkipEmpty(T delimiter)
		{
			int idx = m_currentIndex;

			if (idx > m_all.Length)
				return default(ReadOnlySpan<T>);

			// skip delimiters
			for (; ; )
			{
				if (idx >= m_all.Length)
					return default(ReadOnlySpan<T>);
				if (m_all[idx].Equals(delimiter) == false)
					break;
				idx++;
			}
			m_currentIndex = idx;

			for (; ; )
			{
				if (idx >= m_all.Length)
					break;
				if (m_all[idx].Equals(delimiter))
					break;
				idx++;
			}
			var token = m_all.Slice(m_currentIndex, idx - m_currentIndex);
			m_currentIndex = idx + 1;
			return token;
		}

		/// <summary>
		/// Returns number of items, up to into.Length
		/// </summary>
		public static int Split(ReadOnlySpan<T> data, T delimiter, Span<Range> into)
		{
			int count = 0;
			int start = 0;
			int idx = 0;
			while(idx < data.Length)
			{
				if (data[idx].Equals(delimiter))
				{
					into[count++] = new Range(start, idx);
					if (count == into.Length)
						return count;
					start = idx + 1;
				}
				idx++;
			}
			if (idx > start)
				into[count++] = new Range(start, idx);
			else if (data[idx - 1].Equals(delimiter))
				into[count++] = new Range(idx - 1, idx - 1); // if ending in delimiter; add extra empty token

			return count;
		}

		public static bool SplitOnce(ReadOnlySpan<T> data, T delimiter, out ReadOnlySpan<T> first, out ReadOnlySpan<T> second)
		{
			var idx = data.IndexOf(delimiter);
			if (idx == -1)
			{
				first = default;
				second = default;
				return false;
			}
			first = data.Slice(0, idx);
			second = data.Slice(idx + 1);
			return true;
		}
	}
}
