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
		/// Follows same rules as string.Split(delimiter)
		/// </summary>
		public ReadOnlySpan<T> Next(T delimiter)
		{
			int idx = m_currentIndex;

			if (idx > m_all.Length)
				return default(ReadOnlySpan<T>);

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
		/// Follows same rules as string.Split(delimiter)
		/// </summary>
		public bool GetNext(T delimiter, out ReadOnlySpan<T> token)
		{
			int idx = m_currentIndex;

			if (idx > m_all.Length)
			{
				token = default(ReadOnlySpan<T>);
				return false;
			}

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
	}
}
