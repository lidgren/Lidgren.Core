#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace Lidgren.Core
{
	/// <summary>
	/// Wrapper for mutable array of characters; use ReadOnlySpan property to get result of all appends. Clear() to reuse.
	/// </summary>
	public class MutableString
	{
		private char[] m_backing;
		private int m_length;

		public int Length { get { return m_length; } }

		public Span<char> Span => new Span<char>(m_backing, 0, m_length);
		public ReadOnlySpan<char> ReadOnlySpan => new ReadOnlySpan<char>(m_backing, 0, m_length);
		public ReadOnlyMemory<char> ReadOnlyMemory => new ReadOnlyMemory<char>(m_backing, 0, m_length);

		public int Capacity
		{
			get { return m_backing.Length; }
			set
			{
				if (value != m_backing.Length)
				{
					var arr = new char[value];
					var len = Math.Min(value, m_length);
					m_backing.AsSpan(0, Math.Min(value, len)).CopyTo(arr);
					m_length = len;
					m_backing = arr;
				}
			}
		}

		public MutableString(string copyFromString)
		{
			m_backing = copyFromString.ToCharArray();
			m_length = copyFromString.Length;
		}

		public MutableString(int capacity)
		{
			m_backing = new char[capacity];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			m_length = 0;
		}

		public override string ToString()
		{
			return ReadOnlySpan.ToString();
		}

		public void Replace(char oldChar, char newChar)
		{
			for (int i = 0; i < m_length; i++)
			{
				if (m_backing[i] == oldChar)
					m_backing[i] = newChar;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureCapacity(int added)
		{
			int newLen = m_length + added;
			if (newLen > m_backing.Length)
				Grow(newLen);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void Grow(int minTotalLength)
		{
			// grow 25% but at least to minTotalLength
			int newLen = m_length + (m_length / 4);
			newLen = Math.Max(newLen, minTotalLength);
			Capacity = newLen;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(ReadOnlySpan<char> str)
		{
			EnsureCapacity(str.Length);
			str.CopyTo(m_backing.AsSpan(m_length));
			m_length += str.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(char value)
		{
			EnsureCapacity(1);
			m_backing[m_length++] = value;
		}

		public void Append(float value)
		{
			int len = m_length;
			bool ok = value.TryFormat(m_backing.AsSpan(len), out var written);
			if (ok)
			{
				m_length = len + written;
				return;
			}
			Grow(len + 8);
			Append(value); // try again
		}

		public void Append(double value)
		{
			int len = m_length;
			bool ok = value.TryFormat(m_backing.AsSpan(len), out var written);
			if (ok)
			{
				m_length = len + written;
				return;
			}
			Grow(len + 8);
			Append(value); // try again
		}

		public void Append(double value, ReadOnlySpan<char> format)
		{
			int len = m_length;
			bool ok = value.TryFormat(m_backing.AsSpan(len), out var written, format: format);
			if (ok)
			{
				m_length = len + written;
				return;
			}
			Grow(len + 8);
			Append(value); // try again
		}

		public void Append(int value)
		{
			int len = m_length;
			bool ok = value.TryFormat(m_backing.AsSpan(len), out var written);
			if (ok)
			{
				m_length = len + written;
				return;
			}
			Grow(len + 8);
			Append(value); // try again
		}

		public void Append(int value, ReadOnlySpan<char> format)
		{
			int len = m_length;
			bool ok = value.TryFormat(m_backing.AsSpan(len), out var written, format: format);
			if (ok)
			{
				m_length = len + written;
				return;
			}
			Grow(len + 8);
			Append(value); // try again
		}

		public void Append(uint value)
		{
			int len = m_length;
			bool ok = value.TryFormat(m_backing.AsSpan(len), out var written);
			if (ok)
			{
				m_length = len + written;
				return;
			}
			Grow(len + 8);
			Append(value); // try again
		}

		public void Append(ulong value)
		{
			int len = m_length;
			bool ok = value.TryFormat(m_backing.AsSpan(len), out var written);
			if (ok)
			{
				m_length = len + written;
				return;
			}
			Grow(len + 8);
			Append(value); // try again
		}

		public void Append(long value)
		{
			int len = m_length;
			bool ok = value.TryFormat(m_backing.AsSpan(len), out var written);
			if (ok)
			{
				m_length = len + written;
				return;
			}
			Grow(len + 8);
			Append(value); // try again
		}

		public void Append(bool value)
		{
			int len = m_length;
			bool ok = value.TryFormat(m_backing.AsSpan(len), out var written);
			if (ok)
			{
				m_length = len + written;
				return;
			}
			Grow(len + 8);
			Append(value); // try again
		}
	}
}
